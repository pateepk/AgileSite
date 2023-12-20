using System;
using System.Data;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Base;
using CMS.Synchronization;

[assembly: RegisterObjectType(typeof(StagingTaskInfo), StagingTaskInfo.OBJECT_TYPE)]

namespace CMS.Synchronization
{
    /// <summary>
    /// TaskInfo data container class.
    /// </summary>
    public class StagingTaskInfo : AbstractInfo<StagingTaskInfo>, ISynchronizationTask
    {
        #region "Constants"
        
        /// <summary>
        /// Permission name that allows user manage all kind of staging tasks.
        /// </summary>
        public const string PERMISSION_MANAGE_ALL_TASKS = "ManageAllTasks";


        /// <summary>
        /// Permission name that allows user manage document staging tasks.
        /// </summary>
        public const string PERMISSION_MANAGE_DOCUMENT_TASKS = "ManageDocumentsTasks";


        /// <summary>
        /// Permission name that allows user manage object staging tasks.
        /// </summary>
        public const string PERMISSION_MANAGE_OBJECT_TASKS = "ManageObjectsTasks";


        /// <summary>
        /// Permission name that allows user manage custom tables staging tasks.
        /// </summary>
        public const string PERMISSION_MANAGE_DATA_TASKS = "ManageDataTasks";

        #endregion


        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "staging.task";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(StagingTaskInfoProvider), OBJECT_TYPE, "Staging.Task", "TaskID", null, null, null, "TaskTitle", null, "TaskSiteID", null, null)
        {
            SupportsVersioning = false,
            LogIntegration = false,
            AllowRestore = false,
            SupportsGlobalObjects = true,
            ModuleName = "cms.staging",
            MacroCollectionName = "StagingTask",
        };

        #endregion


        #region "Properties"

        /// <summary>
        /// Task node alias path.
        /// </summary>
        [DatabaseField]
        public virtual string TaskNodeAliasPath
        {
            get
            {
                return GetStringValue("TaskNodeAliasPath", "");
            }
            set
            {
                SetValue("TaskNodeAliasPath", value);
            }
        }


        /// <summary>
        /// Task type.
        /// </summary>
        [DatabaseField(ValueType = typeof(string))]
        public virtual TaskTypeEnum TaskType
        {
            get
            {
                return TaskHelper.GetTaskTypeEnum(ValidationHelper.GetString(GetValue("TaskType"), ""));
            }
            set
            {
                SetValue("TaskType", TaskHelper.GetTaskTypeString(value));
            }
        }


        /// <summary>
        /// Task document ID.
        /// </summary>
        [DatabaseField]
        public virtual int TaskDocumentID
        {
            get
            {
                return GetIntegerValue("TaskDocumentID", 0);
            }
            set
            {
                if (value <= 0)
                {
                    SetValue("TaskDocumentID", null);
                }
                else
                {
                    SetValue("TaskDocumentID", value);
                }
            }
        }


        /// <summary>
        /// Task node ID.
        /// </summary>
        [DatabaseField]
        public virtual int TaskNodeID
        {
            get
            {
                return GetIntegerValue("TaskNodeID", 0);
            }
            set
            {
                if (value <= 0)
                {
                    SetValue("TaskNodeID", null);
                }
                else
                {
                    SetValue("TaskNodeID", value);
                }
            }
        }


        /// <summary>
        /// Task title.
        /// </summary>
        [DatabaseField]
        public virtual string TaskTitle
        {
            get
            {
                return GetStringValue("TaskTitle", "");
            }
            set
            {
                SetValue("TaskTitle", value);
            }
        }


        /// <summary>
        /// Task data.
        /// </summary>
        [DatabaseField]
        public virtual string TaskData
        {
            get
            {
                return GetStringValue("TaskData", "");
            }
            set
            {
                SetValue("TaskData", value);
            }
        }


        /// <summary>
        /// Task time.
        /// </summary>
        [DatabaseField]
        public virtual DateTime TaskTime
        {
            get
            {
                return GetDateTimeValue("TaskTime", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("TaskTime", value);
            }
        }


        /// <summary>
        /// Task ID.
        /// </summary>
        [DatabaseField]
        public virtual int TaskID
        {
            get
            {
                return GetIntegerValue("TaskID", 0);
            }
            set
            {
                SetValue("TaskID", value);
            }
        }


        /// <summary>
        /// Task Site ID.
        /// </summary>
        [DatabaseField]
        public virtual int TaskSiteID
        {
            get
            {
                return GetIntegerValue("TaskSiteID", 0);
            }
            set
            {
                if (value <= 0)
                {
                    SetValue("TaskSiteID", null);
                }
                else
                {
                    SetValue("TaskSiteID", value);
                }
            }
        }


        /// <summary>
        /// Task object type.
        /// </summary>
        [DatabaseField]
        public virtual string TaskObjectType
        {
            get
            {
                return GetStringValue("TaskObjectType", "");
            }
            set
            {
                SetValue("TaskObjectType", value);
            }
        }


        /// <summary>
        /// Task object ID.
        /// </summary>
        [DatabaseField]
        public virtual int TaskObjectID
        {
            get
            {
                return GetIntegerValue("TaskObjectID", 0);
            }
            set
            {
                if (value <= 0)
                {
                    SetValue("TaskObjectID", null);
                }
                else
                {
                    SetValue("TaskObjectID", value);
                }
            }
        }


        /// <summary>
        /// Task is running.
        /// </summary>
        [DatabaseField]
        public virtual bool TaskRunning
        {
            get
            {
                return GetBooleanValue("TaskRunning", false);
            }
            set
            {
                SetValue("TaskRunning", value);
            }
        }


        /// <summary>
        /// List of tasks servers separated by ';', eg. ';server1;server2;server3;'.
        /// </summary>
        [DatabaseField]
        public virtual string TaskServers
        {
            get
            {
                return GetStringValue("TaskServers", "");
            }
            set
            {
                SetValue("TaskServers", value);
            }
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            StagingTaskInfoProvider.DeleteTaskInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            StagingTaskInfoProvider.SetTaskInfo(this);
        }


        /// <summary>
        /// Checks the permissions of the object.
        /// </summary>
        /// <param name="permission">Permission type</param>
        /// <param name="siteName">Name of the site</param>
        /// <param name="userInfo">UserInfo object</param>
        /// <param name="exceptionOnFailure">If true, PermissionCheckException is thrown whenever a permission check fails</param>
        protected override bool CheckPermissionsInternal(PermissionsEnum permission, string siteName, IUserInfo userInfo, bool exceptionOnFailure)
        {
            bool allowed = false;
            switch (permission)
            {
                case PermissionsEnum.Read:
                case PermissionsEnum.Create:
                case PermissionsEnum.Delete:
                case PermissionsEnum.Modify:
                    allowed = userInfo.IsAuthorizedPerResource(TypeInfo.ModuleName, PERMISSION_MANAGE_ALL_TASKS, siteName, false);
                    break;
            }

            return allowed || base.CheckPermissionsInternal(permission, siteName, userInfo, exceptionOnFailure);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty TaskInfo object.
        /// </summary>
        public StagingTaskInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new TaskInfo object from the given DataRow.
        /// </summary>
        public StagingTaskInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Indicates if task was already processed by particular server.
        /// </summary>
        /// <param name="serverName">Server name to be checked</param>
        public bool WasProcessed(string serverName)
        {
            return TaskServers.ToLowerCSafe().Contains(";" + serverName.ToLowerCSafe() + ";");
        }

        #endregion
    }
}