using System;
using System.Collections.Generic;
using System.Data;

using CMS;
using CMS.Core;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Membership;
using CMS.Scheduler;
using CMS.Base;
using CMS.WorkflowEngine;
using CMS.Automation;

[assembly: RegisterObjectType(typeof(AutomationStateInfo), AutomationStateInfo.OBJECT_TYPE)]

namespace CMS.Automation
{
    /// <summary>
    /// AutomationStateInfo data container class.
    /// </summary>
    public class AutomationStateInfo<InfoType> : AutomationStateInfo 
        where InfoType : BaseInfo
    {
        /// <summary>
        /// Object to process automation
        /// </summary>
        public InfoType StateObject
        {
            get;
            set;
        }
    }


    /// <summary>
    /// AutomationStateInfo data container class.
    /// </summary>
    public class AutomationStateInfo : AbstractInfo<AutomationStateInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = PredefinedObjectType.AUTOMATIONSTATE;


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(AutomationStateInfoProvider), OBJECT_TYPE, "CMS.AutomationState", "StateID", "StateLastModified", "StateGUID", null, null, null, "StateSiteID", null, null)
        {
            DependsOn = new List<ObjectDependency> 
            { 
                new ObjectDependency("StateStepID", WorkflowStepInfo.OBJECT_TYPE, ObjectDependencyEnum.Required), 
                new ObjectDependency("StateWorkflowID", WorkflowInfo.OBJECT_TYPE, ObjectDependencyEnum.Required), 
                new ObjectDependency("StateObjectID", null, ObjectDependencyEnum.Required, "StateObjectType"), 
                new ObjectDependency("StateUserID", UserInfo.OBJECT_TYPE, ObjectDependencyEnum.RequiredHasDefault) 
            },
            TouchCacheDependencies = true,
            SupportsVersioning = false,
            ModuleName = ModuleName.ONLINEMARKETING,
            SupportsCloning = false,
            HasScheduledTasks = true,
            SupportsGlobalObjects = true,
            ContainsMacros = false,
            DeleteObjectWithAPI = true,
        };

        #endregion


        #region "Variables"

        /// <summary>
        /// State custom data.
        /// </summary>
        protected ContainerCustomData mStateCustomData = null;

        /// <summary>
        /// Workflow state step timeout
        /// </summary>
        protected DateTime mStateStepTimeout = DateTime.MinValue;

        #endregion


        #region "Properties"

        /// <summary>
        /// ID of state.
        /// </summary>
        public virtual int StateID
        {
            get
            {
                return GetIntegerValue("StateID", 0);
            }
            set
            {
                SetValue("StateID", value);
            }
        }


        /// <summary>
        /// GUID of state.
        /// </summary>
        public virtual Guid StateGUID
        {
            get
            {
                return GetGuidValue("StateGUID", Guid.Empty);
            }
            set
            {
                SetValue("StateGUID", value);
            }
        }


        /// <summary>
        /// Step of the state.
        /// </summary>
        public virtual int StateStepID
        {
            get
            {
                return GetIntegerValue("StateStepID", 0);
            }
            set
            {
                SetValue("StateStepID", value);
            }
        }


        /// <summary>
        /// Step timeout date (for steps with timeout).
        /// </summary>
        [DatabaseMapping(false)]
        public virtual DateTime StateStepTimeout
        {
            get
            {
                if (mStateStepTimeout == DateTime.MinValue)
                {
                    if (StateStepID > 0)
                    {
                        string taskName = WorkflowHelper.GetScheduledTaskName(StateGUID);
                        TaskInfo existingTask = TaskInfoProvider.GetTaskInfo(taskName, StateSiteID);
                        if (existingTask != null)
                        {
                            mStateStepTimeout = existingTask.TaskNextRunTime;
                        }
                    }

                    // Set dummy date
                    if (mStateStepTimeout == DateTime.MinValue)
                    {
                        mStateStepTimeout = DateTimeHelper.ZERO_TIME;
                    }
                }

                return mStateStepTimeout;
            }
        }


        /// <summary>
        /// Object under state.
        /// </summary>
        public virtual int StateObjectID
        {
            get
            {
                return GetIntegerValue("StateObjectID", 0);
            }
            set
            {
                SetValue("StateObjectID", value);
            }
        }


        /// <summary>
        /// Type of object under state.
        /// </summary>
        public virtual string StateObjectType
        {
            get
            {
                return GetStringValue("StateObjectType", "");
            }
            set
            {
                SetValue("StateObjectType", value);
            }
        }


        /// <summary>
        /// Workflow of the state.
        /// </summary>
        public virtual int StateWorkflowID
        {
            get
            {
                return GetIntegerValue("StateWorkflowID", 0);
            }
            set
            {
                SetValue("StateWorkflowID", value);
            }
        }


        /// <summary>
        /// Site identifier of state.
        /// </summary>
        public virtual int StateSiteID
        {
            get
            {
                return GetIntegerValue("StateSiteID", 0);
            }
            set
            {
                SetValue("StateSiteID", value, value > 0);
            }
        }


        /// <summary>
        /// Action status flag.
        /// </summary>
        public virtual string StateActionStatus
        {
            get
            {
                return GetStringValue("StateActionStatus", "");
            }
            set
            {
                SetValue("StateActionStatus", value, !string.IsNullOrEmpty(value));
            }
        }


        /// <summary>
        /// Status of the state.
        /// </summary>
        public virtual ProcessStatusEnum StateStatus
        {
            get
            {
                return (ProcessStatusEnum)GetIntegerValue("StateStatus", (int)ProcessStatusEnum.Processing);
            }
            set
            {
                SetValue("StateStatus", (int)value);
            }
        }


        /// <summary>
        /// Creation time of the state.
        /// </summary>
        public virtual DateTime StateCreated
        {
            get
            {
                return GetDateTimeValue("StateCreated", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("StateCreated", value, DateTimeHelper.ZERO_TIME);
            }
        }


        /// <summary>
        /// ID of the user who started automation process.
        /// </summary>
        /// <remarks>0 indicates process was started automatically.</remarks>
        public virtual int StateUserID
        {
            get
            {
                return GetIntegerValue("StateUserID", 0);
            }
            set
            {
                SetValue("StateUserID", value, value > 0);
            }
        }


        /// <summary>
        /// Time of the last modification.
        /// </summary>
        public virtual DateTime StateLastModified
        {
            get
            {
                return GetDateTimeValue("StateLastModified", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("StateLastModified", value, DateTimeHelper.ZERO_TIME);
            }
        }


        /// <summary>
        /// State custom data.
        /// </summary>
        public virtual ContainerCustomData StateCustomData
        {
            get
            {
                if (mStateCustomData == null)
                {
                    mStateCustomData = new ContainerCustomData(this, "StateCustomData");
                }
                return mStateCustomData;
            }
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            AutomationStateInfoProvider.DeleteAutomationStateInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            AutomationStateInfoProvider.SetAutomationStateInfo(this);
        }


        /// <summary>
        /// Sets value of the specified column.
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="value">Value</param>
        /// <returns>Returns true if the operation was successful</returns>
        public override bool SetValue(string columnName, object value)
        {
            bool result = base.SetValue(columnName, value);

            // Special columns treatment
            switch (columnName.ToLowerCSafe())
            {
                case "statestepid":
                    // Invalidate timeout value
                    mStateStepTimeout = DateTime.MinValue;
                    break;
            }

            return result;
        }


        /// <summary>
        /// Registers properties of this object.
        /// </summary>
        protected sealed override void RegisterProperties()
        {
            base.RegisterProperties();

            RegisterProperty<DateTime>("StateStepTimeout", m => m.StateStepTimeout);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty AutomationStateInfo object.
        /// </summary>
        public AutomationStateInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new AutomationStateInfo object from the given DataRow.
        /// </summary>
        /// <param name="dr">DataRow with the object data.</param>
        public AutomationStateInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Checks the permissions of the object.
        /// </summary>
        /// <param name="permission">Permission type</param>
        /// <param name="siteName">Name of the site</param>
        /// <param name="userInfo">UserInfo object</param>
        /// <param name="exceptionOnFailure">If true, PermissionCheckException is thrown whenever a permission check fails</param>
        protected override bool CheckPermissionsInternal(PermissionsEnum permission, string siteName, IUserInfo userInfo, bool exceptionOnFailure)
        {
            // Always return true for global administrator
            if (userInfo.CheckPrivilegeLevel(UserPrivilegeLevelEnum.Admin))
            {
                return true;
            }

            switch (permission)
            {
                case PermissionsEnum.Read:
                    return userInfo.IsAuthorizedPerResource(ModuleName.ONLINEMARKETING, "ReadProcesses", siteName, exceptionOnFailure);

                case PermissionsEnum.Create:
                case PermissionsEnum.Modify:
                    return userInfo.IsAuthorizedPerResource(ModuleName.ONLINEMARKETING, "ManageProcesses", siteName, exceptionOnFailure);

                case PermissionsEnum.Delete:
                case PermissionsEnum.Destroy:
                    return userInfo.IsAuthorizedPerResource(ModuleName.ONLINEMARKETING, "RemoveProcess", siteName, exceptionOnFailure);

                default:
                    return base.CheckPermissionsInternal(permission, siteName, userInfo, exceptionOnFailure);
            }
        }

        #endregion
    }
}
