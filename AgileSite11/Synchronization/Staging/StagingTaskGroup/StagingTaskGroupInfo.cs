using System;
using System.Data;
using System.Runtime.Serialization;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Synchronization;
using CMS.Base;

[assembly: RegisterObjectType(typeof(TaskGroupInfo), TaskGroupInfo.OBJECT_TYPE)]

namespace CMS.Synchronization
{
    /// <summary>
    /// TaskGroupInfo data container class.
    /// </summary>
    [Serializable]
    public class TaskGroupInfo : AbstractInfo<TaskGroupInfo>, IStagingTaskGroup
    {
        #region "Constants"

        /// <summary>
        /// Permission name that allows user to manage task groups(create, edit, delete).
        /// </summary>
        public const string PERMISSION_MANAGE_TASK_GROUPS = "ManageTaskGroups";

        #endregion


        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        [NonSerialized]
        public const string OBJECT_TYPE = "staging.taskgroup";


        /// <summary>
        /// Type information.
        /// </summary>
        [NonSerialized]
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(TaskGroupInfoProvider), OBJECT_TYPE, "staging.TaskGroup", "TaskGroupID", null, "TaskGroupGuid", "TaskGroupCodeName", null, null, null, null, null)
        {
            SupportsCloning = false,
            ModuleName = "CMS.Staging",
            TouchCacheDependencies = true,
            ImportExportSettings =
            {
                IsExportable = false,
            },
            SynchronizationSettings =
            {
                LogSynchronization = SynchronizationTypeEnum.None,
            },
            MaxCodeNameLength = 50
        };

        #endregion


        #region "Properties"

        /// <summary>
        /// Task group ID
        /// </summary>
        [DatabaseField]
        public virtual int TaskGroupID
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("TaskGroupID"), 0);
            }
            set
            {
                SetValue("TaskGroupID", value);
            }
        }


        /// <summary>
        /// Task group description
        /// </summary>
        [DatabaseField]
        public virtual string TaskGroupDescription
        {
            get
            {
                return ValidationHelper.GetString(GetValue("TaskGroupDescription"), String.Empty);
            }
            set
            {
                SetValue("TaskGroupDescription", value);
            }
        }


        /// <summary>
        /// Task group code name
        /// </summary>
        [DatabaseField]
        public virtual string TaskGroupCodeName
        {
            get
            {
                return ValidationHelper.GetString(GetValue("TaskGroupCodeName"), String.Empty);
            }
            set
            {
                SetValue("TaskGroupCodeName", value);
            }
        }


        /// <summary>
        /// Task group guid
        /// </summary>
        [DatabaseField]
        public virtual Guid TaskGroupGuid
        {
            get
            {
                return ValidationHelper.GetGuid(GetValue("TaskGroupGuid"), Guid.Empty);
            }
            set
            {
                SetValue("TaskGroupGuid", value);
            }
        }

        #endregion


        #region "Type based properties and methods"

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
                    allowed = userInfo.IsAuthorizedPerResource(TypeInfo.ModuleName, PERMISSION_MANAGE_TASK_GROUPS, siteName, false);
                    break;
            }

            return allowed || base.CheckPermissionsInternal(permission, siteName, userInfo, exceptionOnFailure);
        }


        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            TaskGroupInfoProvider.DeleteTaskGroupInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            TaskGroupInfoProvider.SetTaskGroupInfo(this);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor for de-serialization.
        /// </summary>
        /// <param name="info">Serialization info</param>
        /// <param name="context">Streaming context</param>
        public TaskGroupInfo(SerializationInfo info, StreamingContext context)
            : base(info, context, TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates an empty TaskGroupInfo object.
        /// </summary>
        public TaskGroupInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new TaskGroupInfo object from the given DataRow.
        /// </summary>
        /// <param name="dr">DataRow with the object data</param>
        public TaskGroupInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion
    }
}