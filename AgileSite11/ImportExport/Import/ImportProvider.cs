using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Xml;
using System.IO.Compression;

using CMS.DataEngine;
using CMS.EventLog;
using CMS.Helpers;
using CMS.IO;
using CMS.Base;
using CMS.Core;
using CMS.LicenseProvider;
using CMS.Localization;
using CMS.SiteProvider;
using CMS.MacroEngine;
using CMS.Synchronization;

using Directory = CMS.IO.Directory;
using DirectoryInfo = CMS.IO.DirectoryInfo;
using File = CMS.IO.File;
using FileAccess = CMS.IO.FileAccess;
using FileAttributes = CMS.IO.FileAttributes;
using FileInfo = CMS.IO.FileInfo;
using FileMode = CMS.IO.FileMode;
using FileShare = CMS.IO.FileShare;
using FileStream = CMS.IO.FileStream;
using Path = CMS.IO.Path;

using SystemIO = System.IO;

namespace CMS.CMSImportExport
{
    /// <summary>
    /// Ensures import of data and settings.
    /// </summary>
    public static class ImportProvider
    {
        #region "Variables"

        // Import website to webapp documentation topic.
        private const string HELP_TOPIC_IMPORT_WEBSITE_TO_WEBAPP = "waprojects_importing";

        private static bool? mCheckPackageVersion;

        #endregion


        #region "Properties"

        /// <summary>
        /// Indicates if package version should be checked.
        /// </summary>
        private static bool CheckPackageVersion
        {
            get
            {
                if (mCheckPackageVersion == null)
                {
                    mCheckPackageVersion = ValidationHelper.GetBoolean(SettingsHelper.AppSettings["CMSImportCheckPackageVersion"], true);
                }
                return mCheckPackageVersion.Value;
            }
        }

        #endregion


        #region "Progress log"

        /// <summary>
        /// Progress log event handler.
        /// </summary>
        public delegate void OnProgressLogEventHandler(string message);

        /// <summary>
        /// Progress log event.
        /// </summary>
        public static event OnProgressLogEventHandler OnProgressLog;


        /// <summary>
        /// Logs the import progress.
        /// </summary>
        /// <param name="message">Progress message</param>
        public static void LogProgress(string message)
        {
            OnProgressLog?.Invoke(message);
        }

        #endregion


        #region "Methods for temporary files manipulation"

        /// <summary>
        /// Gets the first export file found in the specified directory.
        /// </summary>
        /// <param name="path">Starting path</param>
        public static string GetFirstExportFile(string path)
        {
            // Check old package - get first .xml.export file
            if (Directory.Exists(path))
            {
                // Check files
                DirectoryInfo dir = DirectoryInfo.New(path);
                FileInfo[] files = dir.GetFiles("*.xml.export");
                if ((files != null) && (files.Length > 0))
                {
                    // Return first file
                    FileInfo firstFile = files[0];
                    return firstFile.FullName;
                }

                // Check subdirectories
                DirectoryInfo[] subDirs = dir.GetDirectories();
                if ((subDirs != null) && (subDirs.Length > 0))
                {
                    // Process all subdirectories
                    foreach (DirectoryInfo subDir in subDirs)
                    {
                        string firstFile = GetFirstExportFile(subDir.FullName);
                        if (firstFile != null)
                        {
                            return firstFile;
                        }
                    }
                }
            }

            return null;
        }


