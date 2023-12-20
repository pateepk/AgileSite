using System;
using System.Data;
using System.Runtime.Serialization;
using System.Collections.Generic;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Synchronization;

[assembly: RegisterObjectType(typeof(TaskGroupUserInfo), TaskGroupUserInfo.OBJECT_TYPE)]

namespace CMS.Synchronization
{
    /// <summary>
    /// TaskGroupUserInfo data container class.
    /// </summary>
    [Serializable]
    public class TaskGroupUserInfo : AbstractInfo<TaskGroupUserInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "staging.taskgroupuser";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(TaskGroupUserInfoProvider), OBJECT_TYPE, "staging.TaskGroupUser", "TaskGroupUserID", null, null, null, null, null, null, "TaskGroupID", TaskGroupInfo.OBJECT_TYPE)
        {
            IsBinding = true,
            ModuleName = "CMS.Staging",
            TouchCacheDependencies = true,
            DependsOn = new List<ObjectDependency>() 
			{
			    new ObjectDependency("UserID", "cms.user", ObjectDependencyEnum.Binding), 
            },
            ImportExportSettings =
            {
                IsExportable = false,
            },
            SynchronizationSettings =
            {
                LogSynchronization = SynchronizationTypeEnum.None,
            },
        };

        #endregion


        #region "Properties"

        /// <summary>
        /// Task group user ID
        /// </summary>
        [DatabaseField]
        public virtual int TaskGroupUserID
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("TaskGroupUserID"), 0);
            }
            set
            {
                SetValue("TaskGroupUserID", value);
            }
        }


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
        /// User ID
        /// </summary>
        [DatabaseField]
        public virtual int UserID
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("UserID"), 0);
            }
            set
            {
                SetValue("UserID", value);
            }
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            TaskGroupUserInfoProvider.DeleteTaskGroupUserInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            TaskGroupUserInfoProvider.SetTaskGroupUserInfo(this);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor for de-serialization.
        /// </summary>
        /// <param name="info">Serialization info</param>
        /// <param name="context">Streaming context</param>
        public TaskGroupUserInfo(SerializationInfo info, StreamingContext context)
            : base(info, context, TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates an empty TaskGroupUserInfo object.
        /// </summary>
        public TaskGroupUserInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new TaskGroupUserInfo object from the given DataRow.
        /// </summary>
        /// <param name="dr">DataRow with the object data</param>
        public TaskGroupUserInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion
    }
}