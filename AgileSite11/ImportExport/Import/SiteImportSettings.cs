using System;
using System.Runtime.Serialization;
using System.Security;
using System.Text;
using System.Web;
using System.Data;
using System.Collections.Generic;
using System.Linq;

using CMS.EventLog;
using CMS.Helpers;
using CMS.IO;
using CMS.Base;
using CMS.SiteProvider;
using CMS.DataEngine;
using CMS.LicenseProvider;

namespace CMS.CMSImportExport
{
    /// <summary>
    /// Class representing import settings.
    /// </summary>
    [Serializable]
    public class SiteImportSettings : AbstractImportExportSettings
    {
        #region "Variables"

        private string mSourceFilePath;

        private object mSiteIsIncluded;
        private bool? mLogObjectsWarnings;
        private bool mTemporaryFilesCreated;
        private bool? mIsWebApplication;
        private string mAppCodeFolder;

        private bool? mIsOlderVersion;
        private bool? mIsOlderHotfixVersion;
        private bool? mCopyFiles;

        private string mFullVersion;

        private bool? mRefreshMacroSecurity;
        private IUserInfo mRefreshMacroSecurityUser;

        /// <summary>
        /// File operation collection.
        /// </summary>
        protected FileOperationCollection mFileOperations;

        #endregion


        #region "Properties"

        /// <summary>
        /// Site ID of the site to be used for IDs translation.
        /// </summary>
        public int TranslationSiteId
        {
            get
            {
                if (UseAutomaticSiteForTranslation)
                {
                    return TranslationHelper.AUTO_SITEID;
                }

                return SiteId;
            }
        }


        /// <summary>
        /// Version of the package.
        /// </summary>
        public override string Version
        {
            get
            {
                if (base.Version == null)
                {
                    // Try to get version automatically
                    try
                    {
                        base.Version = ImportProvider.GetVersion(ImportProvider.GetFirstExportFile(SourceFilePath));
                    }
                    catch (Exception ex)
                    {
                        EventLogProvider.LogException("IMPORTEXPORT", "GetVersion", ex);
                        base.Version = "";
                    }
                }

                return base.Version;
            }
            set
            {
                base.Version = value;
            }
        }


        /// <summary>
        /// Hotfix package version
        /// </summary>
        public override string HotfixVersion
        {
            get
            {
                if (base.HotfixVersion == null)
                {
                    // Try to get version automatically
                    try
                    {
                        base.HotfixVersion = ValidationHelper.GetString(GetInfo(ImportExportHelper.INFO_HOTFIX_VERSION), "");
                    }
                    catch (Exception ex)
                    {
                        EventLogProvider.LogException("IMPORTEXPORT", "GetHotfixVersion", ex);
                        base.HotfixVersion = "";
                    }
                }

                return base.HotfixVersion;
            }
            set
            {
                base.HotfixVersion = value;
            }
        }


        /// <summary>
        /// Full package version
        /// </summary>
        public string FullVersion
        {
            get
            {
                return mFullVersion ?? (mFullVersion = ValidationHelper.GetString(GetInfo(ImportExportHelper.INFO_SYSTEM_VERSION), ResHelper.Dash));
            }
        }


        /// <summary>
        /// Indicates if the search tasks should be enabled.
        /// </summary>
        public bool EnableSearchTasks
        {
            get;
            set;
        } = true;


        /// <summary>
        /// Indicates if site name from translation records should be used as a site source for objects.
        /// </summary>
        public bool UseAutomaticSiteForTranslation
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates if the files should be processed.
        /// </summary>
        public bool CopyFiles
        {
            get
            {
                if (mCopyFiles == null)
                {
                    mCopyFiles = VirtualPathHelper.UsingVirtualPathProvider && (ValidationHelper.GetBoolean(SettingsHelper.AppSettings["CMSImportCopyFiles"], true) && !IsOlderHotfixVersion);
                }
                return mCopyFiles.Value;
            }
            set
            {
                mCopyFiles = value;
            }
        }


