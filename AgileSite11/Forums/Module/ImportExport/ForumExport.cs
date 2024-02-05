using System;
using System.Collections.Generic;
using System.Data;

using CMS.CMSImportExport;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.IO;
using CMS.Base;

namespace CMS.Forums
{
    /// <summary>
    /// Handles special actions during the forum export process.
    /// </summary>
    internal static class ForumExport
    {
        #region "Methods"

        /// <summary>
        /// Initializes export handlers
        /// </summary>
        internal static void Init()
        {
            ImportExportEvents.ExportObjects.After += Export_After;
        }


        private static void Export_After(object sender, ExportEventArgs e)
        {
            var objectType = e.ObjectType;
            // General forums and community group forums
            if ((objectType == ForumInfo.OBJECT_TYPE) || (objectType == PredefinedObjectType.GROUP))
            {
                var settings = e.Settings;
                var data = e.Data;

                // There is no data for this object type
                if (DataHelper.DataSourceIsEmpty(data) || DataHelper.DataSourceIsEmpty(data.Tables["Forums_Forum"]))
                {
                    return;
                }

                // Forum layouts
                ExportLayouts(settings);

                // Forum posts
                DataTable table = data.Tables["Forums_Forum"];
                ExportForumPosts(settings, table, objectType == PredefinedObjectType.GROUP);
            }
        }


        private static void ExportLayouts(SiteExportSettings settings)
        {
            if (!settings.CopyFiles)
            {
                return;
            }

            // Check setting
            if (!ValidationHelper.GetBoolean(settings.GetSettings(ImportExportHelper.SETTINGS_COPY_FORUM_CUSTOM_LAYOUTS_FOLDER), true))
            {
                return;
            }

            string safeObjectType = ImportExportHelper.GetSafeObjectTypeName(ForumInfo.OBJECT_TYPE);

            string targetPath = DirectoryHelper.CombinePath(settings.TemporaryFilesPath, ImportExportHelper.DATA_FOLDER, ImportExportHelper.FILES_FOLDER) + "\\";
            targetPath = DirectoryHelper.CombinePath(targetPath, safeObjectType) + "\\" + ImportExportHelper.SRC_FORUM_LAYOUTS_FOLDER + "\\";
            string sourcePath = DirectoryHelper.CombinePath(settings.WebsitePath, ImportExportHelper.SRC_FORUM_LAYOUTS_FOLDER) + "\\";

            // Log process
            ExportProvider.LogProgress(LogStatusEnum.Info, settings, settings.GetAPIString("ExportSite.CopyingForumLayoutsFolder", "Copying forum custom layouts folder"));

            // Export process canceled
            if (settings.ProcessCanceled)
            {
                ExportProvider.ExportCanceled();
            }

            try
            {
                // Forum custom layouts folder
                if (Directory.Exists(sourcePath))
                {
                    ExportProvider.CopyDirectory(sourcePath, targetPath, settings.WebsitePath);
                }
            }
            catch
            {
            }
        }


