using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

using CMS.CMSImportExport;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.IO;
using CMS.Base;
using CMS.SiteProvider;

namespace CMS.Forums
{
    /// <summary>
    /// Handles special actions during the forum import process.
    /// </summary>
    internal static class ForumImport
    {
        #region "Methods"

        /// <summary>
        /// Initializes import handlers
        /// </summary>
        public static void Init()
        {
            ImportExportEvents.ImportObjectType.After += Import_After;
            SpecialActionsEvents.GetBinaryDataSourcePath.Execute += GetBinaryDataSourcePath_Execute;
        }


        private static void Import_After(object sender, ImportDataEventArgs e)
        {
            var objectType = e.ObjectType;
            // General forums and community group forums
            if ((objectType == ForumInfo.OBJECT_TYPE) || (objectType == PredefinedObjectType.GROUP))
            {
                if (!e.ParentImported)
                {
                    return;
                }

                var settings = e.Settings;
                var data = e.Data;

                // There is no data for this object type
                if (DataHelper.DataSourceIsEmpty(data) || DataHelper.DataSourceIsEmpty(data.Tables["Forums_Forum"]))
                {
                    return;
                }

                // Forum layouts
                ImportLayouts(settings);

                // Forum posts
                DataTable table = data.Tables["Forums_Forum"];
                ImportForumPosts(settings, table, e.TranslationHelper, objectType == PredefinedObjectType.GROUP);
            }
        }


        private static void ImportLayouts(SiteImportSettings settings)
        {
            string safeObjectType = ImportExportHelper.GetSafeObjectTypeName(ForumInfo.OBJECT_TYPE);

            string sourcePath = DirectoryHelper.CombinePath(settings.TemporaryFilesPath, ImportExportHelper.DATA_FOLDER, ImportExportHelper.FILES_FOLDER) + "\\";
            sourcePath += DirectoryHelper.CombinePath(safeObjectType, ImportExportHelper.SRC_FORUM_LAYOUTS_FOLDER) + "\\";

            string targetPath = ImportProvider.GetTargetPath(settings, settings.WebsitePath);
            targetPath += ImportExportHelper.SRC_FORUM_LAYOUTS_FOLDER + "\\";

            if (Directory.Exists(sourcePath))
            {
                // Forum custom layouts folder
                settings.FileOperations.Add(ForumInfo.OBJECT_TYPE, sourcePath, targetPath, FileOperationEnum.CopyDirectory);
            }
        }


