using System.Collections.Generic;

using CMS.Base;

namespace CMS.DataEngine
{
    /// <summary>
    /// Encapsulates most general settings for staging and integration bus.
    /// </summary>
    public abstract class AbstractSynchronizationSettings
    {
        private TaskTypeEnum mTaskType = TaskTypeEnum.Unknown;

        /// <summary>
        /// Type of the task.
        /// </summary>
        public TaskTypeEnum TaskType
        {
            get
            {
                return mTaskType;
            }
            set
            {
                mTaskType = value;
            }
        }

        /// <summary>
        /// Indicates if in context of worker call (asynchronous task logging).
        /// </summary>
        public bool WorkerCall
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates if task logging should be executed asynchronously (in separate thread). By default true.
        /// </summary>
        public bool RunAsynchronously
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates if the staging task should be logged. By default true.
        /// </summary>
        public bool LogStaging
        {
            get;
            set;
        }


        /// <summary>
        /// ID of the staging server where the staging task will be logged.
        /// </summary>
        public virtual int ServerID
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates if the integration task should be logged. By default true.
        /// </summary>
        public bool LogIntegration
        {
            get;
            set;
        }


        /// <summary>
        /// Staging task groups to which the staging task belongs.
        /// </summary>
        public IEnumerable<IStagingTaskGroup> TaskGroups
        {
            get;
            set;
        }


        /// <summary>
        /// <see cref="IUserInfo"/> that will be logged with staging task.
        /// If equals to null, no user will be logged with staging task.
        /// </summary>
        public IUserInfo User
        {
            get;
            set;
        }


        /// <summary>
        /// Constructor for integration bus and staging settings.
        /// </summary>
        protected AbstractSynchronizationSettings()
        {
            LogIntegration = true;
            LogStaging = true;

            RunAsynchronously = true;
        }
    }
}