        /// <summary>
        /// Indicates if the code files should be processed.
        /// </summary>
        public bool CopyCodeFiles
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates if the site should be included in the package.
        /// </summary>
        public bool SiteIsIncluded
        {
            get
            {
                if (mSiteIsIncluded == null)
                {
                    if (!TemporaryFilesCreated)
                    {
                        throw new Exception("[SiteImportSettings.SiteIsIncluded]: Temporary files haven't been created yet!");
                    }
                    mSiteIsIncluded = IsIncluded(SiteInfo.OBJECT_TYPE, true);
                }
                return (bool)mSiteIsIncluded;
            }
        }


        /// <summary>
        /// Indicates if site should be imported
        /// </summary>
        public bool ImportSite
        {
            get
            {
                return SiteIsIncluded && ValidationHelper.GetBoolean(GetSettings(ImportExportHelper.SETTINGS_SITE), true);
            }
        }


        /// <summary>
        /// Indicates if new site is being imported (either from a web template or based on blank site).
        /// </summary>
        /// <seealso cref="IsWebTemplate"/>
        public bool IsNewSite
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates if web template is imported.
        /// </summary>
        public bool IsWebTemplate
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates if the macros security parameters should be refreshed during import.
        /// </summary>
        /// <seealso cref="MacroSecurityUser"/>
        public bool RefreshMacroSecurity
        {
            get
            {
                if (mRefreshMacroSecurity == null)
                {
                    mRefreshMacroSecurity = IsWebTemplate;
                }
                return mRefreshMacroSecurity.Value;
            }
            set
            {
                mRefreshMacroSecurity = value;
            }
        }


        /// <summary>
        /// Gets or sets the user whose identity is to be used for macro signing when <see cref="RefreshMacroSecurity"/> is set.
        /// The default value is <see cref="AbstractImportExportSettings.CurrentUser"/>.
        /// </summary>
        public IUserInfo MacroSecurityUser
        {
            get
            {
                return mRefreshMacroSecurityUser ?? (mRefreshMacroSecurityUser = CurrentUser);
            }
            set
            {
                mRefreshMacroSecurityUser = value;
            }
        }


        /// <summary>
        /// Indicates if temporary files were created.
        /// </summary>
        public bool TemporaryFilesCreated
        {
            get
            {
                mSiteIsIncluded = null;
                return mTemporaryFilesCreated;
            }
            set
            {
                mTemporaryFilesCreated = value;
            }
        }


        /// <summary>
        /// Indicates if temporary files were created and should be deleted.
        /// </summary>
        public bool DeleteTemporaryFiles
        {
            get;
            set;
        } = true;


        /// <summary>
        /// Current URL of the import process.
        /// </summary>
        public string CurrentUrl
        {
            get
            {
                if (HttpContext.Current != null)
                {
                    return RequestContext.URL.AbsoluteUri;
                }

                return "http://localhost";
            }
        }


        /// <summary>
        /// Full source file path of the package (ends with \).
        /// </summary>
        public string SourceFilePath
        {
            get
            {
                return mSourceFilePath;
            }
            set
            {
                string path = value;
                if ((path != null) && (!path.EndsWith(ImportExportHelper.PACKAGE_EXTENSION, StringComparison.OrdinalIgnoreCase)))
                {
                    // Ensure correct path
                    path = Path.EnsureEndBackslash(path);
                }
                mSourceFilePath = path;
            }
        }


        /// <summary>
        /// Site display name.
        /// </summary>
        public string SiteDisplayName
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates if package is from older version.
        /// </summary>
        public bool IsOlderVersion
        {
            get
            {
                if (mIsOlderVersion == null)
                {
                    mIsOlderVersion = IsLowerVersion(CMSVersion.MainVersion);
                }

                return mIsOlderVersion.Value;
            }
        }


        /// <summary>
        /// Indicates if package is from older version including hotfix.
        /// </summary>
        public bool IsOlderHotfixVersion
        {
            get
            {
                if (mIsOlderHotfixVersion == null)
                {
                    mIsOlderHotfixVersion = IsLowerVersion(CMSVersion.MainVersion, CMSVersion.HotfixVersion);
                }
                return mIsOlderHotfixVersion.Value;
            }
        }


