using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Web;

using CMS.Base;
using CMS.Core;
using CMS.DataEngine;
using CMS.EventLog;
using CMS.Helpers;
using CMS.IO;
using CMS.SiteProvider;

using CultureInfo = System.Globalization.CultureInfo;

namespace CMS.CMSImportExport
{
    /// <summary>
    /// Abstract class for import and export settings.
    /// </summary>
    [Serializable]
    public class AbstractImportExportSettings : ISerializable
    {
        #region "Hashtables"

        /// <summary>
        /// Hashtable with lists of global selected objects [ObjectType.ToLowerCSafe()] -> [List of code names]
        /// </summary>
        protected SafeDictionary<string, List<string>> mSelectedGlobalObjectsHashtable = new SafeDictionary<string, List<string>>();


        /// <summary>
        /// Hashtable with lists of site selected objects [ObjectType.ToLowerCSafe()] -> [List of code names]
        /// </summary>
        protected SafeDictionary<string, List<string>> mSelectedSiteObjectsHashtable = new SafeDictionary<string, List<string>>();


        /// <summary>
        /// Hashtable with lists of global selected tasks [ObjectType.ToLowerCSafe()] -> [List of task IDs]
        /// </summary>
        protected SafeDictionary<string, List<int>> mSelectedGlobalTasksHashtable = new SafeDictionary<string, List<int>>();

        /// <summary>
        /// Hashtable with lists of site selected tasks [ObjectType.ToLowerCSafe()] -> [List of task IDs]
        /// </summary>
        protected SafeDictionary<string, List<int>> mSelectedSiteTasksHashtable = new SafeDictionary<string, List<int>>();


        /// <summary>
        /// Hashtable with lists of global objects to be processed.
        /// </summary>
        protected Dictionary<string, ProcessObjectEnum> mProcessGlobalObjectsHashtable = new Dictionary<string, ProcessObjectEnum>();


        /// <summary>
        /// Hashtable with lists of site objects to be processed.
        /// </summary>
        protected Dictionary<string, ProcessObjectEnum> mProcessSiteObjectsHashtable = new Dictionary<string, ProcessObjectEnum>();


        /// <summary>
        /// Hashtable with additional settings.
        /// </summary>
        protected Dictionary<string, bool> mSettingsHashtable = new Dictionary<string, bool>();


        /// <summary>
        /// Versions comparations.
        /// </summary>
        protected Dictionary<string, bool> mVersionsComparations = new Dictionary<string, bool>();


        /// <summary>
        /// Event log object.
        /// </summary>
        protected EventLogInfo mEventLog = null;

        #endregion


        #region "Variables"

        /// <summary>
        /// Version of the package.
        /// </summary>
        private string mVersion;


        /// <summary>
        /// Temporary files path.
        /// </summary>
        protected string mTemporaryFilesPath = null;


        /// <summary>
        /// Event log object.
        /// </summary>
        protected EventLogInfo EventLog
        {
            get
            {
                return mEventLog ?? (mEventLog = CreateEventLog());
            }
            set
            {
                mEventLog = value;
            }
        }


        /// <summary>
        /// Event log source.
        /// </summary>
        protected string mEventLogSource = "Import / Export";


        /// <summary>
        /// Event log code.
        /// </summary>
        protected string mEventLogCode = "IMPORTEXPORT";


        /// <summary>
        /// Flag for process cancelation.
        /// </summary>
        protected bool mProcessCanceled = false;


        /// <summary>
        /// Website path.
        /// </summary>
        protected string mWebsitePath = null;


        /// <summary>
        /// Default object process type.
        /// </summary>
        protected ProcessObjectEnum mDefaultProcessObjectType = ProcessObjectEnum.All;


        /// <summary>
        /// Excludes object names.
        /// </summary>
        protected string[] mExcludedNames = null;


        /// <summary>
        /// Site ID.
        /// </summary>
        protected int mSiteId = 0;


        /// <summary>
        /// Site name.
        /// </summary>
        protected string mSiteName = null;


        /// <summary>
        /// Site info.
        /// </summary>
        protected SiteInfo mSiteInfo = null;


        /// <summary>
        /// Data set with additional information.
        /// </summary>
        protected DataSet mInfoDataSet = null;


        /// <summary>
        /// Persistent key to store the settings.
        /// </summary>
        protected string mPersistentSettingsKey = null;


        /// <summary>
        /// If true, write to the log is enabled.
        /// </summary>
        protected bool mWriteLog = false;


        /// <summary>
        /// Current user info.
        /// </summary>
        protected IUserInfo mCurrentUser = null;


        /// <summary>
        /// Current UI culture.
        /// </summary>
        protected CultureInfo mUICulture = null;


        /// <summary>
        /// Administrator user ID.
        /// </summary>
        private int? mAdministratorId;


        /// <summary>
        /// Administrator user.
        /// </summary>
        private IUserInfo mAdministrator;


        private bool mIsError;
        private bool mIsWarning;

        private LogStatusEnum mProgressState = LogStatusEnum.Info;
        private ILogContext mLogContext;

        #endregion


        #region "Constants"

        /// <summary>
        /// Internal status separator.
        /// </summary>
        public const string SEPARATOR = "<#>";


        /// <summary>
        /// Number of backward supported major versions.
        /// </summary>
        private const int SUPPORTED_MAJOR_VERSIONS_RANGE = 1;

        #endregion


        #region "Public properties"

        /// <summary>
        /// Log context used to log progress
        /// </summary>
        public ILogContext LogContext
        {
            get
            {
                return mLogContext ?? (mLogContext = new LogContext());
            }
            set
            {
                mLogContext = value;
            }
        }


        /// <summary>
        /// Specifies code name of the module that is imported or exported as an installable module package.
        /// </summary>
        public string ModuleName
        {
            get
            {
                return ValidationHelper.GetString(GetInfo(ImportExportHelper.MODULE_NAME), String.Empty);
            }
        }


