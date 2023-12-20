using System;
using System.Data;
using System.Runtime.Serialization;
using System.Collections.Generic;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Synchronization;

[assembly: RegisterObjectType(typeof(TaskGroupTaskInfo), TaskGroupTaskInfo.OBJECT_TYPE)]

namespace CMS.Synchronization
{
    /// <summary>
    /// TaskGroupTaskInfo data container class.
    /// </summary>
    [Serializable]
    public class TaskGroupTaskInfo : AbstractInfo<TaskGroupTaskInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "staging.TaskGroupTask";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(TaskGroupTaskInfoProvider), OBJECT_TYPE, "Staging.TaskGroupTask", "TaskGroupTaskID", null, null, null, null, null, null, "TaskID", StagingTaskInfo.OBJECT_TYPE)
        {
            IsBinding = true,
            ModuleName = "CMS.Staging",
            TouchCacheDependencies = true,
            DependsOn = new List<ObjectDependency>() 
			{
			    new ObjectDependency("TaskGroupID", TaskGroupInfo.OBJECT_TYPE, ObjectDependencyEnum.Binding), 
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
        /// Task group task ID
        /// </summary>
        [DatabaseField]
        public virtual int TaskGroupTaskID
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("TaskGroupTaskID"), 0);
            }
            set
            {
                SetValue("TaskGroupTaskID", value);
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
        /// Task ID
        /// </summary>
        [DatabaseField]
        public virtual int TaskID
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("TaskID"), 0);
            }
            set
            {
                SetValue("TaskID", value);
            }
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            TaskGroupTaskInfoProvider.DeleteTaskGroupTask(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            TaskGroupTaskInfoProvider.SetTaskGroupTask(this);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor for de-serialization.
        /// </summary>
        /// <param name="info">Serialization info</param>
        /// <param name="context">Streaming context</param>
        public TaskGroupTaskInfo(SerializationInfo info, StreamingContext context)
            : base(info, context, TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates an empty TaskGroupTaskInfo object.
        /// </summary>
        public TaskGroupTaskInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new TaskGroupTaskInfo object from the given DataRow.
        /// </summary>
        /// <param name="dr">DataRow with the object data</param>
        public TaskGroupTaskInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion
    }
}