        /// <summary>
        /// Site domain name.
        /// </summary>
        public string SiteDomain
        {
            get;
            set;
        }


        /// <summary>
        /// Site description.
        /// </summary>
        public string SiteDescription
        {
            get;
            set;
        }


        /// <summary>
        /// Content only site
        /// </summary>
        public bool SiteIsContentOnly
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates if binding and objects warnings should be logged.
        /// </summary>
        public bool LogObjectsWarnings
        {
            get
            {
                if (mLogObjectsWarnings == null)
                {
                    mLogObjectsWarnings = ValidationHelper.GetBoolean(SettingsHelper.AppSettings["CMSImportLogObjectsWarnings"], false);
                }
                return mLogObjectsWarnings.Value;
            }
        }


        /// <summary>
        /// Indicates if the synchronization should be logged.
        /// </summary>
        public bool LogSynchronization
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates if the integration tasks should be logged.
        /// </summary>
        public bool LogIntegration
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates if the object version should be created for imported objects.
        /// </summary>
        public bool CreateVersion
        {
            get;
            set;
        } = true;


        /// <summary>
        /// Type of the import process.
        /// </summary>
        public ImportTypeEnum ImportType
        {
            get;
            set;
        } = ImportTypeEnum.AllNonConflicting;


        /// <summary>
        /// If true, import is into an existing site (defined by SiteId).
        /// </summary>
        public bool ExistingSite
        {
            get;
            set;
        }


        /// <summary>
        /// Parameter which is used for SQL script execution.
        /// </summary>
        public string ScriptParameter
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates if site search indexes should be rebuilt at the end of import.
        /// </summary>
        public bool RebuildSearchIndex
        {
            get;
            set;
        } = true;


        /// <summary>
        /// Indicates whether the packages comes from web application or web site.
        /// </summary>
        public bool IsWebApplication
        {
            get
            {
                if (mIsWebApplication == null)
                {
                    mIsWebApplication = ValidationHelper.GetBoolean(GetInfo(ImportExportHelper.INFO_SYSTEM_WEBAPP), false);
                }

                return mIsWebApplication.Value;
            }
        }


        /// <summary>
        /// Name of the App_Code folder (depending on the package source - WebApp vs Website)
        /// </summary>
        public string AppCodeFolder
        {
            get
            {
                return mAppCodeFolder ?? (mAppCodeFolder = IsWebApplication ? "Old_App_Code" : "App_Code");
            }
        }


        /// <summary>
        /// File operations collection.
        /// </summary>
        public FileOperationCollection FileOperations
        {
            get
            {
                return mFileOperations ?? (mFileOperations = new FileOperationCollection());
            }
        }


        /// <summary>
        /// If true, the import process allows bulk inserts for the data to speed up the import process.
        /// If bulk insert is allowed, some event handlers may not be fired during the import process.
        /// </summary>
        public bool AllowBulkInsert
        {
            get;
            set;
        }


        /// <summary>
        /// If true, only new objects (including new children and bindings) are imported and already existing objects are not updated with the package data.
        /// This setting is useful especially when importing data from web template which may contain additional child objects but should not overwrite already customized data.
        /// </summary>
        public bool ImportOnlyNewObjects
        {
            get;
            set;
        }

        /// <summary>
        /// List of already imported objects.
        /// </summary>
        internal List<BaseInfo> ImportedObjects { get; set; } = new List<BaseInfo>();

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates import settings.
        /// </summary>
        /// <param name="currentUser">Current user info</param>
        public SiteImportSettings(IUserInfo currentUser)
            : base(currentUser)
        {
            EventLogCode = "IMPORT";
            EventLogSource = GetAPIString("ImportSite.EventLogSource", "Import objects");
        }


