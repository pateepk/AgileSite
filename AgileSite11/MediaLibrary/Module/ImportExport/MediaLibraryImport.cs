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
    /// Handles special actions during the Media library import process.
    /// </summary>
    internal static class MediaLibraryImport
    {
        #region "Methods"

        /// <summary>
        /// Initializes export handlers
        /// </summary>
        public static void Init()
        {
            ImportExportEvents.ImportObjectType.After += Import_After;
            ImportExportEvents.ProcessFileOperation.After += ProcessFileOperation_After;
        }


        static void ProcessFileOperation_After(object sender, ImportProcessFileOperationEventArgs e)
        {
            var settings = e.Settings;
            var operation = e.Operation;
            var objectType = operation.ObjectType;
            if (objectType == MediaLibraryInfo.OBJECT_TYPE)
            {
                if (operation.ParameterType == FileOperationParamaterTypeEnum.Webfarm)
                {
                    // Check the required parameter
                    if (String.IsNullOrEmpty(operation.Parameter))
                    {
                        return;
                    }

                    // Check source file
                    string sourcePath = ImportExportHelper.GetExportFilePath(operation.SourcePath);
                    if (!File.Exists(sourcePath))
                    {
                        return;
                    }

                    // Ensure target path
                    DirectoryHelper.EnsureDiskPath(operation.DestinationPath, settings.WebsitePath);

                    // Get parent directory
                    string parentDir = Path.GetDirectoryName(operation.DestinationPath);

                    // Check the target folder permissions
                    if (!DirectoryHelper.CheckPermissions(parentDir, true, true, false, false))
                    {
                        return;
                    }

                    // Check Copy files settings
                    if (!settings.CopyFiles)
                    {
                        return;
                    }

                    // Manually ensure web farm task for media files
                    using (FileStream file = FileStream.New(operation.DestinationPath, FileMode.Open, FileAccess.Read))
                    {
                        var parameters = operation.Parameter.Split('|');
                        WebFarmHelper.CreateIOTask(MediaTaskType.UpdateMediaFile, operation.DestinationPath, file, "mediafileupload", parameters);
                    }
                }
            }
        }


        private static void Import_After(object sender, ImportDataEventArgs e)
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
                ImportMediaFiles(settings, table, e.TranslationHelper, objectType == PredefinedObjectType.GROUP);
            }
        }


        /// <summary>
        /// Import media files.
        /// </summary>
        /// <param name="settings">Import settings</param>
        /// <param name="table">Parent data</param>
        /// <param name="th">Translation helper</param>
        /// <param name="isGroup">Indicates if group objects are imported</param>
        private static void ImportMediaFiles(SiteImportSettings settings, DataTable table, TranslationHelper th, bool isGroup)
        {
            ProcessObjectEnum processType = settings.GetObjectsProcessType(MediaLibraryInfo.OBJECT_TYPE, true);
            if (processType == ProcessObjectEnum.None)
            {
                return;
            }

            // Check import settings
            if (!ValidationHelper.GetBoolean(settings.GetSettings(ImportExportHelper.SETTINGS_MEDIA_FILES), true))
            {
                return;
            }

            // Import files for all libraries
            foreach (DataRow dr in table.Rows)
            {
                // Process canceled
                if (settings.ProcessCanceled)
                {
                    ImportProvider.ImportCanceled();
                }

                // Get library data
                Guid libraryGuid = ValidationHelper.GetGuid(dr["LibraryGUID"], Guid.Empty);
                string libraryName = dr["LibraryName"].ToString();
                string libraryDisplayName = dr["LibraryDisplayName"].ToString();

                try
                {
                    // Check if library selected
                    if ((processType != ProcessObjectEnum.All) && !settings.IsSelected(MediaLibraryInfo.OBJECT_TYPE, libraryName, true) && !isGroup)
                    {
                        continue;
                    }

                    // Log progress
                    ImportProvider.LogProgress(LogStatusEnum.Info, settings, string.Format(settings.GetAPIString("ImportSite.ImportingLibraryFiles", "Importing media library '{0}' files"), HTMLHelper.HTMLEncode(libraryDisplayName)));

                    // Save the settings progress
                    settings.SavePersistentLog();

                    // Initialize data
                    string libraryObjectType = ImportExportHelper.MEDIAFILE_PREFIX + libraryGuid;
                    
                    // ## Special case - backward compatibility for older versions (media files for community group libraries were stored in a file without the suffix)
                    if (isGroup)
                    {
                        libraryObjectType += "_group";
                    }

                    // Get data
                    DataSet ds = ImportProvider.LoadObjects(settings, libraryObjectType, true);
                    if (!DataHelper.DataSourceIsEmpty(ds))
                    {
                        // Import the objects
                        ImportProvider.ImportObjects(settings, ds, MediaFileInfo.OBJECT_TYPE, false, th, true, ProcessObjectEnum.All, null);

                        // Ensure to copy physical files
                        if (ValidationHelper.GetBoolean(settings.GetSettings(ImportExportHelper.SETTINGS_MEDIA_FILES_PHYSICAL), false))
                        {
                            AddImportFiles(settings, ds, th);
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
                    ImportProvider.LogProgressError(settings, string.Format(settings.GetAPIString("ImportSite.ErrorImportingLibraryFiles", "Error importing media library '{0}' files."), HTMLHelper.HTMLEncode(libraryDisplayName)), ex);
                    throw;
                }
            }
        }


        private static void AddImportFiles(SiteImportSettings settings, DataSet data, TranslationHelper th)
        {
            string sourcePath = DirectoryHelper.CombinePath(settings.TemporaryFilesPath, ImportExportHelper.DATA_FOLDER, ImportExportHelper.FILES_FOLDER) + "\\media_files\\";
            string targetPath = ImportProvider.GetTargetPath(settings, settings.WebsitePath);


            DataTable dataTable = data.Tables["Media_File"];
            if (!DataHelper.DataSourceIsEmpty(dataTable))
            {
                foreach (DataRow dr in dataTable.Rows)
                {
                    // Import process canceled
                    if (settings.ProcessCanceled)
                    {
                        ImportProvider.ImportCanceled();
                    }

                    // Prepare the data
                    th.TranslateColumn(dr, "FileLibraryID", MediaLibraryInfo.OBJECT_TYPE, settings.TranslationSiteId);
                    int libraryId = ValidationHelper.GetInteger(dr["FileLibraryID"], 0);

                    MediaLibraryInfo libraryInfo = MediaLibraryInfoProvider.GetMediaLibraryInfo(libraryId);
                    if (libraryInfo != null)
                    {
                        string filePath = dr["FilePath"].ToString();
                        string fileName = dr["FileName"].ToString();
                        string fileExtension = dr["FileExtension"].ToString();
                        string fileGuid = dr["FileGUID"].ToString();
                        
                        string sourceObjectPath = sourcePath + DirectoryHelper.CombinePath(ImportExportHelper.SITE_MACRO, "media", libraryInfo.LibraryFolder, Path.EnsureBackslashes(filePath));
                        string sourceThumbsPath = Path.GetDirectoryName(sourceObjectPath).TrimEnd('\\') + "\\__thumbs";
                        string targetObjectPath = MediaFileInfoProvider.GetMediaFilePath(filePath, libraryId, settings.SiteName, targetPath);
                        string targetThumbsPath = MediaFileInfoProvider.GetThumbnailPath(settings.SiteName, filePath, libraryId, targetPath);
                        // Prepare parameter for web farm task
                        string objectParameter = string.Join("|", new[] { settings.SiteName, libraryInfo.LibraryFolder, Path.GetDirectoryName(filePath), fileName, fileExtension, fileGuid });

                        // Source files and thumbs folder
                        settings.FileOperations.Add(MediaLibraryInfo.OBJECT_TYPE, sourceThumbsPath, targetThumbsPath, FileOperationEnum.CopyDirectory);
                        settings.FileOperations.Add(MediaLibraryInfo.OBJECT_TYPE, sourceObjectPath, targetObjectPath, FileOperationEnum.CopyFile, FileOperationParamaterTypeEnum.Webfarm, objectParameter);
                    }
                }
            }
        }

        #endregion
    }
}