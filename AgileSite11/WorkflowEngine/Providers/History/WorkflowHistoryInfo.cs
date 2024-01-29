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

[assembly: RegisterObjectType(typeof(WorkflowHistoryInfo), WorkflowHistoryInfo.OBJECT_TYPE)]

namespace CMS.WorkflowEngine
{
    /// <summary>
    /// Class to hold the workflow history record data.
    /// </summary>
    public class WorkflowHistoryInfo : AbstractInfo<WorkflowHistoryInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "cms.workflowhistory";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(WorkflowHistoryInfoProvider), OBJECT_TYPE, "CMS.WorkflowHistory", "WorkflowHistoryID", null, null, null, null, null, null, "VersionHistoryID", PredefinedObjectType.VERSIONHISTORY)
        {
            DependsOn = new List<ObjectDependency>
            {
                new ObjectDependency("StepID", WorkflowStepInfo.OBJECT_TYPE), 
                new ObjectDependency("TargetStepID", WorkflowStepInfo.OBJECT_TYPE), 
                new ObjectDependency("ApprovedByUserID", UserInfo.OBJECT_TYPE), 
                new ObjectDependency("HistoryWorkflowID", WorkflowInfo.OBJECT_TYPE), 
                new ObjectDependency("HistoryObjectID", null, ObjectDependencyEnum.Required, "HistoryObjectType")
            },
            TouchCacheDependencies = true,
            SupportsVersioning = false,
            ModuleName = ModuleName.WORKFLOWENGINE,
            SupportsCloning = false,
            RegisterAsChildToObjectTypes = new List<string>(),
            AllowRestore = false
        };

        #endregion


        #region Properties

        /// <summary>
        /// Workflow history ID.
        /// </summary>
        public int WorkflowHistoryID
        {
            get
            {
                return GetIntegerValue("WorkflowHistoryID", 0);
            }
            set
            {
                SetValue("WorkflowHistoryID", value);
            }
        }


        /// <summary>
        /// Version history ID.
        /// </summary>
        public int VersionHistoryID
        {
            get
            {
                return GetIntegerValue("VersionHistoryID", 0);
            }
            set
            {
                SetValue("VersionHistoryID", value);
            }
        }


        /// <summary>
        /// Step ID.
        /// </summary>
        public int StepID
        {
            get
            {
                return GetIntegerValue("StepID", 0);
            }
            set
            {
                SetValue("StepID", value);
            }
        }


        /// <summary>
        /// Step display name.
        /// </summary>
        public string StepDisplayName
        {
            get
            {
                return GetStringValue("StepDisplayName", "");
            }
            set
            {
                SetValue("StepDisplayName", value);
            }
        }


        /// <summary>
        /// Step name.
        /// </summary>
        public string StepName
        {
            get
            {
                return GetStringValue("StepName", "");
            }
            set
            {
                SetValue("StepName", value);
            }
        }


        /// <summary>
        /// Step type.
        /// </summary>
        public WorkflowStepTypeEnum StepType
        {
            get
            {
                return (WorkflowStepTypeEnum)GetIntegerValue("StepType", 0);
            }
            set
            {
                SetValue("StepType", (int)value);
            }
        }


        /// <summary>
        /// Target step ID.
        /// </summary>
        public int TargetStepID
        {
            get
            {
                return GetIntegerValue("TargetStepID", 0);
            }
            set
            {
                SetValue("TargetStepID", value, 0);
            }
        }


        /// <summary>
        /// Target step display name.
        /// </summary>
        public string TargetStepDisplayName
        {
            get
            {
                return GetStringValue("TargetStepDisplayName", "");
            }
            set
            {
                SetValue("TargetStepDisplayName", value, "");
            }
        }


        /// <summary>
        /// Target step name.
        /// </summary>
        public string TargetStepName
        {
            get
            {
                return GetStringValue("TargetStepName", "");
            }
            set
            {
                SetValue("TargetStepName", value, "");
            }
        }


        /// <summary>
        /// Target step type.
        /// </summary>
        public WorkflowStepTypeEnum TargetStepType
        {
            get
            {
                return (WorkflowStepTypeEnum)GetIntegerValue("TargetStepType", 0);
            }
            set
            {
                SetValue("TargetStepType", (int)value);
            }
        }


        /// <summary>
        /// History workflow ID.
        /// </summary>
        public int HistoryWorkflowID
        {
            get
            {
                return GetIntegerValue("HistoryWorkflowID", 0);
            }
            set
            {
                SetValue("HistoryWorkflowID", value, value > 0);
            }
        }


        /// <summary>
        /// Approved by user ID.
        /// </summary>
        public int ApprovedByUserID
        {
            get
            {
                return GetIntegerValue("ApprovedByUserID", 0);
            }
            set
            {
                SetValue("ApprovedByUserID", value);
            }
        }


        /// <summary>
        /// Approved when.
        /// </summary>
        public DateTime ApprovedWhen
        {
            get
            {
                return GetDateTimeValue("ApprovedWhen", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("ApprovedWhen", value, DateTimeHelper.ZERO_TIME);
            }
        }


        /// <summary>
        /// Comment.
        /// </summary>
        public string Comment
        {
            get
            {
                return GetStringValue("Comment", "");
            }
            set
            {
                SetValue("Comment", value);
            }
        }


        /// <summary>
        /// Was rejected. Specifies direction of the movement. True if the log is for reject action.
        /// </summary>
        public bool WasRejected
        {
            get
            {
                return GetBooleanValue("WasRejected", false);
            }
            set
            {
                SetValue("WasRejected", value);
            }
        }


        /// <summary>
        /// Indicates if the history was used when moved to previous step.
        /// </summary>
        public bool HistoryRejected
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
        /// Object type.
        /// </summary>
        public string HistoryObjectType
        {
            get
            {
                return GetStringValue("HistoryObjectType", null);
            }
            set
            {
                SetValue("HistoryObjectType", value);
            }
        }


        /// <summary>
        /// ID of the related object.
        /// </summary>
        public int HistoryObjectID
        {
            get
            {
                return GetIntegerValue("HistoryObjectID", 0);
            }
            set
            {
                SetValue("HistoryObjectID", value, 0);
            }
        }


        /// <summary>
        /// Type of the transition.
        /// </summary>
        public WorkflowTransitionTypeEnum HistoryTransitionType
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

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            WorkflowHistoryInfoProvider.DeleteWorkflowHistoryInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            WorkflowHistoryInfoProvider.SetWorkflowHistoryInfo(this);
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
                    allowed = UserInfoProvider.IsAuthorizedPerResource(TypeInfo.ModuleName, "ManageWorkflow", siteName, (UserInfo)userInfo);
                    break;
            }

            return allowed || base.CheckPermissionsInternal(permission, siteName, userInfo, exceptionOnFailure);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty WorkflowInfo structure.
        /// </summary>
        public WorkflowHistoryInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates the WorkflowInfo object from the given DataRow data.
        /// </summary>
        /// <param name="dr">Datarow with the workflow info data</param>
        public WorkflowHistoryInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion
    }
}