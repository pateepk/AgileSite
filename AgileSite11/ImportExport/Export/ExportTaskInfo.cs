using System;
using System.Data;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Base;
using CMS.Synchronization;
using CMS.CMSImportExport;

[assembly: RegisterObjectType(typeof(ExportTaskInfo), ExportTaskInfo.OBJECT_TYPE)]

namespace CMS.CMSImportExport
{
    /// <summary>
    /// ExportTaskInfo data container class.
    /// </summary>
    public class ExportTaskInfo : AbstractInfo<ExportTaskInfo>, ISynchronizationTask
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "export.task";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(ExportTaskInfoProvider), OBJECT_TYPE, "Export.Task", "TaskID", "TaskTime", null, null, "TaskTitle", null, "TaskSiteID", null, null)
        {
            TouchCacheDependencies = true,
            SupportsVersioning = false,
            LogIntegration = false,
            ContainsMacros = false,
            SupportsGlobalObjects = true
        };

        #endregion


        #region "Properties"

        /// <summary>
        /// Task site ID.
        /// </summary>
        public virtual int TaskSiteID
        {
            get
            {
                return GetIntegerValue("TaskSiteID", 0);
            }
            set
            {
                SetValue("TaskSiteID", value, value > 0);
            }
        }


        /// <summary>
        /// Task object type.
        /// </summary>
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
        public virtual int TaskObjectID
        {
            get
            {
                return GetIntegerValue("TaskObjectID", 0);
            }
            set
            {
                SetValue("TaskObjectID", value);
            }
        }


        /// <summary>
        /// Task title.
        /// </summary>
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
        /// Task type.
        /// </summary>
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

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            ExportTaskInfoProvider.DeleteExportTaskInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            ExportTaskInfoProvider.SetExportTaskInfo(this);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty ExportTaskInfo object.
        /// </summary>
        public ExportTaskInfo()
            : base(TYPEINFO)
        {
            DisableLogging();
        }


        /// <summary>
        /// Constructor - Creates a new ExportTaskInfo object from the given DataRow.
        /// </summary>
        public ExportTaskInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
            DisableLogging();
        }


        /// <summary>
        /// Disables logging of the object
        /// </summary>
        private void DisableLogging()
        {
            LogSynchronization = SynchronizationTypeEnum.None;
            LogEvents = false;
            LogExport = false;
        }

        #endregion
    }
}