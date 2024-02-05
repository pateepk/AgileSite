using System;
using System.Collections.Generic;
using System.Data;

using CMS;
using CMS.Core;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Membership;
using CMS.Base;
using CMS.WorkflowEngine;
using CMS.Automation;

[assembly: RegisterObjectType(typeof(AutomationHistoryInfo), AutomationHistoryInfo.OBJECT_TYPE)]

namespace CMS.Automation
{
    /// <summary>
    /// AutomationHistoryInfo data container class.
    /// </summary>
    public class AutomationHistoryInfo : AbstractInfo<AutomationHistoryInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "ma.automationhistory";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(AutomationHistoryInfoProvider), OBJECT_TYPE, "CMS.AutomationHistory", "HistoryID", null, null, null, null, null, null, "HistoryStateID", AutomationStateInfo.OBJECT_TYPE)
        {
            DependsOn = new List<ObjectDependency>
            {
                new ObjectDependency("HistoryStepID", WorkflowStepInfo.OBJECT_TYPE, ObjectDependencyEnum.Required), 
                new ObjectDependency("HistoryTargetStepID", WorkflowStepInfo.OBJECT_TYPE, ObjectDependencyEnum.Required), 
                new ObjectDependency("HistoryApprovedByUserID", UserInfo.OBJECT_TYPE), 
                new ObjectDependency("HistoryWorkflowID", WorkflowInfo.OBJECT_TYPE, ObjectDependencyEnum.Required)
            },

            TouchCacheDependencies = true,
            SupportsVersioning = false,

            ModuleName = ModuleName.WORKFLOWENGINE,
            SupportsCloning = false,
            AllowRestore = false,
            ContainsMacros = false
        };

        #endregion


        #region "Properties"

        /// <summary>
        /// ID of history record.
        /// </summary>
        public virtual int HistoryID
        {
            get
            {
                return GetIntegerValue("HistoryID", 0);
            }
            set
            {
                SetValue("HistoryID", value);
            }
        }


        /// <summary>
        /// Original step.
        /// </summary>
        public virtual int HistoryStepID
        {
            get
            {
                return GetIntegerValue("HistoryStepID", 0);
            }
            set
            {
                SetValue("HistoryStepID", value, (value > 0));
            }
        }


        /// <summary>
        /// Original step display name.
        /// </summary>
        public virtual string HistoryStepDisplayName
        {
            get
            {
                return GetStringValue("HistoryStepDisplayName", "");
            }
            set
            {
                SetValue("HistoryStepDisplayName", value);
            }
        }


        /// <summary>
        /// Original step name.
        /// </summary>
        public virtual string HistoryStepName
        {
            get
            {
                return GetStringValue("HistoryStepName", "");
            }
            set
            {
                SetValue("HistoryStepName", value);
            }
        }


        /// <summary>
        /// Original step type.
        /// </summary>
        public virtual WorkflowStepTypeEnum HistoryStepType
        {
            get
            {
                return (WorkflowStepTypeEnum)GetIntegerValue("HistoryStepType", 0);
            }
            set
            {
                SetValue("HistoryStepType", (int)value);
            }
        }


        /// <summary>
        /// Target step.
        /// </summary>
        public virtual int HistoryTargetStepID
        {
            get
            {
                return GetIntegerValue("HistoryTargetStepID", 0);
            }
            set
            {
                SetValue("HistoryTargetStepID", value, (value > 0));
            }
        }


        /// <summary>
        /// Target step display name.
        /// </summary>
        public virtual string HistoryTargetStepDisplayName
        {
            get
            {
                return GetStringValue("HistoryTargetStepDisplayName", "");
            }
            set
            {
                SetValue("HistoryTargetStepDisplayName", value);
            }
        }


        /// <summary>
        /// Target step name.
        /// </summary>
        public virtual string HistoryTargetStepName
        {
            get
            {
                return GetStringValue("HistoryTargetStepName", "");
            }
            set
            {
                SetValue("HistoryTargetStepName", value);
            }
        }


        /// <summary>
        /// Type of transition between original step and target step.
        /// </summary>
        public virtual WorkflowTransitionTypeEnum HistoryTransitionType
        {
            get
            {
                return (WorkflowTransitionTypeEnum)GetIntegerValue("HistoryTransitionType", 0);
            }
            set
            {
                SetValue("HistoryTransitionType", (int)value);
            }
        }


        /// <summary>
        /// Workflow identification.
        /// </summary>
        public virtual int HistoryWorkflowID
        {
            get
            {
                return GetIntegerValue("HistoryWorkflowID", 0);
            }
            set
            {
                SetValue("HistoryWorkflowID", value, (value > 0));
            }
        }


        /// <summary>
        /// User that approved the object.
        /// </summary>
        public virtual int HistoryApprovedByUserID
        {
            get
            {
                return GetIntegerValue("HistoryApprovedByUserID", 0);
            }
            set
            {
                SetValue("HistoryApprovedByUserID", value, (value > 0));
            }
        }


        /// <summary>
        /// Date and time of approval.
        /// </summary>
        public virtual DateTime HistoryApprovedWhen
        {
            get
            {
                return GetDateTimeValue("HistoryApprovedWhen", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("HistoryApprovedWhen", value, DateTimeHelper.ZERO_TIME);
            }
        }


        /// <summary>
        /// Comment of approval.
        /// </summary>
        public virtual string HistoryComment
        {
            get
            {
                return GetStringValue("HistoryComment", "");
            }
            set
            {
                SetValue("HistoryComment", value);
            }
        }


        /// <summary>
        /// Whether the object was rejected.
        /// </summary>
        public virtual bool HistoryWasRejected
        {
            get
            {
                return GetBooleanValue("HistoryWasRejected", false);
            }
            set
            {
                SetValue("HistoryWasRejected", value);
            }
        }


        /// <summary>
        /// Whether history was rejected.
        /// </summary>
        public virtual bool HistoryRejected
        {
            get
            {
                return GetBooleanValue("HistoryRejected", false);
            }
            set
            {
                SetValue("HistoryRejected", value);
            }
        }


        /// <summary>
        /// Target step type.
        /// </summary>
        public virtual WorkflowStepTypeEnum HistoryTargetStepType
        {
            get
            {
                return (WorkflowStepTypeEnum)GetIntegerValue("HistoryTargetStepType", 0);
            }
            set
            {
                SetValue("HistoryTargetStepType", (int)value);
            }
        }


        /// <summary>
        /// Process state identification.
        /// </summary>
        public virtual int HistoryStateID
        {
            get
            {
                return GetIntegerValue("HistoryStateID", 0);
            }
            set
            {
                SetValue("HistoryStateID", value);
            }
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            AutomationHistoryInfoProvider.DeleteAutomationHistoryInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            AutomationHistoryInfoProvider.SetAutomationHistoryInfo(this);
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
                    allowed = UserInfoProvider.IsAuthorizedPerResource(TypeInfo.ModuleName, "ReadProcesses", siteName, (UserInfo)userInfo);
                    break;
                case PermissionsEnum.Create:
                case PermissionsEnum.Delete:
                case PermissionsEnum.Modify:
                    allowed = UserInfoProvider.IsAuthorizedPerResource(TypeInfo.ModuleName, "ManageProcesses", siteName, (UserInfo)userInfo);
                    break;
            }

            return allowed || base.CheckPermissionsInternal(permission, siteName, userInfo, exceptionOnFailure);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty AutomationHistoryInfo object.
        /// </summary>
        public AutomationHistoryInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new AutomationHistoryInfo object from the given DataRow.
        /// </summary>
        /// <param name="dr">DataRow with the object data.</param>
        public AutomationHistoryInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion
    }
}
