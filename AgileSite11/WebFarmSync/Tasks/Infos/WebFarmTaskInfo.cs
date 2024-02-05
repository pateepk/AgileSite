using System;
using System.Data;

using CMS;
using CMS.Core;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.WebFarmSync;
using CMS.Base;
using CMS.LicenseProvider;

[assembly: RegisterObjectType(typeof(WebFarmTaskInfo), WebFarmTaskInfo.OBJECT_TYPE)]

namespace CMS.WebFarmSync
{
    /// <summary>
    /// WebFarmTaskInfo data container class.
    /// </summary>
    public class WebFarmTaskInfo : AbstractInfo<WebFarmTaskInfo>, IWebFarmTask
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "cms.webfarmtask";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(WebFarmTaskInfoProvider), OBJECT_TYPE, "CMS.WebFarmTask", "TaskID", null, "TaskGUID", null, null, "TaskBinaryData", null, null, null)
                                              {
                                                  AllowRestore = false,
                                                  SupportsCloning = false,
                                                  ImportExportSettings = { LogExport = false },
                                                  LogEvents = false,
                                                  SupportsVersioning = false,
                                                  TouchCacheDependencies = false,
                                                  Feature = FeatureEnum.Webfarm,
                                                  ContainsMacros = false
                                              };

        #endregion


        #region "Constants"

        /// <summary>
        /// Separator of task text data
        /// </summary>
        internal const string TASK_DATA_SEPARATOR = "¦";

        /// <summary>
        /// Separator of tasks used for grouping multiple tasks of same type
        /// </summary>
        internal const string MULTIPLE_TASK_DATA_SEPARATOR = "^";

        #endregion


        #region "Properties"

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
        /// Text data.
        /// </summary>
        [DatabaseField]
        public virtual string TaskTextData
        {
            get
            {
                return GetStringValue("TaskTextData", "");
            }
            set
            {
                SetValue("TaskTextData", value);
            }
        }


        /// <summary>
        /// Task type.
        /// </summary>
        [DatabaseField]
        public virtual string TaskType
        {
            get
            {
                return ValidationHelper.GetString(GetValue("TaskType"), "");
            }
            set
            {
                SetValue("TaskType", value);
            }
        }


        /// <summary>
        /// Task target.
        /// </summary>
        [DatabaseField]
        public virtual string TaskTarget
        {
            get
            {
                return GetStringValue("TaskTarget", "");
            }
            set
            {
                SetValue("TaskTarget", value);
            }
        }


        /// <summary>
        /// Task machine name.
        /// </summary>
        [DatabaseField]
        public virtual string TaskMachineName
        {
            get
            {
                return GetStringValue("TaskMachineName", "");
            }
            set
            {
                SetValue("TaskMachineName", value);
            }
        }


        /// <summary>
        /// Time when task was created.
        /// </summary>
        [DatabaseField]
        public virtual DateTime TaskCreated
        {
            get
            {
                return GetDateTimeValue("TaskCreated", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("TaskCreated", value, DateTimeHelper.ZERO_TIME);
            }
        }


        /// <summary>
        /// Binary data.
        /// </summary>
        [DatabaseField]
        public virtual byte[] TaskBinaryData
        {
            get
            {
                return ValidationHelper.GetBinary(GetValue("TaskBinaryData"), null);
            }
            set
            {
                SetValue("TaskBinaryData", value);
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
        /// Indicates if the task is not assigned to any server.
        /// </summary>
        [DatabaseField]
        public virtual bool TaskIsAnonymous
        {
            get
            {
                return GetBooleanValue("TaskIsAnonymous", false);
            }
            set
            {
                SetValue("TaskIsAnonymous", value);
            }
        }


        /// <summary>
        /// Indicates if task is used only to synchronize memory.
        /// </summary>
        [DatabaseField]
        public virtual bool TaskIsMemory
        {
            get
            {
                return GetBooleanValue("TaskIsMemory", false);
            }
            set
            {
                SetValue("TaskIsMemory", value);
            }
        }


        /// <summary>
        /// Contains error message when some occurred during task handling.
        /// </summary>
        [DatabaseField]
        public virtual string TaskErrorMessage
        {
            get
            {
                return GetStringValue("TaskErrorMessage", null);
            }
            set
            {
                SetValue("TaskErrorMessage", value);
            }
        }


        /// <summary>
        /// Target path of file transported by task.
        /// </summary>
        public virtual string TaskFilePath
        {
            get;
            set;
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            WebFarmTaskInfoProvider.DeleteWebFarmTaskInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            WebFarmTaskInfoProvider.SetWebFarmTaskInfo(this);
        }


        /// <summary>
        /// Checks the object license.
        /// </summary>
        /// <param name="action">Object action.</param>
        /// <param name="domainName">Domain name, if not set, uses current domain.</param>
        /// <exception cref="LicenseException">Throws <see cref="LicenseException"/> if license check failed.</exception>
        protected sealed override bool CheckLicense(ObjectActionEnum action = ObjectActionEnum.Read, string domainName = null)
        {
            WebFarmLicenseHelper.CheckLicense(domainName);
            
            return true;
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty WebFarmTaskInfo object.
        /// </summary>
        public WebFarmTaskInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new WebFarmTaskInfo object from the given DataRow.
        /// </summary>
        public WebFarmTaskInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion
    }
}