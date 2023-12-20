using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO.Compression;
using System.Xml;

using CMS.Base;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.IO;
using CMS.SiteProvider;

using SystemIO = System.IO;

namespace CMS.CMSImportExport
{
    /// <summary>
    /// Class for exporting the objects and sites.
    /// </summary>
    public static class ExportProvider
    {
        #region "Variables"

        private static string mExcludedFolders;
        private static string mExcludedFiles;

        #endregion


        #region "Properties"

        /// <summary>
        /// Excluded folders which are not included into the export package.
        /// </summary>
        public static string ExcludedFolders
        {
            get
            {
                return mExcludedFolders ?? (mExcludedFolders = ValidationHelper.GetString(SettingsHelper.AppSettings["CMSExportExcludedFolders"], ".svn"));
            }
            set
            {
                mExcludedFolders = value;
            }
        }


        /// <summary>
        /// Excluded files which are not included into the export package.
        /// </summary>
        public static string ExcludedFiles
        {
            get
            {
                return mExcludedFiles ?? (mExcludedFiles = ValidationHelper.GetString(SettingsHelper.AppSettings["CMSExportExcludedFiles"], "*.scc"));
            }
            set
            {
                mExcludedFiles = value;
            }
        }

        #endregion


        #region "Events and delegates"

        /// <summary>
        /// Progress log event handler.
        /// </summary>
        public delegate void OnProgressLogEventHandler(string message);


        /// <summary>
        /// Progress log event.
        /// </summary>
        public static event OnProgressLogEventHandler OnProgressLog;

        #endregion


        #region "Methods"


        #region "Progress log"

        /// <summary>
        /// Logs the export progress.
        /// </summary>
        /// <param name="message">Progress message</param>
        public static void LogProgress(string message)
        {
            if (OnProgressLog != null)
            {
                OnProgressLog(message);
            }
        }


        /// <summary>
        /// Export canceled.
        /// </summary>
        public static void ExportCanceled()
        {
            throw new ProcessCanceledException();
        }

        #endregion


        #region "File management"

        /// <summary>
        /// Remove readonly atrribute for the file.
        /// </summary>
        /// <param name="filePath">File full path</param>
        private static void UnsetReadonlyAttribute(string filePath)
        {
            FileInfo fi = FileInfo.New(filePath);
            fi.IsReadOnly = false;
            fi.Attributes = FileAttributes.Temporary;

            File.SetAttributes(filePath, FileAttributes.Temporary);
        }