        /// <summary>
        /// Indicates if export or import is being performed as part of installable module package creation or installation.
        /// </summary>
        public bool IsInstallableModule
        {
            get
            {
                return !String.IsNullOrWhiteSpace(ModuleName);
            }
        }


        /// <summary>
        /// Version of the package.
        /// </summary>
        public virtual string Version
        {
            get
            {
                return mVersion;
            }
            set
            {
                mVersion = value;
            }
        }


        /// <summary>
        /// Hotfix version of the package.
        /// </summary>
        public virtual string HotfixVersion
        {
            get;
            set;
        }


        /// <summary>
        /// DataSet with additional information.
        /// </summary>
        public DataSet InfoDataSet
        {
            get
            {
                return mInfoDataSet;
            }
            set
            {
                mInfoDataSet = value;
            }
        }


        /// <summary>
        /// Current user info.
        /// </summary>
        public IUserInfo CurrentUser
        {
            get
            {
                if (mCurrentUser == null)
                {
                    mCurrentUser = CMSActionContext.CurrentUser;
                    if (mCurrentUser.IsPublic())
                    {
                        // Get default administrator user
                        mCurrentUser = Administrator;
                        if (mCurrentUser == null)
                        {
                            throw new Exception("[AbstractImportExportSettings.CurrentUser]: User context is not initialized.");
                        }
                    }
                }

                return mCurrentUser;
            }
            set
            {
                mCurrentUser = value;
            }
        }


        /// <summary>
        /// Event log source.
        /// </summary>
        public string EventLogSource
        {
            get
            {
                return mEventLogSource;
            }
            set
            {
                mEventLogSource = value;
            }
        }


        /// <summary>
        /// Event log code.
        /// </summary>
        public string EventLogCode
        {
            get
            {
                return mEventLogCode;
            }
            set
            {
                mEventLogCode = value;
            }
        }


        /// <summary>
        /// Physical path to the web site root (ends with \).
        /// </summary>
        public string WebsitePath
        {
            get
            {
                // If the path is not initialized
                if (mWebsitePath == null)
                {
                    throw new Exception("[AbstractImportExportSettings]: WebsitePath not initialized!");
                }
                return mWebsitePath;
            }
            set
            {
                if (value != null)
                {
                    value = Path.EnsureEndBackslash(value);
                }
                mWebsitePath = value;
            }
        }


        /// <summary>
        /// Original temporary files path.
        /// </summary>
        public string OriginalTemporaryFilesPath
        {
            get
            {
                return ImportExportHelper.GetTemporaryFolder(WebsitePath);
            }
        }


        /// <summary>
        /// Physical path to the temporary files (ends with \).
        /// </summary>
        public string TemporaryFilesPath
        {
            get
            {
                return mTemporaryFilesPath ?? (mTemporaryFilesPath = OriginalTemporaryFilesPath);
            }
            set
            {
                if (value != null)
                {
                    value = Path.EnsureEndBackslash(value);
                }
                mTemporaryFilesPath = value;
            }
        }


        /// <summary>
        /// Indicates if the process should be canceled.
        /// </summary>
        public bool ProcessCanceled
        {
            get
            {
                return mProcessCanceled;
            }
        }


        /// <summary>
        /// Default object process type.
        /// </summary>
        public ProcessObjectEnum DefaultProcessObjectType
        {
            get
            {
                return mDefaultProcessObjectType;
            }
            set
            {
                mDefaultProcessObjectType = value;
            }
        }


        /// <summary>
        /// Array of expressions. Objects with display names and code names starting with these expresions are filtered out.
        /// </summary>
        public string[] ExcludedNames
        {
            get
            {
                return mExcludedNames;
            }
            set
            {
                mExcludedNames = value;
            }
        }


        /// <summary>
        /// Site ID of the site to be processed.
        /// </summary>
        public int SiteId
        {
            get
            {
                if (SiteInfo != null)
                {
                    return SiteInfo.SiteID;
                }
                return mSiteId;
            }
            set
            {
                mSiteId = value;
                mSiteInfo = null;
            }
        }


        /// <summary>
        /// Site name of the site to be processed.
        /// </summary>
        public string SiteName
        {
            get
            {
                if (SiteInfo != null)
                {
                    return SiteInfo.SiteName;
                }
                return mSiteName;
            }

            set
            {
                mSiteName = value;
                mSiteInfo = null;
            }
        }


        /// <summary>
        /// Site info of the site to be processed.
        /// </summary>
        public SiteInfo SiteInfo
        {
            get
            {
                return mSiteInfo ?? (mSiteInfo = SiteInfoProvider.GetSiteInfo(mSiteName) ?? SiteInfoProvider.GetSiteInfo(mSiteId));
            }
        }


        /// <summary>
        /// Key for presistent storage.
        /// </summary>
        public string PersistentSettingsKey
        {
            get
            {
                return mPersistentSettingsKey;
            }
            set
            {
                mPersistentSettingsKey = value;
            }
        }


        /// <summary>
        /// Indicates if logging to the log is enabled.
        /// </summary>
        public bool WriteLog
        {
            get
            {
                return mWriteLog;
            }
            set
            {
                mWriteLog = value;
            }
        }


        /// <summary>
        /// UI culture.
        /// </summary>
        public CultureInfo UICulture
        {
            get
            {
                return mUICulture ?? (mUICulture = Thread.CurrentThread.CurrentUICulture);
            }
        }


        /// <summary>
        /// Administrator user.
        /// </summary>
        private IUserInfo Administrator
        {
            get
            {
                if (mAdministrator == null)
                {
                    // Try to get administrator from settings
                    var user = ModuleManager.GetReadOnlyObject(PredefinedObjectType.USER).Generalized;
                    mAdministrator = user.GetDefaultObject() as IUserInfo;
                }

                return mAdministrator;
            }
        }


