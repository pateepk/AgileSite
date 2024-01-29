namespace CMS.DataEngine
{
    /// <summary>
    /// Class for log object change.
    /// </summary>
    public class LogObjectChangeSettings : AbstractSynchronizationSettings
    {
        #region "Constants"

        /// <summary>
        /// Constant used to specify all the enabled servers.
        /// </summary>
        public const int ENABLED_SERVERS = -1;

        #endregion


        #region "Variables"

        private int? mServerID;

        private bool mLogIntegrationSimpleTasks = true;
        private bool mCreateVersion = true;
        private bool mLogExportTask = true;
        private bool mDataChanged = true;

        private int mSiteId = -1;

        #endregion


        #region "Properties"

        /// <summary>
        /// Info object.
        /// </summary>
        public GeneralizedInfo InfoObj
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates if the simple integration task should be logged. Default true.
        /// </summary>
        public bool LogIntegrationSimpleTasks
        {
            get
            {
                return mLogIntegrationSimpleTasks;
            }
            set
            {
                mLogIntegrationSimpleTasks = value;
            }
        }


        /// <summary>
        /// Indicates if the log request originates from touching the parent object. Default false.
        /// </summary>
        public bool IsTouchParent
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates id the export task should be logged. Default true.
        /// </summary>
        public bool LogExportTask
        {
            get
            {
                return mLogExportTask;
            }
            set
            {
                mLogExportTask = value;
            }
        }


        /// <summary>
        /// Indicates if the version should be created. Default true.
        /// </summary>
        public bool CreateVersion
        {
            get
            {
                return mCreateVersion;
            }
            set
            {
                mCreateVersion = value;
            }
        }


        /// <summary>
        /// Site ID of the servers.
        /// </summary>
        public int SiteID
        {
            get
            {
                if (mSiteId < 0)
                {
                    mSiteId = InfoObj.ObjectSiteID;
                }

                return mSiteId;
            }
            set
            {
                mSiteId = value;
            }
        }


        /// <summary>
        /// ID of staging server (Or SynchronizationInfoProvider.ENABLED_SERVERS for all enabled servers)
        /// </summary>
        public override int ServerID
        {
            get
            {
                if (mServerID == null)
                {
                    mServerID = ENABLED_SERVERS;
                }

                return mServerID.Value;
            }
            set
            {
                mServerID = value;
            }
        }


        /// <summary>
        /// If true, the data of the object involved has changed. Default true.
        /// </summary>
        public bool DataChanged
        {
            get
            {
                return mDataChanged;
            }
            set
            {
                mDataChanged = value;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="obj">Object</param>
        /// <param name="taskType">Task type</param>
        public LogObjectChangeSettings(GeneralizedInfo obj, TaskTypeEnum taskType)
            :base()
        {
            InfoObj = obj;
            TaskType = taskType;
        }


        /// <summary>
        /// Gets the duplicity key.
        /// </summary>
        /// <param name="keyPrefix">Key prefix</param>
        public string GetDuplicityKey(string keyPrefix)
        {
            return string.Format("{0}|{1}|{2}|{3}|{4}|{5}|{6}|{7}|{8}|{9}|{10}", keyPrefix, TaskType, InfoObj.GetObjectKey(), LogStaging, LogIntegration, LogIntegrationSimpleTasks, LogExportTask, CreateVersion, SiteID, ServerID, IsTouchParent);
        }

        #endregion
    }
}