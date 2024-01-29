using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Helpers.UniGraphConfig;
using CMS.Base;
using CMS.WorkflowEngine;
using CMS.WorkflowEngine.Definitions;
using CMS.WorkflowEngine.GraphConfig;

using SourcePoint = CMS.WorkflowEngine.Definitions.SourcePoint;

[assembly: RegisterObjectType(typeof(WorkflowStepInfo), WorkflowStepInfo.OBJECT_TYPE)]
[assembly: RegisterObjectType(typeof(WorkflowStepInfo), WorkflowStepInfo.OBJECT_TYPE_AUTOMATION)]

namespace CMS.WorkflowEngine
{
    /// <summary>
    /// Class to hold the workflow step record data.
    /// </summary>
    [RegisterAllProperties]
    public class WorkflowStepInfo : AbstractInfo<WorkflowStepInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "cms.workflowstep";

        /// <summary>
        /// Object type for automation step
        /// </summary>
        public const string OBJECT_TYPE_AUTOMATION = "ma.automationstep";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(WorkflowStepInfoProvider), OBJECT_TYPE, "CMS.WorkflowStep", "StepID", "StepLastModified", "StepGUID", "StepName", "StepDisplayName", null, null, "StepWorkflowID", WorkflowInfo.OBJECT_TYPE)
            {
                DependsOn = new List<ObjectDependency> { new ObjectDependency("StepActionID", WorkflowActionInfo.OBJECT_TYPE, ObjectDependencyEnum.Required) },
                Extends = new List<ExtraColumn> { new ExtraColumn(ObjectSettingsInfo.OBJECT_TYPE, "ObjectWorkflowStepID") },
                LogEvents = true,
                TouchCacheDependencies = true,
                SupportsVersioning = false,
                SupportsCloning = false,
                OrderColumn = "StepOrder",
                CheckDependenciesOnDelete = true,
                TypeCondition = new TypeCondition().WhereNotEqualsOrNull("StepWorkflowType", (int)WorkflowTypeEnum.Automation),
                DefaultData = new DefaultDataSettings(),
                ContinuousIntegrationSettings =
                {
                    Enabled = true,
                },
                SerializationSettings =
                {
                    StructuredFields = new IStructuredField[]
                    {
                        new StructuredField("StepDefinition"),
                        new StructuredField("StepActionParameters"), 
                    },
                }
            };


        /// <summary>
        /// Type information for automation step.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO_AUTOMATION = new ObjectTypeInfo(typeof(WorkflowStepInfoProvider), OBJECT_TYPE_AUTOMATION, "CMS.WorkflowStep", "StepID", "StepLastModified", "StepGUID", "StepName", "StepDisplayName", null, null, "StepWorkflowID", WorkflowInfo.OBJECT_TYPE_AUTOMATION)
        {
            DependsOn = new List<ObjectDependency> { new ObjectDependency("StepActionID", WorkflowActionInfo.OBJECT_TYPE_AUTOMATION, ObjectDependencyEnum.Required) },
            LogEvents = true,
            TouchCacheDependencies = true,
            SupportsVersioning = false,
            OriginalTypeInfo = TYPEINFO,
            SupportsCloning = false,
            CheckDependenciesOnDelete = true,
            TypeCondition = new TypeCondition().WhereEquals("StepWorkflowType", (int)WorkflowTypeEnum.Automation),
            ContinuousIntegrationSettings =
            {
                Enabled = true
            },
            SerializationSettings =
            {
                StructuredFields = new IStructuredField[]
                {
                    new StructuredField("StepDefinition"),
                    new StructuredField("StepActionParameters"),
                },
            }
        };

        #endregion


        #region "Variables"

        private readonly object mutex = new object();

        private Step mStepDefinition;
        private ObjectParameters mStepActionParameters;

        #endregion


        #region "Properties"

        /// <summary>
        /// Step code name.
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
        /// Step workflow ID.
        /// </summary>
        public int StepWorkflowID
        {
            get
            {
                return GetIntegerValue("StepWorkflowID", 0);
            }
            set
            {
                SetValue("StepWorkflowID", value, value > 0);
            }
        }


        /// <summary>
        /// Step workflow type.
        /// </summary>
        public WorkflowTypeEnum StepWorkflowType
        {
            get
            {
                return (WorkflowTypeEnum)GetIntegerValue("StepWorkflowType", (int)WorkflowTypeEnum.Basic);
            }
            set
            {
                SetValue("StepWorkflowType", (int)value);
            }
        }


        /// <summary>
        /// Step action ID.
        /// </summary>
        public int StepActionID
        {
            get
            {
                return GetIntegerValue("StepActionID", 0);
            }
            set
            {
                SetValue("StepActionID", value, value > 0);
            }
        }


        /// <summary>
        /// Step action parameters.
        /// </summary>
        [NotRegisterProperty]
        public ObjectParameters StepActionParameters
        {
            get
            {
                if (mStepActionParameters == null)
                {
                    mStepActionParameters = new ObjectParameters();
                    mStepActionParameters.LoadData(ValidationHelper.GetString(GetStringValue("StepActionParameters", String.Empty), String.Empty));
                }
                return mStepActionParameters;
            }
        }


        /// <summary>
        /// Step order index.
        /// </summary>
        public int StepOrder
        {
            get
            {
                return GetIntegerValue("StepOrder", 0);
            }
            set
            {
                SetValue("StepOrder", value, 0);
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
                if (value <= 0)
                {
                    SetValue("StepID", null);
                }
                else
                {
                    SetValue("StepID", value);
                }
            }
        }


        /// <summary>
        /// Step GUID.
        /// </summary>
        public Guid StepGUID
        {
            get
            {
                return GetGuidValue("StepGUID", Guid.Empty);
            }
            set
            {
                SetValue("StepGUID", value, Guid.Empty);
            }
        }


        /// <summary>
        /// Step last modified.
        /// </summary>
        public DateTime StepLastModified
        {
            get
            {
                return GetDateTimeValue("StepLastModified", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("StepLastModified", value, DateTimeHelper.ZERO_TIME);
            }
        }


        /// <summary>
        /// Step type.
        /// </summary>
        public WorkflowStepTypeEnum StepType
        {
            get
            {
                return (WorkflowStepTypeEnum)GetIntegerValue("StepType", (int)WorkflowStepTypeEnum.Standard);
            }
            set
            {
                SetValue("StepType", (int)value);
            }
        }


        /// <summary>
        /// Indicates if step allows reject action.
        /// </summary>
        [DatabaseMapping("((StepAllowReject IS NULL OR StepAllowReject=1) AND StepType NOT IN (2, 101))")]
        public bool StepAllowReject
        {
            get
            {
                bool allowReject = GetBooleanValue("StepAllowReject", true);
                // Do not allow reject for document edit and archived step
                return allowReject && (StepType != WorkflowStepTypeEnum.DocumentArchived) && (StepType != WorkflowStepTypeEnum.DocumentEdit);
            }
            set
            {
                // Do not allow reject for document edit and archived step
                bool allowReject = ((StepType == WorkflowStepTypeEnum.DocumentArchived) || (StepType == WorkflowStepTypeEnum.DocumentEdit)) && value;
                SetValue("StepAllowReject", allowReject);
            }
        }


        /// <summary>
        /// Indicates if step allows direct publish action.
        /// </summary>
        [DatabaseMapping("(StepAllowPublish=1 AND StepType NOT IN (100, 101))")]
        public bool StepAllowPublish
        {
            get
            {
                bool allowPublish = GetBooleanValue("StepAllowPublish", false);
                // Do not allow publish for document published and archived step
                return allowPublish && (StepType != WorkflowStepTypeEnum.DocumentArchived) && (StepType != WorkflowStepTypeEnum.DocumentPublished);
            }
            set
            {
                // Do not allow publish for document published and archived step
                bool allowPublish = (StepType != WorkflowStepTypeEnum.DocumentArchived) && (StepType != WorkflowStepTypeEnum.DocumentPublished) && value;
                SetValue("StepAllowPublish", allowPublish);
            }
        }


        /// <summary>
        /// Security settings for roles
        /// </summary>
        public WorkflowStepSecurityEnum StepRolesSecurity
        {
            get
            {
                return (WorkflowStepSecurityEnum)GetIntegerValue("StepRolesSecurity", (int)WorkflowStepSecurityEnum.OnlyAssigned);
            }
            set
            {
                SetValue("StepRolesSecurity", (int)value);
            }
        }


        /// <summary>
        /// Security settings for users
        /// </summary>
        public WorkflowStepSecurityEnum StepUsersSecurity
        {
            get
            {
                return (WorkflowStepSecurityEnum)GetIntegerValue("StepUsersSecurity", (int)WorkflowStepSecurityEnum.OnlyAssigned);
            }
            set
            {
                SetValue("StepUsersSecurity", (int)value);
            }
        }


        /// <summary>
        /// E-mail template name for ready to approval action when moving to this step
        /// </summary>
        public string StepReadyForApprovalTemplateName
        {
            get
            {
                return GetStringValue("StepReadyForApprovalTemplateName", null);
            }
            set
            {
                SetValue("StepReadyForApprovalTemplateName", value);
            }
        }


        /// <summary>
        /// E-mail template name for approve action when moving to this step
        /// </summary>
        public string StepApprovedTemplateName
        {
            get
            {
                return GetStringValue("StepApprovedTemplateName", null);
            }
            set
            {
                SetValue("StepApprovedTemplateName", value);
            }
        }


        /// <summary>
        /// E-mail template name for reject action when moving from this step
        /// </summary>
        public string StepRejectedTemplateName
        {
            get
            {
                return GetStringValue("StepRejectedTemplateName", null);
            }
            set
            {
                SetValue("StepRejectedTemplateName", value);
            }
        }


        /// <summary>
        /// Indicates if notification e-mails should be sent when moving to this step
        /// </summary>
        public bool? StepSendEmails
        {
            get
            {
                object value = GetValue("StepSendEmails");
                if (value != null)
                {
                    return ValidationHelper.GetBoolean(value, false);
                }

                return null;
            }
            set
            {
                SetValue("StepSendEmails", value);
            }
        }


        /// <summary>
        /// Indicates if e-mails should be sent for approve action
        /// </summary>
        public bool StepSendApproveEmails
        {
            get
            {
                return GetBooleanValue("StepSendApproveEmails", true);
            }
            set
            {
                SetValue("StepSendApproveEmails", value);
            }
        }


        /// <summary>
        /// Indicates if e-mails should be sent for ready to approval action
        /// </summary>
        public bool StepSendReadyForApprovalEmails
        {
            get
            {
                return GetBooleanValue("StepSendReadyForApprovalEmails", true);
            }
            set
            {
                SetValue("StepSendReadyForApprovalEmails", value);
            }
        }


        /// <summary>
        /// Indicates if e-mails should be sent for reject action
        /// </summary>
        public bool StepSendRejectEmails
        {
            get
            {
                return GetBooleanValue("StepSendRejectEmails", true);
            }
            set
            {
                SetValue("StepSendRejectEmails", value);
            }
        }

        #endregion


        #region "Special properties"

        /// <summary>
        /// Related transition which leads to this step. (This property is initialized only when deciding the next steps.)
        /// </summary>
        [NotRegisterProperty]
        public WorkflowTransitionInfo RelatedTransition
        {
            get;
            set;
        }


        /// <summary>
        /// Related workflow history ID to which the step belongs to in context of reject action. (This property is initialized only when deciding the previous steps.)
        /// </summary>
        [NotRegisterProperty]
        public int RelatedHistoryID
        {
            get;
            set;
        }


        /// <summary>
        /// Workflow step full code name in format [workflowcodename].[workflowstepcodename].
        /// </summary>
        [DatabaseMapping(false)]
        [Obsolete("Object is not cached by full name.")]
        public string StepFullName
        {
            get
            {
                WorkflowInfo wi = WorkflowInfoProvider.GetWorkflowInfo(StepWorkflowID);
                if (wi != null)
                {
                    return wi.WorkflowName.ToLowerCSafe() + "." + StepName.ToLowerCSafe();
                }

                return StepName.ToLowerCSafe();
            }
        }


        /// <summary>
        /// Indicates if step represents published document state
        /// </summary>
        public bool StepIsPublished
        {
            get
            {
                return (StepType == WorkflowStepTypeEnum.DocumentPublished);
            }
        }


        /// <summary>
        /// Indicates if step represents start workflow state
        /// </summary>
        public bool StepIsStart
        {
            get
            {
                return (StepType == WorkflowStepTypeEnum.Start);
            }
        }


        /// <summary>
        /// Indicates if step represents finished workflow state
        /// </summary>
        public bool StepIsFinished
        {
            get
            {
                return (StepType == WorkflowStepTypeEnum.Finished);
            }
        }


        /// <summary>
        /// Indicates if step represents edit document state
        /// </summary>
        public bool StepIsEdit
        {
            get
            {
                return (StepType == WorkflowStepTypeEnum.DocumentEdit);
            }
        }


        /// <summary>
        /// Indicates if step represents archived document state
        /// </summary>
        public bool StepIsArchived
        {
            get
            {
                return (StepType == WorkflowStepTypeEnum.DocumentArchived);
            }
        }


        /// <summary>
        /// Indicates if step represents one of the default document step types (DocumentPublished, DocumentArchived, DocumentEdit, Start)
        /// </summary>
        public bool StepIsDefault
        {
            get
            {
                return StepIsArchived || StepIsEdit || StepIsPublished || StepIsStart || StepIsFinished;
            }
        }


        /// <summary>
        /// Indicates if step is action step
        /// </summary>
        public bool StepIsAction
        {
            get
            {
                return (StepType == WorkflowStepTypeEnum.Action);
            }
        }


        /// <summary>
        /// Indicates if step can be deleted. Deletion is not allowed for some step types.
        /// </summary>
        [DatabaseMapping(false)]
        public bool StepIsDeletable
        {
            get
            {
                return !(StepIsEdit || (StepType == WorkflowStepTypeEnum.Start));
            }
        }


        /// <summary>
        /// Indicates if step is allowed due to security settings
        /// </summary>
        [DatabaseMapping(false)]
        public bool StepIsAllowed
        {
            get
            {
                return (StepRolesSecurity != WorkflowStepSecurityEnum.Default) || (StepUsersSecurity != WorkflowStepSecurityEnum.Default);
            }
        }


        /// <summary>
        /// Step definition (advanced settings).
        /// </summary>
        [DatabaseMapping(false)]
        [NotRegisterProperty]
        public Step StepDefinition
        {
            get
            {
                if (mStepDefinition == null)
                {
                    string data = GetStringValue("StepDefinition", null);
                    // Load existing data
                    if ((StepID > 0) || !string.IsNullOrEmpty(data))
                    {
                        mStepDefinition = new Step();
                        mStepDefinition.LoadData(data, StepType);
                    }
                    // Ensure default data
                    else
                    {
                        EnsureDefaultDefinition();
                    }
                }

                return mStepDefinition;
            }
        }


        /// <summary>
        /// Indicates if step has timeout defined
        /// </summary>
        [DatabaseMapping(false)]
        public bool StepHasTimeout
        {
            get
            {
                return StepDefinition.TimeoutEnabled && !string.IsNullOrEmpty(StepDefinition.TimeoutInterval);
            }
        }


        /// <summary>
        /// Indicates if step allows branch
        /// </summary>
        [DatabaseMapping(false)]
        public bool StepAllowBranch
        {
            get
            {
                // These steps allow branch (multiple win transitions)
                if (!StepHasSingleWinTransition)
                {
                    return true;
                }

                switch (StepType)
                {
                    // Steps which also allow branch
                    case WorkflowStepTypeEnum.Condition:
                    case WorkflowStepTypeEnum.MultichoiceFirstWin:
                        return true;
                }

                return false;
            }
        }


        /// <summary>
        /// Indicates if step allows timeout settings
        /// </summary>
        [DatabaseMapping(false)]
        public bool StepAllowTimeout
        {
            get
            {
                // All steps except condition and actions
                switch (StepType)
                {
                    case WorkflowStepTypeEnum.Condition:
                    case WorkflowStepTypeEnum.Action:
                    case WorkflowStepTypeEnum.Start:
                    case WorkflowStepTypeEnum.Finished:
                        return false;
                }

                return true;
            }
        }


        /// <summary>
        /// Indicates if step allows only automatic outgoing transitions
        /// </summary>
        [DatabaseMapping(false)]
        [NotRegisterProperty]
        public bool StepAllowOnlyAutomaticTransitions
        {
            get
            {
                return (StepType == WorkflowStepTypeEnum.Condition) || (StepType == WorkflowStepTypeEnum.MultichoiceFirstWin) || StepIsAction || (StepType == WorkflowStepTypeEnum.Wait) || StepIsStart;
            }
        }


        /// <summary>
        /// Indicates if default timeout target can be specified
        /// </summary>
        [DatabaseMapping(false)]
        [NotRegisterProperty]
        public bool StepAllowDefaultTimeoutTarget
        {
            get
            {
                return StepIsDefault || (StepType == WorkflowStepTypeEnum.Standard) || (StepType == WorkflowStepTypeEnum.Userchoice);
            }
        }


        /// <summary>
        /// Indicates if step allows branch and always has single winning transition
        /// </summary>
        [DatabaseMapping(false)]
        [NotRegisterProperty]
        public bool StepHasSingleWinTransition
        {
            get
            {
                switch (StepType)
                {
                    // Steps where multiple winning transitions are not possible
                    case WorkflowStepTypeEnum.Multichoice:
                    case WorkflowStepTypeEnum.Userchoice:
                        return false;
                }

                return true;
            }
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Type information.
        /// </summary>
        public override ObjectTypeInfo TypeInfo
        {
            get
            {
                if (StepWorkflowType == WorkflowTypeEnum.Automation)
                {
                    return TYPEINFO_AUTOMATION;
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
            WorkflowStepInfoProvider.DeleteWorkflowStepInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            WorkflowStepInfoProvider.SetWorkflowStepInfo(this);
        }


        /// <summary>
        /// Clones workflow transitions objects (has to be done after the whole process because it needs the steps already created).
        /// </summary>
        /// <param name="settings">Cloning settings</param>
        /// <param name="result">Result of the cloning - messages in this object will be altered by processing this method</param>
        /// <param name="originalObject">Original source BaseInfo (object being cloned)</param>
        protected override void InsertAsClonePostprocessing(CloneSettings settings, CloneResult result, BaseInfo originalObject)
        {
            // Transitions are cloned separately after all steps are cloned.
            settings.ExcludedChildTypes.Add(WorkflowTransitionInfo.OBJECT_TYPE);
        }


        /// <summary>
        /// Creates where condition according to Parent, Group and Site settings.
        /// </summary>
        protected override WhereCondition GetSiblingsWhereCondition()
        {
            // Exclude default steps from siblings
            return base.GetSiblingsWhereCondition().Where("StepType NOT IN (2, 100, 101)");
        }


        /// <summary>
        /// Returns number which will be the last order within all the other items (according to Parent, Group and Site settings).
        /// I.e. returns largest existing order + 1.
        /// </summary>
        /// <param name="orderColumn">Name of the order column. If null, OrderColumn from TypeInfo is taken</param>
        protected override int GetLastObjectOrder(string orderColumn)
        {
            var order = base.GetLastObjectOrder(orderColumn);

            // Special case - custom step is always placed behind the Edit step
            if (!StepIsDefault && (order < 2))
            {
                return 2;
            }

            return order;
        }


        /// <summary>
        /// Method called after the InitObjectOrder method is called. Override this to do further actions after order initialization. Does nothing by default.
        /// </summary>
        protected override void InitObjectsOrderPostprocessing()
        {
            QueryDataParameters parameters = new QueryDataParameters();
            parameters.Add("@StepWorkflowID", StepWorkflowID);

            // Make sure Publish and Archive steps are the last steps
            ConnectionHelper.ExecuteQuery("CMS.WorkflowStep.InitStepOrders", parameters);

            base.InitObjectsOrderPostprocessing();
        }


        /// <summary>
        /// Registers the properties of this object
        /// </summary>
        protected sealed override void RegisterProperties()
        {
            base.RegisterProperties();

            RegisterProperty<WorkflowStepTypeEnum>("StepType", s => s.StepType);

            RegisterProperty<bool>("StepAllowPublish", s => s.StepAllowPublish);
            RegisterProperty<bool>("StepAllowReject", s => s.StepAllowReject);

            RegisterProperty<bool>("StepIsArchived", s => s.StepIsArchived);
            RegisterProperty<bool>("StepIsDefault", s => s.StepIsDefault);
            RegisterProperty<bool>("StepIsEdit", s => s.StepIsEdit);
            RegisterProperty<bool>("StepIsStart", s => s.StepIsStart);
            RegisterProperty<bool>("StepIsFinished", s => s.StepIsFinished);
            RegisterProperty<bool>("StepIsPublished", s => s.StepIsPublished);
            RegisterProperty<bool>("StepIsAction", s => s.StepIsAction);

            RegisterProperty<WorkflowInfo>("StepWorkflow", s => WorkflowInfoProvider.GetWorkflowInfo(s.StepWorkflowID));
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty WorkflowStepInfo object.
        /// </summary>
        public WorkflowStepInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new WorkflowStepInfo object from the given DataRow.
        /// </summary>
        /// <param name="dr">DataRow with the data</param>
        public WorkflowStepInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }


        /// <summary>
        /// Loads the default object data
        /// </summary>
        protected override void LoadDefaultData()
        {
            base.LoadDefaultData();

            StepType = WorkflowStepTypeEnum.Standard;
            StepRolesSecurity = WorkflowStepSecurityEnum.Default;
            StepUsersSecurity = WorkflowStepSecurityEnum.Default;
        }

        #endregion


        #region "IAdvancedDataContainer methods"

        /// <summary>
        /// Returns value of column.
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="value">Returns the value</param>
        /// <returns>Returns true if the operation was successful (the value was present)</returns>
        public override bool TryGetValue(string columnName, out object value)
        {
            bool result = base.TryGetValue(columnName, out value);
            if (result)
            {
                return true;
            }

            switch (columnName.ToLowerCSafe())
            {
                // Step advanced settings
                case "stepdefinition":
                    // Returns the definition data XML
                    value = StepDefinition.GetData();
                    result = true;
                    break;

                // Step action parameters
                case "stepactionparameters":
                    // Returns the property values XML
                    value = StepActionParameters.GetData();
                    result = true;
                    break;
            }

            // Ensure the null value
            value = DataHelper.GetNull(value);
            return result;
        }


        /// <summary>
        /// Sets value of the specified node column.
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="value">Value</param>
        public override bool SetValue(string columnName, object value)
        {
            // Convert NULL to DBNull.Value
            if (value == null)
            {
                value = DBNull.Value;
            }

            bool result = base.SetValue(columnName, value);

            // Special columns treatment
            switch (columnName.ToLowerCSafe())
            {
                // Step advanced settings
                case "stepdefinition":
                    mStepDefinition = new Step();
                    mStepDefinition.LoadData(ValidationHelper.GetString(value, String.Empty), StepType);
                    break;

                // Step action parameters
                case "stepactionparameters":
                    StepActionParameters.LoadData(ValidationHelper.GetString(value, String.Empty));
                    break;
            }

            return result;
        }

        #endregion


        #region "Public methods"

        /// <summary>
        /// Indicates if notification e-mails should be sent when moving to this step
        /// </summary>
        /// <param name="siteName">Site name</param>
        /// <param name="emailType">Type of workflow e-mail</param>
        public bool SendEmails(string siteName, WorkflowEmailTypeEnum emailType)
        {
            bool send = false;

            // Use local settings of the step
            if (StepSendEmails.HasValue)
            {
                send = StepSendEmails.Value;
            }
            // Step inherits settings from workflow
            else
            {
                // Get step workflow
                WorkflowInfo wi = WorkflowInfoProvider.GetWorkflowInfo(StepWorkflowID);
                if (wi != null)
                {
                    send = wi.SendEmails(siteName, emailType);
                }
            }

            if (send)
            {
                // Apply step settings
                switch (emailType)
                {
                    case WorkflowEmailTypeEnum.Approved:
                        return StepSendApproveEmails;

                    case WorkflowEmailTypeEnum.ReadyForApproval:
                        return StepSendReadyForApprovalEmails;

                    case WorkflowEmailTypeEnum.Rejected:
                        return StepSendRejectEmails;

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
            // Get step workflow
            WorkflowInfo wi = WorkflowInfoProvider.GetWorkflowInfo(StepWorkflowID);
            if (wi != null)
            {
                string templateName = null;

                switch (emailType)
                {
                    case WorkflowEmailTypeEnum.Approved:
                        templateName = StepApprovedTemplateName;
                        break;

                    case WorkflowEmailTypeEnum.ReadyForApproval:
                        templateName = StepReadyForApprovalTemplateName;
                        break;

                    case WorkflowEmailTypeEnum.Rejected:
                        templateName = StepRejectedTemplateName;
                        break;
                }

                return DataHelper.GetNotEmpty(templateName, wi.GetEmailTemplateName(emailType));
            }

            return null;
        }


        /// <summary>
        /// Returns default transition type for transitions starting in this step.
        /// </summary>
        public WorkflowTransitionTypeEnum GetStepConnectionType(Guid startSourcePointGuid)
        {
            // Only automatic connections can lead from timeout source points
            var sourcePoint = GetSourcePoint(startSourcePointGuid);
            if ((sourcePoint != null) && sourcePoint.Type == SourcePointTypeEnum.Timeout)
            {
                return WorkflowTransitionTypeEnum.Automatic;
            }

            switch (StepType)
            {
                case WorkflowStepTypeEnum.Standard:
                case WorkflowStepTypeEnum.DocumentEdit:
                case WorkflowStepTypeEnum.DocumentPublished:
                case WorkflowStepTypeEnum.DocumentArchived:
                case WorkflowStepTypeEnum.Userchoice:
                    return WorkflowTransitionTypeEnum.Manual;

                case WorkflowStepTypeEnum.Start:
                case WorkflowStepTypeEnum.Condition:
                case WorkflowStepTypeEnum.Multichoice:
                case WorkflowStepTypeEnum.MultichoiceFirstWin:
                case WorkflowStepTypeEnum.Wait:
                case WorkflowStepTypeEnum.Action:
                    return WorkflowTransitionTypeEnum.Automatic;

                default:
                    throw new InvalidOperationException("Unsupported workflow transition type.");
            }
        }


        /// <summary>
        /// Connects step with target step.
        /// </summary>
        /// <param name="sourcePoint">Start step source point</param>
        /// <param name="targetStep">Target step</param>
        public WorkflowTransitionInfo ConnectTo(Guid sourcePoint, WorkflowStepInfo targetStep)
        {
            return WorkflowStepInfoProvider.ConnectSteps(this, sourcePoint, targetStep);
        }


        /// <summary>
        /// Ensures default step definition
        /// </summary>
        public void EnsureDefaultDefinition()
        {
            Step definition = new Step();
            switch (StepType)
            {
                case WorkflowStepTypeEnum.Condition:
                    definition.SourcePoints.Add(new ConditionSourcePoint());
                    definition.SourcePoints.Add(new ElseSourcePoint());
                    break;

                case WorkflowStepTypeEnum.MultichoiceFirstWin:
                case WorkflowStepTypeEnum.Multichoice:
                    if (StepType == WorkflowStepTypeEnum.Multichoice)
                    {
                        definition.DefinitionPoint = new SourcePoint();
                    }

                    definition.SourcePoints.Add(new CaseSourcePoint(1));
                    definition.SourcePoints.Add(new CaseSourcePoint(2));
                    definition.SourcePoints.Add(new ElseSourcePoint());
                    break;

                case WorkflowStepTypeEnum.Userchoice:
                    definition.DefinitionPoint = new SourcePoint();

                    definition.SourcePoints.Add(new ChoiceSourcePoint(1));
                    definition.SourcePoints.Add(new ChoiceSourcePoint(2));
                    break;

                case WorkflowStepTypeEnum.Finished:
                    definition.DefinitionPoint = new SourcePoint();
                    break;

                default:
                    definition.SourcePoints.Add(new SourcePoint());
                    break;
            }

            mStepDefinition = definition;
        }

        #endregion


        #region "Source point manipulation methods"

        /// <summary>
        /// Gets source point
        /// </summary>
        /// <param name="sourcePointGuid">Source point GUID</param>
        public SourcePoint GetSourcePoint(Guid sourcePointGuid)
        {
            if (sourcePointGuid != Guid.Empty)
            {
                var points = from p in StepDefinition.SourcePoints where p.Guid == sourcePointGuid select p;
                return points.FirstOrDefault();
            }

            return null;
        }


        /// <summary>
        /// Adds source point with default definition.
        /// </summary>
        /// <returns>Potential error message</returns>
        /// <remarks>Throws exception if database operation fails.</remarks>
        public string AddSourcePoint()
        {
            return AddSourcePoint(WorkflowHelper.CreateSourcePoint(StepType));
        }


        /// <summary>
        /// Adds source point.
        /// </summary>
        /// <param name="sourcePoint">Source point to be added</param>
        /// <returns>Potential error message</returns>
        /// <remarks>Throws exception if database operation fails.</remarks>
        public string AddSourcePoint(SourcePoint sourcePoint)
        {
            if (StepDefinition == null)
            {
                return ResHelper.GetString("workflowstep.cannothavesourcepoints");
            }

            if (sourcePoint == null)
            {
                return "[WorkflowStepInfo.AddSourcePoint]: Source point is not provided.";
            }

            Node node = WorkflowNode.GetInstance(this);
            int timeoutSourcepointsCount = node.HasTimeout ? 1 : 0;
            if ((StepDefinition.SourcePoints.Count + 1) > NodeSourcePointsLimits.Max[node.Type] + timeoutSourcepointsCount)
            {
                string message = (StepType == WorkflowStepTypeEnum.Userchoice) ? "graphservice.choicetoomanysourcepoints" : "graphservice.toomanysourcepoints";
                return ResHelper.GetString(message);
            }

            int pos = StepDefinition.SourcePoints.Count;
            SourcePointTypeEnum lastType = StepDefinition.SourcePoints.Last().Type;
            if ((lastType == SourcePointTypeEnum.SwitchDefault) || (lastType == SourcePointTypeEnum.Timeout))
            {
                // Default and timeout source points are always at the end
                pos--;
            }

            StepDefinition.SourcePoints.Insert((pos < 0) ? 0 : pos, sourcePoint);

            // Save changes to database
            SetObject();

            return null;
        }


        /// <summary>
        /// Removes specified source point.
        /// </summary>
        /// <param name="sourcePointGuid">Source point GUID</param>
        /// <returns>Potential error message</returns>
        /// <remarks>Throws exception if database operation fails.</remarks>
        public string RemoveSourcePoint(Guid sourcePointGuid)
        {
            lock (mutex)
            {
                SourcePoint sourcePoint = StepDefinition.SourcePoints.FirstOrDefault(i => i.Guid == sourcePointGuid);
                if (sourcePoint == null)
                {
                    // There is nothing to remove
                    return null;
                }

                if ((sourcePoint.Type != SourcePointTypeEnum.SwitchCase) && (sourcePoint.Type != SourcePointTypeEnum.Timeout))
                {
                    return ResHelper.GetString("workflowstep.nondeletableswitchcasetype");
                }

                Node node = WorkflowNode.GetInstance(this);
                int timeoutSourcePointsCount = node.HasTimeoutSourcePoint ? 1 : 0;
                if ((StepDefinition.SourcePoints.Count - 1) < NodeSourcePointsLimits.Min[node.Type] + timeoutSourcePointsCount)
                {
                    string message = (StepType == WorkflowStepTypeEnum.Userchoice) ? "graphservice.choicetoofewsourcepoints" : "graphservice.toofewsourcepoints";
                    return ResHelper.GetString(message);
                }

                using (var tr = new CMSTransactionScope())
                {
                    //Remove bindings to role
                    WorkflowStepRoleInfoProvider.DeleteWorkflowStepRoleInfos(new WhereCondition()
                        .WhereEquals("StepID", StepID)
                        .WhereEquals("StepSourcePointGUID", sourcePointGuid));

                    //Remove bindings to user
                    WorkflowStepUserInfoProvider.DeleteWorkflowStepUserInfos(new WhereCondition()
                        .WhereEquals("StepID", StepID)
                        .WhereEquals("StepSourcePointGUID", sourcePointGuid));

                    //Remove bindings to transition
                    WorkflowTransitionInfoProvider.DeleteWorkflowTransitionInfos(new WhereCondition()
                        .WhereEquals("TransitionStartStepID", StepID)
                        .WhereEquals("TransitionSourcePointGUID", sourcePointGuid));

                    // Remove specified case
                    StepDefinition.SourcePoints.Remove(sourcePoint);
                    SetObject();

                    tr.Commit();
                }
                return null;
            }
        }


        /// <summary>
        /// Moves source point up.
        /// </summary>
        /// <param name="sourcePointGuid">Source point GUID</param>
        public void MoveSourcePointUp(Guid sourcePointGuid)
        {
            lock (mutex)
            {
                if (StepDefinition == null)
                {
                    // Incompatible step type
                    return;
                }
                List<SourcePoint> points = StepDefinition.SourcePoints;

                int index = points.FindIndex(i => i.Guid == sourcePointGuid);
                if (index < 0)
                {
                    // No such source point
                    return;
                }

                SourcePoint point = points[index];
                if (point == null || point.Type == SourcePointTypeEnum.SwitchDefault)
                {
                    // Default case must be at the bottom
                    return;
                }

                if (index == 0)
                {
                    // Cannot be higher
                    return;
                }

                // Swap source points
                SourcePoint neighbour = points[index - 1];
                points[index] = neighbour;
                points[index - 1] = point;

                // Save changes
                SetObject();
            }
        }


        /// <summary>
        /// Moves source point down.
        /// </summary>
        /// <param name="sourcePointGuid">Source point GUID</param>
        public void MoveSourcePointDown(Guid sourcePointGuid)
        {
            lock (mutex)
            {
                if (StepDefinition == null)
                {
                    // Incompatible step type
                    return;
                }
                List<SourcePoint> points = StepDefinition.SourcePoints;

                int currentIndex = points.FindIndex(i => i.Guid == sourcePointGuid);
                int nextIndex = currentIndex + 1;
                if ((nextIndex >= points.Count) || (currentIndex < 0))
                {
                    // Index out of bounds - can not be moved
                    return;
                }
                SourcePointTypeEnum nextType = points[nextIndex].Type;
                if ((nextType == SourcePointTypeEnum.SwitchDefault) || (nextType == SourcePointTypeEnum.Timeout))
                {
                    // Cannot be lower
                    return;
                }

                // Swap source points
                SourcePoint point = points[currentIndex];
                SourcePoint neighbour = points[nextIndex];
                points[currentIndex] = neighbour;
                points[nextIndex] = point;

                // Save changes
                SetObject();
            }
        }

        #endregion
    }
}