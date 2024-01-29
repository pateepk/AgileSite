using System;
using System.Collections.Generic;
using System.Data;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Base;
using CMS.Synchronization;

[assembly: RegisterObjectType(typeof(IntegrationTaskInfo), IntegrationTaskInfo.OBJECT_TYPE)]

namespace CMS.Synchronization
{
    /// <summary>
    /// IntegrationTaskInfo data container class.
    /// </summary>
    public class IntegrationTaskInfo : AbstractInfo<IntegrationTaskInfo>, ISynchronizationTask
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "integration.task";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(IntegrationTaskInfoProvider), OBJECT_TYPE, "Integration.Task", "TaskID", "TaskTime", null, null, "TaskTitle", null, "TaskSiteID", null, null)
        {
            SupportsVersioning = false,
            SupportsGlobalObjects = true,
            LogIntegration = false,
            AllowRestore = false,
            SupportsCloning = false,
            MacroCollectionName = "IntegrationTask",
        };

        #endregion


        #region "Properties"

        /// <summary>
        /// Type of the task.
        /// </summary>
        public virtual TaskTypeEnum TaskType
        {
            get
            {
                return TaskHelper.GetTaskTypeEnum(GetStringValue("TaskType", string.Empty));
            }
            set
            {
                SetValue("TaskType", TaskHelper.GetTaskTypeString(value));
            }
        }


        /// <summary>
        /// Task identifier.
        /// </summary>
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
        /// Determines whether the task is inbound or outbound.
        /// </summary>
        public virtual bool TaskIsInbound
        {
            get
            {
                return GetBooleanValue("TaskIsInbound", false);
            }
            set
            {
                SetValue("TaskIsInbound", value);
            }
        }


        /// <summary>
        /// Object type which the task is bound to.
        /// </summary>
        public virtual string TaskObjectType
        {
            get
            {
                return GetStringValue("TaskObjectType", string.Empty);
            }
            set
            {
                SetValue("TaskObjectType", value);
            }
        }


        /// <summary>
        /// Node identifier which the task is bound to.
        /// </summary>
        public virtual int TaskNodeID
        {
            get
            {
                return GetIntegerValue("TaskNodeID", 0);
            }
            set
            {
                SetValue("TaskNodeID", value, (value > 0));
            }
        }


        /// <summary>
        /// Site identifier which the object is bound to.
        /// </summary>
        public virtual int TaskSiteID
        {
            get
            {
                return GetIntegerValue("TaskSiteID", 0);
            }
            set
            {
                SetValue("TaskSiteID", value, (value > 0));
            }
        }


        /// <summary>
        /// Node alias path which the task is bound to.
        /// </summary>
        public virtual string TaskNodeAliasPath
        {
            get
            {
                return GetStringValue("TaskNodeAliasPath", string.Empty);
            }
            set
            {
                SetValue("TaskNodeAliasPath", value);
            }
        }


        /// <summary>
        /// Selected type of processing (applies only to inbound tasks).
        /// </summary>
        public virtual IntegrationProcessTypeEnum TaskProcessType
        {
            get
            {
                return IntegrationHelper.GetIntegrationProcessTypeEnum(GetStringValue("TaskProcessType", string.Empty));
            }
            set
            {
                SetValue("TaskProcessType", IntegrationHelper.GetIntegrationProcessTypeString(value));
            }
        }


        /// <summary>
        /// Type of data.
        /// </summary>
        public virtual TaskDataTypeEnum TaskDataType
        {
            get
            {
                return IntegrationHelper.GetTaskDataTypeEnum(GetStringValue("TaskDataType", string.Empty));
            }
            set
            {
                SetValue("TaskDataType", IntegrationHelper.GetTaskDataTypeString(value));
            }
        }


        /// <summary>
        /// Document identifier which the task is bound to.
        /// </summary>
        public virtual int TaskDocumentID
        {
            get
            {
                return GetIntegerValue("TaskDocumentID", 0);
            }
            set
            {
                SetValue("TaskDocumentID", value, (value > 0));
            }
        }


        /// <summary>
        /// Object identifier which the task is bound to.
        /// </summary>
        public virtual int TaskObjectID
        {
            get
            {
                return GetIntegerValue("TaskObjectID", 0);
            }
            set
            {
                SetValue("TaskObjectID", value, (value > 0));
            }
        }


        /// <summary>
        /// Title of the task.
        /// </summary>
        public virtual string TaskTitle
        {
            get
            {
                return GetStringValue("TaskTitle", string.Empty);
            }
            set
            {
                SetValue("TaskTitle", value);
            }
        }


        /// <summary>
        /// Time when the task was logged.
        /// </summary>
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
        /// Task data.
        /// </summary>
        public virtual string TaskData
        {
            get
            {
                return GetStringValue("TaskData", string.Empty);
            }
            set
            {
                SetValue("TaskData", value);
            }
        }

        #endregion


        #region "GeneralizedInfo properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            IntegrationTaskInfoProvider.DeleteIntegrationTaskInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            IntegrationTaskInfoProvider.SetIntegrationTaskInfo(this);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty IntegrationTaskInfo object.
        /// </summary>
        public IntegrationTaskInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new IntegrationTaskInfo object from the given DataRow.
        /// </summary>
        /// <param name="dr">DataRow with the object data</param>
        public IntegrationTaskInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion
    }
}