        /// <summary>
        /// Unset files readonly attribute in specified folder.
        /// </summary>
        /// <param name="folder">Folder</param>
        private static void UnsetReadonlyAttributesDirectory(string folder)
        {
            if (Directory.Exists(folder))
            {
                DirectoryInfo sourceFolder = DirectoryInfo.New(folder);
                // For all subfolders
                foreach (DirectoryInfo subFolder in sourceFolder.GetDirectories())
                {
                    UnsetReadonlyAttributesDirectory(folder + @"\" + subFolder.Name);
                }
                // For all files
                foreach (FileInfo sourceFile in sourceFolder.GetFiles())
                {
                    UnsetReadonlyAttribute(folder + @"\" + sourceFile.Name);
                }
            }
        }


        /// <summary>
        /// Delete temporary files in default folder.
        /// </summary>
        public static void DeleteTemporaryFiles()
        {
            // Try to delete all temporary directories, files
            if (CMSHttpContext.Current != null)
            {
                string path = ImportExportHelper.GetTemporaryFolder(null);
                try
                {
                    if (Directory.Exists(path))
                    {
                        UnsetReadonlyAttributesDirectory(path);
                        DirectoryHelper.DeleteDirectory(path, true);
                    }
                }
                catch (Exception ex)
                {
                    string message = String.Format(ResHelper.GetAPIString("ExportSite.ErrorTmpFilesDeletion", "Error during temporary files deletion. Please delete the folder {0} manually."), path);
                    throw new Exception(message, ex);
                }
            }
        }


        /// <summary>
        /// Delete temporary files.
        /// </summary>
        /// <param name="settings">Export settings</param>        
        /// <param name="onlyFolderStructure">Indicates if only folder structure should be deleted</param>        
        public static void DeleteTemporaryFiles(SiteExportSettings settings, bool onlyFolderStructure)
        {
            string path = settings.TemporaryFilesPath;
            try
            {
                if (Directory.Exists(path))
                {
                    UnsetReadonlyAttributesDirectory(path);
                    if (onlyFolderStructure)
                    {
                        DeleteTemporaryFiles(path);
                    }
                    else
                    {
                        DirectoryHelper.DeleteDirectory(path, true);
                    }
                }
            }
            catch (Exception ex)
            {
                string message = string.Format(settings.GetAPIString("ExportSite.ErrorTmpFilesDeletion", "Error during temporary files deletion. Please delete the folder {0} manually."), path);
                throw new Exception(message, ex);
            }
        }


        /// <summary>
        /// Recursively delete all files in specified path.
        /// </summary>
        /// <param name="path">Path of the temporary files</param>
        private static void DeleteTemporaryFiles(string path)
        {
            DirectoryHelper.DeleteDirectoryStructure(path);
        }

        #endregion


        #region "Methods for EventLog"

        /// <summary>
        /// Logs progress.
        /// </summary>
        /// <param name="type">Type of the information</param>
        /// <param name="settings">Export settings</param>
        /// <param name="description">Log message</param>
        public static void LogProgress(LogStatusEnum type, SiteExportSettings settings, string description)
        {
            // Log progress state
            settings.LogProgressState(type, description);
            LogProgress(description);
        }


        /// <summary>
        /// Logs progress error.
        /// </summary>
        /// <param name="settings">Export settings</param>
        /// <param name="description">Error description</param>
        /// <param name="ex">Exception to log</param>
        public static void LogProgressError(SiteExportSettings settings, string description, Exception ex)
        {
            string message = ImportExportHelper.GetFormattedErrorMessage(description, ex);

            // Update export state
            LogProgress(LogStatusEnum.Error, settings, message);
        }

        #endregion


        #region "General export methods"

        /// <summary>
        /// Gets the export data for specified objects.
        /// </summary>
        /// <param name="settings">Export settings</param>
        /// <param name="where">Where condition for the objects</param>
        /// <param name="objectType">Object type</param>
        /// <param name="childData">Indicates if child data should be included</param>
        /// <param name="selectionOnly">If true, the method gets only the data needed for the selection</param>
        /// <param name="th">If set, data translation table is initialized</param>
        public static DataSet GetExportData(SiteExportSettings settings, WhereCondition where, string objectType, bool childData, bool selectionOnly, TranslationHelper th)
        {
            DataSet data;

            var e = new ExportGetDataEventArgs
            {
                Settings = settings,
                Where = where,
                ObjectType = objectType,
                ChildData = childData,
                SelectionOnly = selectionOnly,
                TranslationHelper = th
            };

            // Handle the event
            using (var h = ImportExportEvents.GetExportData.StartEvent(e))
            {
                where = e.Where;

                if (h.CanContinue() && (h.EventArguments.Data == null) && !where.ReturnsNoResults)
                {
                    // Get object type info
                    GeneralizedInfo infoObj = ModuleManager.GetReadOnlyObject(objectType);
                    if (infoObj == null)
                    {
                        throw new Exception("[ExportProvider.GetExportData]: Object type '" + objectType + "' not found.");
                    }

                    // Object type is child of an object
                    var ti = infoObj.TypeInfo;
                    if (ti.ParentIDColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN)
                    {
                        // Get parent where condition
                        GeneralizedInfo parentInfo = ModuleManager.GetReadOnlyObject(infoObj.ParentObjectType);
                        if (parentInfo != null)
                        {
                            string[] codeNames = settings.GetSelectedObjectsArray(infoObj.ParentObjectType, (parentInfo.ObjectSiteID != 0));
                            if (codeNames != null)
                            {
                                // Prepare the selection
                                QueryDataParameters parentParameters = new QueryDataParameters();

                                using (SelectCondition cond = new SelectCondition(parentParameters))
                                {
                                    cond.PrepareCondition(parentInfo.CodeNameColumn, codeNames);

                                    // Some data should be selected
                                    if (!cond.IsEmpty)
                                    {
                                        string parentWhere = cond.WhereCondition;

                                        DataSet parentDS = parentInfo.GetData(parentParameters, parentWhere, null, -1, parentInfo.TypeInfo.IDColumn, false);

                                        if (!DataHelper.DataSourceIsEmpty(parentDS))
                                        {
                                            DataTable parentDT = parentDS.Tables[0];
                                            IList<int> parentIDs = DataHelper.GetIntegerValues(parentDT, parentDT.Columns[0].ColumnName);

                                            if (parentIDs.Count < 1)
                                            {
                                                return null;
                                            }

                                            var childWhere = new WhereCondition();
                                            childWhere.WhereIn(ti.ParentIDColumn, parentIDs);

                                            where.Where(childWhere);
                                        }
                                    }
                                }
                            }
                        }
                    }

                    // Get the data
                    OperationTypeEnum operation = (selectionOnly ? OperationTypeEnum.ExportSelection : OperationTypeEnum.Export);

                    var excluded = new List<string>();
                    if (settings.ExcludedNames != null)
                    {
                        excluded.AddRange(settings.ExcludedNames);
                    }

                    // Handle excluded setting keys
                    if (objectType == SettingsKeyInfo.OBJECT_TYPE)
                    {
                        var excludedSettings = ImportExportHelper.GetExcludedSettingKeys();
                        if (excludedSettings != null)
                        {
                            excluded.AddRange(excludedSettings);
                        }
                    }

                    // Prepare settings for object data selection
                    var dataSettings = new GetObjectsDataSettings(operation, infoObj, where, null, childData, false, th, excluded.ToArray())
                    {
                        OrderBy = selectionOnly ? ti.DefaultOrderBy : ti.ImportExportSettings.OrderBy,
                        SiteId = settings.SiteId
                    };

                    data = ObjectHelper.GetObjectsData(dataSettings);

                    h.EventArguments.Data = data;
                }
                else
                {
                    // Use data from handler
                    data = h.EventArguments.Data;
                }

                h.FinishEvent();
            }

            return data;
        }


        /// <summary>
        /// Gets the export data for specified objects.
        /// </summary>
        /// <param name="settings">Export settings</param>
        /// <param name="objectType">Object type</param>
        /// <param name="siteObjects">Indicates if the object type is site dependent</param>
        /// <param name="childData">Indicates if child data should be included</param>
        /// <param name="selectionOnly">If true, the method gets only the data needed for the selection</param>
        /// <param name="th">If set, data translation table is initialized</param>
        public static DataSet GetExportData(SiteExportSettings settings, string objectType, bool siteObjects, bool childData, bool selectionOnly, TranslationHelper th)
        {
            // Export process canceled
            if (settings.ProcessCanceled)
            {
                ExportCanceled();
            }

            try
            {
                // Get object type info
                GeneralizedInfo infoObj = ModuleManager.GetReadOnlyObject(objectType);

                // Get process type
                ProcessObjectEnum processType = settings.GetObjectsProcessType(objectType, siteObjects);

                // Perform automatic selection
                if (settings.IsAutomaticallySelected(infoObj) && (processType == ProcessObjectEnum.Selected))
                {
                    settings.EnsureAutomaticSelection(infoObj, siteObjects);
                }

                // Export only object types which should be processed
                if (settings.IsObjectTypeProcessed(objectType, siteObjects, ProcessObjectEnum.Default))
                {
                    // Log process
                    LogProgress(LogStatusEnum.Info, settings, string.Format(settings.GetAPIString("ExportSite.Exporting", "Exporting {0}"), ImportExportHelper.GetObjectTypeName(objectType, settings)));

                    // Save the settings to the persistent storage
                    settings.SavePersistentLog();

                    // Export objects
                    var ds = ExportObjects(settings, objectType, siteObjects, childData, selectionOnly, th, infoObj, processType);

                    // Export tasks
                    ExportTasks(settings, infoObj, siteObjects, selectionOnly, processType, ds);

                    return ds;
                }
            }
            catch (ProcessCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                // Log exception
                LogProgressError(settings, string.Format(settings.GetAPIString("ExportSite.ErrorGettingData", "Error getting '{0}' data"), ImportExportHelper.GetObjectTypeName(objectType, settings)), ex);
                throw;
            }

            return null;
        }


        private static DataSet ExportObjects(SiteExportSettings settings, string objectType, bool siteObjects, bool childData, bool selectionOnly, TranslationHelper th, GeneralizedInfo infoObj, ProcessObjectEnum processType)
        {
            DataSet ds = null;
            bool getData = false;

            if (infoObj == null)
            {
                throw new Exception("[ExportProvider.GetExportData]: Object type '" + objectType + "' not found.");
            }

            // Generate where condition
            var where = settings.GetObjectWhereCondition(objectType, siteObjects);

            // Add where condition for selected code names
            if (processType == ProcessObjectEnum.Selected)
            {
                if (infoObj.CodeNameColumn == ObjectTypeInfo.COLUMN_NAME_UNKNOWN)
                {
                    throw new Exception("[ExportProvider.GetExportData]: Code name column for object type '" + objectType + "' is not specified.");
                }

                var codeNames = settings.GetSelectedObjectsArray(objectType, siteObjects);

                if ((codeNames != null) && (codeNames.Length > 0))
                {
                    getData = true;

                    where.WhereIn(infoObj.CodeNameColumn, codeNames);
                }
            }
            // Get all data
            else if (processType == ProcessObjectEnum.All)
            {
                getData = true;
            }

            // Get the data
            if (getData)
            {
                ds = GetExportData(settings, where, objectType, childData, selectionOnly, th);
            }

            return ds ?? new DataSet();
        }


        private static void ExportTasks(SiteExportSettings settings, GeneralizedInfo infoObj, bool siteObjects, bool selectionOnly, ProcessObjectEnum processType, DataSet ds)
        {
            // Include tasks only if should be exported
            if (infoObj.LogExport && !selectionOnly && ValidationHelper.GetBoolean(settings.GetSettings(ImportExportHelper.SETTINGS_TASKS), true))
            {
                // Prepare the condition for tasks
                QueryDataParameters tasksParameters = new QueryDataParameters();

                using (SelectCondition condTasks = new SelectCondition(tasksParameters))
                {
                    string tasksWhere = null;
                    bool getTasks = false;

                    // Get only tasks for selected objects
                    if (processType == ProcessObjectEnum.Selected)
                    {
                        // Selected tasks
                        int[] taskIDs = settings.GetSelectedTasksArray(infoObj.TypeInfo.ObjectType, siteObjects);
                        if ((taskIDs != null) && (taskIDs.Length > 0))
                        {
                            // Prepare the selection via task IDs
                            condTasks.PrepareCondition("TaskID", taskIDs);
                            if (!condTasks.IsEmpty)
                            {
                                tasksWhere = condTasks.WhereCondition;
                                getTasks = true;
                            }
                        }
                    }
                    // Get all tasks
                    else if (processType == ProcessObjectEnum.All)
                    {
                        getTasks = true;
                    }

                    // Get the tasks
                    if (getTasks)
                    {
                        var taskObj = ModuleManager.GetReadOnlyObject(ExportTaskInfo.OBJECT_TYPE);
                        DataSet tasksDS = taskObj.Generalized.GetData(tasksParameters, tasksWhere, null, -1, null, false);
                        if (!DataHelper.DataSourceIsEmpty(tasksDS))
                        {
                            DataHelper.TransferTable(ds, tasksDS, "Export_Task");
                        }
                    }
                }
            }
        }


        /// <summary>
        /// Exports all the objects.
        /// </summary>
        /// <param name="settings">Export settings</param>
        public static void ExportObjectsData(SiteExportSettings settings)
        {
            // Clear the log
            settings.ClearProgressLog();
            settings.WriteLog = true;

            settings.LogProgressState(LogStatusEnum.Info, ResHelper.GetAPIString("SiteExport.PreparingExport", "Preparing export"));

            using (new CMSActionContext { ThreadCulture = CultureHelper.EnglishCulture })
            {
                try
                {
                    // Ensure temporary files path
                    DirectoryHelper.EnsureDiskPath(settings.TemporaryFilesPath, settings.WebsitePath);

                    // Ensure target path
                    DirectoryHelper.EnsureDiskPath(settings.TargetPath, settings.WebsitePath);

                    // Initialize translation helper
                    TranslationHelper th = new TranslationHelper();

                    // Handle the event
                    using (var h = ImportExportEvents.Export.StartEvent(settings, th))
                    {
                        if (h.CanContinue())
                        {
                            // Export objects
                            for (int i = ImportExportHelper.ObjectTypes.Count - 1; i >= 0; --i)
                            {
                                var item = ImportExportHelper.ObjectTypes[i];

                                // Get object type
                                string objectType = item.ObjectType;
                                bool siteObjects = item.IsSite;

                                // Skip whole module objects if set
                                if ((!ImportExportHelper.IsEcommerceObjectType(objectType) || (ValidationHelper.GetBoolean(settings.GetSettings(ImportExportHelper.SETTINGS_PROCESS_ECOMMERCE), true))) &&
                                    (!ImportExportHelper.IsCommunityObjectType(objectType) || (ValidationHelper.GetBoolean(settings.GetSettings(ImportExportHelper.SETTINGS_PROCESS_COMMUNITY), true))))
                                {
                                    // Export site objects only if siteID is set
                                    if (!siteObjects || (settings.SiteId != 0))
                                    {
                                        // Export other object types
                                        ExportObjectType(settings, th, objectType, siteObjects);
                                    }
                                }
                            }

                            // Set additional information
                            if (settings.TimeStamp != DateTimeHelper.ZERO_TIME)
                            {
                                settings.SetInfo(ImportExportHelper.INFO_TIME_STAMP, settings.TimeStamp);
                            }

                            // Save additional information
                            SaveObjects(settings, settings.InfoDataSet, ImportExportHelper.CMS_INFO_TYPE, false);

                            // Save the translation data
                            if (th.HasRecords())
                            {
                                DataSet ds = new DataSet();
                                ds.Tables.Add(th.TranslationTable);
                                SaveObjects(settings, ds, ImportExportHelper.OBJECT_TYPE_TRANSLATION, false);
                            }

                            // Export the files
                            ExportFiles(settings);
                        }

                        // Finish the event
                        h.FinishEvent();
                    }

                    // Finalize the export
                    FinalizeExport(settings, th);
                }
                catch (ProcessCanceledException)
                {
                    // Raise the cancel event
                    ImportExportEvents.ExportCanceled.StartEvent(settings);

                    // Log process cancellation
                    LogProgress(LogStatusEnum.Finish, settings, settings.GetAPIString("ExportSite.ExportCanceled", "Export process has been canceled."));
                }
                catch (Exception ex)
                {
                    // Raise the error event
                    ImportExportEvents.ExportError.StartEvent(settings, ex);

                    // Log error during export
                    LogProgressError(settings, settings.GetAPIString("ExportSite.ErrorExportMessage", "Error exporting objects."), ex);
                    LogProgress(LogStatusEnum.Finish, settings, settings.GetAPIString("ExportSite.ErrorExport", "Error has occurred during export process."));
                    throw;
                }
                finally
                {
                    settings.WriteLog = false;

                    // Write log to the event log
                    settings.FinalizeEventLog();
                }
            }
        }


        /// <summary>
        /// Exports the given object type objects
        /// </summary>
        /// <param name="settings">Export settings</param>
        /// <param name="th">Translation helper</param>
        /// <param name="objectType">Object type</param>
        /// <param name="siteObjects">Flag whether site objects are exported</param>
        private static void ExportObjectType(SiteExportSettings settings, TranslationHelper th, string objectType, bool siteObjects)
        {
            var e = new ExportEventArgs
            {
                Settings = settings,
                TranslationHelper = th,
                ObjectType = objectType,
                SiteObjects = siteObjects
            };

            // Handle the event
            using (var h = ImportExportEvents.ExportObjects.StartEvent(e))
            {
                if (h.CanContinue())
                {
                    DataSet ds = GetExportData(settings, objectType, siteObjects, true, false, th);
                    if (!DataHelper.DataSourceIsEmpty(ds))
                    {
                        SaveObjects(settings, ds, objectType, siteObjects);

                        // Copy object type files
                        CopyFiles(settings, ds, objectType);

                        if (objectType == SiteInfo.OBJECT_TYPE)
                        {
                            // Set information that site is included
                            settings.SetInfo(ImportExportHelper.INFO_SITE_INCLUDED, true);
                        }
                    }

                    h.EventArguments.Data = ds;
                }

                h.FinishEvent();
            }
        }


        /// <summary>
        /// Exports the files
        /// </summary>
        /// <param name="settings">Export settings</param>
        private static void ExportFiles(SiteExportSettings settings)
        {
            // Copy files if set
            if (settings.CopyFiles)
            {
                // Copy global folders
                if (ValidationHelper.GetBoolean(settings.GetSettings(ImportExportHelper.SETTINGS_GLOBAL_FOLDERS), true))
                {
                    CopyGlobalFolders(settings);
                }

                // Copy site folders if site is exported
                if ((settings.SiteId != 0) && ValidationHelper.GetBoolean(settings.GetSettings(ImportExportHelper.SETTINGS_SITE_FOLDERS), true))
                {
                    CopySiteFolders(settings);
                }
            }
        }


        /// <summary>
        /// Finalizes the export process by packaging the output and putting the file to the target folder
        /// </summary>
        /// <param name="settings">Export settings</param>
        /// <param name="th">Translation helper</param>
        private static void FinalizeExport(SiteExportSettings settings, TranslationHelper th)
        {
            // Handle the event
            using (var h = ImportExportEvents.FinalizeExport.StartEvent(settings, th))
            {
                if (h.CanContinue())
                {
                    // Finalize export - create zip package or move the exported files to the destination folder
                    if (settings.CreatePackage)
                    {
                        CreateZipPackage(settings);

                        // Log progress
                        string targetFile = ImportExportHelper.GetSiteUtilsFolderRelativePath() + "Export/" + settings.TargetFileName;
                        string targetUrl = ImportExportHelper.GetExportPackageUrl(settings.TargetFileName, settings.SiteName);
                        string linkString = (targetUrl != null) ? String.Format("<a href=\"{0}\">{1}</a>", targetUrl, settings.GetAPIString("Export.ClickToDownload", "Click to download the package file.")) : String.Empty;
                        string storageName = settings.GetAPIString("Export.StorageProviderName." + StorageHelper.GetStorageProvider(targetFile).Name, "");
                        if (settings.SiteInfo == null)
                        {
                            LogProgress(LogStatusEnum.Finish, settings, String.Format(settings.GetAPIString("ExportObjects.MessageOK", "The objects have been exported to the following location: <strong>{0}</strong> {1}<br /><strong>{2}</strong>"), storageName, targetFile, linkString));
                        }
                        else
                        {
                            LogProgress(LogStatusEnum.Finish, settings, String.Format(settings.GetAPIString("ExportSite.MessageOK", "The site has been exported to the following location: <strong>{0}</strong> {1}<br /><strong>{2}</strong>"), storageName, targetFile, linkString));
                        }
                    }
                    else
                    {
                        // Copy to the destination folder
                        CopyPackageTemplateFiles(settings);

                        // Log progress
                        LogProgress(LogStatusEnum.Finish, settings, settings.GetAPIString("ExportSite.MessageTemplateOK", "Site template has been successfully exported."));
                    }
                }

                h.FinishEvent();
            }
        }


        /// <summary>
        /// Export web template.
        /// </summary>
        /// <param name="exportWebTemplateSettings">Export settings</param>
        public static void ExportWebTemplate(ExportWebTemplateSettings exportWebTemplateSettings)
        {
            var userInfo = exportWebTemplateSettings.UserInfo;

            if (userInfo == null)
            {
                // Try to get administrator from settings
                var user = ModuleManager.GetReadOnlyObject(PredefinedObjectType.USER).Generalized;
                userInfo = user.GetDefaultObject() as IUserInfo;
            }

            // Prepare settings
            var siteExportSettings = new SiteExportSettings(userInfo);

            siteExportSettings.SiteName = exportWebTemplateSettings.SiteCodeName;

            // Initialize web site path
            if (exportWebTemplateSettings.WebsitePath != null)
            {
                siteExportSettings.WebsitePath = exportWebTemplateSettings.WebsitePath;
            }

            // Initialize target path
            if (exportWebTemplateSettings.TargetPath != null)
            {
                siteExportSettings.TargetPath = exportWebTemplateSettings.TargetPath;
                siteExportSettings.TemporaryFilesPath = exportWebTemplateSettings.TargetPath;
            }

            // Set additional settings
            siteExportSettings.ExcludedNames = exportWebTemplateSettings.ExcludedNameExpressions;
            siteExportSettings.DefaultProcessObjectType = ProcessObjectEnum.Selected;
            siteExportSettings.CreatePackage = false;

            // Set additional information
            siteExportSettings.SetInfo(ImportExportHelper.INFO_WEBTEMPLATE_FLAG, true);

            SetWebTemplateInclusion(siteExportSettings);

            // Load default selection
            siteExportSettings.LoadDefaultSelection();

            SetWebTemplateSettings(exportWebTemplateSettings, siteExportSettings);

            IncludeGlobalObjects(exportWebTemplateSettings, siteExportSettings);

            IncludeAdditionalObjects(exportWebTemplateSettings, siteExportSettings);

            // Export site
            ExportObjectsData(siteExportSettings);
        }


        private static void IncludeGlobalObjects(ExportWebTemplateSettings exportWebTemplateSettings, SiteExportSettings siteExportSettings)
        {
            if (exportWebTemplateSettings.IncludedNameExpressions != null)
            {
                // Select objects by included names
                foreach (var name in exportWebTemplateSettings.IncludedNameExpressions)
                {
                    siteExportSettings.SelectGlobalObjects(null, name);
                }
            }
        }


        private static void SetWebTemplateSettings(ExportWebTemplateSettings exportWebTemplateSettings, SiteExportSettings siteExportSettings)
        {
            // Additional settings
            siteExportSettings.SetSettings(ImportExportHelper.SETTINGS_DOC_HISTORY, false);
            siteExportSettings.SetSettings(ImportExportHelper.SETTINGS_TASKS, false);
            siteExportSettings.SetSettings(ImportExportHelper.SETTINGS_GLOBAL_FOLDERS, false);
            siteExportSettings.SetSettings(ImportExportHelper.SETTINGS_WEBTEMPLATE_EXPORT, true);
            siteExportSettings.SetSettings(ImportExportHelper.SETTINGS_MEDIA_FILES_PHYSICAL, true);

            // Disable e-commerce objects export
            if (!exportWebTemplateSettings.ExportEcommerce)
            {
                siteExportSettings.SetSettings(ImportExportHelper.SETTINGS_PROCESS_ECOMMERCE, false);
            }

            // Disable community objects export
            if (!exportWebTemplateSettings.ExportCommunity)
            {
                siteExportSettings.SetSettings(ImportExportHelper.SETTINGS_PROCESS_COMMUNITY, false);
            }
        }


        private static void SetWebTemplateInclusion(SiteExportSettings siteExportSettings)
        {
            // Exclude object types from web template
            foreach (var objType in ObjectTypeManager.AllObjectTypes)
            {
                var obj = ModuleManager.GetReadOnlyObject(objType);
                if (obj != null)
                {
                    switch (obj.TypeInfo.ImportExportSettings.IncludeToWebTemplateExport)
                    {
                        case ObjectRangeEnum.All:
                            // Excluded both objects, global and site
                            siteExportSettings.SetObjectsProcessType(ProcessObjectEnum.All, objType, true);
                            siteExportSettings.SetObjectsProcessType(ProcessObjectEnum.All, objType, false);
                            break;

                        case ObjectRangeEnum.None:
                            // Excluded both objects, global and site
                            siteExportSettings.SetObjectsProcessType(ProcessObjectEnum.None, objType, true);
                            siteExportSettings.SetObjectsProcessType(ProcessObjectEnum.None, objType, false);
                            break;

                        case ObjectRangeEnum.Site:
                            // Exclude global objects
                            siteExportSettings.SetObjectsProcessType(ProcessObjectEnum.None, objType, false);
                            break;

                        case ObjectRangeEnum.Global:
                            // Exclude site objects
                            siteExportSettings.SetObjectsProcessType(ProcessObjectEnum.None, objType, true);
                            break;
                    }
                }
            }
        }


        private static void IncludeAdditionalObjects(ExportWebTemplateSettings exportWebTemplateSettings, SiteExportSettings siteExportSettings)
        {
            // Clear object selection of object types defined in AdditionalObjects collection because all objects of a given object type are selected by default.
            foreach (var additionalObject in exportWebTemplateSettings.AdditionalObjects)
            {
                siteExportSettings.SetSelectedObjects(new List<string>(), additionalObject.ObjectType, additionalObject.IsSiteObject);
            }

            foreach (var additionalObject in exportWebTemplateSettings.AdditionalObjects)
            {
                siteExportSettings.EnsureSelectedObjectsExport(additionalObject.ObjectType, additionalObject.IsSiteObject);

                siteExportSettings.Select(additionalObject.ObjectType, additionalObject.ObjectCodeName, additionalObject.IsSiteObject);
            }
        }


        /// <summary>
        /// Copies the global folders to the destination package.
        /// </summary>
        /// <param name="settings">Export settings</param>
        private static void CopyGlobalFolders(SiteExportSettings settings)
        {
            try
            {
                // Log process
                LogProgress(LogStatusEnum.Info, settings, settings.GetAPIString("ExportSite.CopyingGlobalFolders", "Copying global folders"));

                // Initialize sub paths
                List<string> subPaths = new List<string>();

                // MVC items
                subPaths.Add(ImportExportHelper.AppCodeFolder + "\\Controllers\\");
                subPaths.Add("Controllers\\Global\\");
                subPaths.Add("Models\\Global\\");
                subPaths.Add("Views\\Global\\");

                // Global files
                subPaths.Add(ImportExportHelper.AppCodeFolder + "\\Global\\");
                subPaths.Add("CMSGlobalFiles\\");

                // JavaScript files
                subPaths.Add("CMSScripts\\Custom");

                string webSitePath = settings.WebsitePath;
                string targetPath = DirectoryHelper.CombinePath(settings.TemporaryFilesPath, ImportExportHelper.DATA_FOLDER, ImportExportHelper.FILES_FOLDER, ImportExportHelper.GLOBAL_FOLDER) + "\\";

                // Copy all folders
                foreach (string subPath in subPaths)
                {
                    // Export process canceled
                    if (settings.ProcessCanceled)
                    {
                        ExportCanceled();
                    }

                    string sourcePath = webSitePath + subPath;

                    if (Directory.Exists(sourcePath))
                    {
                        CopyDirectory(sourcePath, targetPath + subPath, settings.WebsitePath);
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
                LogProgressError(settings, settings.GetAPIString("ExportSite.ErrorCopyingGlobalFolders", "Error copying global folders."), ex);
                throw;
            }
        }


        /// <summary>
        /// Copies the site folders to the destination package.
        /// </summary>
        /// <param name="settings">Export settings</param>
        private static void CopySiteFolders(SiteExportSettings settings)
        {
            try
            {
                // Log process
                LogProgress(LogStatusEnum.Info, settings, settings.GetAPIString("ExportSite.CopyingSiteFolders", "Copying site folders"));

                // Initialize sub paths
                List<string> subPaths = new List<string>();

                subPaths.Add("Controllers\\" + ImportExportHelper.SITE_MACRO + "\\");
                subPaths.Add("Models\\" + ImportExportHelper.SITE_MACRO + "\\");
                subPaths.Add("Views\\" + ImportExportHelper.SITE_MACRO + "\\");

                subPaths.Add(ImportExportHelper.AppCodeFolder + "\\" + ImportExportHelper.SITE_MACRO + "\\");
                subPaths.Add("App_Data\\" + ImportExportHelper.SITE_MACRO + "\\");

                subPaths.Add(ImportExportHelper.SITE_MACRO + "\\");

                string webSitePath = settings.WebsitePath;
                string targetPath = DirectoryHelper.CombinePath(settings.TemporaryFilesPath, ImportExportHelper.DATA_FOLDER, ImportExportHelper.FILES_FOLDER, ImportExportHelper.SITE_FOLDER) + "\\";

                // Copy all folders
                foreach (string subPath in subPaths)
                {
                    // Export process canceled
                    if (settings.ProcessCanceled)
                    {
                        ExportCanceled();
                    }

                    string sourcePath = webSitePath + subPath.Replace(ImportExportHelper.SITE_MACRO, settings.SiteName);

                    if (Directory.Exists(sourcePath))
                    {
                        string[] excludedDirs = null;

                        // Exclude form site folder stored attachments and metafiles files
                        if (sourcePath.ToLowerCSafe() == (settings.WebsitePath.ToLowerCSafe() + settings.SiteName.ToLowerCSafe() + "\\"))
                        {
                            excludedDirs = new string[4];
                            excludedDirs[0] = "files";
                            excludedDirs[1] = "metafiles";
                            excludedDirs[2] = "media";
                            excludedDirs[3] = "bizformfiles";
                        }

                        CopyDirectory(sourcePath, targetPath + subPath, excludedDirs, webSitePath);
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
                LogProgressError(settings, settings.GetAPIString("ExportSite.ErrorCopyingSiteFolders", "Error copying site folders."), ex);
                throw;
            }
        }


        /// <summary>
        /// Copies all source files (including code behind files, designer files)
        /// </summary>
        /// <param name="sourceObjectPath">Source object file path</param>
        /// <param name="targetObjectPath">Target object file path</param>
        /// <param name="websitePath">Web site path</param>
        public static void CopySourceFiles(string sourceObjectPath, string targetObjectPath, string websitePath)
        {
            CopyFile(sourceObjectPath, targetObjectPath, websitePath);

            foreach (string suff in ImportExportHelper.LangSuffixes)
            {
                CopyFile(sourceObjectPath + suff, targetObjectPath + suff, websitePath);
                CopyFile(sourceObjectPath + ".designer" + suff, targetObjectPath + ".designer" + suff, websitePath);
            }
        }


        /// <summary>
        /// Copies the object files to the destination package.
        /// </summary>
        /// <param name="settings">Export settings</param>
        /// <param name="ds">DataSet with the objects data</param>
        /// <param name="objectType">Type of the object</param>
        public static void CopyFiles(SiteExportSettings settings, DataSet ds, string objectType)
        {
            if (DataHelper.DataSourceIsEmpty(ds))
            {
                return;
            }

            // Export process canceled
            if (settings.ProcessCanceled)
            {
                ExportCanceled();
            }

            try
            {
                CopyComponentFiles(settings, ds, objectType);
                CopyMetaFiles(settings, ds, objectType);
                CopyAssemblyFiles(settings, ds, objectType);
            }
            catch (ProcessCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                LogProgressError(settings, string.Format(settings.GetAPIString("ExportSite.ErrorCopyObjectFiles", "Error copying '{0}' files"), ImportExportHelper.GetObjectTypeName(objectType, settings)), ex);
                throw;
            }
        }


        private static void CopyMetaFiles(SiteExportSettings settings, DataSet data, string objectType)
        {
            // Copy files only if required
            if (!settings.CopyFiles)
            {
                return;
            }

            if (data == null)
            {
                return;
            }

            DataTable dataTable = data.Tables["CMS_MetaFile"];
            if (DataHelper.DataSourceIsEmpty(dataTable))
            {
                return;
            }

            // Log process
            LogProgress(LogStatusEnum.Info, settings, string.Format(settings.GetAPIString("ExportSite.CopyingMetaFiles", "Copying '{0}' metafiles"), ImportExportHelper.GetObjectTypeName(objectType, settings)));

            string targetPath = DirectoryHelper.CombinePath(settings.TemporaryFilesPath, ImportExportHelper.DATA_FOLDER, ImportExportHelper.FILES_FOLDER) + "\\cms_metafile\\CMSFiles\\";

            // Process all metafiles
            foreach (DataRow dr in dataTable.Rows)
            {
                // Export process canceled
                if (settings.ProcessCanceled)
                {
                    ExportCanceled();
                }

                string guid = dr["MetaFileGUID"].ToString();
                string extension = ValidationHelper.GetString(dr["MetaFileExtension"], "");
                string fileName = guid + extension;

                if (!IsFileExcluded(fileName))
                {
                    // Get the binary
                    object binary = DataHelper.GetDataRowValue(dr, "MetaFileBinary");
                    byte[] fileBinary = null;
                    if (binary != DBNull.Value)
                    {
                        fileBinary = (byte[])binary;
                    }

                    if (fileBinary == null)
                    {
                        // Get the binary data of the meta file
                        MetaFileInfo fileInfo = new MetaFileInfo(dr);

                        fileBinary = MetaFileInfoProvider.GetFile(fileInfo, SiteInfoProvider.GetSiteName(fileInfo.MetaFileSiteID));
                    }

                    // Save the file
                    if ((fileBinary != null) && (guid != ""))
                    {
                        try
                        {
                            string filePath = targetPath + DirectoryHelper.CombinePath(guid.Substring(0, 2), fileName);
                            filePath = ImportExportHelper.GetExportFilePath(filePath);

                            // Copy files
                            DirectoryHelper.EnsureDiskPath(filePath, settings.WebsitePath);
                            File.WriteAllBytes(filePath, fileBinary);

                            // Clear the binary
                            DataHelper.SetDataRowValue(dr, "MetaFileBinary", null);
                        }
                        catch
                        {
                        }
                    }
                }
            }
        }


        private static void CopyComponentFiles(SiteExportSettings settings, DataSet ds, string objectType)
        {
            // Copy files only if required
            if (!settings.CopyFiles)
            {
                return;
            }

            switch (objectType)
            {
                case PredefinedObjectType.DOCUMENTTYPE:
                case PredefinedObjectType.CUSTOMTABLECLASS:
                case DataClassInfo.OBJECT_TYPE_SYSTEMTABLE:
                    // Class + Transformations
                    CopyComponentFile(settings, ds, objectType, "Transformations");
                    break;
            }
        }


        /// <summary>
        /// Copies component files for given object type
        /// </summary>
        /// <param name="settings">Export settings</param>
        /// <param name="data">Data</param>
        /// <param name="objectType">Object type</param>
        /// <param name="componentFolder">Component folder name</param>
        public static void CopyComponentFile(SiteExportSettings settings, DataSet data, string objectType, string componentFolder)
        {
            // Copy files only if required
            if (!settings.CopyFiles)
            {
                return;
            }

            // Prepare the paths
            string sourcePath = settings.WebsitePath + ImportExportHelper.SRC_SKINS_FOLDER + string.Format("\\Components\\{0}\\", componentFolder);
            string targetPath = DirectoryHelper.CombinePath(settings.TemporaryFilesPath, ImportExportHelper.DATA_FOLDER, ImportExportHelper.FILES_FOLDER) + "\\" + Path.Combine(ImportExportHelper.GetSafeObjectTypeName(objectType), ImportExportHelper.SRC_SKINS_FOLDER) + string.Format("\\Components\\{0}\\", componentFolder);

            var infoObj = ModuleManager.GetReadOnlyObject(objectType);

            var dt = ObjectHelper.GetTable(data, infoObj);
            if (DataHelper.DataSourceIsEmpty(dt))
            {
                return;
            }

            // Log process
            LogProgress(LogStatusEnum.Info, settings, string.Format(settings.GetAPIString("ExportSite.CopyingComponentFiles", "Copying '{0}' component files"), ImportExportHelper.GetObjectTypeName(objectType, settings)));

            foreach (DataRow dr in dt.Rows)
            {
                // Export process canceled
                if (settings.ProcessCanceled)
                {
                    ExportCanceled();
                }

                // Prepare the data
                var nameColumn = infoObj.Generalized.CodeNameColumn;
                string objectName = ValidationHelper.GetSafeFileName(dr[nameColumn].ToString());

                string targetObjectPath = Path.Combine(targetPath, objectName);
                string sourceObjectPath = Path.Combine(sourcePath, objectName);

                try
                {
                    // Skin folder
                    if (Directory.Exists(sourceObjectPath))
                    {
                        CopyDirectory(sourceObjectPath, targetObjectPath, settings.WebsitePath);
                    }
                }
                catch
                {
                }
            }
        }


        /// <summary>
        /// Copies assembly files for given object type
        /// </summary>
        /// <param name="settings">Export settings</param>
        /// <param name="data">Data</param>
        /// <param name="objectType">Object type</param>
        public static void CopyAssemblyFiles(SiteExportSettings settings, DataSet data, string objectType)
        {
            // Copy files only if required
            if (!settings.CopyFiles)
            {
                return;
            }

            if (!ValidationHelper.GetBoolean(settings.GetSettings(ImportExportHelper.SETTINGS_ASSEMBLIES), false))
            {
                return;
            }

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
            var dt = ObjectHelper.GetTable(data, infoObj);
            if (DataHelper.DataSourceIsEmpty(dt))
            {
                return;
            }

            string sourcePath = settings.WebsitePath + @"bin\";
            string targetPath = DirectoryHelper.CombinePath(settings.TemporaryFilesPath, ImportExportHelper.DATA_FOLDER, ImportExportHelper.FILES_FOLDER) + "\\" + ImportExportHelper.GetSafeObjectTypeName(objectType) + @"\bin\";

            // Log process
            LogProgress(LogStatusEnum.Info, settings, string.Format(settings.GetAPIString("ExportSite.CopyingAssemblies", "Copying '{0}' assembly files"), ImportExportHelper.GetObjectTypeName(objectType, settings)));

            // Process all assembly files
            foreach (DataRow dr in dt.Rows)
            {
                // Export process canceled
                if (settings.ProcessCanceled)
                {
                    ExportCanceled();
                }
                string assemblyName = ValidationHelper.GetString(dr[assemblyColumn], null);

                // Export assembly
                ExportAssembly(settings.WebsitePath, targetPath, sourcePath, assemblyName);
            }
        }


        /// <summary>
        /// Exports assembly file.
        /// </summary>
        /// <param name="webSitePath">Physical path to the web site root (ends with \).</param>
        /// <param name="targetPath">Destination path</param>
        /// <param name="sourcePath">Source path</param>
        /// <param name="assemblyName">Name of assembly to export</param>
        public static void ExportAssembly(string webSitePath, string targetPath, string sourcePath, string assemblyName)
        {
            // Prepare the data
            if (!string.IsNullOrEmpty(assemblyName))
            {
                string fileName = assemblyName + ".dll";
                // Do not handle App_Code and default system assemblies
                if (!assemblyName.EqualsCSafe(ClassHelper.ASSEMBLY_APPCODE, true) && !assemblyName.StartsWithCSafe("cms.", true))
                {
                    string sourceFilePath = sourcePath + fileName;
                    string targetFilePath = targetPath + fileName;

                    try
                    {
                        CopyFile(sourceFilePath, targetFilePath, webSitePath);
                    }
                    catch
                    {
                    }
                }
            }
        }


        /// <summary>
        /// Saves the objects to the file.
        /// </summary>
        /// <param name="settings">Export settings</param>
        /// <param name="ds">DataSet with the objects data</param>
        /// <param name="objectType">Type of the object</param>
        /// <param name="siteObjects">Indicates if object type is site dependent</param>
        public static void SaveObjects(SiteExportSettings settings, DataSet ds, string objectType, bool siteObjects)
        {
            if (DataHelper.DataSourceIsEmpty(ds))
            {
                return;
            }

            // Export process canceled
            if (settings.ProcessCanceled)
            {
                ExportCanceled();
            }

            // Lowercase table names for compatibility
            DataHelper.LowerCaseTableNames(ds);

            SystemIO.Stream writer = null;
            XmlWriter xml = null;

            try
            {
                // Get safe object type name
                string safeObjectType = ImportExportHelper.GetSafeObjectTypeName(objectType);

                // Get object type file path
                string filePath = settings.GetObjectTypeFilePath(objectType, siteObjects);
                string webSitePath = settings.WebsitePath;

                // Ensure disk path
                DirectoryHelper.EnsureDiskPath(filePath, webSitePath);
                filePath = ImportExportHelper.GetExportFilePath(filePath);

                // Writer setting
                XmlWriterSettings ws = new XmlWriterSettings();
                ws.CloseOutput = true;
                ws.Indent = true;
                ws.OmitXmlDeclaration = true;
                ws.CheckCharacters = false;

                // Open writer
                writer = FileStream.New(filePath, FileMode.Create, FileAccess.Write, FileShare.None, 8192);
                xml = XmlWriter.Create(writer, ws);

                // Main document element
                string rootElement = safeObjectType;
                xml.WriteStartElement(rootElement);
                xml.WriteAttributeString("version", CMSVersion.MainVersion);

                // Write data
                if (ds != null)
                {
                    ds.WriteXml(xml);
                }

                xml.WriteEndElement();
            }
            catch (ProcessCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                // Log exception
                LogProgressError(settings, string.Format(settings.GetAPIString("ExportSite.ErrorSavingData", "Error saving '{0}' data"), ImportExportHelper.GetObjectTypeName(objectType, settings)), ex);
                throw;
            }
            finally
            {
                // Safely close writers
                if (xml != null)
                {
                    xml.Close();
                }

                if (writer != null)
                {
                    writer.Close();
                }
            }
        }


        /// <summary>
        /// Export site to file.
        /// </summary>
        /// <param name="objectId">Exported object ID</param>
        /// <param name="objectType">Exported object type</param>
        /// <param name="fullExportFilePath">Full path to the target file(for example c:\export\filename.zip)</param>
        /// <param name="websitePath">Path to the web site root(If there is the HttpContext available, the parameter can be null)</param>
        /// <param name="userInfo">Current user info</param>
        public static void ExportObject(int objectId, string objectType, string fullExportFilePath, string websitePath, IUserInfo userInfo)
        {
            // Get generalized object
            BaseInfo infoObj = ProviderHelper.GetInfoById(objectType, objectId);

            ExportObject(infoObj, fullExportFilePath, websitePath, userInfo);
        }


        /// <summary>
        /// Export site to file.
        /// </summary>
        /// <param name="infoObj">Object to be exported</param>
        /// <param name="fullExportFilePath">Full path to the target file(for example c:\export\filename.zip)</param>
        /// <param name="websitePath">Path to the web site root(If there is the HttpContext available, the parameter can be null)</param>
        /// <param name="userInfo">Current user info</param>
        public static void ExportObject(GeneralizedInfo infoObj, string fullExportFilePath, string websitePath, IUserInfo userInfo)
        {
            // Prepare settings
            SiteExportSettings settings = new SiteExportSettings(userInfo);

            // Initialize web site path
            if (websitePath != null)
            {
                settings.WebsitePath = websitePath;
            }

            // Initialize export file path
            if (fullExportFilePath != null)
            {
                settings.TargetPath = Path.GetDirectoryName(fullExportFilePath);
                settings.TargetFileName = Path.GetFileName(fullExportFilePath);
            }

            // Initialize single object export            
            ImportExportHelper.InitSingleObjectExportSettings(settings, infoObj);

            // Export object
            ExportObjectsData(settings);
        }


        /// <summary>
        /// Export site to file.
        /// </summary>
        /// <param name="siteName">Site name of the exported site</param>
        /// <param name="fullExportFilePath">Full path to the target file(for example c:\export\filename.zip)</param>
        /// <param name="websitePath">Path to the web site root(If there is the HttpContext available, the parameter can be null)</param>
        /// <param name="template">Indicates if the web template should be exported</param>
        /// <param name="userInfo">Current user info</param>
        public static void ExportSite(string siteName, string fullExportFilePath, string websitePath, bool template, IUserInfo userInfo)
        {
            // Export web template
            if (template)
            {
                ExportWebTemplate(new ExportWebTemplateSettings
                {
                    SiteCodeName = siteName,
                    WebsitePath = websitePath,
                    TargetPath = fullExportFilePath,
                    ExportEcommerce = true,
                    ExportCommunity = true,
                    UserInfo = userInfo,
                });
            }
            else
            {
                // Prepare settings
                SiteExportSettings settings = new SiteExportSettings(userInfo);
                settings.SiteName = siteName;

                // Initialize web site path
                if (websitePath != null)
                {
                    settings.WebsitePath = websitePath;
                }

                // Initialize export file path
                if (fullExportFilePath != null)
                {
                    settings.TargetPath = Path.GetDirectoryName(fullExportFilePath);
                    settings.TargetFileName = Path.GetFileName(fullExportFilePath);
                }

                // Set additinal settings
                settings.DefaultProcessObjectType = ProcessObjectEnum.Selected;

                // Load default selection
                settings.LoadDefaultSelection();

                // Export site
                ExportObjectsData(settings);
            }
        }

        #endregion


        #region "Compressing methods"

        /// <summary>
        /// Finalize export - copy template files.
        /// </summary>
        /// <param name="settings">Export settings</param>
        private static void CopyPackageTemplateFiles(SiteExportSettings settings)
        {
            // Export process canceled
            if (settings.ProcessCanceled)
            {
                ExportCanceled();
            }

            string temporaryPath = settings.TemporaryFilesPath;
            string targetPath = settings.TargetPath;

            try
            {
                // Copy the files only if the folders are different
                if (temporaryPath != targetPath)
                {
                    // Log process
                    LogProgress(LogStatusEnum.Info, settings, settings.GetAPIString("ExportSite.Finalizing", "Finalizing the package"));

                    CopyDirectory(temporaryPath, targetPath, settings.WebsitePath);
                }
            }
            catch (ProcessCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                LogProgressError(settings, settings.GetAPIString("ExportSite.ErrorFinalizing", "Error finalizing package"), ex);
                throw;
            }
        }


        /// <summary>
        /// Finalize export - create ZIP export package.
        /// </summary>
        /// <param name="settings">Export settings</param>
        private static void CreateZipPackage(SiteExportSettings settings)
        {
            // Export process canceled
            if (settings.ProcessCanceled)
            {
                ExportCanceled();
            }

            string temporaryPath = settings.TemporaryFilesPath;
            string targetPath = settings.TargetPath;
            string temporaryFilePath = DirectoryHelper.CombinePath(temporaryPath, settings.TargetFileName);
            string targetFilePath = DirectoryHelper.CombinePath(settings.TargetPath, settings.TargetFileName);

            // Log process
            LogProgress(LogStatusEnum.Info, settings, settings.GetAPIString("ExportSite.Compressing", "The file is being compressed"));

            try
            {
                // Ensure the export directory
                DirectoryHelper.EnsureDiskPath(targetPath, settings.WebsitePath);

                // Delete the previous file
                if (File.Exists(temporaryFilePath))
                {
                    File.Delete(temporaryFilePath);
                }

                // Create zip file stream
                using (FileStream fs = File.Create(targetFilePath))
                {
                    using (var zipArchive = new ZipArchive(fs, ZipArchiveMode.Create, true))
                    {
                        CompressFolder(settings, zipArchive, temporaryPath, temporaryPath);
                    }
                }
            }
            catch (ProcessCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                LogProgressError(settings, settings.GetAPIString("ExportSite.ErrorCompressing", "Error compressing package"), ex);
                throw;
            }
            finally
            {
                // Export process canceled, delete exported file
                if (settings.ProcessCanceled)
                {
                    try
                    {
                        File.Delete(targetFilePath);
                    }
                    catch
                    {
                    }

                    ExportCanceled();
                }
            }
        }


        /// <summary>
        /// Compress specified folder.
        /// </summary>
        /// <param name="settings">Export settings</param>
        /// <param name="zipArchive">ZIP archive</param>
        /// <param name="path">Destination path</param>
        /// <param name="tmpPath">Temporary files path</param>
        private static void CompressFolder(SiteExportSettings settings, ZipArchive zipArchive, string path, string tmpPath)
        {
            if (settings.ProcessCanceled)
            {
                ExportCanceled();
            }

            foreach (string dir in Directory.GetDirectories(path))
            {
                CompressFolder(settings, zipArchive, dir, tmpPath);
            }

            foreach (string file in Directory.GetFiles(path))
            {
                CompressEntry(zipArchive, file, tmpPath);
            }
        }


        /// <summary>
        /// Compress entry of the ZIP export package file.
        /// </summary>
        /// <param name="zipArchive">ZIP archive</param>
        /// <param name="entryPath">Path of the entry</param>
        /// <param name="tmpPath">Temporary files path</param>
        internal static void CompressEntry(ZipArchive zipArchive, string entryPath, string tmpPath)
        {
            using (FileStream fs = File.OpenRead(entryPath))
            {
                string entryName = Path.EnsureSlashes(entryPath.Substring(tmpPath.Length));
                using (var entryStream = zipArchive.CreateEntry(entryName).Open())
                {
                    fs.CopyTo(entryStream);
                }
            }
        }

        #endregion


        #region "File manipulation methods"

        /// <summary>
        /// Copy file for the export and unset the readonly attributes.
        /// </summary>
        /// <param name="sourcePath">Source path</param>
        /// <param name="destPath">Destination path</param>
        /// <param name="websitePath">Web site path</param>
        public static void CopyFile(string sourcePath, string destPath, string websitePath)
        {
            // Try to copy file
            try
            {
                if (File.Exists(sourcePath))
                {
                    string fileName = Path.GetFileName(sourcePath);
                    if (!IsFileExcluded(fileName))
                    {
                        destPath = ImportExportHelper.GetExportFilePath(destPath);

                        // Ensure path
                        DirectoryHelper.EnsureDiskPath(destPath, websitePath);

                        // Copy the file
                        DirectoryHelper.CopyFile(sourcePath, destPath);
                        UnsetReadonlyAttribute(ImportExportHelper.GetExportFilePath(destPath));
                    }
                }
            }
            catch
            {
            }
        }


        /// <summary>
        /// Indicates if file name is excluded.
        /// </summary>
        /// <param name="fileName">File name</param>
        public static bool IsFileExcluded(string fileName)
        {
            string[] masks = ExcludedFiles.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            if (masks.Length != 0)
            {
                foreach (string mask in masks)
                {
                    if (FileHelper.IsMatch(fileName, mask))
                    {
                        return true;
                    }
                }
            }
            return false;
        }


        /// <summary>
        /// Copies specified directory including its subdirectories and all underlying files.
        /// </summary>
        /// <param name="sourcePath">Full disk path of the source directory</param>
        /// <param name="targetPath">Full disk path of the new copy of the directory including its name</param>
        /// <param name="websitePath">Web site path</param>
        public static void CopyDirectory(string sourcePath, string targetPath, string websitePath)
        {
            CopyDirectory(sourcePath, targetPath, null, websitePath);
        }


        /// <summary>
        /// Get directories in source folder
        /// </summary>
        /// <param name="sourceFolder">Source folder</param>
        public static List<string> GetDirectories(DirectoryInfo sourceFolder)
        {
            if (sourceFolder == null)
            {
                return null;
            }

            // Get all directories names
            List<string> names = new List<string>();
            DirectoryInfo[] dirs = sourceFolder.GetDirectories();
            foreach (DirectoryInfo di in dirs)
            {
                names.Add(di.Name);
            }

            // No excluded folders
            if (ExcludedFolders == null)
            {
                return names;
            }

            string[] masks = ExcludedFolders.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            if (masks.Length != 0)
            {
                foreach (string mask in masks)
                {
                    DirectoryInfo[] excludedFolders = sourceFolder.GetDirectories(mask);
                    foreach (DirectoryInfo di in excludedFolders)
                    {
                        names.Remove(di.Name);
                    }
                }
            }
            return names;
        }


        /// <summary>
        /// Gets files in source folder
        /// </summary>
        /// <param name="sourceFolder">Source folder</param>
        public static Hashtable GetFiles(DirectoryInfo sourceFolder)
        {
            if (sourceFolder == null)
            {
                return null;
            }

            // Get all files names
            Hashtable names = new Hashtable();
            FileInfo[] files = sourceFolder.GetFiles();
            foreach (FileInfo fi in files)
            {
                names[fi.Name] = fi.FullName;
            }

            // No excluded folders
            if (ExcludedFiles == null)
            {
                return names;
            }

            string[] masks = ExcludedFiles.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            if (masks.Length != 0)
            {
                foreach (string mask in masks)
                {
                    FileInfo[] excludedFiles = sourceFolder.GetFiles(mask);
                    foreach (FileInfo fi in excludedFiles)
                    {
                        names.Remove(fi.Name);
                    }
                }
            }
            return names;
        }


        /// <summary>
        /// Copies specified directory including its sub-directories and all underlying files.
        /// </summary>
        /// <param name="sourcePath">Full disk path of the source directory</param>
        /// <param name="targetPath">Full disk path of the new copy of the directory including its name</param>
        /// <param name="excludedDirs">Sub-directories to be excluded</param>
        /// <param name="websitePath">Web site path</param>
        public static void CopyDirectory(string sourcePath, string targetPath, string[] excludedDirs, string websitePath)
        {
            DirectoryInfo sourceFolder = DirectoryInfo.New(sourcePath);

            // There is a folder to copy
            if (sourceFolder.Exists)
            {
                // Ensure path
                DirectoryHelper.EnsureDiskPath(targetPath, websitePath);

                // Create the directory if not exists
                if (!Directory.Exists(targetPath))
                {
                    DirectoryHelper.CreateDirectory(targetPath);
                }

                // Copy sub folders
                List<string> dirs = GetDirectories(sourceFolder);
                if (dirs != null)
                {
                    foreach (string subFolder in dirs)
                    {
                        bool proceed = true;
                        if (excludedDirs != null)
                        {
                            foreach (string exDir in excludedDirs)
                            {
                                proceed &= (subFolder.ToLowerCSafe() != exDir.ToLowerCSafe());
                            }
                        }

                        if (proceed)
                        {
                            CopyDirectory(DirectoryHelper.CombinePath(sourcePath, subFolder), DirectoryHelper.CombinePath(targetPath, subFolder), websitePath);
                        }
                    }
                }

                // Copy files
                Hashtable files = GetFiles(sourceFolder);
                if (files != null)
                {
                    foreach (string sourceFile in files.Keys)
                    {
                        CopyFile((string)files[sourceFile], DirectoryHelper.CombinePath(targetPath, sourceFile), websitePath);
                    }
                }
            }
        }

        #endregion

        #endregion
    }
}