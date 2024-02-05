using System;
using System.Data;

using CMS.CMSImportExport;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.IO;
using CMS.Base;

namespace CMS.MediaLibrary
{
    /// <summary>
    /// Handles special actions during the Media library export process.
    /// </summary>
    internal static class MediaLibraryExport
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
            // General library or community group library
            if ((objectType == MediaLibraryInfo.OBJECT_TYPE) || (objectType == PredefinedObjectType.GROUP))
            {
                var settings = e.Settings;
                var data = e.Data;

                // There is no data for this object type
                if (DataHelper.DataSourceIsEmpty(data) || DataHelper.DataSourceIsEmpty(data.Tables["Media_Library"]))
                {
                    return;
                }

                // Library files
                DataTable table = data.Tables["Media_Library"];
                ExportMediaFiles(settings, table, objectType == PredefinedObjectType.GROUP);
            }
        }


        /// <summary>
        /// Export media files.
        /// </summary>
        /// <param name="settings">Export settings</param>
        /// <param name="table">Parent data</param>
        /// <param name="isGroup">Indicates if group objects are exported</param>
        private static void ExportMediaFiles(SiteExportSettings settings, DataTable table, bool isGroup)
        {
            // Check export setting
            if (!ValidationHelper.GetBoolean(settings.GetSettings(ImportExportHelper.SETTINGS_MEDIA_FILES), true))
            {
                return;
            }

            // Get file object
            GeneralizedInfo fileObj = ModuleManager.GetReadOnlyObject(MediaFileInfo.OBJECT_TYPE);
            if (fileObj != null)
            {
                foreach (DataRow dr in table.Rows)
                {
                    // Export process canceled
                    if (settings.ProcessCanceled)
                    {
                        ExportProvider.ExportCanceled();
                    }

                    // Get library data
                    Guid libraryGuid = ValidationHelper.GetGuid(dr["LibraryGUID"], Guid.Empty);
                    string libraryDisplayName = ValidationHelper.GetString(dr["LibraryDisplayName"], "");

                    // Log progress
                    ExportProvider.LogProgress(LogStatusEnum.Info, settings, string.Format(settings.GetAPIString("ExportSite.ExportingLibraryFiles", "Exporting media library '{0}' files"), HTMLHelper.HTMLEncode(libraryDisplayName)));

                    // Save the settings
                    settings.SavePersistentLog();

                    try
                    {
                        // Initialize data
                        int libraryId = ValidationHelper.GetInteger(dr["LibraryID"], 0);
                        string libraryObjectType = ImportExportHelper.MEDIAFILE_PREFIX + libraryGuid;
                        if (isGroup)
                        {
                            libraryObjectType += "_group";
                        }

                        // Get library files data
                        DataSet ds = fileObj.GetData(null, "FileLibraryID = " + libraryId);
                        if (!DataHelper.DataSourceIsEmpty(ds))
                        {
                            ds.Tables[0].TableName = "Media_File";

                            // Save data
                            ExportProvider.SaveObjects(settings, ds, libraryObjectType, true);

                            // Copy files
                            if (ValidationHelper.GetBoolean(settings.GetSettings(ImportExportHelper.SETTINGS_MEDIA_FILES_PHYSICAL), false))
                            {
                                CopyFiles(settings, ds);
                            }
                        }
                    }
                    catch (ProcessCanceledException)
                    {
                        throw;
                    }
                    catch (Exception ex)
                    {
                        // Log exception
                        ExportProvider.LogProgressError(settings, string.Format(settings.GetAPIString("ExportSite.ErrorExportingMediaLibraryFiles", "Error exporting media library '{0}' files."), libraryDisplayName), ex);
                        throw;
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

            var table = data.Tables[0];
            string webSitePath = settings.WebsitePath;
            string sourcePath = webSitePath;

            string targetPath = DirectoryHelper.CombinePath(settings.TemporaryFilesPath, ImportExportHelper.DATA_FOLDER, ImportExportHelper.FILES_FOLDER) + "\\";
            targetPath = DirectoryHelper.CombinePath(targetPath, "media_files") + "\\";

            // Log process
            ExportProvider.LogProgress(LogStatusEnum.Info, settings, string.Format(settings.GetAPIString("ExportSite.CopyingFiles", "Copying '{0}' files"), ImportExportHelper.GetObjectTypeName(MediaLibraryInfo.OBJECT_TYPE, settings)));

            foreach (DataRow dr in table.Rows)
            {
                // Export process canceled
                if (settings.ProcessCanceled)
                {
                    ExportProvider.ExportCanceled();
                }

                // Prepare the data
                int fileId = ValidationHelper.GetInteger(dr["FileID"], 0);
                string filePath = ValidationHelper.GetString(dr["FilePath"], null);
                int libraryId = ValidationHelper.GetInteger(dr["FileLibraryID"], 0);

                MediaLibraryInfo libraryInfo = MediaLibraryInfoProvider.GetMediaLibraryInfo(libraryId);
                if (libraryInfo != null)
                {
                    string libraryFolder = libraryInfo.LibraryFolder;
                    string targetObjectPath = targetPath + DirectoryHelper.CombinePath(ImportExportHelper.SITE_MACRO, "media", libraryFolder, Path.EnsureBackslashes(filePath));
                    string targetThumbsPath = Path.GetDirectoryName(targetObjectPath).TrimEnd('\\') + "\\__thumbs";
                    string sourceObjectPath = MediaFileInfoProvider.GetMediaFilePath(fileId, settings.SiteName, sourcePath);
                    string sourceThumbsPath = MediaFileInfoProvider.GetThumbnailPath(settings.SiteName, filePath, libraryId, sourcePath);
                    
                    try
                    {
                        // Copy thumbnails folder
                        ExportProvider.CopyDirectory(sourceThumbsPath, targetThumbsPath, webSitePath);
                        // Copy files
                        ExportProvider.CopyFile(sourceObjectPath, targetObjectPath, webSitePath);
                    }
                    catch
                    {
                    }
                }
            }
        }

        #endregion
    }
}