        /// <summary>
        /// Administrator user ID.
        /// </summary>
        public int AdministratorId
        {
            get
            {
                if (mAdministratorId == null)
                {
                    // User was found, store user ID
                    if (Administrator != null)
                    {
                        mAdministratorId = Administrator.UserID;
                    }
                    else
                    {
                        throw new Exception("[AbstractImportExportSettings.AdministratorId]: Unable to resolve user ID.");
                    }
                }

                return mAdministratorId.Value;
            }
            set
            {
                mAdministratorId = value;
            }
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates abstract settings.
        /// </summary>
        /// <param name="currentUser">Current user info</param>
        protected AbstractImportExportSettings(IUserInfo currentUser)
        {
            // Clear the event log
            EventLog = null;

            // Initialize current UI culture
            mUICulture = Thread.CurrentThread.CurrentUICulture;

            // Initialize current user
            CurrentUser = currentUser;

            if (HttpContext.Current != null)
            {
                // Init web site path
                WebsitePath = SystemContext.WebApplicationPhysicalPath;

                // Init additional information which require context
                SetInfo(ImportExportHelper.INFO_MACHINE_NAME, SystemContext.MachineName);
                SetInfo(ImportExportHelper.INFO_DOMAIN_NAME, RequestContext.FullDomain);
            }

            // Init additional information
            SetInfo(ImportExportHelper.INFO_CURRENT_USER, CurrentUser.UserName);
            SetInfo(ImportExportHelper.INFO_START_TIME, DateTime.Now.ToString());
            SetInfo(ImportExportHelper.INFO_SYSTEM_VERSION, CMSVersion.GetVersion(true, true, true, true, true));
            SetInfo(ImportExportHelper.INFO_HOTFIX_VERSION, CMSVersion.HotfixVersion);
            SetInfo(ImportExportHelper.INFO_SYSTEM_WEBAPP, SystemContext.IsWebApplicationProject);
        }

        #endregion


        #region "Public methods"

        /// <summary>
        /// Gets additional settings.
        /// </summary>
        /// <param name="settingsKey">Settings key</param>
        public bool? GetSettings(string settingsKey)
        {
            settingsKey = settingsKey.ToLowerCSafe();

            if (mSettingsHashtable.ContainsKey(settingsKey))
            {
                return mSettingsHashtable[settingsKey];
            }
            return null;
        }


        /// <summary>
        /// Sets additional settings.
        /// </summary>
        /// <param name="settingsKey">Settings key</param>
        /// <param name="value">Value of the settings</param>
        public void SetSettings(string settingsKey, bool value)
        {
            settingsKey = settingsKey.ToLowerCSafe();

            mSettingsHashtable[settingsKey] = value;
        }


        /// <summary>
        /// Gets additional information.
        /// </summary>
        /// <param name="infoKey">Information key</param>
        public object GetInfo(string infoKey)
        {
            infoKey = infoKey.ToLowerCSafe();

            if ((mInfoDataSet != null) && (mInfoDataSet.Tables.Contains(ImportExportHelper.CMS_INFO_TYPE)))
            {
                // Get row with information
                DataRow[] rows = mInfoDataSet.Tables[ImportExportHelper.CMS_INFO_TYPE].Select("Key = '" + infoKey + "'");

                if (rows.Length != 0)
                {
                    return rows[0]["Value"];
                }
            }
            return null;
        }


        /// <summary>
        /// Sets additional information.
        /// </summary>
        /// <param name="infoKey">Information key</param>
        /// <param name="value">Value of the information</param>
        public void SetInfo(string infoKey, object value)
        {
            // Initialize info DataSet
            if (mInfoDataSet == null)
            {
                mInfoDataSet = new DataSet();
                DataTable table = new DataTable(ImportExportHelper.CMS_INFO_TYPE);
                table.Columns.Add("Key");
                table.Columns.Add("Value");
                mInfoDataSet.Tables.Add(table);
            }

            infoKey = infoKey.ToLowerCSafe();
            DataTable infoTable = mInfoDataSet.Tables[ImportExportHelper.CMS_INFO_TYPE];
            DataRow[] rows = infoTable.Select("Key = '" + infoKey + "'");

            // Insert new information
            if (rows.Length == 0)
            {
                mInfoDataSet.Tables[ImportExportHelper.CMS_INFO_TYPE].Rows.Add(new[] { infoKey, value });
            }
            // Update current value
            else
            {
                rows[0]["Value"] = value;
            }
        }


        /// <summary>
        /// Gets selected objects of specified type.
        /// </summary>
        /// <param name="type">Type of the objects</param>
        /// <param name="siteObjects">Indicates if the objects are site dependent</param>
        public List<string> GetSelectedObjects(string type, bool siteObjects)
        {
            type = type.ToLowerCSafe();

            List<string> value;
            SafeDictionary<string, List<string>> values = siteObjects ? mSelectedSiteObjectsHashtable : mSelectedGlobalObjectsHashtable;
            if (values.TryGetValue(type, out value) && value.Count > 0)
            {
                return value;
            }

            return null;
        }


        /// <summary>
        /// Gets selected objects of specified type.
        /// </summary>
        /// <param name="type">Type of the objects</param>
        /// <param name="siteObjects">Indicates if the objects are site dependent</param>
        public string[] GetSelectedObjectsArray(string type, bool siteObjects)
        {
            var list = GetSelectedObjects(type, siteObjects);
            if ((list != null) && (list.Count != 0))
            {
                return list.ToArray();
            }

            return null;
        }


        /// <summary>
        /// Sets selected objects of specified type.
        /// </summary>
        /// <param name="list">List of selected objects code names</param>
        /// <param name="type">Type of the objects</param>
        /// <param name="siteObjects">Indicates if the objects are site dependent</param>
        public void SetSelectedObjects(List<string> list, string type, bool siteObjects)
        {
            type = type.ToLowerCSafe();
            if (siteObjects)
            {
                mSelectedSiteObjectsHashtable[type] = list;
            }
            else
            {
                mSelectedGlobalObjectsHashtable[type] = list;
            }
        }


        /// <summary>
        /// Check if the object with specified code name is selected.
        /// </summary>
        /// <param name="type">Type of the objects</param>
        /// <param name="codeName">Code name of the object</param>
        /// <param name="siteObjects">Indicates if the objects are site dependent</param>
        public bool IsSelected(string type, string codeName, bool siteObjects)
        {
            if (string.IsNullOrEmpty(codeName))
            {
                return false;
            }
            codeName = codeName.ToLowerCSafe();
            type = type.ToLowerCSafe();

            List<string> value;
            SafeDictionary<string, List<string>> values = siteObjects ? mSelectedSiteObjectsHashtable : mSelectedGlobalObjectsHashtable;

            return values.TryGetValue(type, out value) && (value != null) && value.Contains(codeName, StringComparer.InvariantCultureIgnoreCase);
        }


        /// <summary>
        /// Selects the given object.
        /// </summary>
        /// <param name="type">Type of the objects</param>
        /// <param name="codeName">Code name of the object</param>
        /// <param name="siteObjects">Indicates if the objects are site dependent</param>
        public void Select(string type, string codeName, bool siteObjects)
        {
            if (string.IsNullOrEmpty(codeName))
            {
                return;
            }
            codeName = codeName.ToLowerCSafe();

            // Get the current array
            var objects = GetSelectedObjects(type, siteObjects);
            if (objects != null)
            {
                // Add if item missing
                if (!objects.Contains(codeName))
                {
                    objects.Add(codeName);
                }
            }
            else
            {
                // Create new array for selected tasks
                objects = new List<string>();
                objects.Add(codeName);

                SetSelectedObjects(objects, type, siteObjects);
            }
        }


        /// <summary>
        /// Selects the given object.
        /// </summary>
        /// <param name="type">Type of the objects</param>
        /// <param name="codeName">Code name of the object</param>
        /// <param name="siteObjects">Indicates if the objects are site dependent</param>
        public void Deselect(string type, string codeName, bool siteObjects)
        {
            if (string.IsNullOrEmpty(codeName))
            {
                return;
            }

            // Get the current array
            var objects = GetSelectedObjects(type, siteObjects);
            if (objects != null)
            {
                codeName = codeName.ToLowerCSafe();

                // Add if item missing
                if (objects.Contains(codeName))
                {
                    objects.Remove(codeName);
                }
            }
        }


        /// <summary>
        /// Check if the object with specified code name is processed.
        /// </summary>
        /// <param name="objectType">Type of the objects</param>
        /// <param name="codeName">Code name of the object</param>
        /// <param name="siteObjects">Indicates if the objects are site dependent</param>
        public bool IsProcessed(string objectType, string codeName, bool siteObjects)
        {
            ProcessObjectEnum processType = DefaultProcessObjectType;

            // Get process type if not set
            if (processType == ProcessObjectEnum.Default)
            {
                processType = GetObjectsProcessType(objectType, siteObjects);
            }

            // If proccess type is only selected
            if (processType == ProcessObjectEnum.Selected)
            {
                // Check if selected
                return IsSelected(objectType, codeName, siteObjects);
            }
            return (processType != ProcessObjectEnum.None);
        }


        /// <summary>
        /// Check if the object with specified code name is processed.
        /// </summary>
        /// <param name="objectType">Type of the objects</param>
        /// <param name="siteObjects">Indicates if the objects are site dependent</param>
        /// <param name="processType">Explicitly set process type of the object type (Use ProcessObjectEnum.Default as default settings)</param>
        public bool IsObjectTypeProcessed(string objectType, bool siteObjects, ProcessObjectEnum processType)
        {
            if (processType == ProcessObjectEnum.Default)
            {
                processType = GetObjectsProcessType(objectType, siteObjects);
            }

            if (processType != ProcessObjectEnum.None)
            {
                return ((processType != ProcessObjectEnum.Selected) || (GetSelectedObjects(objectType, siteObjects) != null) || (GetSelectedTasks(objectType, siteObjects) != null));
            }

            return false;
        }


        /// <summary>
        /// Gets selected tasks of specified type.
        /// </summary>
        /// <param name="type">Type of the tasks</param>
        /// <param name="siteTasks">Indicates if the tasks are site dependent</param>
        public List<int> GetSelectedTasks(string type, bool siteTasks)
        {
            List<int> value;
            SafeDictionary<string, List<int>> values = siteTasks ? mSelectedSiteTasksHashtable : mSelectedGlobalTasksHashtable;
            if (values.TryGetValue(type, out value) && value.Count > 0)
            {
                return value;
            }

            return null;
        }


        /// <summary>
        /// Gets selected tasks of specified type.
        /// </summary>
        /// <param name="type">Type of the tasks</param>
        /// <param name="siteTasks">Indicates if the tasks are site dependent</param>
        public int[] GetSelectedTasksArray(string type, bool siteTasks)
        {
            var list = GetSelectedTasks(type, siteTasks);
            if ((list != null) && (list.Count != 0))
            {
                return list.ToArray();
            }

            return null;
        }


        /// <summary>
        /// Sets selected objects of specified object type.
        /// </summary>
        /// <param name="list">List of selected tasks code names</param>
        /// <param name="type">Type of the tasks</param>
        /// <param name="siteObjects">Indicates if the tasks are site dependent</param>
        public void SetSelectedTasks(List<int> list, string type, bool siteObjects)
        {
            if (siteObjects)
            {
                mSelectedSiteTasksHashtable[type] = list;
            }
            else
            {
                mSelectedGlobalTasksHashtable[type] = list;
            }
        }


        /// <summary>
        /// Check if the task with specified code name is selected.
        /// </summary>
        /// <param name="type">Type of the tasks</param>
        /// <param name="taskId">Task ID</param>
        /// <param name="siteObjects">Indicates if the tasks are site dependent</param>
        public bool IsTaskSelected(string type, int taskId, bool siteObjects)
        {
            List<int> value;
            SafeDictionary<string, List<int>> values = siteObjects ? mSelectedSiteTasksHashtable : mSelectedGlobalTasksHashtable;

            return values.TryGetValue(type, out value) && value != null && value.Contains(taskId);
        }


        /// <summary>
        /// Selects the given task.
        /// </summary>
        /// <param name="type">Type of the tasks</param>
        /// <param name="taskId">Task ID</param>
        /// <param name="siteObjects">Indicates if the tasks are site dependent</param>
        public void SelectTask(string type, int taskId, bool siteObjects)
        {
            // Get the current array
            var tasks = GetSelectedTasks(type, siteObjects);
            if (tasks != null)
            {
                // Add if item missing
                if (!tasks.Contains(taskId))
                {
                    tasks.Add(taskId);
                }
            }
            else
            {
                // Create new array for selected tasks
                tasks = new List<int>();
                tasks.Add(taskId);

                SetSelectedTasks(tasks, type, siteObjects);
            }
        }


        /// <summary>
        /// Selects the given task.
        /// </summary>
        /// <param name="type">Type of the tasks</param>
        /// <param name="taskId">Task ID</param>
        /// <param name="siteObjects">Indicates if the tasks are site dependent</param>
        public void DeselectTask(string type, int taskId, bool siteObjects)
        {
            // Get the current array
            var tasks = GetSelectedTasks(type, siteObjects);
            if (tasks != null)
            {
                // Add if item missing
                if (tasks.Contains(taskId))
                {
                    tasks.Remove(taskId);
                }
            }
        }


        /// <summary>
        /// Check if there is no selected object or task in general.
        /// </summary>
        public bool IsEmptySelection()
        {
            return IsEmptyObjectSelection() && IsEmptyTaskSelection();
        }


        /// <summary>
        /// Check if there is no selected object in general.
        /// </summary>
        public bool IsEmptyObjectSelection()
        {
            // Site objects
            bool siteObjects = IsEmptyObjectSelection(true);

            // Global objects
            bool globalObjects = IsEmptyObjectSelection(false);

            return siteObjects && globalObjects;
        }


        /// <summary>
        /// Check if there is no selected object.
        /// </summary>
        /// <param name="siteObjects">Indicates if the objects are site dependent</param>
        public bool IsEmptyObjectSelection(bool siteObjects)
        {
            if (siteObjects)
            {
                if (mSelectedSiteObjectsHashtable != null)
                {
                    foreach (DictionaryEntry entry in mSelectedSiteObjectsHashtable)
                    {
                        var objectType = entry.Key.ToString();
                        // Get object type info
                        GeneralizedInfo infoObj = ModuleManager.GetReadOnlyObject(objectType);
                        if (infoObj != null)
                        {
                            // Do not check automatically selected object types
                            if (IsAutomaticallySelected(infoObj))
                            {
                                continue;
                            }

                            // Do not consider site as selected object
                            if (objectType != SiteInfo.OBJECT_TYPE)
                            {
                                if ((entry.Value != null) && (((List<string>)entry.Value).Count != 0))
                                {
                                    return false;
                                }
                            }
                        }
                    }
                }
                return true;
            }

            if (mSelectedGlobalObjectsHashtable != null)
            {
                foreach (DictionaryEntry entry in mSelectedGlobalObjectsHashtable)
                {
                    var objectType = entry.Key.ToString();
                    // Get object type info
                    GeneralizedInfo infoObj = ModuleManager.GetReadOnlyObject(objectType);
                    if (infoObj != null)
                    {
                        // Do not check automatically selected object types
                        if (IsAutomaticallySelected(infoObj))
                        {
                            continue;
                        }

                        if ((entry.Value != null) && (((List<string>)entry.Value).Count != 0))
                        {
                            return false;
                        }
                    }
                }
            }

            return true;
        }


        /// <summary>
        /// Check if there is no selected tasks in general.
        /// </summary>
        public bool IsEmptyTaskSelection()
        {
            return IsEmptyTaskSelection(true) && IsEmptyTaskSelection(false);
        }


        /// <summary>
        /// Check if there is no selected tasks.
        /// </summary>
        /// <param name="siteObjects">Indicates if the tasks are site dependent</param>
        public bool IsEmptyTaskSelection(bool siteObjects)
        {
            if (siteObjects)
            {
                if (mSelectedSiteTasksHashtable != null)
                {
                    foreach (List<int> array in mSelectedSiteTasksHashtable.Values)
                    {
                        if ((array != null) && (array.Count != 0))
                        {
                            return false;
                        }
                    }
                }
                return true;
            }
            else
            {
                if (mSelectedGlobalTasksHashtable != null)
                {
                    foreach (List<int> array in mSelectedGlobalTasksHashtable.Values)
                    {
                        if ((array != null) && (array.Count != 0))
                        {
                            return false;
                        }
                    }
                }
                return true;
            }
        }


        /// <summary>
        /// Gets object types that have been selected to process.
        /// </summary>
        /// <param name="siteObjects">Indicates if the objects are site dependent</param>
        public List<string> GetSelectedObjectTypes(bool siteObjects)
        {
            List<string> list = new List<string>();

            // Get object types for global objects
            if (!siteObjects)
            {
                foreach (string type in mSelectedGlobalObjectsHashtable.Keys)
                {
                    if ((mSelectedGlobalObjectsHashtable[type] != null) && (mSelectedGlobalObjectsHashtable.Count != 0))
                    {
                        list.Add(type);
                    }
                }
                return list;
            }
            else
            {
                // Get object types for site objects
                foreach (string type in mSelectedSiteObjectsHashtable.Keys)
                {
                    // There are selected objects
                    if ((mSelectedSiteObjectsHashtable[type] != null) && (mSelectedSiteObjectsHashtable.Count != 0))
                    {
                        list.Add(type);
                    }
                }
                return list;
            }
        }


        /// <summary>
        /// Gets process type of object type.
        /// </summary>
        /// <param name="type">Type of the object</param>
        /// <param name="siteObjects">Indicates if the objects are site dependent</param>
        public ProcessObjectEnum GetObjectsProcessType(string type, bool siteObjects)
        {
            type = type.ToLowerCSafe();

            Dictionary<string, ProcessObjectEnum> values = siteObjects ? mProcessSiteObjectsHashtable : mProcessGlobalObjectsHashtable;
            ProcessObjectEnum value;
            if (values.TryGetValue(type, out value) && value != ProcessObjectEnum.Default)
            {
                return value;
            }

            return DefaultProcessObjectType;
        }


        /// <summary>
        /// Sets objects process type of specified type.
        /// </summary>
        /// <param name="processType">Process type of the object</param>
        /// <param name="type">Type of the object</param>
        /// <param name="siteObjects">Indicates if the objects are site dependent</param>
        public void SetObjectsProcessType(ProcessObjectEnum processType, string type, bool siteObjects)
        {
            type = type.ToLowerCSafe();
            if (siteObjects)
            {
                mProcessSiteObjectsHashtable[type] = processType;
            }
            else
            {
                mProcessGlobalObjectsHashtable[type] = processType;
            }
        }


        /// <summary>
        /// Indicates if given object type is automatically selected.
        /// </summary>
        /// <param name="infoObj">Object instance</param>
        public bool IsAutomaticallySelected(GeneralizedInfo infoObj)
        {
            return infoObj.TypeInfo.ImportExportSettings.IsAutomaticallySelected;
        }


        /// <summary>
        /// Gets the where condition for specified type of object.
        /// </summary>
        /// <param name="objectType">Object type</param>
        /// <param name="siteObject">Site object</param>
        public WhereCondition GetObjectWhereCondition(string objectType, bool siteObject)
        {
            var where = new WhereCondition();

            GeneralizedInfo infoObj = ModuleManager.GetReadOnlyObject(objectType);

            var ti = infoObj.TypeInfo;

            // Exclude objects by name
            if (ExcludedNames != null)
            {
                string codeNameColumn = ti.CodeNameColumn;
                string displayNameColumn = ti.DisplayNameColumn;

                // Filter by code name column
                if (codeNameColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN)
                {
                    where.Where(ObjectHelper.GetExcludedNamesWhereCondition(ExcludedNames, codeNameColumn));
                }

                // Filter by display name column
                if (displayNameColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN)
                {
                    where.Where(ObjectHelper.GetExcludedNamesWhereCondition(ExcludedNames, displayNameColumn));
                }
            }

            // Get basic object type where condition
            string exportWhere = ti.ImportExportSettings.WhereCondition;

            where.Where(exportWhere);

            var e = new GetObjectWhereConditionArgs
                {
                    Settings = this,
                    ObjectType = objectType,
                    SiteObjects = siteObject,
                    Where = where
                };

            // Handle the event
            SpecialActionsEvents.GetObjectWhereCondition.StartEvent(e);

            where = e.Where;

            if (!e.CombineWhereCondition)
            {
                return where;
            }

            // Site ID
            if (ti.SiteIDColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN)
            {
                if (siteObject)
                {
                    where.WhereEquals(ti.SiteIDColumn, SiteId);
                }
                else
                {
                    where.WhereNull(ti.SiteIDColumn);
                }
            }

            // Group ID
            if (ti.GroupIDColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN)
            {
                where.WhereNull(ti.GroupIDColumn);
            }

            return where;
        }


        /// <summary>
        /// Gets file path within the package for object type.
        /// </summary>
        /// <param name="objectType">Type of the object</param>
        /// <param name="siteObjects">Indicates if the object is site dependent</param>
        public string GetObjectTypeFilePath(string objectType, bool siteObjects)
        {
            string safeObjectType = ImportExportHelper.GetSafeObjectTypeName(objectType);

            // Prepare the path
            string filePath = DirectoryHelper.CombinePath(TemporaryFilesPath, ImportExportHelper.DATA_FOLDER) + "\\";

            // Get object type subfolder
            filePath += ImportExportHelper.GetObjectTypeSubFolder(this, objectType, siteObjects);
            filePath += safeObjectType + ".xml";

            return filePath;
        }
        

        /// <summary>
        /// Indicates if current version is lower than given one.
        /// </summary>
        /// <param name="version">Version to compare</param>
        /// <param name="hotfixVersion">Hotfix version</param>
        public bool IsLowerVersion(string version, string hotfixVersion = null)
        {
            string key = string.Format("{0}_{1}_{2}_{3}", Version.ToLowerCSafe(), HotfixVersion.ToLowerCSafe(), version.ToLowerCSafe(), hotfixVersion.ToLowerCSafe());

            // Store result
            if (!mVersionsComparations.ContainsKey(key))
            {
                mVersionsComparations[key] = ImportExportHelper.IsLowerVersion(Version, HotfixVersion, version, hotfixVersion);
            }

            return mVersionsComparations[key];
        }

        /// <summary>
        /// Indicates whether import package version is supported. Import of unsupported package version may result in error.
        /// Unsupported version is a version which is two or more major versions behind the current one.
        /// </summary>
        /// <returns>Return true when package version is not supported for import.</returns>
        public bool IsUnsupportedVersion()
        {
            return IsLowerVersion($"{CMSVersion.Version.Major - SUPPORTED_MAJOR_VERSIONS_RANGE}.0");
        }


        /// <summary>
        /// Cancels the process.
        /// </summary>
        public void Cancel()
        {
            mProcessCanceled = true;
        }


        /// <summary>
        /// Saves the log to the persistent storage.
        /// </summary>
        public void SavePersistentLog()
        {
            if (mPersistentSettingsKey != null)
            {
                try
                {
                    PersistentStorageHelper.SetValue(mPersistentSettingsKey + "_log", LogContext.Log);
                }
                catch (Exception ex)
                {
                    EventLogProvider.LogException("IMPORTEXPORT", "SavePersistentLog", ex);
                }
            }
        }


        /// <summary>
        /// Saves the settings object to the persistent storage.
        /// </summary>
        public void SavePersistent()
        {
            if (mPersistentSettingsKey != null)
            {
                try
                {
                    // Save the settings
                    PersistentStorageHelper.SetValue(mPersistentSettingsKey, this);

                    // Save the log
                    SavePersistentLog();
                }
                catch (Exception ex)
                {
                    EventLogProvider.LogException("IMPORTEXPORT", "SavePersistent", ex);
                }
            }
        }


        /// <summary>
        /// Gets the settings from the persistent storage.
        /// </summary>
        /// <param name="key">Key to the persistent storage</param>
        public static AbstractImportExportSettings GetFromPersistentStorage(string key)
        {
            if (key != null)
            {
                var settings = (AbstractImportExportSettings)PersistentStorageHelper.GetValue(key);
                if ((settings != null) && String.IsNullOrEmpty(settings.LogContext.Log))
                {
                    settings.LogContext.Log = (string)PersistentStorageHelper.GetValue(key + "_log");
                }

                return settings;
            }

            return null;
        }

        #endregion


        #region "Logging methods"

        /// <summary>
        /// Gets progress state.
        /// </summary>
        public LogStatusEnum GetProgressState()
        {
            return mProgressState;
        }


        /// <summary>
        /// Clears progress log.
        /// </summary>
        public void ClearProgressLog()
        {
            mIsError = false;
            mIsWarning = false;
            mProgressState = LogStatusEnum.Info;

            if (mLogContext != null)
            {
                mLogContext.Clear();
            }
        }


        /// <summary>
        /// Logs progress state.
        /// </summary>
        /// <param name="type">Type of the message</param>
        /// <param name="message">Message to be logged</param>
        public void LogProgressState(LogStatusEnum type, string message)
        {
            // Do not log standard info events if disabled
            switch (type)
            {
                case LogStatusEnum.Info:
                    if (!WriteLog)
                    {
                        return;
                    }
                    break;
            }

            bool logError = false;
            bool logWarning = false;

            string logMessage = null;
            string messageType = null;

            switch (type)
            {
                case LogStatusEnum.Info:
                case LogStatusEnum.Start:
                    logMessage = message;
                    break;

                case LogStatusEnum.Error:
                    {
                        messageType = "##ERROR##";
                        logMessage = "<strong>" + ResHelper.GetAPIString("Global.ErrorSign", "ERROR:") + "&nbsp;</strong>" + message;
                        logError = true;
                        mIsError = true;
                        mProgressState = LogStatusEnum.Error;
                    }
                    break;

                case LogStatusEnum.Warning:
                    {
                        messageType = "##WARNING##";
                        logMessage = "<strong>" + ResHelper.GetAPIString("Global.Warning", "WARNING:") + "&nbsp;</strong>" + message;
                        logWarning = true;
                        mIsWarning = true;
                        if (mProgressState != LogStatusEnum.Error)
                        {
                            mProgressState = LogStatusEnum.Warning;
                        }
                    }
                    break;

                case LogStatusEnum.Finish:
                case LogStatusEnum.UnexpectedFinish:
                    logMessage = "<strong>" + message + "</strong>";
                    mProgressState = LogStatusEnum.Finish;
                    break;
            }
            
            // Log to context
            LogContext.AppendText(messageType + logMessage);

            // Log into EventLog
            using (var context = new CMSActionContext())
            {
                // Enable logging into EventLog
                context.LogEvents = true;

                // If there is an EventLog
                if (EventLog != null)
                {
                    EventLog.EventDescription = logMessage + "<br />" + EventLog.EventDescription;

                    // Create site event log only if single object is exported
                    if ((SiteId != 0) && ValidationHelper.GetBoolean(GetInfo(ImportExportHelper.INFO_SINGLE_OBJECT), false))
                    {
                        EventLog.SiteID = SiteId;
                    }

                    if (logWarning)
                    {
                        EventLog.EventType = EventType.WARNING;
                    }

                    if (logError)
                    {
                        EventLog.EventType = EventType.ERROR;
                    }
                }
                else
                {
                    EventLogProvider.LogEvent(EventType.ERROR, EventLogSource, EventLogCode + "ERROR", "Logging process unexpectly stopped", "", CurrentUser.UserID, CurrentUser.UserName, 0, "", "", SiteId);
                }
            }
        }


        /// <summary>
        /// Logs error progress state.
        /// </summary>
        /// <param name="description">Message to be logged</param>
        /// <param name="ex">Exception to be logged</param>
        protected void LogProgressError(string description, Exception ex)
        {
            string message = ImportExportHelper.GetFormattedErrorMessage(description, ex);

            // Log progress
            LogProgressState(LogStatusEnum.Error, message);
        }


        /// <summary>
        /// Creates new event log.
        /// </summary>
        private EventLogInfo CreateEventLog()
        {
            EventLogInfo newEvent = null;

            try
            {
                // New event declaration
                newEvent = new EventLogInfo();

                newEvent.EventType = EventType.INFORMATION;
                newEvent.EventTime = DateTime.Now;
                newEvent.Source = TextHelper.LimitLength(EventLogSource, 100);
                newEvent.EventCode = TextHelper.LimitLength(EventLogCode, 100);
                newEvent.UserID = CurrentUser.UserID;
                newEvent.UserName = TextHelper.LimitLength(CurrentUser.UserName, 250, "");
                newEvent.EventDescription = "";
                newEvent.IPAddress = TextHelper.LimitLength(RequestContext.UserHostAddress, 100);
                newEvent.EventMachineName = TextHelper.LimitLength(SystemContext.MachineName, 100);
            }
            catch
            {
                // Unable to log into eventlog
            }

            return newEvent;
        }


        /// <summary>
        /// Logs event object into system.
        /// </summary>
        internal void FinalizeEventLog()
        {
            if (EventLog != null)
            {
                EventLogProvider.LogEvent(EventLog);
            }
        }


        /// <summary>
        /// Returns true if there are warnings during the process.
        /// </summary>
        public bool IsWarning()
        {
            return mIsWarning;
        }


        /// <summary>
        /// Returns true if there is an error during the process.
        /// </summary>
        public bool IsError()
        {
            return mIsError;
        }


        /// <summary>
        /// Gets resource string in correct culture.
        /// </summary>
        /// <param name="key">Resource string key</param>
        /// <param name="defaultValue">Default value</param>
        public string GetAPIString(string key, string defaultValue)
        {
            return ResHelper.GetAPIString(key, UICulture.Name, defaultValue);
        }

        #endregion


        #region "ISerializable Members"

        /// <summary>
        /// Serialization function.
        /// </summary>
        /// <param name="info">Serialization info</param>
        /// <param name="ctxt">Streaming context</param>
        public virtual void GetObjectData(SerializationInfo info, StreamingContext ctxt)
        {
            info.AddValue("SelectedGlobalObjectsHashtable", mSelectedGlobalObjectsHashtable);
            info.AddValue("SelectedSiteObjectsHashtable", mSelectedSiteObjectsHashtable);

            info.AddValue("SelectedGlobalTasksHashtable", mSelectedGlobalTasksHashtable);
            info.AddValue("SelectedSiteTasksHashtable", mSelectedSiteTasksHashtable);

            info.AddValue("ProcessGlobalObjectsHashtable", mProcessGlobalObjectsHashtable);
            info.AddValue("ProcessSiteObjectsHashtable", mProcessSiteObjectsHashtable);
            info.AddValue("VersionsComparations", mVersionsComparations);

            info.AddValue("WebsitePath", mWebsitePath);
            info.AddValue("ProcessCancelled", mProcessCanceled);
            info.AddValue("DefaultProcessObjectType", mDefaultProcessObjectType);
            info.AddValue("Settings", mSettingsHashtable);

            // Log is stored separately
            //info.AddValue("ProgressLog", mProgressLog);

            info.AddValue("EventLogCode", mEventLogCode);
            info.AddValue("EventLogSource", mEventLogSource);
            info.AddValue("SiteID", mSiteId);
            info.AddValue("SiteName", mSiteName);
            info.AddValue("TemporaryFilesPath", mTemporaryFilesPath);
            info.AddValue("InfoDataSet", mInfoDataSet);
            info.AddValue("PersistentSettingsKey", mPersistentSettingsKey);
            info.AddValue("WriteLog", mWriteLog);
            info.AddValue("Version", mVersion);

            // All the info objects (LogInfo, CurrentUser, etc.) cannot be logged because it's not serializable
        }


        /// <summary>
        /// Constructor - Constructor for deserialization.
        /// </summary>
        /// <param name="info">Serialization info</param>
        /// <param name="ctxt">Streaming context</param>
        public AbstractImportExportSettings(SerializationInfo info, StreamingContext ctxt)
        {
            mSelectedGlobalObjectsHashtable = (SafeDictionary<string, List<string>>)info.GetValue("SelectedGlobalObjectsHashtable", typeof(SafeDictionary<string, List<string>>));
            mSelectedSiteObjectsHashtable = (SafeDictionary<string, List<string>>)info.GetValue("SelectedSiteObjectsHashtable", typeof(SafeDictionary<string, List<string>>));

            mSelectedGlobalTasksHashtable = (SafeDictionary<string, List<int>>)info.GetValue("SelectedGlobalTasksHashtable", typeof(SafeDictionary<string, List<int>>));
            mSelectedSiteTasksHashtable = (SafeDictionary<string, List<int>>)info.GetValue("SelectedSiteTasksHashtable", typeof(SafeDictionary<string, List<int>>));

            mProcessGlobalObjectsHashtable = (Dictionary<string, ProcessObjectEnum>)info.GetValue("ProcessGlobalObjectsHashtable", typeof(Dictionary<string, ProcessObjectEnum>));
            mProcessSiteObjectsHashtable = (Dictionary<string, ProcessObjectEnum>)info.GetValue("ProcessSiteObjectsHashtable", typeof(Dictionary<string, ProcessObjectEnum>));
            mVersionsComparations = (Dictionary<string, bool>)info.GetValue("VersionsComparations", typeof(Dictionary<string, bool>));

            mDefaultProcessObjectType = (ProcessObjectEnum)info.GetValue("DefaultProcessObjectType", typeof(ProcessObjectEnum));
            mWebsitePath = info.GetString("WebsitePath");
            mProcessCanceled = info.GetBoolean("ProcessCancelled");
            mSettingsHashtable = (Dictionary<string, bool>)info.GetValue("Settings", typeof(Dictionary<string, bool>));

            // Log is stored separately
            //mProgressLog = info.GetString("ProgressLog");

            mEventLogCode = info.GetString("EventLogCode");
            mEventLogSource = info.GetString("EventLogSource");
            mSiteId = info.GetInt32("SiteID");
            mSiteName = info.GetString("SiteName");
            mTemporaryFilesPath = info.GetString("TemporaryFilesPath");
            mInfoDataSet = (DataSet)info.GetValue("InfoDataSet", typeof(DataSet));
            mPersistentSettingsKey = info.GetString("PersistentSettingsKey");
            mWriteLog = info.GetBoolean("WriteLog");
            mVersion = info.GetString("Version");

            // All the info objects (LogInfo, CurrentUser, etc.) cannot be logged because it's not serializable
        }

        #endregion
    }
}