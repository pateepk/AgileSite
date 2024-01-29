using System;
using System.Collections.Generic;
using System.Data;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Modules;
using CMS.Scheduler;

[assembly: RegisterObjectType(typeof(TaskInfo), TaskInfo.OBJECT_TYPE)]
[assembly: RegisterObjectType(typeof(TaskInfo), TaskInfo.OBJECT_TYPE_OBJECTTASK)]

namespace CMS.Scheduler
{
    /// <summary>
    /// TaskInfo data container class.
    /// </summary>
    public class TaskInfo : AbstractInfo<TaskInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "cms.scheduledtask";

        /// <summary>
        /// Object type for object tasks
        /// </summary>
        public const string OBJECT_TYPE_OBJECTTASK = PredefinedObjectType.OBJECTSCHEDULEDTASK;


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(TaskInfoProvider), OBJECT_TYPE, "CMS.ScheduledTask", "TaskID", "TaskLastModified", "TaskGUID", "TaskName", "TaskDisplayName", null, "TaskSiteID", null, null)
        {
            SynchronizationSettings =
            {
                LogSynchronization = SynchronizationTypeEnum.None,
                ObjectTreeLocations = new List<ObjectTreeLocation>
                {
                    new ObjectTreeLocation(SITE, CONFIGURATION),
                    new ObjectTreeLocation(GLOBAL, CONFIGURATION),
                }
            },

            LogEvents = true,
            DependsOn = new List<ObjectDependency>
            {
                new ObjectDependency("TaskResourceID", ResourceInfo.OBJECT_TYPE),
                new ObjectDependency("TaskUserID", PredefinedObjectType.USER),
            },
            ModuleName = "cms.scheduledtasks",
            ResourceIDColumn = "TaskResourceID",
            AssemblyNameColumn = "TaskAssemblyName",
            SupportsGlobalObjects = true,
            ImportExportSettings =
            {
                IncludeToWebTemplateExport = ObjectRangeEnum.Site,
                IsExportable = true,
                LogExport = true,
                ObjectTreeLocations = new List<ObjectTreeLocation>
                {
                    new ObjectTreeLocation(SITE, CONFIGURATION),
                    new ObjectTreeLocation(GLOBAL, CONFIGURATION),
                },
                AlwaysCheckExisting = true
            },
            EnabledColumn = "TaskEnabled",
            TypeCondition = new TypeCondition().WhereIsNull("TaskObjectID"),
            DefaultData = new DefaultDataSettings
            {
                Where = "TaskServerName IS NULL OR TaskServerName = ''",
                ExcludedPrefixes = { "custom" },
                ExcludedColumns = new List<string> { "TaskExecutions", "TaskLastExecutionReset", "TaskObjectType", "TaskObjectID", "TaskLastRunTime", "TaskUserID", "TaskLastResult", "TaskExecutingServerName", "TaskIsRunning" }
            },
            SerializationSettings =
            {
                ExcludedFieldNames  = { "TaskExecutions", "TaskLastExecutionReset", "TaskLastRunTime", "TaskNextRunTime", "TaskLastResult", "TaskExecutingServerName", "TaskIsRunning" }
            },
            ContinuousIntegrationSettings =
            {
                Enabled = true,
                
                // Filter out system scheduled tasks
                FilterCondition = new WhereCondition().WhereNull("TaskType").Or().WhereNotEquals("TaskType", (int)ScheduledTaskTypeEnum.System)
            }
        };


        /// <summary>
        /// Type information for object scheduled tasks.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO_OBJECT = new ObjectTypeInfo(typeof(TaskInfoProvider), OBJECT_TYPE_OBJECTTASK, "CMS.ScheduledTask", "TaskID", "TaskLastModified", "TaskGUID", "TaskName", "TaskDisplayName", null, "TaskSiteID", null, null)
        {
            MacroCollectionName = "ObjectTask",
            SynchronizationSettings =
            {
                LogSynchronization = SynchronizationTypeEnum.None,
            },
            DependsOn = new List<ObjectDependency>
            {
                new ObjectDependency("TaskResourceID", ResourceInfo.OBJECT_TYPE),
                new ObjectDependency("TaskUserID", PredefinedObjectType.USER),
                new ObjectDependency("TaskObjectID", null, ObjectDependencyEnum.Required, "TaskObjectType"),
            },
            ModuleName = "cms.scheduledtasks",
            ResourceIDColumn = "TaskResourceID",
            AssemblyNameColumn = "TaskAssemblyName",
            SupportsGlobalObjects = true,
            AllowRestore = false,
            SupportsCloning = false,
            OriginalTypeInfo = TYPEINFO,
            EnabledColumn = "TaskEnabled",
            ImportExportSettings =
            {
                IncludeToWebTemplateExport = ObjectRangeEnum.Site,
                LogExport = true,
                AlwaysCheckExisting = true
            },
            TypeCondition = new TypeCondition().WhereIsNotNull("TaskObjectID"),
        };

        #endregion


        #region "Properties"

        /// <summary>
        /// Time of last run.
        /// </summary>
        [DatabaseField]
        public virtual DateTime TaskLastRunTime
        {
            get
            {
                return GetDateTimeValue("TaskLastRunTime", TaskInfoProvider.NO_TIME);
            }
            set
            {
                SetValue("TaskLastRunTime", value);
            }
        }


        /// <summary>
        /// Time of next run.
        /// </summary>
        [DatabaseField]
        public virtual DateTime TaskNextRunTime
        {
            get
            {
                return GetDateTimeValue("TaskNextRunTime", TaskInfoProvider.NO_TIME);
            }
            set
            {
                SetValue("TaskNextRunTime", value, TaskInfoProvider.NO_TIME);
            }
        }


        /// <summary>
        /// Assembly name of task.
        /// </summary>
        [DatabaseField]
        public virtual string TaskAssemblyName
        {
            get
            {
                return GetStringValue("TaskAssemblyName", "");
            }
            set
            {
                SetValue("TaskAssemblyName", value);
            }
        }


        /// <summary>
        /// Task site ID.
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
                if (value > 0)
                {
                    SetValue("TaskSiteID", value);
                }
                else
                {
                    SetValue("TaskSiteID", null);
                }
            }
        }


        /// <summary>
        /// Task last result.
        /// </summary>
        [DatabaseField]
        public virtual string TaskLastResult
        {
            get
            {
                return GetStringValue("TaskLastResult", "");
            }
            set
            {
                SetValue("TaskLastResult", value);
            }
        }


        /// <summary>
        /// Task class.
        /// </summary>
        [DatabaseField]
        public virtual string TaskClass
        {
            get
            {
                return GetStringValue("TaskClass", "");
            }
            set
            {
                SetValue("TaskClass", value);
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
        /// Task name.
        /// </summary>
        [DatabaseField]
        public virtual string TaskName
        {
            get
            {
                return GetStringValue("TaskName", "");
            }
            set
            {
                SetValue("TaskName", value);
            }
        }


        /// <summary>
        /// Indicates whether task is enabled.
        /// </summary>
        [DatabaseField]
        public virtual bool TaskEnabled
        {
            get
            {
                return GetBooleanValue("TaskEnabled", false);
            }
            set
            {
                SetValue("TaskEnabled", value);
            }
        }


        /// <summary>
        /// Indicates whether task is running.
        /// </summary>
        [DatabaseField]
        public virtual bool TaskIsRunning
        {
            get
            {
                return GetBooleanValue("TaskIsRunning", false);
            }
            set
            {
                SetValue("TaskIsRunning", value);
            }
        }


        /// <summary>
        /// Interval between execution of task.
        /// </summary>
        [DatabaseField]
        public virtual string TaskInterval
        {
            get
            {
                return GetStringValue("TaskInterval", "");
            }
            set
            {
                SetValue("TaskInterval", value);
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
        /// Display name.
        /// </summary>
        [DatabaseField]
        public virtual string TaskDisplayName
        {
            get
            {
                return GetStringValue("TaskDisplayName", "");
            }
            set
            {
                SetValue("TaskDisplayName", value);
            }
        }


        /// <summary>
        /// If true, the task is deleted after successful execution.
        /// </summary>
        [DatabaseField]
        public virtual bool TaskDeleteAfterLastRun
        {
            get
            {
                return GetBooleanValue("TaskDeleteAfterLastRun", false);
            }
            set
            {
                SetValue("TaskDeleteAfterLastRun", value);
            }
        }


        /// <summary>
        /// Task Server Name.
        /// </summary>
        [DatabaseField]
        public virtual string TaskServerName
        {
            get
            {
                return GetStringValue("TaskServerName", "");
            }
            set
            {
                SetValue("TaskServerName", value);
            }
        }


        /// <summary>
        /// Task GUID.
        /// </summary>
        [DatabaseField]
        public virtual Guid TaskGUID
        {
            get
            {
                return GetGuidValue("TaskGUID", Guid.Empty);
            }
            set
            {
                SetValue("TaskGUID", value, Guid.Empty);
            }
        }


        /// <summary>
        /// Object last modified.
        /// </summary>
        [DatabaseField]
        public virtual DateTime TaskLastModified
        {
            get
            {
                return GetDateTimeValue("TaskLastModified", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("TaskLastModified", value, DateTimeHelper.ZERO_TIME);
            }
        }


        /// <summary>
        /// Number of task executions.
        /// </summary>
        [DatabaseField]
        public virtual int TaskExecutions
        {
            get
            {
                return GetIntegerValue("TaskExecutions", 0);
            }
            set
            {
                SetValue("TaskExecutions", value);
            }
        }


        /// <summary>
        /// Date of last execution count reset.
        /// </summary>
        [DatabaseField]
        public virtual DateTime TaskLastExecutionReset
        {
            get
            {
                return GetDateTimeValue("TaskLastExecutionReset", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("TaskLastExecutionReset", value);
            }
        }


        /// <summary>
        /// Task macro condition.
        /// </summary>
        [DatabaseField]
        public virtual string TaskCondition
        {
            get
            {
                return GetStringValue("TaskCondition", "");
            }
            set
            {
                SetValue("TaskCondition", value);
            }
        }


        /// <summary>
        /// If true, the task is executed for each site individually. Available only for global tasks.
        /// </summary>
        [DatabaseField]
        public virtual bool TaskRunIndividuallyForEachSite
        {
            get
            {
                return GetBooleanValue("TaskRunIndividually", false);
            }
            set
            {
                SetValue("TaskRunIndividually", value);
            }
        }


        /// <summary>
        /// Task resource (module) ID.
        /// </summary>
        [DatabaseField]
        public virtual int TaskResourceID
        {
            get
            {
                return GetIntegerValue("TaskResourceID", 0);
            }
            set
            {
                SetValue("TaskResourceID", value, (value > 0));
            }
        }


        /// <summary>
        /// Task user ID to specify the user context for the task execution.
        /// </summary>
        [DatabaseField]
        public virtual int TaskUserID
        {
            get
            {
                return GetIntegerValue("TaskUserID", 0);
            }
            set
            {
                SetValue("TaskUserID", value, (value > 0));
            }
        }


        /// <summary>
        /// If true, task runs in separate thread.
        /// </summary>
        [DatabaseField]
        public virtual bool TaskRunInSeparateThread
        {
            get
            {
                return GetBooleanValue("TaskRunInSeparateThread", false);
            }
            set
            {
                SetValue("TaskRunInSeparateThread", value);
            }
        }


        /// <summary>
        /// Indicates whether the task is processed by an external service.
        /// </summary>
        [DatabaseField]
        public virtual bool TaskUseExternalService
        {
            get
            {
                return GetBooleanValue("TaskUseExternalService", false);
            }
            set
            {
                SetValue("TaskUseExternalService", value);
            }
        }


        /// <summary>
        /// Indicates whether the task can be processed by an external service.
        /// </summary>
        [DatabaseField]
        public virtual bool TaskAllowExternalService
        {
            get
            {
                return GetBooleanValue("TaskAllowExternalService", false);
            }
            set
            {
                SetValue("TaskAllowExternalService", value);
            }
        }


        /// <summary>
        /// Indicates whether the task can be processed by an external service.
        /// </summary>
        [DatabaseField(ValueType = typeof(int))]
        public virtual ScheduledTaskTypeEnum TaskType
        {
            get
            {
                return (ScheduledTaskTypeEnum)GetIntegerValue("TaskType", 0);
            }
            set
            {
                SetValue("TaskType", (int)value);
            }
        }


        /// <summary>
        /// Type of object associated to this task.
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
                SetValue("TaskObjectType", value, String.Empty);
            }
        }


        /// <summary>
        /// ID of object associated to this task.
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
                SetValue("TaskObjectID", value, (value > 0));
            }
        }


        /// <summary>
        /// Name of the server executing the task right now (may be empty for single-server scenarios)
        /// </summary>
        [DatabaseField]
        public string TaskExecutingServerName
        {
            get
            {
                return GetStringValue("TaskExecutingServerName", String.Empty);
            }
            set
            {
                SetValue("TaskExecutingServerName", value, String.Empty);
            }
        }


        /// <summary>
        /// Current site name of the execution (stored in memory)
        /// </summary>
        public string CurrentSiteName
        {
            get;
            set;
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Object type information.
        /// </summary>
        public override ObjectTypeInfo TypeInfo
        {
            get
            {
                return TaskObjectID > 0 ? TYPEINFO_OBJECT : TYPEINFO;
            }
        }


        /// <summary>
        /// Indicates if the object supports deleting to recycle bin.
        /// </summary>
        protected override bool AllowRestore
        {
            get
            {
                return base.AllowRestore && (TaskType != ScheduledTaskTypeEnum.System);
            }
            set
            {
                base.AllowRestore = value;
            }
        }


        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            TaskInfoProvider.DeleteTaskInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            TaskInfoProvider.SetTaskInfo(this);
        }


        /// <summary>
        /// Loads the default data to the object.
        /// </summary>
        protected override void LoadDefaultData()
        {
            base.LoadDefaultData();

            TaskIsRunning = false;
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty TaskInfo object.
        /// </summary>
        public TaskInfo()
            : base(TYPEINFO)
        {
            DisableLogging();
        }


        /// <summary>
        /// Constructor - Creates a new TaskInfo object from the given DataRow.
        /// </summary>
        public TaskInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
            DisableLogging();
        }


        /// <summary>
        /// Disables logging for the object
        /// </summary>
        private void DisableLogging()
        {
            // Disable events logging only on object base
            LogEvents = false;
            LogIntegration = false;
        }

        #endregion
    }
}