        /// <summary>
        /// Export forum posts.
        /// </summary>
        /// <param name="settings">Export settings</param>
        /// <param name="table">Parent data</param>
        /// <param name="groupPosts">Indicates if the posts are exported for group forums</param>
        private static void ExportForumPosts(SiteExportSettings settings, DataTable table, bool groupPosts)
        {
            // Check export setting
            if (!ValidationHelper.GetBoolean(settings.GetSettings(ImportExportHelper.SETTINGS_FORUM_POSTS), true))
            {
                return;
            }

            // Get forum post object
            GeneralizedInfo postObj = ModuleManager.GetReadOnlyObject(ForumPostInfo.OBJECT_TYPE);
            if (postObj != null)
            {
                var postIDsArray = new List<int>();

                foreach (DataRow dr in table.Rows)
                {
                    // Export process canceled
                    if (settings.ProcessCanceled)
                    {
                        ExportProvider.ExportCanceled();
                    }

                    // Get forum data
                    Guid forumGuid = ValidationHelper.GetGuid(dr["ForumGUID"], Guid.Empty);
                    string forumDisplayName = ValidationHelper.GetString(dr["ForumDisplayName"], "");

                    // Log progress
                    ExportProvider.LogProgress(LogStatusEnum.Info, settings, string.Format(settings.GetAPIString("ExportSite.ExportingForumPosts", "Exporting forum '{0}' posts"), HTMLHelper.HTMLEncode(forumDisplayName)));

                    // Save the settings
                    settings.SavePersistentLog();

                    try
                    {
                        // Initialize data
                        int forumId = ValidationHelper.GetInteger(dr["ForumID"], 0);
                        string forumObjectType = ImportExportHelper.FORUMPOST_PREFIX + forumGuid;
                        if (groupPosts)
                        {
                            forumObjectType += "_group";
                        }

                        // Get forum posts data
                        DataSet ds = postObj.GetData(null, "PostForumID = " + forumId, "PostLevel ASC");
                        if (!DataHelper.DataSourceIsEmpty(ds))
                        {
                            ds.Tables[0].TableName = "Forums_ForumPost";

                            // Save data
                            ExportProvider.SaveObjects(settings, ds, forumObjectType, true);

                            // Get post IDs data
                            var postIDs = DataHelper.GetIntegerValues(ds.Tables["Forums_ForumPost"], "PostID");
                            postIDsArray.AddRange(postIDs);
                        }
                    }
                    catch (ProcessCanceledException)
                    {
                        throw;
                    }
                    catch (Exception ex)
                    {
                        // Log exception
                        ExportProvider.LogProgressError(settings, string.Format(settings.GetAPIString("ExportSite.ErrorExportingForumPosts", "Error exporting forum '{0}' posts."), HTMLHelper.HTMLEncode(forumDisplayName)), ex);
                        throw;
                    }
                }

                // Export processed attachments
                QueryDataParameters parameters = new QueryDataParameters();

                using (var cond = new SelectCondition(parameters))
                {
                    cond.PrepareCondition("AttachmentPostID", postIDsArray);

                    string attachmentWhere = cond.WhereCondition;

                    // Some data should be selected
                    if (attachmentWhere != SqlHelper.NO_DATA_WHERE)
                    {
                        GeneralizedInfo attachmentObj = ModuleManager.GetReadOnlyObject(ForumAttachmentInfo.OBJECT_TYPE);
                        DataSet attachmentDS = attachmentObj.GetData(parameters, attachmentWhere, null, -1, null, false);
                        DataHelper.SetTableName(attachmentDS, "Forums_Attachment");

                        if (DataHelper.DataSourceIsEmpty(attachmentDS))
                        {
                            return;
                        }

                        // Save data
                        string forumAttachmentObjectType = ForumAttachmentInfo.OBJECT_TYPE;
                        if (groupPosts)
                        {
                            forumAttachmentObjectType += "_group";
                        }
                        ExportProvider.SaveObjects(settings, attachmentDS, forumAttachmentObjectType, true);

                        // Copy files
                        CopyFiles(settings, attachmentDS);
                    }
                }
            }
        }


        private static void CopyFiles(SiteExportSettings settings, DataSet data)
        {
            if (!settings.CopyFiles)
            {
                return;
            }

            // Log process
            ExportProvider.LogProgress(LogStatusEnum.Info, settings, settings.GetAPIString("ExportSite.CopyingForumAttachments", "Copying forum attachments."));

            // Process all forum attachments
            var table = data.Tables[0];
            foreach (DataRow dr in table.Rows)
            {
                // Export process canceled
                if (settings.ProcessCanceled)
                {
                    ExportProvider.ExportCanceled();
                }

                string guid = dr["AttachmentGUID"].ToString();
                string extension = ValidationHelper.GetString(dr["AttachmentFileExtension"], "");
                string fileName = guid + extension;

                if (!ExportProvider.IsFileExcluded(fileName))
                {
                    // Get the binary
                    object binary = DataHelper.GetDataRowValue(dr, "AttachmentBinary");
                    byte[] fileBinary = null;
                    if (binary != DBNull.Value)
                    {
                        fileBinary = (byte[])binary;
                    }
                    if (fileBinary == null)
                    {
                        // Get the graph object
                        Guid attachmentGuid = ValidationHelper.GetGuid(guid, Guid.Empty);
                        fileBinary = ForumAttachmentInfoProvider.GetAttachmentFile(attachmentGuid, settings.SiteName);
                    }

                    // Save the file
                    if ((fileBinary != null) && (guid != ""))
                    {
                        try
                        {
                            string targetPath = DirectoryHelper.CombinePath(settings.TemporaryFilesPath, ImportExportHelper.DATA_FOLDER, ImportExportHelper.FILES_FOLDER) + "\\forums_attachment\\CMSFiles\\";
                            string filePath = targetPath + DirectoryHelper.CombinePath(guid.Substring(0, 2), fileName);
                            filePath = ImportExportHelper.GetExportFilePath(filePath);

                            // Copy file
                            DirectoryHelper.EnsureDiskPath(filePath, settings.WebsitePath);
                            File.WriteAllBytes(filePath, fileBinary);

                            // Clear the binary
                            DataHelper.SetDataRowValue(dr, "AttachmentBinary", null);
                        }
                        catch
                        {
                        }
                    }
                }
            }
        }

        #endregion
    }
}