        /// <summary>
        /// Creates temporary files.
        /// </summary>
        /// <param name="settings">Site import settings</param>
        public static void CreateTemporaryFiles(SiteImportSettings settings)
        {
            try
            {
                if (!settings.TemporaryFilesCreated)
                {
                    if (settings.SourceFilePath == null)
                    {
                        throw new Exception(settings.GetAPIString("ImportSite.ErrorTmpFilesCreation", "Error during temporary files creation.") + " Original exception: SourceFilePath not initialized!");
                    }

                    // Log progress
                    LogProgress(LogStatusEnum.Start, settings, settings.GetAPIString("ImportSite.PreparingTemporaryFiles", "Preparing temporary files"));

                    // Ensure default temporary files path (for repeated import)
                    settings.TemporaryFilesPath = settings.OriginalTemporaryFilesPath;

                    // Ensure deletion of the temporary files from previous import
                    DeleteTemporaryFiles(settings, true);

                    // Try to create temporary files from Zip archive
                    if (settings.SourceFilePath.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
                    {
                        // Uncompress temporary files
                        Uncompress(settings);
                    }
                    else
                    {
                        // Use same folder, do not delete temporary files
                        settings.TemporaryFilesPath = settings.SourceFilePath;
                        settings.DeleteTemporaryFiles = false;
                    }

                    // Check old package - get first .xml.export file
                    string firstFile = GetFirstExportFile(settings.TemporaryFilesPath + ImportExportHelper.DATA_FOLDER);
                    if (firstFile != null)
                    {
                        settings.Version = GetVersion(firstFile);
                    }
                    else
                    {
                        // Package is not valid
                        throw new Exception("[ImportProvider.CreateTemporaryFiles]: Data not found or the package is not a valid export package. (" + settings.TemporaryFilesPath + ImportExportHelper.DATA_FOLDER + ")");
                    }

                    // Older version is importing
                    if (settings.IsOlderVersion)
                    {
                        // Use original temporary files path
                        settings.SourceFilePath = settings.TemporaryFilesPath;
                        settings.TemporaryFilesPath = settings.OriginalTemporaryFilesPath;
                        settings.TemporaryFilesPath = ImportConverter.EnsureConversion(settings);
                        settings.DeleteTemporaryFiles = true;
                    }


                    GC.Collect();
                    settings.TemporaryFilesCreated = true;
                }

                // Load additional information
                settings.InfoDataSet = null;
                settings.InfoDataSet = LoadObjects(settings, ImportExportHelper.CMS_INFO_TYPE, false);
                settings.ClearInfoData();

                // Check hotfix version of the package
                if (CheckPackageVersion)
                {
                    if (ImportExportHelper.IsLowerVersion(CMSVersion.MainVersion, CMSVersion.HotfixVersion, settings.Version, settings.HotfixVersion))
                    {
                        throw new Exception("[ImportProvider.LoadObjects]: Cannot import data from newer hotfix version '" + settings.HotfixVersion + "' to current hotfix version '" + CMSVersion.HotfixVersion + "'.");
                    }
                }
            }
            catch (ProcessCanceledException)
            {
                throw;
            }
            // If there is an error during temporary files creation, delete already created
            catch (Exception ex)
            {
                // Log error
                LogProgressError(settings, settings.GetAPIString("ImportSite.ErrorTmpFilesCreation", "Error during temporary files creation."), ex);

                DeleteTemporaryFiles(settings, true);

                throw new Exception(settings.GetAPIString("ImportSite.ErrorTmpFilesCreation", "Error during temporary files creation.") + " Original exception: " + ex.Message, ex);
            }
        }


        /// <summary>
        /// Delete temporary files.
        /// </summary>
        /// <param name="settings">Import settings</param>        
        /// <param name="onlyFolderStructure">True if only folder structure should be deleted and folders should be left</param>        
        public static void DeleteTemporaryFiles(SiteImportSettings settings, bool onlyFolderStructure)
        {
            string path = settings.TemporaryFilesPath;

            if (settings.DeleteTemporaryFiles)
            {
                try
                {
                    // Delete the temporary files
                    if (Directory.Exists(path))
                    {
                        if (onlyFolderStructure)
                        {
                            DeleteTemporaryFiles(path);
                        }
                        else
                        {
                            DirectoryHelper.DeleteDirectory(path, true);
                        }
                        settings.TemporaryFilesCreated = false;
                    }
                }
                catch (ProcessCanceledException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    string message = string.Format(settings.GetAPIString("ImportSite.ErrorTmpFilesDeletion", "Error during temporary files deletion. Please delete the folder {0} manually."), path);
                    LogProgressError(settings, message, ex);
                    throw;
                }
            }
            else if (ZipStorageProvider.IsZipFolderPath(path))
            {
                // Dispose the zip storage provider to release memory
                ZipStorageProvider.Dispose(path);
            }
        }


        /// <summary>
        /// Recursively delete all files in specified path.
        /// </summary>
        /// <param name="path">Path</param>
        private static void DeleteTemporaryFiles(string path)
        {
            DirectoryHelper.DeleteDirectoryStructure(path);
        }


        /// <summary>
        /// Uncompress the zip file.
        /// </summary>
        /// <param name="settings">Site import settings</param>
        private static void Uncompress(SiteImportSettings settings)
        {
            string targetFolder = settings.TemporaryFilesPath;
            string webRootPath = settings.WebsitePath;
            string zipFile = settings.SourceFilePath;

            // Create output stream
            using (var fileStream = File.OpenRead(zipFile))
            {
                using (var zipArchive = new ZipArchive(fileStream, ZipArchiveMode.Read, true))
                {
                    foreach (var theEntry in zipArchive.Entries)
                    {
                        var fileName = Path.GetFileName(theEntry.FullName);

                        if (String.IsNullOrEmpty(fileName))
                        {
                            continue;
                        }

                        var directoryName = DirectoryHelper.EnsurePathBackSlash(Path.GetDirectoryName(theEntry.FullName).TrimStart('\\'));
                        DirectoryHelper.EnsureDiskPath(Path.Combine(targetFolder, directoryName), webRootPath);

                        using (var targetFileStream = File.Create(targetFolder + theEntry.FullName))
                        {
                            using (var entryStream = theEntry.Open())
                            {
                                entryStream.CopyTo(targetFileStream);
                            }
                        }
                    }
                }
            }
        }

        #endregion


        #region "Files handling methods"

        /// <summary>
        /// Copy all files and folders from file list.
        /// </summary>
        /// <param name="settings">Import settings</param>
        /// <param name="th">Translation helper</param>
        private static void CopyImportFiles(SiteImportSettings settings, TranslationHelper th)
        {
            FileOperationCollection operations = settings.FileOperations;

            if (settings.CopyFiles && (operations.Count > 0))
            {
                // Log progress
                LogProgress(LogStatusEnum.Info, settings, settings.GetAPIString("ImportProvider.CopyingImportFiles", "Copying objects files"));

                // Save the settings
                settings.SavePersistentLog();

                bool anyFileCopied = false;
                bool assemblyWarningLogged = false;
                // Get through all files operations
                int i = 0;
                while (i < operations.Count)
                {
                    // Export process canceled
                    if (settings.ProcessCanceled)
                    {
                        ImportCanceled();
                    }

                    FileOperation operation = operations[i];

                    // Handle the event
                    using (var h = ImportExportEvents.ProcessFileOperation.StartEvent(settings, th, operation))
                    {
                        if (h.CanContinue())
                        {
                            switch (operation.Operation)
                            {
                                // Copy whole folder
                                case FileOperationEnum.CopyDirectory:
                                    try
                                    {
                                        if (Directory.Exists(operation.SourcePath))
                                        {
                                            // Ensure target path
                                            DirectoryHelper.EnsureDiskPath(operation.DestinationPath, settings.WebsitePath);

                                            // Check the target folder permissions
                                            if (DirectoryHelper.CheckPermissions(operation.DestinationPath, true, true, false, false))
                                            {
                                                // Copy directory
                                                if (settings.CopyFiles)
                                                {
                                                    anyFileCopied = true;

                                                    CopyDirectory(settings, operation.SourcePath, operation.DestinationPath);
                                                }

                                                // Remove from file list
                                                operations.Remove(operation);
                                            }
                                            else
                                            {
                                                // Change file operation result
                                                operation.Result = FileResultEnum.PermissionsError;
                                                ++i;
                                            }
                                        }
                                        else
                                        {
                                            // Remove from file list
                                            operations.Remove(operation);
                                        }
                                    }
                                    catch (ProcessCanceledException)
                                    {
                                        throw;
                                    }
                                    catch (Exception ex)
                                    {
                                        LogProgressError(settings, string.Format(settings.GetAPIString("ImportProvider.ErrorCopyDir", "Error copying folder '{0}' to the destination folder '{1}'."), operation.SourcePath, operation.DestinationPath), ex);
                                        throw;
                                    }
                                    break;

                                // Copy file
                                case FileOperationEnum.CopyFile:
                                    try
                                    {
                                        string sourcePath = ImportExportHelper.GetExportFilePath(operation.SourcePath);
                                        if (File.Exists(sourcePath))
                                        {
                                            // Ensure target path
                                            DirectoryHelper.EnsureDiskPath(operation.DestinationPath, settings.WebsitePath);

                                            // Get parent directory
                                            string parentDir = Path.GetDirectoryName(operation.DestinationPath);

                                            // Check the target folder permissions
                                            if (DirectoryHelper.CheckPermissions(parentDir, true, true, false, false))
                                            {
                                                // Copy file
                                                if (settings.CopyFiles)
                                                {
                                                    anyFileCopied = true;

                                                    CopyFile(settings, sourcePath, operation.DestinationPath);

                                                    if (operation.ParameterType == FileOperationParamaterTypeEnum.Assembly)
                                                    {
                                                        if (!String.IsNullOrEmpty(operation.Parameter))
                                                        {
                                                            string[] parameters = operation.Parameter.Split('|');
                                                            bool versionMatch = ValidationHelper.GetBoolean(parameters[0], true);
                                                            string destPath = parameters[1];
                                                            string originalPath = parameters[2];
                                                            // Versions don't match
                                                            if (!versionMatch && !assemblyWarningLogged)
                                                            {
                                                                // Log warning
                                                                LogProgress(LogStatusEnum.Warning, settings, string.Format(settings.GetAPIString("ImportProvider.ProcessDLLWarning", "The system version doesn't match the package version. The assembly files have been copied to a separate location '{0}'. Please copy the files manually to the '{1}' folder if the action was intended."), destPath, originalPath));
                                                                assemblyWarningLogged = true;
                                                            }
                                                        }
                                                    }
                                                }

                                                // Remove from file list
                                                operations.Remove(operation);
                                            }
                                            else
                                            {
                                                // Change file operation result
                                                operations.SetResult(operation, FileResultEnum.PermissionsError);
                                                ++i;
                                            }
                                        }
                                        else
                                        {
                                            // Remove from file list
                                            operations.Remove(operation);
                                        }
                                    }
                                    catch (ProcessCanceledException)
                                    {
                                        throw;
                                    }
                                    catch (Exception ex)
                                    {
                                        LogProgressError(settings, string.Format(settings.GetAPIString("ImportProvider.ErrorCopyFile", "Error copying file '{0}' to the destination '{1}'."), operation.SourcePath, operation.DestinationPath), ex);
                                        throw;
                                    }
                                    break;

                                default:
                                    ++i;
                                    break;
                            }
                        }

                        h.FinishEvent();
                    }
                }

                // Log files copy results
                string warning = operations.GetResultList(FileResultEnum.PermissionsError);
                if (warning != null)
                {
                    LogProgress(LogStatusEnum.Warning, settings, string.Format(settings.GetAPIString("ImportProvider.PermissionWarning", "Insufficient write permissions. Please copy manually files and folders listed below: {0}"), HTMLHelper.EnsureHtmlLineEndings(warning)));
                }

                // Log conversion warnings
                if (!settings.IsWebApplication && SystemContext.IsWebApplicationProject)
                {
                    string srcPath = GetTargetPath(settings, settings.WebsitePath, true);
                    if (Directory.Exists(srcPath))
                    {
                        // Log warning about conversion to web application
                        LogProgress(LogStatusEnum.Warning, settings, string.Format(settings.GetAPIString("ImportProvider.ConvertWebSiteWebAppWarning", "Additional actions are required to complete the import process. All files from the package were copied to the separate '{0}' folder. Please follow the steps described {1}."), srcPath, "<a href=\"" + DocumentationHelper.GetDocumentationTopicUrl(HELP_TOPIC_IMPORT_WEBSITE_TO_WEBAPP) + "\" target=\"_blank\">" + settings.GetAPIString("general.here", "here") + "</a>"));
                    }
                }
                else if (settings.IsWebApplication)
                {
                    if (!SystemContext.IsWebApplicationProject)
                    {
                        // Move files to web site folder
                        string srcPath = GetTargetPath(settings, settings.WebsitePath, true);
                        if (Directory.Exists(srcPath))
                        {
                            string destPath = GetTargetPath(settings, settings.WebsitePath, false);

                            // Log progress
                            LogProgress(LogStatusEnum.Info, settings, settings.GetAPIString("ImportProvider.ConvertingImportFiles", "Converting objects files"));

                            DirectoryHelper.CopyDirectory(srcPath, destPath);
                            DirectoryHelper.DeleteDirectoryStructure(srcPath);
                        }
                    }
                    else if (anyFileCopied)
                    {
                        // In case of WebApplication, files have to be manually added to sln file
                        LogProgress(LogStatusEnum.Warning, settings, string.Format(settings.GetAPIString("ImportProvider.WebAplicationFilesToSln", "Additional actions are required to complete the import process. All files from the package were copied to their locations, however they need to be manually added to the solution. Please follow the steps described {0}."), "<a href=\"" + DocumentationHelper.GetDocumentationTopicUrl(HELP_TOPIC_IMPORT_WEBSITE_TO_WEBAPP) + "\" target=\"_blank\">" + settings.GetAPIString("general.here", "here") + "</a>"));
                    }
                }
            }
            else
            {
                // Do not copy files - clear file operations
                operations.Clear();
            }
        }


        /// <summary>
        /// Copy file for the import and unset the read-only attributes.
        /// </summary>
        /// <param name="settings">Import settings</param>
        /// <param name="sourcePath">Source path</param>
        /// <param name="destPath">Destination path</param>
        private static void CopyFile(SiteImportSettings settings, string sourcePath, string destPath)
        {
            try
            {
                // Ensure target path
                destPath = EnsureTargetPath(settings, destPath);

                // Get files path
                sourcePath = ImportExportHelper.GetExportFilePath(sourcePath);
                string realDestPath = ImportExportHelper.GetRealFilePath(destPath);

                // Do not allow to copy any files info App_Code in the precompiled website
                if (SystemContext.IsPrecompiledWebsite)
                {
                    if (realDestPath.ToLowerInvariant().Contains("\\app_code\\") || realDestPath.ToLowerInvariant().Contains("\\old_app_code\\"))
                    {
                        return;
                    }
                }

                FileInfo sourceInfo = FileInfo.New(sourcePath);

                // ## Conversion
                // Do not copy designer files for web project
                if (ImportExportHelper.IsDesignerCodeBehind(realDestPath) && !SystemContext.IsWebApplicationProject)
                {
                    return;
                }

                if (!CheckForCodeFiles(settings, sourceInfo))
                {
                    // If the file is code file, than abort copying
                    return;
                }

                FileInfo targetInfo = FileInfo.New(realDestPath);

                if (sourceInfo.Exists)
                {
                    // Check if the copy file is necessary
                    bool targetExists = targetInfo.Exists;
                    bool fileDiff = false;
                    if (targetExists)
                    {
                        if (sourceInfo.Length != targetInfo.Length)
                        {
                            // Different length, different file
                            fileDiff = true;
                        }
                        else
                        {
                            try
                            {
                                // Compare the content
                                fileDiff = !FileHelper.FilesMatch(sourcePath, realDestPath);
                            }
                            catch
                            {
                                // On error copy the file
                                fileDiff = true;
                            }
                        }
                    }

                    // If target file doesn't exist or has different size copy file
                    if (!targetExists || fileDiff)
                    {
                        try
                        {
                            // Copy file
                            DirectoryHelper.CopyFile(sourcePath, realDestPath);

                            // Adjust file time stamp
                            targetInfo.CreationTime = DateTime.Now;
                            targetInfo.LastAccessTime = DateTime.Now;
                            targetInfo.LastWriteTime = DateTime.Now;
                            targetInfo.IsReadOnly = false;
                            targetInfo.Attributes = FileAttributes.Normal;

                            File.SetAttributes(realDestPath, FileAttributes.Normal);
                        }
                        finally
                        {
                            if (File.Exists(realDestPath))
                            {
                                // Ensure namespaces conversion
                                ImportConverter.EnsureNamespaceConversion(settings, realDestPath);

                                // Convert codebehind to codefile
                                if (settings.IsWebApplication && !SystemContext.IsWebApplicationProject && ImportExportHelper.IsPostProcessFile(targetInfo.Extension))
                                {
                                    ProjectHelper.ChangeCodeBehindToCodeFile(realDestPath);
                                }
                            }
                        }
                    }
                }
            }
            catch (SystemIO.FileNotFoundException)
            {
            }
            catch (SystemIO.DirectoryNotFoundException)
            {
            }
        }


        /// <summary>
        /// Gets correct target path (especially for App_Code paths)
        /// </summary>
        /// <param name="settings">Site import settings</param>
        /// <param name="originalPath">Original target path</param>
        private static string EnsureTargetPath(SiteImportSettings settings, string originalPath)
        {
            // Ensure conversion between WebApp and Website
            if (settings.IsWebApplication && !SystemContext.IsWebApplicationProject)
            {
                // Old_App_Code -> App_Code
                originalPath = originalPath.Replace("\\Old_App_Code\\", "\\App_Code\\");
            }
            else if (!settings.IsWebApplication && SystemContext.IsWebApplicationProject)
            {
                // App_Code -> Old_App_Code
                originalPath = originalPath.Replace("\\App_Code\\", "\\Old_App_Code\\");
            }

            return originalPath;
        }


        /// <summary>
        /// Copy directory.
        /// </summary>
        /// <param name="settings">Import settings</param>
        /// <param name="sourcePath">Source path</param>
        /// <param name="targetPath">Target path</param>
        private static void CopyDirectory(SiteImportSettings settings, string sourcePath, string targetPath)
        {
            // Ensure target path
            targetPath = EnsureTargetPath(settings, Path.EnsureEndBackslash(targetPath));

            DirectoryInfo sourceFolder = DirectoryInfo.New(sourcePath);

            // Create the directory if not exists
            if (!Directory.Exists(targetPath))
            {
                DirectoryHelper.CreateDirectory(targetPath);
            }

            // Copy subfolders
            foreach (DirectoryInfo subFolder in sourceFolder.GetDirectories())
            {
                CopyDirectory(settings, DirectoryHelper.CombinePath(sourcePath, subFolder.Name), DirectoryHelper.CombinePath(targetPath, subFolder.Name));
            }

            // Copy files
            foreach (FileInfo sourceFile in sourceFolder.GetFiles())
            {
                CopyFile(settings, sourceFile.FullName, DirectoryHelper.CombinePath(targetPath, sourceFile.Name));
            }
        }


        /// <summary>
        /// Returns true if given file is allowed to be processed according to "Copy code files" setting.
        /// </summary>
        /// <param name="settings">Import settings</param>
        /// <param name="file">File to check</param>
        private static bool CheckForCodeFiles(SiteImportSettings settings, FileInfo file)
        {
            return settings.CopyCodeFiles || !IsCodeFile(file);
        }


        /// <summary>
        /// Returns true if the file is code file (checks the extension)
        /// </summary>
        /// <param name="file">File to check</param>
        private static bool IsCodeFile(FileInfo file)
        {
            string extension = file.Extension.ToLowerInvariant();
            if (extension == ".export")
            {
                // Trim the export extension
                string name = file.Name.Substring(0, file.Name.Length - ".export".Length);
                int index = name.LastIndexOf(".", StringComparison.Ordinal);
                if (index > 0)
                {
                    // Get the real extension
                    extension = name.Substring(index);
                }
            }

            switch (extension.Trim('.'))
            {
                case "ascx":
                case "aspx":
                case "master":
                case "ashx":
                case "asmx":
                case "cs":
                case "vb":
                    return true;
            }
            return false;
        }

        #endregion


        #region "Methods for EventLog"

        /// <summary>
        /// Logs progress.
        /// </summary>
        /// <param name="type">Type of the information</param>
        /// <param name="settings">Import settings</param>
        /// <param name="description">Log message</param>
        public static void LogProgress(LogStatusEnum type, SiteImportSettings settings, string description)
        {
            // Log the progress
            settings.LogProgressState(type, description);
            LogProgress(description);
        }


        /// <summary>
        /// Logs progress error.
        /// </summary>
        /// <param name="settings">Import settings</param>
        /// <param name="description">Error description</param>
        /// <param name="ex">Exception to log</param>
        public static void LogProgressError(SiteImportSettings settings, string description, Exception ex)
        {
            string message = ImportExportHelper.GetFormattedErrorMessage(description, ex);

            // Log progress
            LogProgress(LogStatusEnum.Error, settings, message);
        }

        #endregion


        #region "General import methods"

        /// <summary>
        /// Imports all the objects.
        /// </summary>
        /// <param name="settings">Import settings</param>
        public static void ImportObjectsData(SiteImportSettings settings)
        {
            LicenseCheckDisabler.ExecuteWithoutLicenseCheck(() => ImportObjectsDataInternal(settings));
        }


        /// <summary>
        /// Imports all the objects.
        /// </summary>
        /// <param name="settings">Import settings</param>
        [CanDisableLicenseCheck("io/0MzjVC9CS2sClxZEvJAuDzN6aizFfNiY+HJOuLDOKdQJ2FgNeA0VYRzwegCBEH4U88Z1thbicft0dAz0YdQ==")]
        private static void ImportObjectsDataInternal(SiteImportSettings settings)
        {
            try
            {
                // Handle the event
                using (var hImport = ImportExportEvents.Import.StartEvent(settings, null))
                {
                    if (hImport.CanContinue())
                    {
                        // Clear the log
                        settings.ClearProgressLog();
                        settings.WriteLog = true;

                        using (var context = new CMSActionContext(settings.CurrentUser))
                        {
                            // Set up culture context
                            context.ThreadCulture = CultureHelper.EnglishCulture;

                            // Disable all context properties
                            context.DisableAll();
                            context.CreateSearchTask = settings.EnableSearchTasks;

                            TranslationHelper th = null;

                            try
                            {
                                // Prepare the temporary files
                                CreateTemporaryFiles(settings);

                                // Log import start
                                LogProgress(LogStatusEnum.Info, settings, settings.GetAPIString("ImportSite.PreparingImport", "Preparing import process"));

                                // Save the settings progress
                                settings.SavePersistentLog();

                                // For web templates do not import objects where parent object is missing
                                if (ValidationHelper.GetBoolean(settings.GetInfo(ImportExportHelper.INFO_WEBTEMPLATE_FLAG), false))
                                {
                                    settings.SetSettings(ImportExportHelper.SETTINGS_SKIP_OBJECT_ON_TRANSLATION_ERROR, true);
                                }

                                // Load the translation data
                                DataSet translationDS = LoadObjects(settings, ImportExportHelper.OBJECT_TYPE_TRANSLATION, false);
                                th = !DataHelper.DataSourceIsEmpty(translationDS) ? new TranslationHelper(translationDS.Tables[0]) : new TranslationHelper();

                                // Handle the event
                                using (var h = ImportExportEvents.ImportObjects.StartEvent(settings, th))
                                {
                                    if (h.CanContinue())
                                    {
                                        bool importSite = settings.ImportSite;

                                        IDictionary<int, int> resourceInfoImportedIds = null;

                                        // Import objects
                                        foreach (var item in ImportExportHelper.ObjectTypes)
                                        {
                                            // Get object type information
                                            string objectType = item.ObjectType;

                                            try
                                            {
                                                bool siteObject = item.IsSite;

                                                if (!siteObject || importSite)
                                                {
                                                    // For existing site import site object only if the settings are set
                                                    if ((objectType != SiteInfo.OBJECT_TYPE) || !settings.ExistingSite || (ValidationHelper.GetBoolean(settings.GetSettings(ImportExportHelper.SETTINGS_UPDATE_SITE_DEFINITION), true)))
                                                    {
                                                        // Import objects
                                                        var importedIds = ImportObjectType(settings, objectType, siteObject, th, ProcessObjectEnum.Default, null);

                                                        if (objectType.Equals("cms.resource", StringComparison.OrdinalIgnoreCase))
                                                        {
                                                            resourceInfoImportedIds = importedIds;
                                                        }
                                                    }
                                                }
                                            }
                                            catch (DataClassNotFoundException ex)
                                            {
                                                string msg = settings.HandleObjectTypeClassNotFound(objectType, ex);
                                                if (!String.IsNullOrEmpty(msg))
                                                {
                                                    LogProgress(msg);
                                                }
                                            }
                                        }

                                        // Seal imported modules
                                        if (resourceInfoImportedIds != null && settings.Version.StartsWith("8", StringComparison.Ordinal))
                                        {
                                            SealImportedModules(resourceInfoImportedIds.Values.ToList());
                                        }

                                        // Additional actions
                                        ProcessAdditionalActions(settings, th);

                                        // Add global folders
                                        if (ValidationHelper.GetBoolean(settings.GetSettings(ImportExportHelper.SETTINGS_GLOBAL_FOLDERS), true))
                                        {
                                            AddGlobalFolders(settings);
                                        }

                                        if (importSite)
                                        {
                                            // Add site related folders
                                            if (ValidationHelper.GetBoolean(settings.GetSettings(ImportExportHelper.SETTINGS_SITE_FOLDERS), true))
                                            {
                                                AddSiteFolders(settings);
                                            }
                                        }

                                        // Copy all folders and files from files list
                                        CopyImportFiles(settings, th);

                                        // Try to run site if set
                                        bool runSite = importSite && ValidationHelper.GetBoolean(settings.GetSettings(ImportExportHelper.SETTINGS_RUN_SITE), true);
                                        if ((settings.SiteId != 0) && (runSite))
                                        {
                                            try
                                            {
                                                // Log start of site
                                                LogProgress(LogStatusEnum.Info, settings, string.Format(settings.GetAPIString("ImportSite.StartingSite", "Starting site '{0}'"), HTMLHelper.HTMLEncode(ResHelper.LocalizeString(settings.SiteDisplayName))));

                                                // Run site
                                                SiteInfoProvider.RunSite(settings.SiteName);
                                            }
                                            catch (RunningSiteException)
                                            {
                                                LogProgress(LogStatusEnum.Warning, settings, string.Format(settings.GetAPIString("ImportSite.StartingSiteWarning", "Failed to start site '{0}', there is already a running site with this domain alias, you need to stop the other site to run this site and run the site manually."), HTMLHelper.HTMLEncode(ResHelper.LocalizeString(settings.SiteDisplayName))));
                                            }
                                            catch (Exception ex)
                                            {
                                                // Log exception
                                                LogProgressError(settings, string.Format(settings.GetAPIString("ImportSite.StartingSiteError", "Error starting site '{0}'."), HTMLHelper.HTMLEncode(ResHelper.LocalizeString(settings.SiteDisplayName))), ex);
                                            }
                                        }
                                    }

                                    h.FinishEvent();
                                }

                                // Log warnings during process
                                var finishMessage =
                                    settings.IsWarning() ?
                                    settings.GetAPIString("ImportSite.ImportFinishedWithWarnings", "Import has finished with minor problems. Please see warning messages below.") :
                                    settings.GetAPIString("ImportSite.ImportFinished", "Import has successfully finished");

                                LogProgress(LogStatusEnum.Finish, settings, finishMessage);
                            }
                            catch (ProcessCanceledException)
                            {
                                // Raise the cancel event
                                ImportExportEvents.ImportCanceled.StartEvent(settings, th);

                                // Log process cancellation
                                LogProgress(LogStatusEnum.Finish, settings, settings.GetAPIString("ImportSite.ImportCanceled", "Import process has been canceled."));
                            }
                            catch (Exception ex)
                            {
                                // Raise the error event
                                ImportExportEvents.ImportError.StartEvent(settings, th, ex);

                                // Log error
                                LogProgressError(settings, settings.GetAPIString("ImportSite.ErrorImport", "Error during import process"), ex);
                                LogProgress(LogStatusEnum.Finish, settings, settings.GetAPIString("ImportSite.ErrorImport", "Error during import process"));
                                throw;
                            }
                            finally
                            {
                                settings.WriteLog = false;

                                // Clear cache
                                CacheHelper.ClearCache(null, true);

                                // Ensure reloading the resource strings from the ResX files
                                LocalizationHelper.Clear();

                                // Clear hash tables
                                ModuleManager.ClearHashtables();
                            }
                        }
                    }

                    hImport.FinishEvent();
                }
            }
            finally
            {
                // Write log to the event log
                settings.FinalizeEventLog();
            }
        }


        /// <summary>
        /// Seals modules which were imported during this import.
        /// </summary>
        /// <param name="resourceIds">List of imported module IDs.</param>
        /// <remarks>
        /// The <see cref="ModuleName.CUSTOMSYSTEM"/> module and modules from older version are never sealed.
        /// </remarks>
        private static void SealImportedModules(List<int> resourceIds)
        {
            var resourceTypeInfo = ObjectTypeManager.GetTypeInfo(PredefinedObjectType.RESOURCE);
            if (resourceTypeInfo != null)
            {
                var resourceInfos = new ObjectQuery(PredefinedObjectType.RESOURCE).WhereIn(resourceTypeInfo.IDColumn, resourceIds);
                foreach (BaseInfo resourceInfo in resourceInfos)
                {
                    var resourceName = (string)resourceInfo[resourceTypeInfo.CodeNameColumn];

                    // If module is from older version keep it editable. Custom system module is always editable
                    resourceInfo["ResourceIsInDevelopment"] = (resourceName == ModuleName.CUSTOMSYSTEM);
                    resourceInfo.Generalized.SetObject();
                }
            }
        }


        /// <summary>
        /// Imports the specific object type.
        /// </summary>
        /// <param name="settings">Import settings</param>
        /// <param name="objectType">Object type to import</param>
        /// <param name="siteObject">Site objects</param>
        /// <param name="th">Translation helper</param>
        /// <param name="processType">Process type</param>
        /// <param name="importedParentIds">Table of imported parent object IDs. When set, only objects with parent from the table are imported</param>
        public static Dictionary<int, int> ImportObjectType(SiteImportSettings settings, string objectType, bool siteObject, TranslationHelper th, ProcessObjectEnum processType, List<int> importedParentIds)
        {
            var importedIds = new Dictionary<int, int>();

            var e = new ImportDataEventArgs
            {
                Settings = settings,
                TranslationHelper = th,
                ObjectType = objectType,
                SiteObjects = siteObject,
            };

            // Handle the event
            using (var h = ImportExportEvents.ImportObjectType.StartEvent(e))
            {
                if (h.CanContinue())
                {
                    // Load data
                    DataSet ds = LoadObjects(settings, objectType, siteObject);
                    if (!DataHelper.DataSourceIsEmpty(ds))
                    {
                        // Process tasks first
                        if (ValidationHelper.GetBoolean(settings.GetSettings(ImportExportHelper.SETTINGS_TASKS), true))
                        {
                            var importedObjects = new List<ImportedObject>();
                            var result = ProcessTasks(settings, ds, objectType, siteObject, th, importedObjects);

                            LogSynchronizationTasks(settings, importedObjects, result);
                        }

                        // Import objects
                        importedIds = ImportObjects(settings, ds, objectType, siteObject, th, true, processType, importedParentIds);
                        bool imported = (importedIds.Count != 0);
                        h.EventArguments.ParentImported = imported;

                        // Add objects physical files to the post processing list
                        AddImportFiles(settings, ds, objectType, siteObject, th);
                    }

                    h.EventArguments.Data = ds;
                }

                h.FinishEvent();
            }

            return importedIds;
        }


        /// <summary>
        /// Gets the Dataset of existing database objects.
        /// </summary>
        /// <param name="settings">Import settings</param>
        /// <param name="objectType">Object type</param>
        /// <param name="siteObjects">Site objects</param>
        /// <param name="selectionOnly">If true, only data needed for selection are retrieved</param>
        public static DataSet GetExistingObjects(SiteImportSettings settings, string objectType, bool siteObjects, bool selectionOnly)
        {
            // Get info object
            GeneralizedInfo infoObj = ModuleManager.GetReadOnlyObject(objectType);
            if (infoObj == null)
            {
                throw new Exception("[ImportProvider.GetExistingObjects]: Object type '" + objectType + "' not found.");
            }

            // If import to the new site, no existing objects are available
            if (siteObjects && (settings.SiteId <= 0))
            {
                return null;
            }

            // Get where condition
            var where = settings.GetObjectWhereCondition(objectType, siteObjects);

            string columns = null;
            if (selectionOnly)
            {
                columns = infoObj.CodeNameColumn;

                var ti = infoObj.TypeInfo;
                if ((ti.GUIDColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN) && (infoObj.CodeNameColumn != ti.GUIDColumn))
                {
                    columns += "," + ti.GUIDColumn;
                }
            }

            // Get the data without checking the license
            DataSet ds = infoObj.GetDataQuery(
                true,
                q => q
                    .Where(where)
                    .Columns(columns),
                false
            ).Result;

            return ds;
        }


        /// <summary>
        /// Gets the objects data table.
        /// </summary>
        /// <param name="settings">Import settings</param>
        /// <param name="objectType">Object type</param>
        /// <param name="siteObjects">Site objects</param>
        public static DataTable GetObjectsData(SiteImportSettings settings, string objectType, bool siteObjects)
        {
            // Get info object
            GeneralizedInfo infoObj = ModuleManager.GetReadOnlyObject(objectType);
            if (infoObj == null)
            {
                throw new Exception("[ImportProvider.GetObjectsData]: Object type '" + objectType + "' not found.");
            }

            // Get the data
            DataSet data = LoadObjects(settings, objectType, siteObjects);
            if (!DataHelper.DataSourceIsEmpty(data))
            {
                return ObjectHelper.GetTable(data, infoObj);
            }

            return null;
        }


        /// <summary>
        /// Returns the version of the specified file.
        /// </summary>
        /// <param name="filePath">File path</param>
        public static string GetVersion(string filePath)
        {
            string version = CMSVersion.MainVersion;

            SystemIO.Stream reader = null;
            XmlReader xml = null;

            try
            {
                if (File.Exists(filePath))
                {
                    // Reader setting
                    XmlReaderSettings rs = new XmlReaderSettings();
                    rs.CloseInput = true;
                    rs.CheckCharacters = false;

                    // Open reader
                    reader = FileStream.New(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 8192);
                    xml = XmlReader.Create(reader, rs);

                    // Read main document element
                    do
                    {
                        xml.Read();
                    } while ((xml.NodeType != XmlNodeType.Element) && !xml.EOF);

                    // Get version
                    if (xml.HasAttributes)
                    {
                        xml.MoveToAttribute("version");
                    }

                    if ((xml.NodeType != XmlNodeType.Attribute) || (xml.Name.ToLowerInvariant() != "version"))
                    {
                        throw new Exception("[ImportProvider.GetVersion]: Cannot find version attribute.");
                    }

                    // Get version
                    version = xml.Value;
                }
            }
            finally
            {
                // Safely close readers
                if (xml != null)
                {
                    xml.Close();
                }

                if (reader != null)
                {
                    reader.Close();
                }
            }

            return version;
        }


        /// <summary>
        /// Loads the objects from the import package.
        /// </summary>
        /// <param name="settings">Import settings</param>
        /// <param name="objectType">Object type</param>
        /// <param name="siteObjects">Site objects</param>
        /// <param name="selectionOnly">If true, the objects are loaded only for the selection reasons</param>
        /// <param name="forceXMLStructure">If true, data set structure is loaded from xml even if there is object representation.</param>
        public static DataSet LoadObjects(SiteImportSettings settings, string objectType, bool siteObjects, bool selectionOnly = false, bool forceXMLStructure = false)
        {
            DataSet ds;

            var e = new ImportGetDataEventArgs
            {
                Settings = settings,
                ObjectType = objectType,
                SiteObjects = siteObjects,
                SelectionOnly = selectionOnly
            };

            // Handle the event
            using (var h = ImportExportEvents.GetImportData.StartEvent(e))
            {
                if (h.CanContinue() && (h.EventArguments.Data == null))
                {
                    // Get empty data set
                    GeneralizedInfo infoObj;
                    ds = GetEmptyDataSet(settings, objectType, siteObjects, selectionOnly, out infoObj, forceXMLStructure);

                    // Turn off constrains check
                    ds.EnforceConstraints = false;

                    // Raise prepare data event
                    var eData = new ImportGetDataEventArgs
                    {
                        Data = ds,
                        Settings = settings,
                        ObjectType = objectType,
                        SiteObjects = siteObjects,
                        SelectionOnly = selectionOnly
                    };

                    // Handle the event
                    SpecialActionsEvents.PrepareDataStructure.StartEvent(eData);

                    // Lowercase table names for compatibility
                    DataHelper.LowerCaseTableNames(ds);

                    // Export process canceled
                    if (settings.ProcessCanceled)
                    {
                        ImportCanceled();
                    }

                    SystemIO.Stream reader = null;
                    XmlReader xml = null;

                    try
                    {
                        string safeObjectType = ImportExportHelper.GetSafeObjectTypeName(objectType);

                        // Prepare the path
                        string filePath = DirectoryHelper.CombinePath(settings.TemporaryFilesPath, ImportExportHelper.DATA_FOLDER) + "\\";

                        // Get object type subfolder
                        filePath += ImportExportHelper.GetObjectTypeSubFolder(settings, objectType, siteObjects);
                        filePath += safeObjectType + ".xml";
                        string rootElement = safeObjectType;

                        filePath = ImportExportHelper.GetExportFilePath(filePath);

                        // Import only if file exists
                        if (File.Exists(filePath))
                        {
                            // Reader setting
                            XmlReaderSettings rs = new XmlReaderSettings();
                            rs.CloseInput = true;
                            rs.CheckCharacters = false;

                            // Open reader
                            reader = FileStream.New(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 8192);
                            xml = XmlReader.Create(reader, rs);

                            // Read main document element
                            do
                            {
                                xml.Read();
                            } while ((xml.NodeType != XmlNodeType.Element) && !xml.EOF);

                            if (xml.Name.ToLowerInvariant() != rootElement.ToLowerInvariant())
                            {
                                throw new Exception("[ImportProvider.LoadObjects]: The required page element is '" + safeObjectType + "', element found is '" + xml.Name + "'.");
                            }

                            // Get version
                            if (xml.HasAttributes)
                            {
                                xml.MoveToAttribute("version");
                            }

                            if ((xml.NodeType != XmlNodeType.Attribute) || (xml.Name.ToLowerInvariant() != "version"))
                            {
                                throw new Exception("[ImportProvider.LoadObjects]: Cannot find version attribute.");
                            }

                            // Check version of the package
                            if (CheckPackageVersion)
                            {
                                string version = xml.Value;
                                if (ImportExportHelper.IsLowerVersion(CMSVersion.MainVersion, null, version, null))
                                {
                                    throw new Exception("[ImportProvider.LoadObjects]: Cannot import data from newer version '" + version + "' to current version '" + CMSVersion.MainVersion + "'.");
                                }
                            }

                            // Get the dataset
                            do
                            {
                                xml.Read();
                            } while (((xml.NodeType != XmlNodeType.Element) || (xml.Name != "NewDataSet")) && !xml.EOF);

                            // Read data
                            if (xml.Name == "NewDataSet")
                            {
                                ds.ReadXml(xml);
                            }

                            // Filter data by object type condition for selection, some object types may overlap
                            if (selectionOnly && !DataHelper.DataSourceIsEmpty(ds))
                            {
                                // Remove unwanted rows from data collection with dependence on type condition
                                if (infoObj.TypeInfo.TypeCondition != null)
                                {
                                    var dt = ObjectHelper.GetTable(ds, infoObj);
                                    var where = EnsureDataRowFilterExpression(infoObj.TypeInfo.WhereCondition);
                                    DataHelper.KeepOnlyRows(dt, where);

                                    dt.AcceptChanges();
                                }
                            }
                        }
                    }
                    catch (ProcessCanceledException)
                    {
                        throw;
                    }
                    catch (Exception ex)
                    {
                        // Log error
                        LogProgressError(settings, string.Format(settings.GetAPIString("ImportSite.ErrorLoadingData", "Error loading '{0}' data"), ImportExportHelper.GetObjectTypeName(objectType, settings)), ex);
                        throw;
                    }
                    finally
                    {
                        // Safely close readers
                        if (xml != null)
                        {
                            xml.Close();
                        }

                        if (reader != null)
                        {
                            reader.Close();
                        }
                    }

                    h.EventArguments.Data = ds;
                    h.EventArguments.Object = infoObj;
                }
                else
                {
                    // Get handler data
                    ds = h.EventArguments.Data;
                }

                h.FinishEvent();
            }

            return ds;
        }


        /// <summary>
        /// Removes Unicode characters from SQL syntax where condition
        /// </summary>
        /// <remarks>
        /// DataTable filter expression doesn't support unicode string option in filter syntax
        /// </remarks>
        /// <param name="where">SQL syntax where condition</param>
        private static string EnsureDataRowFilterExpression(string where)
        {
            return where.Replace(" N'", " '").Replace("(N'", "('");
        }


        private static DataSet GetEmptyDataSet(SiteImportSettings settings, string objectType, bool siteObjects, bool selectionOnly, out GeneralizedInfo infoObj, bool forceXMLStructure = false)
        {
            DataSet ds;

            // Raise prepare data event
            var eData = new ImportGetDataEventArgs
            {
                Settings = settings,
                ObjectType = objectType,
                SiteObjects = siteObjects,
                SelectionOnly = selectionOnly
            };

            // Handle the event
            SpecialActionsEvents.GetEmptyObject.StartEvent(eData);

            // Ensure empty object
            infoObj = eData.Object ?? ModuleManager.GetReadOnlyObject(objectType);

            // Get data set
            if (forceXMLStructure || (infoObj == null) || (infoObj.MainObject is NotImplementedInfo))
            {
                // Create empty data set
                ds = new DataSet();

                // Ensure translation table
                if (objectType == ImportExportHelper.OBJECT_TYPE_TRANSLATION)
                {
                    ds.Tables.Add(TranslationHelper.GetEmptyTable());
                }
            }
            else
            {
                // Get objects DataSet
                if (selectionOnly)
                {
                    // Code name column
                    ds = DataHelper.GetSingleColumnDataSet(ObjectHelper.GetSerializationTableName(infoObj), infoObj.CodeNameColumn, typeof(string));

                    // Display name column
                    var dt = ds.Tables[0];
                    if (infoObj.CodeNameColumn != infoObj.DisplayNameColumn)
                    {
                        DataHelper.EnsureColumn(dt, infoObj.DisplayNameColumn, typeof(string));
                    }

                    // GUID column
                    var ti = infoObj.TypeInfo;
                    if ((ti.GUIDColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN) && (infoObj.CodeNameColumn != ti.GUIDColumn))
                    {
                        DataHelper.EnsureColumn(dt, ti.GUIDColumn, typeof(Guid));
                    }

                    // Columns used by type condition
                    var tc = ti.TypeCondition;
                    if (tc != null)
                    {
                        foreach (var conditionColumn in tc.ConditionColumns)
                        {
                            DataHelper.EnsureColumn(dt, conditionColumn, ti.ClassStructureInfo.GetColumnType(conditionColumn));
                        }
                    }
                }
                else
                {
                    ds = ObjectHelper.GetObjectsDataSet(OperationTypeEnum.Export, infoObj, true);
                }

                // Add tasks table
                DataSet tasksDS;
                if (selectionOnly)
                {
                    tasksDS = DataHelper.GetSingleColumnDataSet("Export_Task", "TaskID", typeof(int));

                    DataTable dt = tasksDS.Tables[0];
                    dt.Columns.Add("TaskTitle", typeof(string));
                    dt.Columns.Add("TaskType", typeof(string));
                    dt.Columns.Add("TaskTime", typeof(DateTime));
                }
                else
                {
                    tasksDS = DataClassInfoProvider.GetDataSet("Export.Task");
                }

                DataHelper.TransferTable(ds, tasksDS, "Export_Task");
            }

            return ds;
        }


        /// <summary>
        /// Process additional actions after import.
        /// </summary>
        /// <param name="settings">Site import settings</param>
        /// <param name="th">Translation helper</param>
        private static void ProcessAdditionalActions(SiteImportSettings settings, TranslationHelper th)
        {
            using (new ImportSpecialCaseContext(settings))
            {
                // Log progress
                LogProgress(LogStatusEnum.Info, settings, settings.GetAPIString("ImportProvider.ProcessAdditionalActions", "Processing additional actions"));

                // Save the settings
                settings.SavePersistentLog();

                try
                {
                    // Handle the event
                    using (var h = SpecialActionsEvents.ProcessAdditionalActions.StartEvent(settings, th))
                    {
                        if (h.CanContinue())
                        {
                            if (settings.IsOlderVersion)
                            {
                                // Log progress
                                LogProgress(LogStatusEnum.Info, settings, settings.GetAPIString("ImportProvider.EnsureUIElementBackCompatibily", "Ensuring backward compatibility of UI elements"));

                                // Ensure top level UI elements backward compatibility
                                QueryDataParameters parameters = new QueryDataParameters();
                                parameters.Add("siteId", settings.SiteId);
                                parameters.Add("packageVersion", settings.Version);
                                parameters.Add("topLevelElements", true);
                                ConnectionHelper.ExecuteQuery("CMS.UIElement.UpdateAfterImport", parameters);

                                // Ensure child UI elements backward compatibility
                                parameters = new QueryDataParameters();
                                parameters.Add("siteId", settings.SiteId);
                                parameters.Add("packageVersion", settings.Version);
                                parameters.Add("topLevelElements", false);
                                ConnectionHelper.ExecuteQuery("CMS.UIElement.UpdateAfterImport", parameters);
                            }
                        }

                        h.FinishEvent();
                    }
                }
                catch (ProcessCanceledException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    // Log exception
                    LogProgressError(settings, settings.GetAPIString("SiteImport.ProcessAdditionalActionsError", "Error processing additional actions."), ex);
                    throw;
                }
            }
        }


        /// <summary>
        /// Imports the objects.
        /// </summary>
        /// <param name="settings">Import settings</param>
        /// <param name="ds">Source DataSet</param>
        /// <param name="objectType">Object type</param>
        /// <param name="siteObjects">Site objects</param>
        /// <param name="th">Translation helper</param>
        /// <param name="importedObjects">Imported objects</param>
        public static UpdateResultEnum ProcessTasks(SiteImportSettings settings, DataSet ds, string objectType, bool siteObjects, TranslationHelper th, List<ImportedObject> importedObjects)
        {
            // Import process canceled
            if (settings.ProcessCanceled)
            {
                ImportCanceled();
            }

            UpdateResultEnum result = UpdateResultEnum.OK;

            // Get the tasks
            DataTable tasksDT = ds.Tables["Export_Task"];
            if (!DataHelper.DataSourceIsEmpty(tasksDT))
            {
                string objectName = "";

                try
                {
                    ProcessObjectEnum processType = settings.GetObjectsProcessType(objectType, siteObjects);
                    if (processType != ProcessObjectEnum.None)
                    {
                        // Get the rows to process
                        DataRow[] processRows;
                        if (processType == ProcessObjectEnum.All)
                        {
                            // All objects
                            processRows = tasksDT.Select();
                        }
                        else
                        {
                            // Selected objects
                            int[] taskIDs = settings.GetSelectedTasksArray(objectType, siteObjects);
                            string where = SqlHelper.GetWhereCondition("TaskID", taskIDs.AsEnumerable());
                            processRows = tasksDT.Select(where);
                        }

                        // Process the tasks
                        if (processRows.Length != 0)
                        {
                            // Log progress
                            LogProgress(LogStatusEnum.Info, settings, string.Format(settings.GetAPIString("ImportSite.ProcessingTasks", "Processing '{0}' object tasks"), ImportExportHelper.GetObjectTypeName(objectType, settings)));

                            foreach (DataRow dr in processRows)
                            {
                                // Import process canceled
                                if (settings.ProcessCanceled)
                                {
                                    ImportCanceled();
                                }

                                // Process the task
                                ExportTaskInfo taskObj = new ExportTaskInfo(dr);
                                objectName = TextHelper.LimitLength(taskObj.TaskTitle, 100);

                                var parameters = new ImportParameters
                                {
                                    Data = ds,
                                    ImportedObjects = importedObjects
                                };

                                ProcessTask(settings, taskObj, th, parameters);
                            }
                        }
                    }
                    else
                    {
                        result = UpdateResultEnum.NotProcessed;
                    }
                }
                catch (ProcessCanceledException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    // Log exception
                    LogProgressError(settings, string.Format(settings.GetAPIString("ImportSite.ErrorProcessTasks", "Error processing '{0}' object tasks."), ImportExportHelper.GetObjectTypeName(objectType, settings)) + " (" + HTMLHelper.HTMLEncode(objectName) + ")", ex);
                    throw;
                }
            }

            return result;
        }


        /// <summary>
        /// Processes the task.
        /// </summary>
        /// <param name="settings">Import settings</param>
        /// <param name="taskObj">Task object</param>
        /// <param name="th">Translation table</param>
        /// <param name="parameters">Import parameters</param>
        public static void ProcessTask(SiteImportSettings settings, ExportTaskInfo taskObj, TranslationHelper th, ImportParameters parameters)
        {
            switch (taskObj.TaskType)
            {
                // Delete task
                case TaskTypeEnum.DeleteObject:
                    DataSet objectDS = GetDataSet(taskObj.TaskData, taskObj.TaskType, taskObj.TaskObjectType);
                    if (!DataHelper.DataSourceIsEmpty(objectDS))
                    {
                        // Get the object type
                        GeneralizedInfo infoObj = ModuleManager.GetReadOnlyObject(taskObj.TaskObjectType);
                        if (infoObj == null)
                        {
                            throw new Exception("[ImportProvider.ProcessTask: Object type '" + taskObj.TaskObjectType + "' not found.");
                        }
                        // Get the table
                        var dt = ObjectHelper.GetTable(objectDS, infoObj);
                        if (!DataHelper.DataSourceIsEmpty(dt))
                        {
                            foreach (DataRow objectRow in dt.Rows)
                            {
                                // Load the object
                                GeneralizedInfo deleteObj = GetObject(objectRow, taskObj.TaskObjectType);

                                // Init default columns
                                if (deleteObj.ObjectSiteID > 0)
                                {
                                    deleteObj.ObjectSiteID = settings.SiteId;
                                }
                                deleteObj.ObjectID = 0;

                                // Translate object columns
                                TranslateColumns(settings, deleteObj, th, false, true, false);

                                // Get existing object
                                BaseInfo existingObj = deleteObj.GetExisting();
                                if (existingObj != null)
                                {
                                    try
                                    {
                                        ProcessDeleteTask(settings, taskObj, existingObj, th, parameters);
                                    }
                                    catch
                                    {
                                        LogProgress(LogStatusEnum.Warning, settings, string.Format(settings.GetAPIString("ImportSite.WarningProcessTasks", "Unable to delete '{0}' object. Please delete the object manually."), ImportExportHelper.GetObjectTypeName(existingObj.TypeInfo.ObjectType, settings)) + " (" + HTMLHelper.HTMLEncode(TextHelper.LimitLength(taskObj.TaskTitle, 100)) + ")");
                                    }
                                }
                            }
                        }
                    }
                    break;

                default:
                    throw new Exception("[ImportProvider.ProcessTask]: Task type '" + taskObj.TaskType + "' not supported.");
            }
        }


        private static void ProcessDeleteTask(SiteImportSettings settings, ExportTaskInfo taskObj, BaseInfo existingObj, TranslationHelper th, ImportParameters parameters)
        {
            var e = new ProcessDeleteTaskArgs
            {
                Settings = settings,
                Task = taskObj,
                Object = existingObj,
                Parameters = parameters,
                TranslationHelper = th
            };

            // Handle the event
            using (var h = SpecialActionsEvents.ProcessDeleteTask.StartEvent(e))
            {
                if (h.CanContinue())
                {
                    // Delete the existing object
                    DeleteObject(settings, existingObj, null, parameters);
                }

                h.FinishEvent();
            }
        }


        /// <summary>
        /// Returns the dataset loaded from the given task data.
        /// </summary>
        /// <param name="taskData">Task data to make the dataset from</param>
        /// <param name="taskType">Task type</param>
        /// <param name="taskObjectType">Task object type</param>
        public static DataSet GetDataSet(string taskData, TaskTypeEnum taskType, string taskObjectType)
        {
            DataSet ds;

            // Process the task
            switch (taskType)
            {
                // Object tasks
                case TaskTypeEnum.DeleteObject:
                    {
                        // Get the object definition
                        BaseInfo infoObj = ModuleManager.GetReadOnlyObject(taskObjectType);

                        // Get the DataSet
                        ds = ObjectHelper.GetObjectsDataSet(OperationTypeEnum.Export, infoObj, false);
                        ds.Tables.Add(TranslationHelper.GetEmptyTable());
                    }
                    break;

                // Unknown task
                default:
                    ds = new DataSet();
                    break;
            }

            // Parse the data
            XmlParserContext xmlContext = new XmlParserContext(null, null, null, XmlSpace.None);
            XmlReader reader = new XmlTextReader(taskData, XmlNodeType.Element, xmlContext);
            ds.ReadXml(reader);

            return ds;
        }


        /// <summary>
        /// Imports the objects.
        /// </summary>
        /// <param name="settings">Import settings</param>
        /// <param name="data">Source DataSet</param>
        /// <param name="objectType">Object type</param>
        /// <param name="siteObject">Site objects</param>
        /// <param name="th">Translation helper</param>
        /// <param name="importChild">Import child objects?</param>
        /// <param name="processType">Process type</param>
        /// <param name="importedParentIds">Table of imported parent object IDs. When set, only objects with parent from the table are imported</param>
        public static Dictionary<int, int> ImportObjects(SiteImportSettings settings, DataSet data, string objectType, bool siteObject, TranslationHelper th, bool importChild, ProcessObjectEnum processType, List<int> importedParentIds)
        {
            // Import process canceled
            if (settings.ProcessCanceled)
            {
                ImportCanceled();
            }

            Dictionary<int, int> importedObjectsIds = new Dictionary<int, int>();

            // Process only if some data present
            if (!DataHelper.DataSourceIsEmpty(data))
            {
                bool objectTypeProcessed = settings.IsObjectTypeProcessed(objectType, siteObject, processType);

                // Get info object
                GeneralizedInfo infoObj = ModuleManager.GetReadOnlyObject(objectType);
                if (infoObj == null)
                {
                    throw new Exception("[ImportProvider.ImportObjects]: Object type '" + objectType + "' not found.");
                }

                // Handle categories
                var ti = infoObj.TypeInfo;
                var category = ti.CategoryObject;

                if ((category != null) && !ti.IsCategory)
                {
                    // Update all categories
                    ProcessObjectEnum processCategory = objectTypeProcessed ? ProcessObjectEnum.All : ProcessObjectEnum.None;

                    var categoryTypeInfo = category.TypeInfo;

                    settings.SetObjectsProcessType(processCategory, categoryTypeInfo.ObjectType, false);
                    ImportObjects(settings, data, categoryTypeInfo.ObjectType, false, th, false, processCategory, null);
                }

                string objectName = "";

                try
                {
                    // Get process type if not set
                    if (processType == ProcessObjectEnum.Default)
                    {
                        processType = settings.GetObjectsProcessType(objectType, siteObject);
                    }
                    if (processType != ProcessObjectEnum.None)
                    {
                        // Perform automatic selection
                        if (settings.IsAutomaticallySelected(infoObj) && (processType == ProcessObjectEnum.Selected))
                        {
                            settings.EnsureAutomaticSelection(infoObj, siteObject);
                        }

                        // Get the table
                        var dt = ObjectHelper.GetTable(data, infoObj);
                        if (!DataHelper.DataSourceIsEmpty(dt))
                        {
                            // Log progress only if there are some objects to be imported
                            if (objectTypeProcessed)
                            {
                                if (ti.ImportExportSettings.LogProgress)
                                {
                                    LogProgress(LogStatusEnum.Info, settings, string.Format(settings.GetAPIString("ImportSite.Importing", "Importing {0}"), ImportExportHelper.GetObjectTypeName(objectType, settings)));

                                    // Save the settings progress
                                    settings.SavePersistentLog();
                                }
                            }

                            List<GeneralizedInfo> postProcessList = new List<GeneralizedInfo>();

                            // Import objects
                            foreach (DataRow dr in dt.Rows)
                            {
                                // Import process canceled
                                if (settings.ProcessCanceled)
                                {
                                    ImportCanceled();
                                }

                                // Update the object
                                infoObj = GetObject(dr, objectType);

                                // Check that loaded data matches object type, otherwise process next row
                                if (!infoObj.TypeInfo.ObjectType.Equals(ti.ObjectType, StringComparison.OrdinalIgnoreCase))
                                {
                                    continue;
                                }

                                int objectId = infoObj.ObjectID;
                                string objectCodeName = infoObj.ObjectCodeName;
                                objectName = TextHelper.LimitLength(infoObj.ObjectDisplayName, 100);

                                // ## Special cases
                                switch (objectType)
                                {
                                    // Do not import system settings
                                    case SettingsKeyInfo.OBJECT_TYPE:
                                        if (ImportExportHelper.IsSettingKeyExcluded(objectCodeName))
                                        {
                                            continue;
                                        }
                                        break;
                                }

                                // Update the object
                                var importedObjects = new List<ImportedObject>();

                                // Check existing objects only for global objects, or site objects when importing to existing site
                                // Site objects will be new for a new site import so there is no need to check existing
                                // Site itself is first created, and then updated with data, therefore also needs to check existing object
                                var checkExisting = !siteObject || settings.ExistingSite || (objectType == SiteInfo.OBJECT_TYPE);

                                var parameters = new ImportParameters
                                {
                                    TranslationHelper = th,
                                    SiteObject = siteObject,
                                    Data = data,
                                    ObjectProcessType = processType,
                                    ImportedObjects = importedObjects,
                                    PostProcessList = postProcessList,
                                    UpdateChildObjects = importChild,
                                    PostProcessing = false,
                                    CheckExisting = checkExisting
                                };

                                var result = ImportObject(settings, infoObj, parameters, importedParentIds);

                                // If imported, process additional actions
                                switch (result.Result)
                                {
                                    case UpdateResultEnum.PostProcess:
                                        {
                                            // Add to post process list
                                            GeneralizedInfo postObj = GetObject(dr, objectType);
                                            postProcessList.Add(postObj);
                                        }
                                        break;

                                    case UpdateResultEnum.OK:
                                        {
                                            // Add imported object ID
                                            importedObjectsIds[objectId] = infoObj.ObjectID;

                                            LogSynchronizationTasks(settings, importedObjects, result.Result);

                                            parameters.SetValue("dr", dr);

                                            var e = new ImportEventArgs
                                            {
                                                Settings = settings,
                                                TranslationHelper = th,
                                                Object = infoObj,
                                                Parameters = parameters
                                            };

                                            // Handle the event
                                            SpecialActionsEvents.ObjectProcessed.StartEvent(e);
                                        }
                                        break;
                                }
                            }

                            // Process post list
                            ObjectsPostProcessing(settings, infoObj, siteObject, data, th, processType, postProcessList, importedObjectsIds);
                        }
                        else
                        {
                            objectTypeProcessed = false;
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
                    LogProgressError(settings, string.Format(settings.GetAPIString("ImportSite.ErrorImportObjects", "Error importing {0}"), ImportExportHelper.GetObjectTypeName(objectType, settings)) + " (" + HTMLHelper.HTMLEncode(objectName) + ")", ex);
                    throw;
                }
                finally
                {
                    if (objectTypeProcessed)
                    {
                        // Refresh child data counts
                        RefreshDataCounts(infoObj, category);

                        // Re-init orders
                        infoObj.InitObjectsOrder();

                        // Action after update of all objects
                        switch (objectType)
                        {
                            // Update site information
                            case SiteInfo.OBJECT_TYPE:
                                {
                                    settings.SiteDisplayName = infoObj.ObjectDisplayName;
                                    settings.SiteDomain = ValidationHelper.GetString(infoObj.GetValue("SiteDomainName"), "");
                                    settings.SiteIsContentOnly = ValidationHelper.GetBoolean(infoObj.GetValue("SiteIsContentOnly"), false);
                                }
                                break;
                        }
                    }
                }
            }

            return importedObjectsIds;
        }


        /// <summary>
        /// Creates a new object instance from the given data row
        /// </summary>
        /// <param name="dr">Data row</param>
        /// <param name="objectType">Object type</param>
        private static BaseInfo GetObject(DataRow dr, string objectType)
        {
            return ModuleManager.GetObject(new LoadDataSettings(dr, objectType)
            {
                DataIsExternal = true
            });
        }


        private static void ObjectsPostProcessing(SiteImportSettings settings, GeneralizedInfo infoObj, bool siteObject, DataSet data, TranslationHelper th, ProcessObjectEnum processType, List<GeneralizedInfo> postProcessList, Dictionary<int, int> importedObjectsIDs)
        {
            // Prepare import parameters
            var parameters = new ImportParameters
            {
                ObjectProcessType = processType,
                Data = data,
                SiteObject = siteObject,
                ObjectOriginalID = infoObj.ObjectID,
                PostProcessing = true,
                PostProcessList = postProcessList
            };

            var e = new ImportEventArgs
            {
                Settings = settings,
                TranslationHelper = th,
                Object = infoObj,
                Parameters = parameters
            };

            // Handle the event
            using (var h = SpecialActionsEvents.ObjectsPostProcessing.StartEvent(e))
            {
                if (h.CanContinue())
                {
                    if (postProcessList.Count > 0)
                    {
                        foreach (GeneralizedInfo postObject in postProcessList)
                        {
                            var importedObjects = new List<ImportedObject>();

                            UpdateResultEnum postResult;

                            var postTypeInfo = postObject.TypeInfo;
                            if (postTypeInfo.IsBinding)
                            {
                                // Update the binding
                                string error = TranslateColumns(settings, postObject, th);

                                // Update/Insert the binding if translation is successful
                                if (error == "")
                                {
                                    postObject.ObjectID = 0;
                                    SetObject(settings, postObject, null, importedObjects);
                                    postResult = UpdateResultEnum.OK;
                                }
                                else
                                {
                                    postResult = UpdateResultEnum.Error;
                                    if (settings.LogObjectsWarnings)
                                    {
                                        // Log warning
                                        LogProgress(LogStatusEnum.Warning, settings, string.Format(settings.GetAPIString("ImportSite.WarningProcessPostBindingObject", "The '{0}' object couldn't be bound with postprocessing, check the object manually."), ImportExportHelper.GetObjectTypeName(postTypeInfo.ObjectType, settings)) + "(" + postObject.ObjectGUID + ")");
                                    }
                                }
                            }
                            else
                            {
                                int objectId = postObject.ObjectID;

                                var importParameters = new ImportParameters
                                {
                                    TranslationHelper = th,
                                    SiteObject = siteObject,
                                    ImportedObjects = importedObjects,
                                    PostProcessing = true,
                                    ObjectProcessType = ProcessObjectEnum.Default,
                                    Data = data,
                                    UpdateChildObjects = true
                                };

                                // Update the object
                                postResult = ImportObject(settings, postObject, importParameters, null).Result;

                                // Add imported object ID
                                if ((postTypeInfo.ObjectType == infoObj.TypeInfo.ObjectType) && (postResult == UpdateResultEnum.OK))
                                {
                                    importedObjectsIDs[objectId] = postObject.ObjectID;
                                }
                            }

                            LogSynchronizationTasks(settings, importedObjects, postResult);
                        }
                    }
                }

                h.FinishEvent();
            }
        }


        private static void RefreshDataCounts(GeneralizedInfo infoObj, GeneralizedInfo category)
        {
            // Try to refresh objects count for all objects except the categories
            var ti = infoObj.TypeInfo;

            if (!ti.IsCategory)
            {
                var objectType = (category != null) ? category.TypeInfo.ObjectType : ti.ObjectType;

                // Get the provider
                var provider = InfoProviderLoader.GetInfoProvider(objectType, false) as IRelatedObjectCountProvider;
                if (provider != null)
                {
                    provider.RefreshObjectsCounts();
                }

                // Process child types
                foreach (var childType in ti.ChildObjectTypes)
                {
                    if (childType.Equals(objectType, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    var child = ModuleManager.GetReadOnlyObject(childType);
                    if (child == null)
                    {
                        continue;
                    }

                    var childCategory = child.TypeInfo.CategoryObject;
                    RefreshDataCounts(child, childCategory);
                }
            }
        }


        private static void LogSynchronizationTasks(SiteImportSettings settings, IEnumerable<ImportedObject> list, UpdateResultEnum result)
        {
            if (result != UpdateResultEnum.OK || !settings.LogSynchronization && !settings.LogIntegration)
            {
                return;
            }

            foreach (ImportedObject obj in list)
            {
                LogSynchronizationTask(settings, obj);
            }
        }


        private static void LogSynchronizationTask(SiteImportSettings settings, ImportedObject obj)
        {
            var infoObj = obj.Object;
            var taskType = obj.TaskType;

            var logIntegration = settings.LogIntegration;
            var logSynchronization = IsStagingTaskLogged(infoObj, settings);

            if (!logSynchronization && !logIntegration)
            {
                return;
            }

            using (CMSActionContext context = new CMSActionContext())
            {
                context.LogSynchronization = logSynchronization;
                context.LogIntegration = logIntegration;
                context.AllowAsyncActions = true;

                if (taskType == TaskTypeEnum.AddToSite || taskType == TaskTypeEnum.RemoveFromSite)
                {
                    if (settings.SiteId > 0)
                    {
                        var objChangeSettings = new LogObjectChangeSettings(infoObj, TaskTypeEnum.AddToSite)
                        {
                            SiteID = settings.SiteId
                        };

                        SynchronizationHelper.LogObjectChange(objChangeSettings);
                    }
                }
                else
                {
                    SynchronizationHelper.LogObjectChange(infoObj, taskType);
                }
            }
        }


        private static bool IsStagingTaskLogged(GeneralizedInfo infoObj, SiteImportSettings settings)
        {
            if (!settings.LogSynchronization)
            {
                return false;
            }

            return IsParentObject(infoObj) || IsChildObjectDataSynchronizedSeparately(infoObj);
        }


        private static bool IsParentObject(GeneralizedInfo infoObj)
        {
            return string.IsNullOrEmpty(infoObj.ParentObjectType);
        }


        private static bool IsChildObjectDataSynchronizedSeparately(GeneralizedInfo infoObj)
        {
            var isChild = !IsParentObject(infoObj);
            var settings = infoObj.TypeInfo.SynchronizationSettings;

            // Log staging task for child objects which are not included in parent data or separate task should be logged
            return isChild && (settings.IncludeToSynchronizationParentDataSet == IncludeToParentEnum.None || settings.LogSynchronization == SynchronizationTypeEnum.LogSynchronization);
        }


        /// <summary>
        /// Updates the object. Returns true if the object was updated.
        /// </summary>
        /// <param name="settings">Import settings</param>
        /// <param name="infoObj">Object to update</param>
        /// <param name="parameters">Import parameters. The import process may modify the parameter values, always provide a new instance of parameters</param>
        /// <param name="importedParentIds">Table of imported parent object IDs. When set, only objects with parent from the table are imported</param>
        public static ImportObjectResult ImportObject(SiteImportSettings settings, GeneralizedInfo infoObj, ImportParameters parameters, List<int> importedParentIds)
        {
            UpdateResultEnum result = UpdateResultEnum.OK;
            bool someDataImported = false;

            // Get process type
            var process = GetObjectProcessType(settings, infoObj, parameters.SiteObject, parameters.ObjectProcessType);
            if (process != ProcessObjectEnum.None)
            {
                // Check parent import status
                if (!CheckParent(settings, infoObj, importedParentIds, ref result))
                {
                    return new ImportObjectResult(result, false);
                }

                // Update import parameters
                parameters.ObjectProcessType = process;
                parameters.ObjectOriginalID = infoObj.ObjectID;

                var e = new ImportEventArgs
                {
                    Settings = settings,
                    TranslationHelper = parameters.TranslationHelper,
                    Object = infoObj,
                    ParentObject = parameters.ParentObject,
                    Parameters = parameters
                };

                // Handle the event
                using (var h = ImportExportEvents.ImportObject.StartEvent(e))
                {
                    if (h.CanContinue())
                    {
                        var ti = infoObj.TypeInfo;

                        // Set object site ID
                        if ((ti.SiteIDColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN) && (infoObj.ObjectSiteID > 0))
                        {
                            infoObj.ObjectSiteID = settings.SiteId;
                        }

                        // Reset object ID so that it behaves as external object and does full update
                        infoObj.ObjectID = 0;

                        // Translate columns
                        if (!TranslateColumns(settings, infoObj, parameters, ref result))
                        {
                            return new ImportObjectResult(result, false);
                        }

                        // Check existing object if configured, or if a nested child
                        // Not a nested child such as UserSettings, these are pre-created from parent already so we want to overwrite them, not create new
                        var isNested = IsNestedType(ti);

                        var checkExisting = ti.ImportExportSettings.AlwaysCheckExisting || parameters.CheckExisting || isNested;

                        // Get existing object
                        var existing = checkExisting ? GetExistingObject(settings, infoObj, parameters) : null;

                        var isNew = (existing == null);
                        if (!isNew)
                        {
                            // Skip update if only new objects should be imported. Do not apply this setting for post-processed and nested items
                            var isSkipObjectUpdateChanged = false;
                            if (settings.ImportOnlyNewObjects && !parameters.PostProcessing && !isNested)
                            {
                                isSkipObjectUpdateChanged = !parameters.SkipObjectUpdate;
                                parameters.SkipObjectUpdate = true;
                            }

                            // Handle existing object actions and stop the process if required
                            if (!HandleExistingObject(settings, ref infoObj, parameters, ref result))
                            {
                                return new ImportObjectResult(result, false);
                            }

                            if (!parameters.SkipObjectUpdate || isSkipObjectUpdateChanged)
                            {
                                // Assign object to site if required
                                if ((parameters.ObjectProcessType == ProcessObjectEnum.SiteBinding) && (settings.SiteId > 0))
                                {
                                    GeneralizedInfo bindingObj = ti.SiteBindingObject;

                                    if (bindingObj != null)
                                    {
                                        // Only assign object to site (use clone)
                                        AssignToSite(settings, bindingObj.Clone(), infoObj, parameters);
                                    }
                                }
                            }
                        }

                        // Process main object
                        someDataImported |= ProcessMainObject(settings, infoObj, parameters);

                        // Process object child objects, bindings and meta files
                        someDataImported |= ProcessObjectData(settings, infoObj, parameters, isNew);

                        // Create object version only if some data was imported to prevent creating too many unnecessary versions
                        if (someDataImported)
                        {
                            CreateObjectVersion(settings, infoObj, existing, process);
                        }

                        // Only site assignment for existing objects
                        if ((process == ProcessObjectEnum.SiteBinding) && (existing == null))
                        {
                            // Do not process further
                            return new ImportObjectResult(UpdateResultEnum.NotProcessed, someDataImported);
                        }
                    }

                    h.FinishEvent();
                }
            }
            else
            {
                // Object not processed
                result = UpdateResultEnum.NotProcessed;
            }

            return new ImportObjectResult(result, someDataImported);
        }


        private static bool HandleExistingObject(SiteImportSettings settings, ref GeneralizedInfo infoObj, ImportParameters parameters, ref UpdateResultEnum result)
        {
            var e = new ImportEventArgs
            {
                Settings = settings,
                TranslationHelper = parameters.TranslationHelper,
                Object = infoObj,
                ParentObject = parameters.ParentObject,
                Parameters = parameters
            };

            // Handle the event
            using (var h = SpecialActionsEvents.HandleExistingObject.StartEvent(e))
            {
                if (h.CanContinue())
                {
                    if (parameters.SkipObjectUpdate)
                    {
                        // Use existing object and prevent updating object
                        infoObj = parameters.ExistingObject;

                        // Register translation
                        parameters.TranslationHelper.AddIDTranslation(infoObj.TypeInfo.ObjectType, parameters.ObjectOriginalID, infoObj.ObjectID, 0);
                    }
                    else
                    {
                        // Handle existing object
                        var existing = parameters.ExistingObject.Generalized;

                        // Existing object found, BUT GUIDs don't match
                        if (!parameters.PostProcessing && (infoObj.ObjectGUID != existing.ObjectGUID))
                        {
                            result = UpdateResultEnum.PostProcess;
                            return false;
                        }

                        // Exclude ID and GUID in existing object
                        var excludedColumns = new[] { existing.TypeInfo.IDColumn, existing.TypeInfo.GUIDColumn };

                        foreach (var columnName in existing.ColumnNames.Where(columnName => !excludedColumns.Contains(columnName)))
                        {
                            // Set new values to the existing object
                            existing.SetValue(columnName, infoObj.GetValue(columnName));
                        }

                        infoObj = existing;
                    }
                }

                h.FinishEvent();
            }

            return true;
        }


        private static GeneralizedInfo GetExistingObject(SiteImportSettings settings, GeneralizedInfo infoObj, ImportParameters parameters)
        {
            GeneralizedInfo existing = null;

            ImportEventArgs e = new ImportEventArgs
            {
                Settings = settings,
                TranslationHelper = parameters.TranslationHelper,
                Object = infoObj,
                ParentObject = parameters.ParentObject,
                Parameters = parameters
            };

            // Handle the event
            using (var h = SpecialActionsEvents.GetExistingObject.StartEvent(e))
            {
                if (h.CanContinue())
                {
                    // Get existing object
                    existing = infoObj.GetExisting();

                    // Store existing object for further use
                    parameters.ExistingObject = existing;
                }

                h.FinishEvent();
            }

            return existing;
        }


        private static bool CheckParent(SiteImportSettings settings, GeneralizedInfo infoObj, List<int> importedParentIds, ref UpdateResultEnum result)
        {
            // Check parent ID
            if (!ParentImported(infoObj, importedParentIds))
            {
                if (IsObjectPostProcess(infoObj, new[] { infoObj.TypeInfo.ParentIDColumn }))
                {
                    result = UpdateResultEnum.PostProcess;
                    return false;
                }

                if (settings.LogObjectsWarnings)
                {
                    LogProgress(LogStatusEnum.Warning, settings, string.Format(settings.GetAPIString("ImportSite.WarningParentNotImported", "Parent object for object '{0}' not found."), ImportExportHelper.GetObjectTypeName(infoObj.TypeInfo.ObjectType, settings)) + "(" + HTMLHelper.HTMLEncode(infoObj.ObjectCodeName) + ")");
                }

                result = UpdateResultEnum.Error;
                return false;
            }
            return true;
        }


        private static bool TranslateColumns(SiteImportSettings settings, GeneralizedInfo infoObj, ImportParameters parameters, ref UpdateResultEnum result)
        {
            var error = TranslateColumns(settings, infoObj, parameters.TranslationHelper);
            if (error != "")
            {
                // Check if inherited document type is just missing the binding to the parent type
                if ((infoObj.MainObject is DataClassInfo) && error.Equals("ClassInheritsFromClassID", StringComparison.OrdinalIgnoreCase))
                {
                    // Import as not inherited in case the parent class would be missing
                    DataClassInfo ci = (DataClassInfo)infoObj.MainObject;

                    // Store original class inherited class ID
                    parameters.SetValue("originalInheritedClassId", ci.ClassInheritsFromClassID);
                    ci.ClassInheritsFromClassID = 0;

                    result = UpdateResultEnum.PostProcess;
                }
                else if (IsObjectPostProcess(infoObj, error.Split(';')))
                {
                    result = UpdateResultEnum.PostProcess;
                    return false;
                }
                else
                {
                    // Skip site binding error
                    if (parameters.ObjectProcessType == ProcessObjectEnum.SiteBinding)
                    {
                        result = UpdateResultEnum.Skipped;
                        return false;
                    }

                    // Skip the object on translation error or when the object is not required (parent is always required)
                    var ti = infoObj.TypeInfo;
                    if ((!error.Contains(ti.ParentIDColumn) && !ti.RequiredObject) || ValidationHelper.GetBoolean(settings.GetSettings(ImportExportHelper.SETTINGS_SKIP_OBJECT_ON_TRANSLATION_ERROR), false))
                    {
                        if (settings.LogObjectsWarnings)
                        {
                            LogProgress(LogStatusEnum.Warning, settings, string.Format(settings.GetAPIString("ImportSite.WarningNotRequired", "Cannot import object '{0}' due to binding errors."), error, ImportExportHelper.GetObjectTypeName(ti.ObjectType, settings)) + "(" + HTMLHelper.HTMLEncode(infoObj.ObjectCodeName) + ")");
                        }

                        result = UpdateResultEnum.Error;
                        return false;
                    }

                    throw new Exception("[ImportProvider.TranslateColumns]: Cannot translate columns '" + error + "', import the dependent objects first.");
                }
            }

            return true;
        }


        /// <summary>
        /// Processes the object data such as children, bindings, metafiles etc.
        /// </summary>
        /// <param name="settings">Import settings</param>
        /// <param name="infoObj">Imported object</param>
        /// <param name="parameters">Import parameters</param>
        /// <param name="parentIsNew">If true, the data is imported for a new parent object (no need to match to existing data)</param>
        private static bool ProcessObjectData(SiteImportSettings settings, GeneralizedInfo infoObj, ImportParameters parameters, bool parentIsNew)
        {
            var e = new ImportEventArgs
            {
                Settings = settings,
                TranslationHelper = parameters.TranslationHelper,
                Object = infoObj,
                ParentObject = parameters.ParentObject,
                Parameters = parameters
            };

            var someDataImported = false;

            // Handle the event
            using (var h = SpecialActionsEvents.ProcessObjectData.StartEvent(e))
            {
                if (h.CanContinue())
                {
                    if (parameters.ObjectProcessType == ProcessObjectEnum.All)
                    {
                        #region "Child and binding objects import"

                        // Clear binary data to free memory
                        ClearBinaryData(infoObj);

                        // Update child objects
                        if (parameters.UpdateChildObjects)
                        {
                            // Process child objects
                            someDataImported |= ProcessChildren(settings, infoObj, parameters, parentIsNew);

                            // Process bindings
                            someDataImported |= ProcessBindings(settings, infoObj, parameters, parentIsNew);

                            // Meta files
                            someDataImported |= ProcessMetaFiles(settings, infoObj, parameters, parentIsNew);
                        }

                        #endregion
                    }
                }

                h.FinishEvent();
            }

            return someDataImported;
        }


        private static bool ProcessMetaFiles(SiteImportSettings settings, GeneralizedInfo infoObj, ImportParameters parameters, bool parentIsNew)
        {
            if (infoObj.TypeInfo.HasMetaFiles)
            {
                DataTable filesDT = parameters.Data.Tables["CMS_MetaFile"];
                return MetaFileInfoProvider.UpdateMetaFiles(infoObj, filesDT, parameters.ObjectOriginalID, o => LoadBinaryData(settings, o, parameters), false, parentIsNew, settings.ImportOnlyNewObjects);
            }

            return false;
        }


        /// <summary>
        /// Processes the object bindings. Returns true if some bindings were updated.
        /// </summary>
        /// <param name="settings">Import settings</param>
        /// <param name="infoObj">Imported object</param>
        /// <param name="parameters">Import parameters</param>
        /// <param name="parentIsNew">If true, the bindings are imported for a new parent object (no need to check for existing)</param>
        private static bool ProcessBindings(SiteImportSettings settings, GeneralizedInfo infoObj, ImportParameters parameters, bool parentIsNew)
        {
            var e = new ImportEventArgs
            {
                Settings = settings,
                TranslationHelper = parameters.TranslationHelper,
                Object = infoObj,
                Parameters = parameters,
            };

            var someBindingsUpdated = false;

            // Handle the event
            using (var hBindings = ImportExportEvents.ImportBindings.StartEvent(e))
            {
                if (hBindings.CanContinue())
                {
                    // Update bindings
                    var ti = infoObj.TypeInfo;
                    var bindingTypes = ti.BindingObjectTypes;

                    foreach (string bindingType in bindingTypes)
                    {
                        GeneralizedInfo bindingObj = ModuleManager.GetObject(bindingType);
                        if (bindingObj == null)
                        {
                            // Skip missing object type
                            continue;
                        }

                        var eBindingType = new ImportObjectTypeArgs
                        {
                            Settings = settings,
                            TranslationHelper = parameters.TranslationHelper,
                            ObjectType = bindingType,
                            ParentObject = infoObj,
                            Parameters = parameters
                        };

                        // Handle the event
                        using (var hBindingType = ImportExportEvents.ImportBindingType.StartEvent(eBindingType))
                        {
                            if (hBindingType.CanContinue())
                            {
                                var bindingTypeInfo = bindingObj.TypeInfo;

                                if (bindingTypeInfo.ImportExportSettings.IncludeToExportParentDataSet != IncludeToParentEnum.None)
                                {
                                    // Check column definition
                                    if (bindingTypeInfo.ParentIDColumn == ObjectTypeInfo.COLUMN_NAME_UNKNOWN)
                                    {
                                        throw new Exception("[ImportProvider.ProcessBindings]: Parent ID column for object type '" + bindingType + "' is not defined.");
                                    }

                                    // For binding which are not site bindings
                                    if (!bindingTypeInfo.IsSiteBinding)
                                    {
                                        // Get where condition for all related bindings (use object type from TypeInfo!)
                                        var bindingColumns = bindingTypeInfo.GetTypeColumns(ti);
                                        if (bindingColumns.Count == 0)
                                        {
                                            throw new Exception("[ImportProvider.ProcessBindings]: Binding column of binding object type '" + bindingType + "' for object type '" + ti.ObjectType + "' not found.");
                                        }

                                        // Get the first found column from binding. If the binding is between the same object types, parent wins
                                        var parentColumn = bindingColumns.First();
                                        var targetColumn = bindingTypeInfo.GetBindingColumns().FirstOrDefault(col => !col.Equals(parentColumn, StringComparison.OrdinalIgnoreCase));
                                        if (String.IsNullOrEmpty(targetColumn))
                                        {
                                            throw new Exception("[ImportProvider.ProcessBindings]: Target column of binding object type '" + bindingType + "' not found.");
                                        }

                                        // Get existing bindings
                                        DataSet existingBindingDS = null;
                                        var skipBindings = new HashSet<Tuple<int, int>>();

                                        var complete =
                                            // No deletes if only new objects are imported
                                            !settings.ImportOnlyNewObjects &&
                                            // No deletes if the inclusion type is not complete (other inclusion type Incremental does not allow deletion)
                                            (bindingTypeInfo.ImportExportSettings.IncludeToExportParentDataSet == IncludeToParentEnum.Complete);

                                        var deleteOrUpdate =
                                            // No deletes if parent is new (not necessary - performance optimization)
                                            !parentIsNew &&
                                            complete;

                                        if (deleteOrUpdate)
                                        {
                                            existingBindingDS = GetExistingBindings(settings, infoObj, bindingColumns, bindingObj);
                                        }
                                        else if (settings.ImportOnlyNewObjects)
                                        {
                                            existingBindingDS = GetExistingBindings(settings, infoObj, bindingColumns, bindingObj, parentColumn + "," + targetColumn);

                                            // Create the list of existing bindings (these bindings will not be updated)
                                            DataHelper.ForEachRow(
                                                existingBindingDS,
                                                dr => skipBindings.Add(new Tuple<int, int>(ValidationHelper.GetInteger(dr[parentColumn], 0), ValidationHelper.GetInteger(dr[targetColumn], 0)))
                                                );
                                        }

                                        // Get binding objects table
                                        var bindingDT = ObjectHelper.GetTable(parameters.Data, bindingObj);

                                        // Get binding rows
                                        DataRow[] bindingRows = null;
                                        if (!DataHelper.DataSourceIsEmpty(bindingDT))
                                        {
                                            string sourceBindingWhere = GetWhereCondition(parameters.ObjectOriginalID, bindingColumns.ToArray());
                                            bindingRows = bindingDT.Select(sourceBindingWhere);
                                        }

                                        #region "Delete existing bindings"

                                        // Delete all bindings
                                        if (deleteOrUpdate)
                                        {
                                            if (!DataHelper.DataSourceIsEmpty(existingBindingDS))
                                            {
                                                foreach (DataRow dr in existingBindingDS.Tables[0].Rows)
                                                {
                                                    // Delete the binding
                                                    GeneralizedInfo binding = GetObject(dr, bindingType);
                                                    DeleteObject(settings, binding, infoObj, parameters);

                                                    someBindingsUpdated = true;
                                                }
                                            }
                                        }

                                        #endregion

                                        // Allow bulk insert only with complete inclusion (we start with no bindings as the previous process deleted them) or when only new items are inserted (bindings are pre-filtered to only new ones)
                                        // Otherwise insert the bindings the regular way one-by one
                                        var bulkInsert = settings.AllowBulkInsert && (complete || settings.ImportOnlyNewObjects);

                                        // Add current bindings
                                        if ((bindingRows != null) && (bindingRows.Length > 0))
                                        {
                                            List<BaseInfo> bulkList = null;

                                            // Ensure the list for bulk insert
                                            if (bulkInsert)
                                            {
                                                bulkList = new List<BaseInfo>();
                                            }

                                            // Pre-cache the target ID translations
                                            var targetIDs = bindingRows.Select(dr => ValidationHelper.GetInteger(dr[targetColumn], 0));
                                            var targetType = bindingTypeInfo.GetObjectTypeForColumn(targetColumn);

                                            parameters.TranslationHelper.GetNewIDs(targetType, targetIDs, settings.TranslationSiteId);

                                            // Process the bindings
                                            foreach (DataRow dr in bindingRows)
                                            {
                                                GeneralizedInfo binding = GetObject(dr, bindingType);

                                                var eBinding = new ImportEventArgs
                                                {
                                                    Settings = settings,
                                                    TranslationHelper = parameters.TranslationHelper,
                                                    Object = binding,
                                                    ParentObject = infoObj,
                                                    Parameters = parameters
                                                };

                                                // Handle the event
                                                using (var hBinding = ImportExportEvents.ImportBinding.StartEvent(eBinding))
                                                {
                                                    if (hBinding.CanContinue())
                                                    {
                                                        // Translate columns
                                                        string error = TranslateColumns(settings, binding, parameters.TranslationHelper);

                                                        // Update / insert the binding if translation is successful
                                                        if (error == "")
                                                        {
                                                            // Set new site ID
                                                            binding.ObjectSiteID = settings.SiteId;
                                                            binding.ObjectID = 0;

                                                            // Check if the binding can be imported
                                                            if (settings.ImportOnlyNewObjects)
                                                            {
                                                                var parentId = ValidationHelper.GetInteger(binding.GetValue(parentColumn), 0);
                                                                var targetId = ValidationHelper.GetInteger(binding.GetValue(targetColumn), 0);
                                                                if (skipBindings.Contains(new Tuple<int, int>(parentId, targetId)))
                                                                {
                                                                    // Do not update already existing bindings
                                                                    continue;
                                                                }
                                                            }

                                                            if (bulkInsert)
                                                            {
                                                                // Add to bulk insert list
                                                                bulkList.Add(binding);
                                                            }
                                                            else
                                                            {
                                                                // Insert immediately
                                                                SetObject(settings, binding, infoObj, parameters.ImportedObjects);

                                                                someBindingsUpdated = true;
                                                            }
                                                        }
                                                        else if (IsObjectPostProcess(binding, error.Split(';')))
                                                        {
                                                            if (parameters.PostProcessList != null)
                                                            {
                                                                // Restore original object data
                                                                binding = GetObject(dr, bindingType);

                                                                // Add to post process list
                                                                parameters.PostProcessList.Add(binding);
                                                            }
                                                        }
                                                        else
                                                        {
                                                            if (settings.LogObjectsWarnings)
                                                            {
                                                                // Log warning
                                                                LogProgress(LogStatusEnum.Warning, settings, string.Format(settings.GetAPIString("ImportSite.WarningProcessBindingObject", "Binding object of type '{0}' was not processed."), ImportExportHelper.GetObjectTypeName(bindingType, settings)) + "(" + binding.ObjectGUID + ")");
                                                            }
                                                        }
                                                    }

                                                    hBinding.FinishEvent();
                                                }
                                            }

                                            #region "Bulk insert bindings"

                                            // Bulk insert data if allowed
                                            if (bulkInsert && (bulkList.Count > 0))
                                            {
                                                var provider = bindingObj.TypeInfo.ProviderObject;

                                                provider.BulkInsertInfos(bulkList);

                                                // Add inserted bindings into the imported objects list
                                                foreach (var binding in bulkList)
                                                {
                                                    AddToImportedObjectsList(settings, binding, infoObj, parameters.ImportedObjects, false);
                                                }

                                                someBindingsUpdated = true;
                                            }

                                            #endregion
                                        }
                                    }
                                    else if (settings.SiteId > 0)
                                    {
                                        // Create site binding if the target site is set
                                        AssignToSite(settings, bindingObj, infoObj, parameters);
                                    }
                                }
                            }

                            hBindingType.FinishEvent();
                        }
                    }
                }

                hBindings.FinishEvent();
            }

            return someBindingsUpdated;
        }


        private static DataSet GetExistingBindings(SiteImportSettings settings, GeneralizedInfo infoObj, IEnumerable<string> parentColumns, GeneralizedInfo bindingObj, string columns = null)
        {
            var sourceBindingWhere = GetWhereCondition(infoObj.ObjectID, parentColumns.ToArray());
            var bindingWhere = new WhereCondition(sourceBindingWhere);

            // Some data should be selected
            if (!bindingWhere.ReturnsNoResults)
            {
                var bindingTypeInfo = bindingObj.TypeInfo;

                // Add site condition
                if (!settings.UseAutomaticSiteForTranslation && (bindingTypeInfo.ObjectDependencies != null))
                {
                    foreach (var dep in bindingTypeInfo.ObjectDependencies)
                    {
                        string depType = bindingObj.GetDependencyObjectType(dep);

                        var depObj = ModuleManager.GetReadOnlyObject(depType);

                        string siteIdColumn = depObj.TypeInfo.SiteIDColumn;
                        if (siteIdColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN)
                        {
                            var global = new Tuple<string, int?>(PredefinedObjectType.SITE, null);
                            var site = (settings.SiteId > 0) ? new Tuple<string, int?>(PredefinedObjectType.SITE, settings.SiteId) : null;

                            bindingWhere.Where(bindingTypeInfo.GetDependencyWhereCondition(depType, "OR", global, site));
                        }
                    }
                }

                return
                    bindingObj.GetDataQuery(
                        true,
                        q =>
                        {
                            q.Where(bindingWhere);
                            if (!String.IsNullOrEmpty(columns))
                            {
                                q.Columns(columns);
                            }
                        },
                        false
                    ).Result;
            }

            return null;
        }


        private static string GetWhereCondition(int id, string[] parentColumns)
        {
            if (!parentColumns.Any())
            {
                return null;
            }

            var ids = new[] { id };
            var conditions = parentColumns.Select(columnName => SqlHelper.GetWhereCondition(columnName, ids.AsEnumerable()));

            return TextHelper.Join(" OR ", conditions);
        }


        /// <summary>
        /// Processes the object children. Returns true if some child objects were updated.
        /// </summary>
        /// <param name="settings">Import settings</param>
        /// <param name="infoObj">Imported object</param>
        /// <param name="parameters">Import parameters</param>
        /// <param name="parentIsNew">If true, the children are imported for a new parent object (no need to check for existing)</param>
        private static bool ProcessChildren(SiteImportSettings settings, GeneralizedInfo infoObj, ImportParameters parameters, bool parentIsNew)
        {
            var eChildren = new ImportEventArgs
            {
                Settings = settings,
                TranslationHelper = parameters.TranslationHelper,
                Object = infoObj,
                Parameters = parameters,
            };

            var someChildrenUpdated = false;

            // Handle the event
            using (var hChildren = ImportExportEvents.ImportChildren.StartEvent(eChildren))
            {
                if (hChildren.CanContinue())
                {
                    var childTypes = infoObj.TypeInfo.ChildObjectTypes;

                    foreach (string childType in childTypes)
                    {
                        GeneralizedInfo childObj = ModuleManager.GetReadOnlyObject(childType);
                        if (childObj == null)
                        {
                            // Skip missing object type
                            continue;
                        }

                        var eChildType = new ImportObjectTypeArgs
                        {
                            Settings = settings,
                            TranslationHelper = parameters.TranslationHelper,
                            ObjectType = childType,
                            ParentObject = infoObj,
                            Parameters = parameters
                        };

                        // Handle the event
                        using (var hChildType = ImportExportEvents.ImportChildType.StartEvent(eChildType))
                        {
                            if (hChildType.CanContinue())
                            {
                                var childTypeInfo = childObj.TypeInfo;
                                if (childTypeInfo.ImportExportSettings.IncludeToExportParentDataSet != IncludeToParentEnum.None)
                                {
                                    // Check columns definition
                                    if (childTypeInfo.ParentIDColumn == ObjectTypeInfo.COLUMN_NAME_UNKNOWN)
                                    {
                                        throw new Exception("[ImportProvider.ProcessChildren]: Parent ID column for object type '" + childType + "' is not defined.");
                                    }

                                    // Get existing child objects
                                    DataSet existingChildDS = null;

                                    // Evaluate if the process is allowed to delete existing children
                                    var allowDeleteExisting =
                                        // No deletes if only new objects are imported
                                        !settings.ImportOnlyNewObjects &&
                                        // No deletes if parent is new (not necessary - performance optimization)
                                        !parentIsNew &&
                                        // No deletes if the inclusion type is not complete (other inclusion type Incremental does not allow deletion)
                                        (childTypeInfo.ImportExportSettings.IncludeToExportParentDataSet == IncludeToParentEnum.Complete);

                                    if (allowDeleteExisting)
                                    {
                                        var siteId = settings.SiteIsIncluded ? settings.SiteId : 0;
                                        var addSiteCondition = !settings.UseAutomaticSiteForTranslation;

                                        existingChildDS = ObjectHelper.GetExistingChildren(infoObj, addSiteCondition, siteId, childType).Result;
                                    }

                                    // Get child objects table
                                    var childDT = ObjectHelper.GetTable(parameters.Data, childObj);

                                    // Get child rows
                                    DataRow[] childRows = null;
                                    string sourceChildrenWhere = null;

                                    if (!DataHelper.DataSourceIsEmpty(childDT))
                                    {
                                        // Add custom where condition
                                        int parentId = infoObj.ObjectID;
                                        infoObj.ObjectID = parameters.ObjectOriginalID;

                                        var childWhere = new WhereCondition().WhereEquals(childTypeInfo.ParentIDColumn, parameters.ObjectOriginalID);

                                        sourceChildrenWhere = infoObj.GetChildWhereCondition(childWhere, childTypeInfo.ObjectType).ToString(true);

                                        infoObj.ObjectID = parentId;
                                        childRows = childDT.Select(sourceChildrenWhere);
                                    }

                                    // If some child objects are present, process
                                    if ((childRows != null) && (childRows.Length > 0))
                                    {
                                        #region "Remove existing children"

                                        // Remove existing child objects
                                        if (allowDeleteExisting)
                                        {
                                            if (!DataHelper.DataSourceIsEmpty(existingChildDS))
                                            {
                                                foreach (DataRow dr in existingChildDS.Tables[0].Rows)
                                                {
                                                    string codeNameWhere = "";

                                                    // Add GUID where
                                                    if (childTypeInfo.GUIDColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN)
                                                    {
                                                        Guid guid = ValidationHelper.GetGuid(dr[childTypeInfo.GUIDColumn], Guid.Empty);
                                                        if (guid != Guid.Empty)
                                                        {
                                                            codeNameWhere = SqlHelper.AddWhereCondition(codeNameWhere, String.Format("{0} = '{1}'", childTypeInfo.GUIDColumn, guid), "OR");
                                                        }
                                                    }

                                                    // Add code name where (secondary lookup)
                                                    if (childObj.CodeNameColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN)
                                                    {
                                                        if (childObj.CodeNameColumn != childTypeInfo.GUIDColumn)
                                                        {
                                                            string codeName = ValidationHelper.GetString(dr[childObj.CodeNameColumn], "");
                                                            codeNameWhere = SqlHelper.AddWhereCondition(codeNameWhere, String.Format("{0} = '{1}'", childObj.CodeNameColumn, SqlHelper.GetSafeQueryString(codeName, false)), "OR");
                                                        }
                                                    }

                                                    // Get existing record
                                                    DataRow[] newRows = null;

                                                    if (!String.IsNullOrEmpty(codeNameWhere))
                                                    {
                                                        string completeChildWhere = SqlHelper.AddWhereCondition(sourceChildrenWhere, codeNameWhere);
                                                        newRows = childDT.Select(completeChildWhere);
                                                    }

                                                    if ((newRows == null) || (newRows.Length == 0))
                                                    {
                                                        // Delete existing child
                                                        GeneralizedInfo child = GetObject(dr, childType);
                                                        DeleteObject(settings, child, infoObj, parameters);

                                                        someChildrenUpdated = true;
                                                    }
                                                }
                                            }
                                        }

                                        #endregion

                                        // Update child objects
                                        foreach (DataRow dr in childRows)
                                        {
                                            // Update the child object
                                            BaseInfo child = GetObject(dr, childType);

                                            var eChild = new ImportEventArgs
                                            {
                                                Settings = settings,
                                                TranslationHelper = parameters.TranslationHelper,
                                                Object = child,
                                                ParentObject = infoObj,
                                                Parameters = parameters
                                            };

                                            // Handle the event
                                            using (var hChild = ImportExportEvents.ImportChild.StartEvent(eChild))
                                            {
                                                if (hChild.CanContinue())
                                                {
                                                    // Prepare parameter for importing child object
                                                    var childParameters = new ImportParameters
                                                    {
                                                        TranslationHelper = parameters.TranslationHelper,
                                                        SiteObject = parameters.SiteObject,
                                                        Data = parameters.Data,
                                                        UpdateChildObjects = true,
                                                        ObjectProcessType = ProcessObjectEnum.All,
                                                        PostProcessList = parameters.PostProcessList,
                                                        ImportedObjects = parameters.ImportedObjects,
                                                        PostProcessing = parameters.PostProcessing,
                                                        CheckExisting = !parentIsNew,
                                                        CheckUnique = !parentIsNew,
                                                        ParentObject = infoObj,

                                                        // Propagate the SkipObjectUpdate flag for nested types
                                                        SkipObjectUpdate = IsNestedType(childTypeInfo) && parameters.SkipObjectUpdate

                                                    };

                                                    // Try to update the child object
                                                    var result = ImportObject(settings, child, childParameters, null);

                                                    someChildrenUpdated |= result.SomeDataImported;

                                                    // If there was an existing object that was updated then replace current info object
                                                    if (childParameters.CheckExisting && childParameters.ExistingObject != null)
                                                    {
                                                        hChild.EventArguments.Object = childParameters.ExistingObject;
                                                    }

                                                    switch (result.Result)
                                                    {
                                                        case UpdateResultEnum.PostProcess:
                                                            if (parameters.PostProcessList != null)
                                                            {
                                                                // Restore original object data
                                                                child = GetObject(dr, childType);

                                                                // Add to post process list
                                                                parameters.PostProcessList.Add(child);
                                                            }
                                                            break;
                                                    }
                                                }

                                                hChild.FinishEvent();
                                            }
                                        }
                                    }
                                    else if (allowDeleteExisting)
                                    {
                                        #region "Delete all children"

                                        // Delete all child objects
                                        if (!DataHelper.DataSourceIsEmpty(existingChildDS))
                                        {
                                            foreach (DataRow dr in existingChildDS.Tables[0].Rows)
                                            {
                                                // Delete the child object
                                                BaseInfo child = GetObject(dr, childType);
                                                DeleteObject(settings, child, infoObj, parameters);
                                                someChildrenUpdated = true;
                                            }
                                        }

                                        #endregion
                                    }
                                }
                            }

                            hChildType.FinishEvent();
                        }
                    }
                }

                hChildren.FinishEvent();
            }

            return someChildrenUpdated;
        }


        private static string TranslateColumns(SiteImportSettings settings, GeneralizedInfo infoObj, TranslationHelper th, bool siteId = false, bool parentId = true, bool dependencies = true)
        {
            th.SetDefaultValue(PredefinedObjectType.USER, settings.AdministratorId);
            string error = th.TranslateColumns(infoObj, siteId, parentId, dependencies, settings.TranslationSiteId, null);
            th.RemoveDefaultValue(PredefinedObjectType.USER);

            return error;
        }


        private static void ClearBinaryData(GeneralizedInfo infoObj)
        {
            string binaryColumn = infoObj.TypeInfo.BinaryColumn;
            if (binaryColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN)
            {
                infoObj.SetValue(binaryColumn, null);
            }
        }


        /// <summary>
        /// Processes the main object data. Returns true if some data was updated
        /// </summary>
        /// <param name="settings">Import settings</param>
        /// <param name="infoObj">Object to import</param>
        /// <param name="parameters">Import parameters</param>
        private static bool ProcessMainObject(SiteImportSettings settings, GeneralizedInfo infoObj, ImportParameters parameters)
        {
            var e = new ImportEventArgs
            {
                Settings = settings,
                TranslationHelper = parameters.TranslationHelper,
                Object = infoObj,
                ParentObject = parameters.ParentObject,
                Parameters = parameters
            };

            var someDataUpdated = false;

            // Handle the event
            using (var h = SpecialActionsEvents.ProcessMainObject.StartEvent(e))
            {
                if (h.CanContinue())
                {
                    if (!parameters.SkipObjectUpdate && (parameters.ObjectProcessType == ProcessObjectEnum.All))
                    {
                        // Get binary data from disk
                        LoadBinaryData(settings, infoObj, parameters);

                        // Update the object to the database
                        infoObj.MakeComplete(false);

                        // Disable validation
                        infoObj.ValidateCodeName = false;
                        infoObj.CheckUnique = false;

                        if (parameters.ExistingObject != null)
                        {
                            using (CMSActionContext context = new CMSActionContext())
                            {
                                // Ensure version with original data for versioned object
                                context.CreateVersion = settings.CreateVersion;
                                SynchronizationHelper.EnsureObjectVersion(infoObj);
                            }
                        }

                        // Sign again all macros in objects.
                        var ti = infoObj.TypeInfo;

                        if (settings.RefreshMacroSecurity)
                        {
                            try
                            {
                                var identityOption = MacroIdentityOption.FromUserInfo(settings.MacroSecurityUser);
                                MacroSecurityProcessor.RefreshSecurityParameters(infoObj, identityOption, false);
                            }
                            catch (Exception ex)
                            {
                                string message = "Signing " + TypeHelper.GetNiceObjectTypeName(ti.ObjectType) + " " + infoObj.ObjectDisplayName + " failed: " + ex.Message;
                                EventLogProvider.LogEvent(EventType.ERROR, "Import", "MACROSECURITY", message);
                            }
                        }

                        // Force the IDPath to be regenerated on the target in all cases
                        if (ti.ObjectIDPathColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN)
                        {
                            infoObj.SetValue(ti.ObjectIDPathColumn, null);
                        }

                        // Set whether the object should check uniqueness of code name (when not checked, does not need transaction while saving)
                        bool originalCheckUnique = infoObj.CheckUnique;
                        try
                        {
                            infoObj.CheckUnique = parameters.CheckUnique;

                            // Save to the database
                            SetObject(settings, infoObj, parameters.ParentObject, parameters.ImportedObjects);
                            settings.ImportedObjects.Add(infoObj);
                        }
                        finally
                        {
                            infoObj.CheckUnique = originalCheckUnique;
                        }

                        // Re-enable validation
                        infoObj.ValidateCodeName = true;
                        infoObj.CheckUnique = true;

                        // Add ID translation
                        if (ti.IDColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN)
                        {
                            parameters.TranslationHelper.AddIDTranslation(ti.ObjectType, parameters.ObjectOriginalID, infoObj.ObjectID, infoObj.ObjectGroupID);
                        }

                        someDataUpdated = true;
                    }
                }

                h.FinishEvent();
            }

            return someDataUpdated;
        }


        private static void CreateObjectVersion(SiteImportSettings settings, GeneralizedInfo infoObj, GeneralizedInfo existing, ProcessObjectEnum process)
        {
            if (process != ProcessObjectEnum.SiteBinding)
            {
                // Only for existing objects
                if (existing != null)
                {
                    using (CMSActionContext context = new CMSActionContext())
                    {
                        context.CreateVersion = settings.CreateVersion;
                        if (SynchronizationHelper.CheckCreateVersion(infoObj, TaskTypeEnum.UpdateObject))
                        {
                            ObjectVersionManager.CreateVersion(infoObj, settings.CurrentUser.UserID);
                        }
                    }
                }
            }
        }


        /// <summary>
        /// Assign object to site.
        /// </summary>
        /// <param name="settings">Site import settings</param>
        /// <param name="bindingObj">Binding object</param>
        /// <param name="infoObj">Object to assign</param>
        /// <param name="parameters">Import parameters</param>
        private static void AssignToSite(SiteImportSettings settings, GeneralizedInfo bindingObj, GeneralizedInfo infoObj, ImportParameters parameters)
        {
            var bindingTable = ObjectHelper.GetTable(parameters.Data, bindingObj);
            if (bindingTable != null)
            {
                var bindingTypeInfo = bindingObj.TypeInfo;

                var bindingDR = bindingTable.Select(bindingTypeInfo.ParentIDColumn + " = " + parameters.ObjectOriginalID);
                if (bindingDR.Length > 0)
                {
                    string bindingType = bindingTypeInfo.ObjectType;
                    GeneralizedInfo binding = GetObject(bindingDR[0], bindingType);
                    binding.ObjectID = 0;
                    binding.ObjectSiteID = settings.SiteId;
                    binding.ObjectParentID = infoObj.ObjectID;

                    // Update the binding
                    string error = TranslateColumns(settings, binding, parameters.TranslationHelper, false, false);

                    // Update/Insert the binding if translation is successful
                    if (error == "")
                    {
                        // Update / insert the site binding
                        SetObject(settings, binding, infoObj, parameters.ImportedObjects);
                    }
                    else
                    {
                        if (settings.LogObjectsWarnings)
                        {
                            // Log warning
                            LogProgress(LogStatusEnum.Warning, settings, string.Format(settings.GetAPIString("ImportSite.WarningProcessBindingObject", "Binding object of type '{0}' was not processed."), ImportExportHelper.GetObjectTypeName(bindingType, settings)) + "(" + binding.ObjectGUID + ")");
                        }
                    }
                }
            }
        }


        /// <summary>
        /// Sets imported object
        /// </summary>
        /// <param name="settings">Import settings</param>
        /// <param name="infoObj">Object instance</param>
        /// <param name="parentInfo">Parent object</param>
        /// <param name="importedObjects">List of already imported objects</param>
        private static void SetObject(SiteImportSettings settings, GeneralizedInfo infoObj, GeneralizedInfo parentInfo, List<ImportedObject> importedObjects)
        {
            infoObj.SetObject();

            AddToImportedObjectsList(settings, infoObj, parentInfo, importedObjects, false);
        }


        private static void AddToImportedObjectsList(SiteImportSettings settings, GeneralizedInfo infoObj, GeneralizedInfo parentInfo, List<ImportedObject> importedObjects, bool delete)
        {
            if (!settings.LogIntegration && !settings.LogSynchronization)
            {
                return;
            }

            if (infoObj.TypeInfo.IsSiteBinding)
            {
                if (parentInfo != null)
                {
                    importedObjects.Add(new ImportedObject(parentInfo, delete ? TaskTypeEnum.RemoveFromSite : TaskTypeEnum.AddToSite));
                }
            }
            else
            {
                importedObjects.Add(new ImportedObject(infoObj, delete ? TaskTypeEnum.DeleteObject : TaskTypeEnum.UpdateObject));
            }
        }


        private static void DeleteObject(SiteImportSettings settings, GeneralizedInfo infoObj, GeneralizedInfo parentInfo, ImportParameters parameters)
        {
            AddToImportedObjectsList(settings, infoObj, parentInfo, parameters.ImportedObjects, true);

            infoObj.DeleteObject();
        }


        /// <summary>
        /// Indicates if parent for given object was imported.
        /// </summary>
        /// <param name="infoObj">Info object</param>
        /// <param name="importedParentIds">Parent object IDs</param>
        private static bool ParentImported(GeneralizedInfo infoObj, List<int> importedParentIds)
        {
            if (importedParentIds != null)
            {
                int parentId = infoObj.ObjectParentID;
                if (parentId > 0)
                {
                    return importedParentIds.Contains(parentId);
                }
            }
            return true;
        }


        private static void LoadBinaryData(SiteImportSettings settings, GeneralizedInfo infoObj, ImportParameters parameters)
        {
            var e = new GetBinaryDataSourcePathEventArgs
            {
                Object = infoObj,
                ParentObject = parameters.ParentObject,
                Parameters = parameters,
                Settings = settings,
                TranslationHelper = parameters.TranslationHelper
            };

            // Handle the event
            SpecialActionsEvents.GetBinaryDataSourcePath.StartEvent(e);

            string sourcePath = e.Path;
            if (!string.IsNullOrEmpty(sourcePath))
            {
                // Get the data
                byte[] binaryData = null;
                if (File.Exists(sourcePath))
                {
                    binaryData = File.ReadAllBytes(sourcePath);
                }

                // Save the data to the object
                if (binaryData != null)
                {
                    infoObj.SetValue(infoObj.TypeInfo.BinaryColumn, binaryData);
                }
            }
        }


        /// <summary>
        /// Gets source path for binary data
        /// </summary>
        /// <param name="settings">Site import settings</param>
        /// <param name="infoObj">Info object instance</param>
        /// <param name="folderName">Binary data folder name</param>
        /// <param name="fileName">File GUID</param>
        /// <param name="extension">File extension</param>
        public static string GetBinaryDataSourcePath(SiteImportSettings settings, GeneralizedInfo infoObj, string folderName, string fileName, string extension)
        {
            string sourcePath = DirectoryHelper.CombinePath(settings.TemporaryFilesPath, ImportExportHelper.DATA_FOLDER, ImportExportHelper.FILES_FOLDER) + string.Format("\\{0}\\", folderName);
            sourcePath += DirectoryHelper.CombinePath(fileName.Substring(0, 2), fileName) + extension;
            return ImportExportHelper.GetExportFilePath(sourcePath);
        }


        /// <summary>
        /// Adds the object files to the import files list.
        /// </summary>
        /// <param name="settings">Import settings</param>
        /// <param name="ds">DataSet with the objects data</param>
        /// <param name="objectType">Object type</param>
        /// <param name="siteObject">Indicates if object is site object</param>
        /// <param name="th">Translation helper</param>
        /// <param name="parentId">Parent object ID</param>
        /// <param name="parentCodeName">Code name of the parent object</param>
        public static void AddImportFiles(SiteImportSettings settings, DataSet ds, string objectType, bool siteObject, TranslationHelper th, int parentId = 0, string parentCodeName = null)
        {
            // Import process canceled
            if (settings.ProcessCanceled)
            {
                ImportCanceled();
            }

            try
            {
                AddComponentFiles(settings, ds, objectType, siteObject);
                GetAssemblyFiles(settings, ds, objectType, siteObject);
            }
            catch (ProcessCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                // Log error
                LogProgressError(settings, string.Format(settings.GetAPIString("ImportSite.ErrorPreparingObjectFilesList", "Error preparing objects '{0}' files copy list"), ImportExportHelper.GetObjectTypeName(objectType, settings)), ex);
                throw;
            }
        }


        private static void GetAssemblyFiles(SiteImportSettings settings, DataSet data, string objectType, bool siteObject)
        {
            if (!ValidationHelper.GetBoolean(settings.GetSettings(ImportExportHelper.SETTINGS_ASSEMBLIES), false))
            {
                return;
            }

            string safeObjectType = ImportExportHelper.GetSafeObjectTypeName(objectType);

            // Reset path
            string sourcePath = DirectoryHelper.CombinePath(settings.TemporaryFilesPath, ImportExportHelper.DATA_FOLDER, ImportExportHelper.FILES_FOLDER) + "\\";

            // Add target path for site (testing and conversion)
            string targetPath = GetTargetPath(settings, settings.WebsitePath);

            // Get object info
            BaseInfo infoObj = ModuleManager.GetReadOnlyObject(objectType);
            string assemblyColumn = infoObj.TypeInfo.AssemblyNameColumn;

            if (string.IsNullOrEmpty(assemblyColumn))
            {
                return;
            }

            if (data == null)
            {
                return;
            }

            // Get table
            string codeNameColumn = infoObj.Generalized.CodeNameColumn;

            var dt = ObjectHelper.GetTable(data, infoObj);
            if (DataHelper.DataSourceIsEmpty(dt))
            {
                return;
            }

            sourcePath += safeObjectType + "\\";
            sourcePath += @"bin\";

            foreach (DataRow dr in dt.Rows)
            {
                // Import process canceled
                if (settings.ProcessCanceled)
                {
                    ImportCanceled();
                }

                string objectName = dr[codeNameColumn].ToString();
                string assemblyName = ValidationHelper.GetString(dr[assemblyColumn], null);

                // Import assembly
                ImportAssembly(settings, objectType, siteObject, sourcePath, targetPath, objectName, assemblyName);
            }
        }


        private static void AddComponentFiles(SiteImportSettings settings, DataSet ds, string objectType, bool siteObject)
        {
            switch (objectType)
            {
                // Skins' folders
                case PredefinedObjectType.DOCUMENTTYPE:
                case PredefinedObjectType.CUSTOMTABLECLASS:
                case DataClassInfo.OBJECT_TYPE_SYSTEMTABLE:
                    // Class + Transformations
                    AddComponentFiles(objectType, settings, siteObject, ds, "Transformations");
                    break;
            }
        }


        /// <summary>
        /// Adds object component files for processing
        /// </summary>
        /// <param name="objectType">Object type</param>
        /// <param name="settings">Site import settings</param>
        /// <param name="siteObject">Indicates if site object is being processed</param>
        /// <param name="data">Object data</param>
        /// <param name="componentFolder">Name of the component folder</param>
        public static void AddComponentFiles(string objectType, SiteImportSettings settings, bool siteObject, DataSet data, string componentFolder)
        {
            if (data == null)
            {
                return;
            }

            // Prepare the paths
            string sourcePath = DirectoryHelper.CombinePath(settings.TemporaryFilesPath, ImportExportHelper.DATA_FOLDER, ImportExportHelper.FILES_FOLDER) + "\\" + Path.Combine(ImportExportHelper.GetSafeObjectTypeName(objectType), ImportExportHelper.SRC_SKINS_FOLDER) + string.Format("\\Components\\{0}\\", componentFolder);
            string targetPath = GetTargetPath(settings, settings.WebsitePath) + ImportExportHelper.SRC_SKINS_FOLDER + string.Format("\\Components\\{0}\\", componentFolder);

            var infoObj = ModuleManager.GetReadOnlyObject(objectType);

            var dt = ObjectHelper.GetTable(data, infoObj);
            if (DataHelper.DataSourceIsEmpty(dt))
            {
                return;
            }

            foreach (DataRow dr in dt.Rows)
            {
                // Import process canceled
                if (settings.ProcessCanceled)
                {
                    ImportCanceled();
                }

                // Prepare the data
                var nameColumn = infoObj.Generalized.CodeNameColumn;
                string objectName = dr[nameColumn].ToString();

                // Copy files if object is processed
                if (!settings.IsProcessed(objectType, objectName, siteObject))
                {
                    continue;
                }

                objectName = ValidationHelper.GetSafeFileName(objectName);
                string targetObjectPath = Path.Combine(targetPath, objectName);
                string sourceObjectPath = Path.Combine(sourcePath, objectName);

                settings.FileOperations.Add(objectType, sourceObjectPath, targetObjectPath, FileOperationEnum.CopyDirectory);
            }
        }


        /// <summary>
        /// Imports assembly file.
        /// </summary>
        /// <param name="settings">Import settings object</param>
        /// <param name="objectType">Object type the assembly belongs to</param>
        /// <param name="siteObject">Indicates if object is site object</param>
        /// <param name="sourcePath">Source path</param>
        /// <param name="targetPath">Destination path</param>
        /// <param name="objectName">Code name of an object the assembly belongs to</param>
        /// <param name="assemblyName">Name of the assembly</param>
        public static void ImportAssembly(SiteImportSettings settings, string objectType, bool siteObject, string sourcePath, string targetPath, string objectName, string assemblyName)
        {
            // Do not handle App_Code and system DLLs
            if (!string.IsNullOrEmpty(assemblyName) && !assemblyName.Equals(ClassHelper.ASSEMBLY_APPCODE, StringComparison.OrdinalIgnoreCase) && !assemblyName.StartsWith("cms.", StringComparison.OrdinalIgnoreCase))
            {
                string fileName = assemblyName + ".dll";
                string sourceFilePath = sourcePath + fileName;

                // Copy files if object is processed and file is in the package
                if (File.Exists(ImportExportHelper.GetExportFilePath(sourceFilePath)) && settings.IsProcessed(objectType, objectName, siteObject))
                {
                    string destPath = targetPath + @"bin\";

                    // Check full version of the system in comparison of the package version for default system assemblies
                    bool versionMatch = (settings.FullVersion == CMSVersion.GetVersion(true, true, true, true, true));
                    if (!versionMatch)
                    {
                        // Do not combine path with temporary folder, it is already included
                        if (!UseSeparateFolder(settings))
                        {
                            // Copy DLL files to the separate folder
                            destPath = GetTargetPath(settings, settings.WebsitePath, true) + @"bin\";
                        }
                    }

                    // Copy assembly
                    string targetFilePath = destPath + fileName;
                    settings.FileOperations.Add(objectType, sourceFilePath, targetFilePath, FileOperationEnum.CopyFile, FileOperationParamaterTypeEnum.Assembly, string.Join("|", versionMatch.ToString(), destPath, GetTargetPath(settings, settings.WebsitePath, false) + @"bin\"));
                }
            }
        }


        /// <summary>
        /// Adds object source files for processing (including code behind files, designer files)
        /// </summary>
        /// <param name="objectType">Object type</param>
        /// <param name="settings">Site import settings</param>
        /// <param name="sourceObjectPath">Source object file path</param>
        /// <param name="targetObjectPath">Target object file path</param>
        public static void AddSourceFiles(string objectType, SiteImportSettings settings, string sourceObjectPath, string targetObjectPath)
        {
            settings.FileOperations.Add(objectType, sourceObjectPath, targetObjectPath, FileOperationEnum.CopyFile);
            foreach (string suffix in ImportExportHelper.LangSuffixes)
            {
                settings.FileOperations.Add(objectType, sourceObjectPath + suffix, targetObjectPath + suffix, FileOperationEnum.CopyFile);
            }

            // Process designer files only for web application
            if (settings.IsWebApplication && SystemContext.IsWebApplicationProject)
            {
                foreach (string suffix in ImportExportHelper.LangSuffixes)
                {
                    settings.FileOperations.Add(objectType, sourceObjectPath + ".designer" + suffix, targetObjectPath + ".designer" + suffix, FileOperationEnum.CopyFile);
                }
            }
        }


        /// <summary>
        /// Indicates if separate folder should be used for import files
        /// </summary>
        /// <param name="settings">Site import settings</param>
        private static bool UseSeparateFolder(SiteImportSettings settings)
        {
            return (settings.IsWebApplication && !SystemContext.IsWebApplicationProject) || (!settings.IsWebApplication && SystemContext.IsWebApplicationProject);
        }


        /// <summary>
        /// Gets real target path
        /// </summary>
        /// <param name="settings">Import settings</param>
        /// <param name="originalPath">Original target path</param>
        public static string GetTargetPath(SiteImportSettings settings, string originalPath)
        {
            return GetTargetPath(settings, originalPath, UseSeparateFolder(settings));
        }


        /// <summary>
        /// Gets real target path
        /// </summary>
        /// <param name="settings">Import settings</param>
        /// <param name="originalPath">Original target path</param>
        /// <param name="includeSeparateFolder">Indicates if path should be combined with separate folder</param>
        public static string GetTargetPath(SiteImportSettings settings, string originalPath, bool includeSeparateFolder)
        {
            // Testing folder
            originalPath = DirectoryHelper.CombinePath(originalPath, ImportExportHelper.ImportWebSiteDirectory);

            // Include separate folder
            if (includeSeparateFolder)
            {
                originalPath = DirectoryHelper.CombinePath(ImportExportHelper.GetTemporaryFolder(originalPath), ImportExportHelper.FILES_FOLDER);
            }

            return Path.EnsureEndBackslash(originalPath);
        }


        /// <summary>
        /// Adds the global folders to import files list.
        /// </summary>
        /// <param name="settings">Import settings</param>
        public static void AddGlobalFolders(SiteImportSettings settings)
        {
            try
            {
                string webSitePath = settings.WebsitePath;

                // Add target path for site (testing and conversion)
                webSitePath = GetTargetPath(settings, webSitePath);

                string targetPath = webSitePath;
                string sourcePath = DirectoryHelper.CombinePath(settings.TemporaryFilesPath, ImportExportHelper.DATA_FOLDER, ImportExportHelper.FILES_FOLDER, ImportExportHelper.GLOBAL_FOLDER) + "\\";

                // Copy folder
                if (Directory.Exists(sourcePath))
                {
                    settings.FileOperations.Add(null, sourcePath, targetPath, FileOperationEnum.CopyDirectory);
                }
            }
            catch (ProcessCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                // Log exception
                LogProgressError(settings, settings.GetAPIString("ImportSite.ErrorPreparingGlobalFoldersList", "Error preparing global folders copy list"), ex);
                throw;
            }
        }


        /// <summary>
        /// Adds the site folders to import files list.
        /// </summary>
        /// <param name="settings">Import settings</param>
        public static void AddSiteFolders(SiteImportSettings settings)
        {
            // Proceed only if site name is set
            if (settings.SiteName != null)
            {
                try
                {
                    // Initialize sub paths
                    List<string> subPaths = new List<string>();

                    subPaths.Add("Controllers\\" + ImportExportHelper.SITE_MACRO + "\\");
                    subPaths.Add("Models\\" + ImportExportHelper.SITE_MACRO + "\\");
                    subPaths.Add("Views\\" + ImportExportHelper.SITE_MACRO + "\\");

                    subPaths.Add(settings.AppCodeFolder + "\\" + ImportExportHelper.SITE_MACRO + "\\");
                    subPaths.Add("App_Data\\" + ImportExportHelper.SITE_MACRO + "\\");

                    subPaths.Add(ImportExportHelper.SITE_MACRO + "\\");

                    string webSitePath = settings.WebsitePath;

                    // Add target path for site (testing and conversion)
                    webSitePath = GetTargetPath(settings, webSitePath);

                    string sourcePath = DirectoryHelper.CombinePath(settings.TemporaryFilesPath, ImportExportHelper.DATA_FOLDER, ImportExportHelper.FILES_FOLDER, ImportExportHelper.SITE_FOLDER) + "\\";

                    // Copy all folders
                    foreach (string subPath in subPaths)
                    {
                        // Import process canceled
                        if (settings.ProcessCanceled)
                        {
                            ImportCanceled();
                        }

                        string targetPath = webSitePath + subPath.Replace(ImportExportHelper.SITE_MACRO, settings.SiteName);

                        if (Directory.Exists(sourcePath + subPath))
                        {
                            settings.FileOperations.Add(null, sourcePath + subPath, targetPath, FileOperationEnum.CopyDirectory);
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
                    LogProgressError(settings, settings.GetAPIString("ExportSite.ErrorPreparingSiteFoldersList", "Error preparing site folders copy list"), ex);
                    throw;
                }
            }
        }


        /// <summary>
        /// Gets all possible packages from import folder.
        /// </summary>
        /// <returns>Array of file names</returns>
        public static List<string> GetImportFilesList()
        {
            return GetImportFilesList(null);
        }


        /// <summary>
        /// Gets all possible packages from import folder.
        /// </summary>
        /// <returns>Array of file names</returns>
        public static List<string> GetImportFilesList(string folder)
        {
            string path;
            if (folder == null)
            {
                path = ImportExportHelper.GetSiteUtilsFolder() + "Import";
            }
            else
            {
                path = HttpContext.Current.Server.MapPath(folder);
            }

            var records = new List<string>();

            // If there is context
            if (HttpContext.Current != null)
            {
                // If import path exists
                if (Directory.Exists(path))
                {
                    // Get import templates list
                    var dirs = Directory.GetDirectories(path);
                    records.AddRange(dirs.Select(t => t.Substring(t.LastIndexOf("\\", StringComparison.Ordinal) + 1)));

                    // Get import packages list
                    var files = Directory.GetFiles(path, "*.zip");
                    records.AddRange(files.Select(t => t.Substring(t.LastIndexOf("\\", StringComparison.Ordinal) + 1)));
                }
            }

            return records;
        }


        /// <summary>
        /// Import canceled.
        /// </summary>
        public static void ImportCanceled()
        {
            throw new ProcessCanceledException();
        }


        /// <summary>
        /// Import site from the file.
        /// </summary>
        /// <param name="siteName">Site name</param>
        /// <param name="siteDisplayName">Site display name</param>
        /// <param name="siteDomain">Site domain</param>
        /// <param name="fullSourcePath">Full path to the source file</param>
        /// <param name="websitePath">Path to the web site root(optional)</param>
        /// <param name="currentUser">Current user info</param>
        public static void ImportSite(string siteName, string siteDisplayName, string siteDomain, string fullSourcePath, string websitePath, IUserInfo currentUser)
        {
            bool siteIsFirst = !DatabaseHelper.IsDatabaseAvailable || (SiteInfoProvider.GetSitesCount() == 0);

            // Initialize import settings
            SiteImportSettings settings = new SiteImportSettings(currentUser);
            settings.ImportType = (siteIsFirst ? ImportTypeEnum.AllNonConflicting : ImportTypeEnum.New);
            settings.SiteName = siteName;
            settings.SiteDisplayName = siteDisplayName;
            settings.SiteDomain = siteDomain;
            settings.SourceFilePath = fullSourcePath;
            settings.WebsitePath = websitePath;

            // Load default selection
            settings.LoadDefaultSelection();

            // Import site
            ImportObjectsData(settings);

            // Delete temporary files
            DeleteTemporaryFiles(settings, false);
        }


        /// <summary>
        /// Gets object process type
        /// </summary>
        /// <param name="settings">Site import settings</param>
        /// <param name="infoObj">Object</param>
        /// <param name="siteObject">Site object</param>
        /// <param name="processType">Process type</param>
        private static ProcessObjectEnum GetObjectProcessType(SiteImportSettings settings, GeneralizedInfo infoObj, bool siteObject, ProcessObjectEnum processType)
        {
            string objectType = infoObj.TypeInfo.ObjectType;
            if (processType == ProcessObjectEnum.Default)
            {
                processType = settings.GetObjectsProcessType(objectType, siteObject);
            }

            if (processType == ProcessObjectEnum.Selected)
            {
                // Check if selected
                string codeName = infoObj.ObjectCodeName;
                string guid = infoObj.ObjectGUID.ToString();
                if (!settings.IsSelected(objectType, codeName, siteObject) && !settings.IsSelected(objectType, guid, siteObject))
                {
                    // Do not import if not selected or add site binding
                    var addSiteBindings = ValidationHelper.GetBoolean(settings.GetSettings(ImportExportHelper.SETTINGS_ADD_SITE_BINDINGS), true);

                    processType = addSiteBindings ? ProcessObjectEnum.SiteBinding : ProcessObjectEnum.None;
                }
                else
                {
                    // Overwrite existing (full update)
                    processType = ProcessObjectEnum.All;
                }
            }

            return processType;
        }


        /// <summary>
        /// Indicates if the object supports post processing
        /// </summary>
        /// <param name="infoObj">Info object</param>
        /// <param name="failedTranslationColumnNames">Collection of column names whose values could not be translated.</param>
        private static bool IsObjectPostProcess(BaseInfo infoObj, IEnumerable<string> failedTranslationColumnNames)
        {
            return SynchronizationHelper.IsPostProcessingAllowedForFailedTranslation(infoObj.TypeInfo, failedTranslationColumnNames);
        }


        /// <summary>
        /// Indicates whether the given ObjectTypeInfo represents a nested type.
        /// </summary>
        /// <param name="type">Object typeInfo</param>
        private static bool IsNestedType(ObjectTypeInfo type)
        {
            if ((type == null) || (type.ParentTypeInfo == null) || (type.ParentTypeInfo.NestedInfoTypes == null))
            {
                return false;
            }

            return type.ParentTypeInfo.NestedInfoTypes.Contains(type.ObjectType);
        }

        #endregion
    }
}