        /// <summary>
        /// Import forum posts.
        /// </summary>
        /// <param name="settings">Import settings</param>
        /// <param name="table">Parent data</param>
        /// <param name="th">Translation helper</param>
        /// <param name="groupPosts">Indicates if the posts are imported for group forums</param>
        private static void ImportForumPosts(SiteImportSettings settings, DataTable table, TranslationHelper th, bool groupPosts)
        {
            ProcessObjectEnum processType = settings.GetObjectsProcessType(ForumInfo.OBJECT_TYPE, true);
            if (processType == ProcessObjectEnum.None)
            {
                return;
            }

            // Check import settings
            if (!ValidationHelper.GetBoolean(settings.GetSettings(ImportExportHelper.SETTINGS_FORUM_POSTS), true) || settings.ExistingSite)
            {
                return;
            }

            var infoObj = ModuleManager.GetReadOnlyObject(ForumPostInfo.OBJECT_TYPE);
            
            var postsIds = new Dictionary<int, int>();

            foreach (DataRow dr in table.Rows)
            {
                // Process canceled
                if (settings.ProcessCanceled)
                {
                    ImportProvider.ImportCanceled();
                }

                // Get forum name
                Guid forumGuid = ValidationHelper.GetGuid(dr["ForumGUID"], Guid.Empty);
                string forumName = dr["ForumName"].ToString();
                string forumDisplayName = dr["ForumDisplayName"].ToString();

                // Get the SiteID of the forum the posts will be provided with
                int? forumSiteID = ValidationHelper.GetInteger(dr[ForumInfo.TYPEINFO.SiteIDColumn], -1);
                if (forumSiteID.Value <= 0)
                {
                    forumSiteID = null;
                }
                else
                {
                    // Translate old SiteID to new SiteID
                    forumSiteID = th.GetNewID(SiteInfo.TYPEINFO.ObjectType, forumSiteID.Value, SiteInfo.TYPEINFO.CodeNameColumn, TranslationHelper.AUTO_SITEID, null, null, null);
                }

                try
                {
                    // Check selected forums
                    if ((processType != ProcessObjectEnum.All) && !settings.IsSelected(ForumInfo.OBJECT_TYPE, forumName, true) && !groupPosts)
                    {
                        continue;
                    }

                    // Log progress
                    ImportProvider.LogProgress(LogStatusEnum.Info, settings, string.Format(settings.GetAPIString("ImportSite.ImportingForumPosts", "Importing forum '{0}' posts"), HTMLHelper.HTMLEncode(forumDisplayName)));

                    // Save the settings progress
                    settings.SavePersistentLog();

                    // Initialize data
                    string forumObjectType = ImportExportHelper.FORUMPOST_PREFIX + forumGuid;

                    if (groupPosts)
                    {
                        forumObjectType += "_group";
                    }

                    // Get data
                    var ds = ImportProvider.LoadObjects(settings, forumObjectType, true);
                    var subtable = ObjectHelper.GetTable(ds, infoObj);
                        
                    if (!DataHelper.DataSourceIsEmpty(subtable))
                    {
                        if (subtable.Columns.Contains(ForumPostInfo.TYPEINFO.SiteIDColumn))
                        {
                            foreach (DataRow dataRow in subtable.Rows)
                            {
                                dataRow[ForumPostInfo.TYPEINFO.SiteIDColumn] = forumSiteID;
                            }
                        }

                        // Import the objects
                        var partialTable = ImportProvider.ImportObjects(settings, ds, ForumPostInfo.OBJECT_TYPE, false, th, true, ProcessObjectEnum.All, null);
                        postsIds = DataHelper.Merge<Dictionary<int, int>, int, int>(postsIds, partialTable, false);
                    }

                    // Refresh forum posts and threads count
                    ForumInfoProvider.RefreshDataCount(forumName, settings.SiteId);
                }
                catch (ProcessCanceledException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    // Log exception
                    ImportProvider.LogProgressError(settings, string.Format(settings.GetAPIString("ImportSite.ErrorImportingForumPosts", "Error importing forum '{0}' posts."), HTMLHelper.HTMLEncode(forumDisplayName)), ex);
                    throw;
                }
            }

            // Process forum attachment
            string forumAttachmentObjectType = ForumAttachmentInfo.OBJECT_TYPE;
            
            if (groupPosts)
            {
                forumAttachmentObjectType += "_group";
            }

            DataSet dsAttachment = ImportProvider.LoadObjects(settings, forumAttachmentObjectType, true);
            if (!DataHelper.DataSourceIsEmpty(dsAttachment))
            {
                // Import the objects
                var posts = postsIds.Keys.ToList();
                ImportProvider.ImportObjects(settings, dsAttachment, ForumAttachmentInfo.OBJECT_TYPE, false, th, true, ProcessObjectEnum.All, posts);
            }
        }


        private static void GetBinaryDataSourcePath_Execute(object sender, GetBinaryDataSourcePathEventArgs e)
        {
            var settings = e.Settings;
            var infoObj = e.Object;

            if (infoObj.TypeInfo.ObjectType == ForumAttachmentInfo.OBJECT_TYPE)
            {
                var attachment = (ForumAttachmentInfo)infoObj;

                // Get path
                e.Path = ImportProvider.GetBinaryDataSourcePath(settings, infoObj, "forums_attachment\\CMSFiles", attachment.AttachmentGUID.ToString(), attachment.AttachmentFileExtension);
            }
        }

        #endregion
    }
}