        /// <summary>
        /// Constructor - Constructor for deserialization.
        /// </summary>
        /// <param name="info">Serialization info</param>
        /// <param name="ctxt">Streaming context</param>
        public SiteImportSettings(SerializationInfo info, StreamingContext ctxt)
            : base(info, ctxt)
        {
            //Get the values from info and assign them to the appropriate properties
            mIsOlderVersion = info.GetBoolean("IsOlderVersion");

            SiteDescription = info.GetString("SiteDescription");
            SiteDomain = info.GetString("SiteDomain");
            SiteDisplayName = info.GetString("SiteDisplayName");
            SiteIsContentOnly = info.GetBoolean("SiteIsContentOnly");

            mSourceFilePath = info.GetString("SourceFilePath");
            mTemporaryFilesCreated = info.GetBoolean("TemporaryFilesCreated");
            DeleteTemporaryFiles = info.GetBoolean("DeleteTemporaryFiles");
            mSiteIsIncluded = info.GetValue("SiteIsIncldued", typeof(object));
            mCopyFiles = info.GetBoolean("CopyFiles");
            ImportType = (ImportTypeEnum)info.GetValue("ImportType", typeof(ImportTypeEnum));
            ExistingSite = info.GetBoolean("ExistingSite");
            EnableSearchTasks = info.GetBoolean("EnableSearchTasks");
            ScriptParameter = info.GetString("ScriptParameter");
            IsNewSite = info.GetBoolean("IsNewSite");
            IsWebTemplate = info.GetBoolean("IsWebTemplate");
            LogSynchronization = info.GetBoolean("LogSynchronization");
            LogIntegration = info.GetBoolean("LogIntegration");
            RebuildSearchIndex = info.GetBoolean("RebuildSearchIndex");
            mFileOperations = (FileOperationCollection)info.GetValue("FileOperations", typeof(FileOperationCollection));
            UseAutomaticSiteForTranslation = info.GetBoolean("UseAutomaticSiteForTranslation");
            mRefreshMacroSecurity = info.GetBoolean("RefreshMacroSecurity");

            CopyCodeFiles = info.GetBoolean("CopyCodeFiles");
            AllowBulkInsert = info.GetBoolean("AllowBulkInsert");
            ImportOnlyNewObjects = info.GetBoolean("ImportOnlyNewObjects");
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Serialization function.
        /// </summary>
        /// <param name="info">Serialization info</param>
        /// <param name="ctxt">Streaming context</param>
        [SecurityCritical]
        public override void GetObjectData(SerializationInfo info, StreamingContext ctxt)
        {
            // Serialize base class
            base.GetObjectData(info, ctxt);

            info.AddValue("IsOlderVersion", ValidationHelper.GetBoolean(mIsOlderVersion, false));

            info.AddValue("SiteDescription", SiteDescription);
            info.AddValue("SiteDomain", SiteDomain);
            info.AddValue("SiteDisplayName", SiteDisplayName);
            info.AddValue("SiteIsContentOnly", SiteIsContentOnly);

            info.AddValue("SourceFilePath", mSourceFilePath);
            info.AddValue("TemporaryFilesCreated", mTemporaryFilesCreated);
            info.AddValue("DeleteTemporaryFiles", DeleteTemporaryFiles);
            info.AddValue("SiteIsIncldued", ValidationHelper.GetBoolean(mSiteIsIncluded, false));
            info.AddValue("CopyFiles", ValidationHelper.GetBoolean(mCopyFiles, false));
            info.AddValue("ImportType", ImportType);
            info.AddValue("ExistingSite", ExistingSite);
            info.AddValue("EnableSearchTasks", EnableSearchTasks);
            info.AddValue("ScriptParameter", ScriptParameter);
            info.AddValue("IsNewSite", IsNewSite);
            info.AddValue("IsWebTemplate", IsWebTemplate);
            info.AddValue("LogSynchronization", LogSynchronization);
            info.AddValue("LogIntegration", LogIntegration);
            info.AddValue("RebuildSearchIndex", RebuildSearchIndex);
            info.AddValue("FileOperations", mFileOperations);
            info.AddValue("UseAutomaticSiteForTranslation", UseAutomaticSiteForTranslation);
            info.AddValue("RefreshMacroSecurity", ValidationHelper.GetBoolean(mRefreshMacroSecurity, false));

            info.AddValue("CopyCodeFiles", CopyCodeFiles);
            info.AddValue("AllowBulkInsert", AllowBulkInsert);
            info.AddValue("ImportOnlyNewObjects", ImportOnlyNewObjects);
        }


        /// <summary>
        /// Loads the default selection to the export settings tables.
        /// </summary>
        /// <param name="loadAdditionalSettings">Indicates if additional settings should be loaded</param>
        public void LoadDefaultSelection(bool loadAdditionalSettings = true)
        {
            // Preload all the classes to improve performance
            DataClassInfoProvider.LoadAllClasses();

            // Objects
            foreach (var item in ImportExportHelper.ObjectTypes)
            {
                string objectType = item.ObjectType;
                try
                {
                    bool siteObject = item.IsSite;

                    var e = new ImportLoadSelectionArgs
                    {
                        ObjectType = objectType,
                        SiteObject = siteObject,
                        Settings = this
                    };

                    // Handle the event
                    SpecialActionsEvents.ImportLoadDefaultSelection.StartEvent(e);

                    if (e.Select)
                    {
                        var parameters = new DefaultSelectionParameters
                        {
                            ObjectType = objectType,
                            SiteObjects = siteObject
                        };

                        switch (objectType)
                        {
                            // Site - Always select
                            case SiteInfo.OBJECT_TYPE:
                                parameters.ImportType = ImportTypeEnum.AllNonConflicting;
                                LoadDefaultSelection(parameters);
                                break;

                            case LicenseKeyInfo.OBJECT_TYPE:
                                if (IsOlderVersion)
                                {
                                    SetObjectsProcessType(ProcessObjectEnum.None, objectType, siteObject);
                                }
                                else
                                {
                                    parameters.ImportType = ImportTypeEnum.Default;
                                    LoadDefaultSelection(parameters);
                                }
                                break;

                            // Other objects
                            default:
                                parameters.ImportType = ImportTypeEnum.Default;
                                LoadDefaultSelection(parameters);
                                break;
                        }
                    }
                }
                catch (DataClassNotFoundException ex)
                {
                    // This is valid scenario when installing a new module - object type is registered, but its class is not imported yet.
                    HandleObjectTypeClassNotFound(objectType, ex);
                }
            }

            // Other important settings
            bool importSettings = (ImportType != ImportTypeEnum.None);

            SetSettings(ImportExportHelper.SETTINGS_DOC_HISTORY, importSettings);
            SetSettings(ImportExportHelper.SETTINGS_DOC_ACLS, importSettings);
            SetSettings(ImportExportHelper.SETTINGS_DOC_RELATIONSHIPS, importSettings);
            SetSettings(ImportExportHelper.SETTINGS_EVENT_ATTENDEES, importSettings);
            SetSettings(ImportExportHelper.SETTINGS_BLOG_COMMENTS, importSettings);

            SetSettings(ImportExportHelper.SETTINGS_WORKFLOW_TRIGGERS, importSettings);
            SetSettings(ImportExportHelper.SETTINGS_WORKFLOW_SCOPES, importSettings);
            SetSettings(ImportExportHelper.SETTINGS_PAGETEMPLATE_SCOPES, importSettings);
            SetSettings(ImportExportHelper.SETTINGS_PAGETEMPLATE_VARIANTS, importSettings);
            SetSettings(ImportExportHelper.SETTINGS_USER_PERSONALIZATIONS, importSettings);
            SetSettings(ImportExportHelper.SETTINGS_GROUP_MEMBERSHIP, importSettings);
            SetSettings(ImportExportHelper.SETTINGS_USER_SITE_DASHBOARDS, importSettings);


            // Additional settings
            if (loadAdditionalSettings)
            {
                SetSettings(ImportExportHelper.SETTINGS_BIZFORM_DATA, importSettings);
                SetSettings(ImportExportHelper.SETTINGS_CUSTOMTABLE_DATA, importSettings);
                SetSettings(ImportExportHelper.SETTINGS_FORUM_POSTS, importSettings);
                SetSettings(ImportExportHelper.SETTINGS_MEDIA_FILES, importSettings);
                SetSettings(ImportExportHelper.SETTINGS_MEDIA_FILES_PHYSICAL, importSettings);
                SetSettings(ImportExportHelper.SETTINGS_BOARD_MESSAGES, importSettings);
                SetSettings(ImportExportHelper.SETTINGS_USER_DASHBOARDS, importSettings);
            }
        }


        /// <summary>
        /// Handles DataClassNotFoundException. Logs warning if missing data class belongs to the object type that is not imported
        /// by the currently installed module. Returns the message that was logged or empty string.
        /// </summary>
        /// <param name="objectType">Object type that caused the exception.</param>
        /// <param name="ex">Exception that is handled.</param>
        /// <returns>Returns the message that was logged or empty string.</returns>
        public string HandleObjectTypeClassNotFound(string objectType, DataClassNotFoundException ex)
        {
            ObjectTypeInfo typeInfo = ObjectTypeManager.GetTypeInfo(objectType);
            string message = String.Empty;

            if (typeInfo != null)
            {
                string moduleName = typeInfo.ModuleName;

                // If object type belongs to the module that is being currently imported by the package, no warning is logged.
                if (!IsInstallableModule || !String.Equals(moduleName, ModuleName, StringComparison.OrdinalIgnoreCase))
                {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendFormat("The '{0}' class belonging to the '{1}' object type was not found.", ex.ClassName, objectType);
                    if (!String.IsNullOrEmpty(moduleName))
                    {
                        sb.AppendFormat(" This could be due to incomplete installation of the '{0}' module.", moduleName);
                    }
                    message = sb.ToString();
                    LogProgressState(LogStatusEnum.Warning, message);
                }
            }
            else
            {
                message = ex.Message;
                LogProgressState(LogStatusEnum.Warning, message);
            }

            return message;
        }


        /// <summary>
        /// Loads the default selection to the import settings tables.
        /// </summary>
        /// <param name="parameters">object containing selection parameters</param>
        public void LoadDefaultSelection(DefaultSelectionParameters parameters)
        {
            if (parameters == null)
            {
                throw new ArgumentNullException("parameters");
            }

            try
            {
                // Clear the event log
                EventLog = null;
                // Clear progress log
                if (parameters.ClearProgressLog)
                {
                    ClearProgressLog();
                }

                // Get object type info
                GeneralizedInfo infoObj = ModuleManager.GetReadOnlyObject(parameters.ObjectType);
                if (infoObj != null)
                {
                    // Do not preselect automatically selected object types
                    if (IsAutomaticallySelected(infoObj))
                    {
                        return;
                    }

                    // Get default import type
                    if (parameters.ImportType == ImportTypeEnum.Default)
                    {
                        parameters.ImportType = ImportType;
                    }

                    // Code name column has to be defined
                    if (infoObj.CodeNameColumn == ObjectTypeInfo.COLUMN_NAME_UNKNOWN)
                    {
                        throw new Exception("[SiteImportSettings.LoadDefaultSelection]: Missing code name column information for object type '" + parameters.ObjectType + "'.");
                    }

                    // Initialize list of objects and tasks
                    List<string> list = new List<string>();
                    List<int> taskList = new List<int>();

                    // Get objects from package
                    if (parameters.ImportType != ImportTypeEnum.None)
                    {
                        // Load the data
                        DataSet ds = ImportProvider.LoadObjects(this, parameters.ObjectType, parameters.SiteObjects, true);
                        if (!DataHelper.DataSourceIsEmpty(ds))
                        {
                            // Get the table
                            if (parameters.LoadObjects)
                            {
                                var dt = ObjectHelper.GetTable(ds, infoObj);

                                // Objects
                                if (!DataHelper.DataSourceIsEmpty(dt))
                                {
                                    int colIndex = dt.Columns.IndexOf(infoObj.CodeNameColumn);

                                    // Use guid column when codenames not present
                                    var ti = infoObj.TypeInfo;

                                    if ((ti.GUIDColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN) && (ValidationHelper.GetString(dt.Rows[0][infoObj.CodeNameColumn], null) == null))
                                    {
                                        colIndex = dt.Columns.IndexOf(ti.GUIDColumn);
                                    }

                                    var dataRows = string.IsNullOrEmpty(parameters.FilterCurrentWhereCondition) ? dt.Select() : dt.Select(parameters.FilterCurrentWhereCondition);
                                    foreach (DataRow dr in dataRows)
                                    {
                                        string codeName = ValidationHelper.GetString(dr[colIndex], "").ToLowerInvariant();
                                        if (codeName != "")
                                        {
                                            list.Add(codeName);
                                        }
                                    }                                  
                                }
                            }

                            // Tasks
                            if (parameters.LoadTasks)
                            {
                                DataTable dt = ds.Tables["Export_Task"];
                                if (!DataHelper.DataSourceIsEmpty(dt))
                                {
                                    int colIndex = dt.Columns.IndexOf("TaskID");

                                    foreach (DataRow dr in dt.Rows)
                                    {
                                        int taskId = ValidationHelper.GetInteger(dr[colIndex], 0);
                                        if (taskId > 0)
                                        {
                                            taskList.Add(taskId);
                                        }
                                    }
                                }
                            }
                        }
                    }

                    // Set selected objects
                    if (parameters.LoadObjects)
                    {
                        SetSelectedObjects(list, parameters.ObjectType, parameters.SiteObjects);
                    }

                    // Set selected tasks
                    if (parameters.LoadTasks)
                    {
                        SetSelectedTasks(taskList, parameters.ObjectType, parameters.SiteObjects);
                    }

                    // Remove existing
                    if (parameters.LoadObjects && (parameters.ImportType == ImportTypeEnum.New) && (list.Count > 0))
                    {
                        // Get existing objects
                        DataSet ds = ImportProvider.GetExistingObjects(this, parameters.ObjectType, parameters.SiteObjects, true);
                        if (!DataHelper.DataSourceIsEmpty(ds))
                        {
                            int colIndex = ds.Tables[0].Columns.IndexOf(infoObj.CodeNameColumn);

                            // Remove the existing objects from the list
                            foreach (DataRow dr in ds.Tables[0].Rows)
                            {
                                string codeName = ValidationHelper.GetString(dr[colIndex], "").ToLowerInvariant();
                                if (codeName != "")
                                {
                                    int index = list.IndexOf(codeName);
                                    if (index >= 0)
                                    {
                                        list.RemoveAt(index);
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    throw new Exception("[SiteImportSettings.LoadDefaultSelection]: Object type '" + parameters.ObjectType + "' not found.");
                }
            }
            catch (Exception ex)
            {
                // Log error
                LogProgressError(null, ex);

                // Write log to the event log
                FinalizeEventLog();

                throw;
            }
        }


        /// <summary>
        /// Ensures automatic selection for given object type.
        /// </summary>
        /// <param name="infoObj">Info object</param>
        /// <param name="siteObjects">Indicates if object is site object</param>
        public void EnsureAutomaticSelection(GeneralizedInfo infoObj, bool siteObjects)
        {
            if (infoObj != null)
            {
                var ti = infoObj.TypeInfo;

                var e = new ImportLoadSelectionArgs
                {
                    ObjectType = ti.ObjectType,
                    SiteObject = siteObjects,
                    Settings = this,
                    DependencyObjectType = infoObj.ParentObjectType,
                    DependencyIDColumn = ti.ParentIDColumn,
                };

                // Get parent where condition
                if (ti.ParentIDColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN)
                {
                    e.DependencyObject = ModuleManager.GetReadOnlyObject(infoObj.ParentObjectType);
                }

                // Handle the event
                SpecialActionsEvents.ImportEnsureAutomaticSelection.StartEvent(e);
                if (e.Select)
                {
                    // Check if continue without parent processing
                    bool processParent = true;
                    var dependencyObject = e.DependencyObject;

                    if ((dependencyObject != null) && (dependencyObject.Generalized.ObjectSiteID == 0) && siteObjects)
                    {
                        processParent = e.ProcessDependency;
                    }

                    if (!processParent)
                    {
                        // Get the data
                        DataSet ds = ImportProvider.LoadObjects(this, ti.ObjectType, true);
                        if (!DataHelper.DataSourceIsEmpty(ds))
                        {
                            // Get selected code names
                            List<string> selectedObjects = new List<string>();
                            foreach (DataRow drChild in ds.Tables[0].Rows)
                            {
                                string codeName = DataHelper.GetDataRowValue(drChild, infoObj.CodeNameColumn).ToString();
                                selectedObjects.Add(codeName);
                            }

                            // Decide if site object due to parent
                            SetSelectedObjects(selectedObjects, ti.ObjectType, true);
                        }
                    }
                    else
                    {
                        var depTypeInfo = dependencyObject.TypeInfo;

                        bool siteObj = (depTypeInfo.SiteIDColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN) && e.DependencyIsSiteObject;
                        string[] codeNames = GetSelectedObjectsArray(e.DependencyObjectType, siteObj);
                        string parentWhere = GetParentWhereCondition(dependencyObject, codeNames);

                        // Some data should be selected
                        if (parentWhere != SqlHelper.NO_DATA_WHERE)
                        {
                            DataSet parentDS = ImportProvider.LoadObjects(this, depTypeInfo.ObjectType, siteObj);

                            if (!DataHelper.DataSourceIsEmpty(parentDS))
                            {
                                // Get only selected parents
                                DataRow[] parentObjects = parentDS.Tables[0].Select(parentWhere);
                                if (parentObjects.Length > 0)
                                {
                                    List<int> parentIDs = new List<int>();

                                    foreach (DataRow drParent in parentObjects)
                                    {
                                        parentIDs.Add((int)DataHelper.GetDataRowValue(drParent, depTypeInfo.IDColumn));
                                    }

                                    // Create where condition for the child object
                                    string childWhere = SqlHelper.GetWhereCondition(e.DependencyIDColumn, parentIDs.AsEnumerable());

                                    // Get the data
                                    DataSet ds = ImportProvider.LoadObjects(this, ti.ObjectType, siteObjects);
                                    if (!DataHelper.DataSourceIsEmpty(ds))
                                    {
                                        DataRow[] childObjects = ds.Tables[0].Select(childWhere);
                                        if (childObjects.Length > 0)
                                        {
                                            // Get selected code names
                                            List<string> selectedObjects = new List<string>();
                                            foreach (DataRow drChild in childObjects)
                                            {
                                                string codeName = DataHelper.GetDataRowValue(drChild, infoObj.CodeNameColumn).ToString();
                                                selectedObjects.Add(codeName);
                                            }

                                            // Select objects due to parent
                                            SetSelectedObjects(selectedObjects, ti.ObjectType, siteObjects);
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            // Deselect all objects
                            SetSelectedObjects(null, ti.ObjectType, siteObjects);
                        }
                    }
                }
            }
        }


        private static string GetParentWhereCondition(GeneralizedInfo parentInfo, IEnumerable<string> codeNames)
        {
            // GUID column is used
            var parentWhere = parentInfo.TypeInfo.CodeNameColumn == ObjectTypeInfo.COLUMN_NAME_UNKNOWN 
                                ? SqlHelper.GetWhereCondition<Guid>(parentInfo.CodeNameColumn, codeNames, false) 
                                : SqlHelper.GetWhereCondition<string>(parentInfo.CodeNameColumn, codeNames, false);
            return parentWhere;
        }


        /// <summary>
        /// Check if object type is included in the package.
        /// </summary>
        /// <param name="objectType">Type of the object</param>
        /// <param name="siteObjects">Indicates if the object is site dependent</param>
        public bool IsIncluded(string objectType, bool siteObjects)
        {
            // Get object type file path
            string filePath = GetObjectTypeFilePath(objectType, siteObjects);
            filePath = ImportExportHelper.GetExportFilePath(filePath);

            // Object type is included if file exists
            return File.Exists(filePath);
        }


        /// <summary>
        /// Check if object type (site or global) is included in the package.
        /// </summary>
        /// <param name="objectType">Type of the object</param>
        public bool IsIncluded(string objectType)
        {
            return IsIncluded(objectType, false) || IsIncluded(objectType, true);
        }


        /// <summary>
        /// Clears info data
        /// </summary>
        public void ClearInfoData()
        {
            HotfixVersion = null;
            mIsOlderVersion = null;
            mIsOlderHotfixVersion = null;
            mFullVersion = null;
            mIsWebApplication = null;
        }

        #endregion
    }
}