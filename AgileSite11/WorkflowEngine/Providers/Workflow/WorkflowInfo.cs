using System;
using System.Collections.Generic;
using System.Data;

using CMS;
using CMS.Core;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Base;
using CMS.WorkflowEngine;

[assembly: RegisterObjectType(typeof(WorkflowInfo), WorkflowInfo.OBJECT_TYPE)]
[assembly: RegisterObjectType(typeof(WorkflowInfo), WorkflowInfo.OBJECT_TYPE_AUTOMATION)]

namespace CMS.WorkflowEngine
{
    /// <summary>
    /// Class to hold the workflow record data.
    /// </summary>
    public class WorkflowInfo : AbstractInfo<WorkflowInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "cms.workflow";

        /// <summary>
        /// Object type for automation process
        /// </summary>
        public const string OBJECT_TYPE_AUTOMATION = "ma.automationprocess";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(WorkflowInfoProvider), OBJECT_TYPE, "CMS.Workflow", "WorkflowID", "WorkflowLastModified", "WorkflowGUID", "WorkflowName", "WorkflowDisplayName", null, null, null, null)
        {
            SynchronizationSettings =
            {
                LogSynchronization = SynchronizationTypeEnum.LogSynchronization,
                ObjectTreeLocations = new List<ObjectTreeLocation>
                {
                    new ObjectTreeLocation(GLOBAL, CONFIGURATION),
                }
            },

            LogEvents = true,
            TouchCacheDependencies = true,
            SupportsVersioning = true,
            ModuleName = ModuleName.WORKFLOWENGINE,
            Feature = FeatureEnum.WorkflowVersioning,
            TypeCondition = new TypeCondition().WhereNotEqualsOrNull("WorkflowType", (int)WorkflowTypeEnum.Automation),
            EnabledColumn = "WorkflowEnabled",
            ImportExportSettings =
            {
                IncludeToWebTemplateExport = ObjectRangeEnum.Site,
                LogExport = true,
                IsExportable = true,
                ObjectTreeLocations = new List<ObjectTreeLocation>
                {
                    new ObjectTreeLocation(GLOBAL, CONFIGURATION),
                },
            },
            ContinuousIntegrationSettings =
            {
                Enabled = true
            },
            DefaultData = new DefaultDataSettings
            {
                ExcludedColumns = new List<string> { "WorkflowRecurrenceType" }
            }
        };


        /// <summary>
        /// Type information for automation process.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO_PROCESS = new ObjectTypeInfo(typeof(WorkflowInfoProvider), OBJECT_TYPE_AUTOMATION, "CMS.Workflow", "WorkflowID", "WorkflowLastModified", "WorkflowGUID", "WorkflowName", "WorkflowDisplayName", null, null, null, null)
        {
            SynchronizationSettings =
            {
                LogSynchronization = SynchronizationTypeEnum.LogSynchronization,
                ObjectTreeLocations = new List<ObjectTreeLocation>
                {
                    new ObjectTreeLocation(GLOBAL, WorkflowModule.ONLINEMARKETING),
                }
            },

            LogEvents = true,
            TouchCacheDependencies = true,
            SupportsVersioning = true,
            MacroCollectionName = "MA.AutomationProcess",
            ModuleName = ModuleName.ONLINEMARKETING,
            TypeCondition = new TypeCondition().WhereEquals("WorkflowType", (int)WorkflowTypeEnum.Automation),
            OriginalTypeInfo = TYPEINFO,
            EnabledColumn = "WorkflowEnabled",
            ImportExportSettings =
            {
                IncludeToWebTemplateExport = ObjectRangeEnum.None,
                LogExport = true,
                IsExportable = true,
                ObjectTreeLocations = new List<ObjectTreeLocation>
                {
                    new ObjectTreeLocation(GLOBAL, WorkflowModule.ONLINEMARKETING),
                },
            },
            ContinuousIntegrationSettings =
            {
                Enabled = true
            }
        };

        #endregion


        #region "Properties"

        /// <summary>
        /// Workflow code name.
        /// </summary>
        public string WorkflowName
        {
            get
            {
                return GetStringValue("WorkflowName", "");
            }
            set
            {
                SetValue("WorkflowName", value);
            }
        }


        /// <summary>
        /// Workflow display name.
        /// </summary>
        public string WorkflowDisplayName
        {
            get
            {
                return GetStringValue("WorkflowDisplayName", "");
            }
            set
            {
                SetValue("WorkflowDisplayName", value);
            }
        }


        /// <summary>
        /// Workflow ID.
        /// </summary>
        public int WorkflowID
        {
            get
            {
                return GetIntegerValue("WorkflowID", 0);
            }
            set
            {
                SetValue("WorkflowID", value);
            }
        }


        /// <summary>
        /// Workflow GUID.
        /// </summary>
        public virtual Guid WorkflowGUID
        {
            get
            {
                return GetGuidValue("WorkflowGUID", Guid.Empty);
            }
            set
            {
                SetValue("WorkflowGUID", value, Guid.Empty);
            }
        }


        /// <summary>
        /// Object last modified.
        /// </summary>
        public virtual DateTime WorkflowLastModified
        {
            get
            {
                return GetDateTimeValue("WorkflowLastModified", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("WorkflowLastModified", value, DateTimeHelper.ZERO_TIME);
            }
        }


        /// <summary>
        /// Indicates if this workflow will automatically publish changes.
        /// </summary>
        public bool WorkflowAutoPublishChanges
        {
            get
            {
                return GetBooleanValue("WorkflowAutoPublishChanges", false);
            }
            set
            {
                SetValue("WorkflowAutoPublishChanges", value);
            }
        }


        /// <summary>
        /// Indicates if this workflow will use check-in/check-out functionality
        /// </summary>
        public bool? WorkflowUseCheckinCheckout
        {
            get
            {
                object value = GetValue("WorkflowUseCheckinCheckout");

                if (value != null)
                {
                    return ValidationHelper.GetBoolean(value, false);
                }

                return null;
            }
            set
            {
                SetValue("WorkflowUseCheckinCheckout", value);
            }
        }


        /// <summary>
        /// Type of the workflow
        /// </summary>
        public WorkflowTypeEnum WorkflowType
        {
            get
            {
                return (WorkflowTypeEnum)GetIntegerValue("WorkflowType", (int)WorkflowTypeEnum.Basic);
            }
            set
            {
                SetValue("WorkflowType", (int)value);
            }
        }


        /// <summary>
        /// Indicates if notification e-mails should be sent
        /// </summary>
        public bool? WorkflowSendEmails
        {
            get
            {
                object value = GetValue("WorkflowSendEmails");
                if (value != null)
                {
                    return ValidationHelper.GetBoolean(value, true);
                }

                return null;
            }
            set
            {
                SetValue("WorkflowSendEmails", value);
            }
        }


        /// <summary>
        /// E-mail template name for approve action
        /// </summary>
        public string WorkflowApprovedTemplateName
        {
            get
            {
                return GetStringValue("WorkflowApprovedTemplateName", null);
            }
            set
            {
                SetValue("WorkflowApprovedTemplateName", value);
            }
        }


        /// <summary>
        /// E-mail template name for ready to approval action
        /// </summary>
        public string WorkflowReadyForApprovalTemplateName
        {
            get
            {
                return GetStringValue("WorkflowReadyForApprovalTemplateName", null);
            }
            set
            {
                SetValue("WorkflowReadyForApprovalTemplateName", value);
            }
        }


        /// <summary>
        /// E-mail template name for reject action
        /// </summary>
        public string WorkflowRejectedTemplateName
        {
            get
            {
                return GetStringValue("WorkflowRejectedTemplateName", null);
            }
            set
            {
                SetValue("WorkflowRejectedTemplateName", value);
            }
        }


        /// <summary>
        /// E-mail template name for document publish action
        /// </summary>
        public string WorkflowPublishedTemplateName
        {
            get
            {
                return GetStringValue("WorkflowPublishedTemplateName", null);
            }
            set
            {
                SetValue("WorkflowPublishedTemplateName", value);
            }
        }


        /// <summary>
        /// E-mail template name for document archive action
        /// </summary>
        public string WorkflowArchivedTemplateName
        {
            get
            {
                return GetStringValue("WorkflowArchivedTemplateName", null);
            }
            set
            {
                SetValue("WorkflowArchivedTemplateName", value);
            }
        }


        /// <summary>
        /// E-mail template name for general notification
        /// </summary>
        public string WorkflowNotificationTemplateName
        {
            get
            {
                return GetStringValue("WorkflowNotificationTemplateName", null);
            }
            set
            {
                SetValue("WorkflowNotificationTemplateName", value);
            }
        }


        /// <summary>
        /// Indicates if e-mails should be sent for approve action
        /// </summary>
        public bool WorkflowSendApproveEmails
        {
            get
            {
                return GetBooleanValue("WorkflowSendApproveEmails", true);
            }
            set
            {
                SetValue("WorkflowSendApproveEmails", value);
            }
        }


        /// <summary>
        /// Indicates if e-mails should be sent for ready to approval action
        /// </summary>
        public bool WorkflowSendReadyForApprovalEmails
        {
            get
            {
                return GetBooleanValue("WorkflowSendReadyForApprovalEmails", true);
            }
            set
            {
                SetValue("WorkflowSendReadyForApprovalEmails", value);
            }
        }


        /// <summary>
        /// Indicates if e-mails should be sent for reject action
        /// </summary>
        public bool WorkflowSendRejectEmails
        {
            get
            {
                return GetBooleanValue("WorkflowSendRejectEmails", true);
            }
            set
            {
                SetValue("WorkflowSendRejectEmails", value);
            }
        }


        /// <summary>
        /// Indicates if e-mails should be sent for document publish action
        /// </summary>
        public bool WorkflowSendPublishEmails
        {
            get
            {
                return GetBooleanValue("WorkflowSendPublishEmails", true);
            }
            set
            {
                SetValue("WorkflowSendPublishEmails", value);
            }
        }


        /// <summary>
        /// Indicates if e-mails should be sent for document archive action
        /// </summary>
        public bool WorkflowSendArchiveEmails
        {
            get
            {
                return GetBooleanValue("WorkflowSendArchiveEmails", true);
            }
            set
            {
                SetValue("WorkflowSendArchiveEmails", value);
            }
        }


        /// <summary>
        /// Gets or sets the workflow allowed objects.
        /// </summary>
        public string WorkflowAllowedObjects
        {
            get
            {
                return GetStringValue("WorkflowAllowedObjects", null);
            }
            set
            {
                SetValue("WorkflowAllowedObjects", value);
            }
        }


        /// <summary>
        /// Gets or sets the recurring type of workflow.
        /// </summary>
        public ProcessRecurrenceTypeEnum WorkflowRecurrenceType
        {
            get
            {
                return (ProcessRecurrenceTypeEnum)GetIntegerValue("WorkflowRecurrenceType", 0);
            }
            set
            {
                SetValue("WorkflowRecurrenceType", (int)value);
            }
        }


        /// <summary>
        /// Indicates if workflow is enabled or disabled
        /// </summary>
        public bool WorkflowEnabled
        {
            get
            {
                return GetBooleanValue("WorkflowEnabled", true);
            }
            set
            {
                SetValue("WorkflowEnabled", value);
            }
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Object full name if defined
        /// </summary>
        protected override string ObjectFullName
        {
            get
            {
                return ObjectHelper.BuildFullName(WorkflowName, WorkflowType.ToString());
            }
        }


        /// <summary>
        /// Type information.
        /// </summary>
        public override ObjectTypeInfo TypeInfo
        {
            get
            {
                if (IsAutomation)
                {
                    return TYPEINFO_PROCESS;
                }
                else
                {
                    return TYPEINFO;
                }
            }
        }


        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            WorkflowInfoProvider.DeleteWorkflowInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            WorkflowInfoProvider.SetWorkflowInfo(this);
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
            // Special permission for automation process
            if (!IsAutomation)
            {
                return base.CheckPermissionsInternal(permission, siteName, userInfo, exceptionOnFailure);
            }

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
                case PermissionsEnum.Delete:
                case PermissionsEnum.Modify:
                case PermissionsEnum.Destroy:
                    return userInfo.IsAuthorizedPerResource(ModuleName.ONLINEMARKETING, "ManageProcesses", siteName, exceptionOnFailure);

                default:
                    return base.CheckPermissionsInternal(permission, siteName, userInfo, exceptionOnFailure);
            }
        }

        #endregion


        #region "Special properties"

        /// <summary>
        /// Indicates if workflow is document approval workflow
        /// </summary>
        public bool IsDocumentWorkflow
        {
            get
            {
                return !IsAutomation && (string.IsNullOrEmpty(WorkflowAllowedObjects) || WorkflowAllowedObjects.Contains(PredefinedObjectType.GROUP_DOCUMENTS));
            }
        }


        /// <summary>
        /// Indicates if workflow is basic workflow
        /// </summary>
        public bool IsBasic
        {
            get
            {
                return WorkflowType == WorkflowTypeEnum.Basic;
            }
        }


        /// <summary>
        /// Indicates if workflow is approval workflow
        /// </summary>
        public bool IsApproval
        {
            get
            {
                return WorkflowType == WorkflowTypeEnum.Approval;
            }
        }


        /// <summary>
        /// Indicates if workflow is advanced workflow
        /// </summary>
        public bool IsAdvanced
        {
            get
            {
                return WorkflowType == WorkflowTypeEnum.Advanced;
            }
        }


        /// <summary>
        /// Indicates if workflow is automation workflow
        /// </summary>
        public bool IsAutomation
        {
            get
            {
                return WorkflowType == WorkflowTypeEnum.Automation;
            }
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty WorkflowInfo structure.
        /// </summary>
        public WorkflowInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates the WorkflowInfo object from the given DataRow data.
        /// </summary>
        /// <param name="dr">Datarow with the workflow info data</param>
        public WorkflowInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Clones workflow transitions objects (has to be done after the whole process because it needs the steps already created).
        /// </summary>
        /// <param name="settings">Cloning settings</param>
        /// <param name="result">Result of the cloning - messages in this object will be altered by processing this method</param>
        /// <param name="originalObject">Original source BaseInfo (object being cloned)</param>
        protected override void InsertAsClonePostprocessing(CloneSettings settings, CloneResult result, BaseInfo originalObject)
        {
            // We need to clone WorkflowStepTransition objects if the steps were cloned
            if (settings.IncludeChildren && !settings.ExcludedChildTypes.Contains(WorkflowStepInfo.OBJECT_TYPE))
            {
                DataSet ds = WorkflowTransitionInfoProvider.GetWorkflowTransitions().WhereEquals("TransitionWorkflowID", originalObject.Generalized.ObjectID);
                if (!DataHelper.DataSourceIsEmpty(ds))
                {
                    int originalParentId = settings.ParentID;
                    settings.ParentID = 0;

                    foreach (DataRow dr in ds.Tables[0].Rows)
                    {
                        WorkflowTransitionInfo transition = new WorkflowTransitionInfo(dr);
                        transition.TransitionEndStepID = settings.Translations.GetNewID(WorkflowStepInfo.OBJECT_TYPE, transition.TransitionEndStepID, null, 0, null, null, null);
                        transition.TransitionStartStepID = settings.Translations.GetNewID(WorkflowStepInfo.OBJECT_TYPE, transition.TransitionStartStepID, null, 0, null, null, null);
                        transition.TransitionWorkflowID = WorkflowID;
                        transition.Generalized.InsertAsClone(settings, result);
                    }

                    settings.ParentID = originalParentId;
                }
            }

            if (!settings.IncludeChildren || settings.ExcludedChildTypes.Contains(WorkflowStepInfo.OBJECT_TYPE))
            {
                // Create default steps (Edit, Publish, etc.) if they were not cloned explicitly
                WorkflowStepInfoProvider.CreateDefaultWorkflowSteps(this);
            }
        }


        /// <summary>
        /// Registers the properties of this object
        /// </summary>
        protected sealed override void RegisterProperties()
        {
            base.RegisterProperties();

            RegisterProperty<bool>("IsAdvanced", w => w.IsAdvanced);
            RegisterProperty<bool>("IsApproval", w => w.IsApproval);
            RegisterProperty<bool>("IsAutomation", w => w.IsAutomation);
            RegisterProperty<bool>("IsBasic", w => w.IsBasic);
            RegisterProperty<bool>("IsDocumentWorkflow", w => w.IsDocumentWorkflow);
        }


        /// <summary>
        /// Loads the default data to the object.
        /// </summary>
        protected override void LoadDefaultData()
        {
            base.LoadDefaultData();

            WorkflowEnabled = true;
        }

        #endregion


        #region "Public Methods"

        /// <summary>
        /// Indicates if current workflow uses check-in/check-out functionality
        /// </summary>
        /// <param name="siteName">Site name</param>
        public bool UseCheckInCheckOut(string siteName)
        {
            // Use local settings of the workflow
            if (WorkflowUseCheckinCheckout.HasValue)
            {
                return WorkflowUseCheckinCheckout.Value;
            }
            // Workflow inherits settings from site settings
            else
            {
                return SettingsKeyInfoProvider.GetBoolValue(siteName + ".CMSUseCheckinCheckout");
            }
        }


        /// <summary>
        /// Indicates if notification e-mails should be sent for current workflow
        /// </summary>
        /// <param name="siteName">Site name</param>
        /// <param name="emailType">Type of workflow e-mail</param>
        public bool SendEmails(string siteName, WorkflowEmailTypeEnum emailType)
        {
            bool send;

            // Use local settings of the workflow
            if (WorkflowSendEmails.HasValue)
            {
                send = WorkflowSendEmails.Value;
            }
            // Workflow inherits settings from site settings
            else
            {
                send = WorkflowHelper.SendWorkflowEmails(siteName);
            }

            if (send)
            {
                // Apply workflow settings
                switch (emailType)
                {
                    case WorkflowEmailTypeEnum.Approved:
                        return WorkflowSendApproveEmails;

                    case WorkflowEmailTypeEnum.ReadyForApproval:
                        return WorkflowSendReadyForApprovalEmails;

                    case WorkflowEmailTypeEnum.Rejected:
                        return WorkflowSendRejectEmails;

                    case WorkflowEmailTypeEnum.Published:
                        return WorkflowSendPublishEmails;

                    case WorkflowEmailTypeEnum.Archived:
                        return WorkflowSendArchiveEmails;

                    default:
                        return true;
                }
            }

            return false;
        }


        /// <summary>
        /// Gets e-mail template to be used.
        /// </summary>
        /// <param name="emailType">Type of workflow e-mail</param>
        public string GetEmailTemplateName(WorkflowEmailTypeEnum emailType)
        {
            string template;

            switch (emailType)
            {
                case WorkflowEmailTypeEnum.Approved:
                    template = WorkflowApprovedTemplateName;
                    break;

                case WorkflowEmailTypeEnum.ReadyForApproval:
                    template = WorkflowReadyForApprovalTemplateName;
                    break;

                case WorkflowEmailTypeEnum.Rejected:
                    template = WorkflowRejectedTemplateName;
                    break;

                case WorkflowEmailTypeEnum.Published:
                    template = WorkflowPublishedTemplateName;
                    break;

                case WorkflowEmailTypeEnum.Archived:
                    template = WorkflowArchivedTemplateName;
                    break;

                case WorkflowEmailTypeEnum.Notification:
                    template = WorkflowNotificationTemplateName;
                    break;

                default:
                    throw new Exception("[WorkflowInfo.GetEmailTemplateName]: Unknown e-mail type.");
            }

            return DataHelper.GetNotEmpty(template, WorkflowHelper.GetDefaultEmailTemplateName(emailType));
        }

        #endregion
    }
}