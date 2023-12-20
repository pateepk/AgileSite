using System;

using CMS.Base;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.WorkflowEngine.Definitions;

namespace CMS.WorkflowEngine
{
    /// <summary>
    /// Class for workflow helper methods.
    /// </summary>
    public static class WorkflowHelper
    {
        #region "Constants"

        /// <summary>
        /// Maximum number of allowed step hops (to prevent endless cycles)
        /// </summary>
        private const int MAX_STEP_HOPS = 100;

        /// <summary>
        /// Action status - running action
        /// </summary>
        public const string ACTION_SATUS_RUNNING = "##RUNNING##";

        #endregion


        #region "Variables"

        private static int? mMaxStepsHopsCount = null;

        #endregion


        #region "Properties"

        /// <summary>
        /// Maximum number of allowed step hops (to prevent endless cycles)
        /// </summary>
        public static int MaxStepsHopsCount
        {
            get
            {
                if (mMaxStepsHopsCount == null)
                {
                    mMaxStepsHopsCount = ValidationHelper.GetInteger(SettingsHelper.AppSettings["CMSWorkflowMaxStepHopsCount"], MAX_STEP_HOPS);
                }

                return mMaxStepsHopsCount.Value;
            }
            set
            {
                mMaxStepsHopsCount = value;
            }
        }
        
        #endregion


        #region "Public methods"

        /// <summary>
        /// Gets macro resolver name for given workflow
        /// </summary>
        /// <param name="workflow">Workflow</param>
        public static string GetResolverName(WorkflowInfo workflow)
        {
            if (workflow == null)
            {
                return String.Empty;
            }
            else if (workflow.IsDocumentWorkflow)
            {
                return "WorkflowSimpleDocumentResolver";
            }
            else if (workflow.IsAutomation)
            {
                return "AutomationSimpleResolver";
            }
            else
            {
                return "WorkflowSimpleInfoResolver";
            }
        }


        /// <summary>
        /// Gets workflow e-mails default sender from settings
        /// </summary>
        /// <param name="siteName">Site name</param>
        public static string GetWorkflowEmailsSender(string siteName)
        {
            return SettingsKeyInfoProvider.GetValue(siteName + ".CMSSendWorkflowEmailsFrom");
        }


        /// <summary>
        /// Gets settings indicating if workflow e-mails should be sent
        /// </summary>
        /// <param name="siteName">Site name</param>
        public static bool SendWorkflowEmails(string siteName)
        {
            return SettingsKeyInfoProvider.GetBoolValue(siteName + ".CMSSendWorkflowEmails");
        }


        /// <summary>
        /// Gets scheduled task name for given object and step
        /// </summary>
        /// <param name="stateGuid">State GUID</param>
        public static string GetScheduledTaskName(Guid stateGuid)
        {
            return string.Format("Timeout.Wait.{0}", stateGuid);
        }

        #endregion


        #region "Enumeration methods"

        /// <summary>
        /// Creates source point with respect to workflow step type.
        /// </summary>
        /// <param name="stepType">Workflow step type</param>
        public static SourcePoint CreateSourcePoint(WorkflowStepTypeEnum stepType)
        {
            switch (stepType)
            {
                case WorkflowStepTypeEnum.Multichoice:
                case WorkflowStepTypeEnum.MultichoiceFirstWin:
                    return new CaseSourcePoint();

                case WorkflowStepTypeEnum.Userchoice:
                    return new ChoiceSourcePoint();

                default:
                    return null;
            }
        }


        /// <summary>
        /// Gets workflow type string representation
        /// </summary>
        /// <param name="type">Workflow type</param>
        /// <param name="versioning">Indicates if workflow automatically publishes changes</param>
        public static string GetWorkflowTypeString(WorkflowTypeEnum type, bool versioning)
        {
            if (versioning)
            {
                return ResHelper.GetAPIString("workflow.type.versioning", "Basic versioning");
            }
            else
            {
                switch (type)
                {
                    case WorkflowTypeEnum.Approval:
                        return ResHelper.GetAPIString("workflow.type.approval", "Advanced publishing");

                    case WorkflowTypeEnum.Advanced:
                        return ResHelper.GetAPIString("workflow.type.advanced", "Advanced");

                    case WorkflowTypeEnum.Automation:
                        return ResHelper.GetAPIString("workflow.type.automation", "Automation");

                    case WorkflowTypeEnum.Basic:
                    default:
                        return ResHelper.GetAPIString("workflow.type.basic", "Basic publishing");
                }
            }
        }


        /// <summary>
        /// Gets workflow action string representation
        /// </summary>
        /// <param name="action">Workflow action</param>
        public static string GetWorkflowActionString(WorkflowActionEnum action)
        {
            switch (action)
            {
                case WorkflowActionEnum.Approve:
                    return ResHelper.GetAPIString("workflow.actionapprove", "Approved");

                case WorkflowActionEnum.Reject:
                    return ResHelper.GetAPIString("workflow.actionreject", "Rejected");

                case WorkflowActionEnum.Publish:
                    return ResHelper.GetAPIString("workflow.actionpublish", "Published");

                case WorkflowActionEnum.Archive:
                    return ResHelper.GetAPIString("workflow.actionarchive", "Archived");

                default:
                    return ResHelper.GetAPIString("general.unknown", "Unknown");
            }
        }


        /// <summary>
        /// Gets workflow trigger type string representation.
        /// </summary>
        /// <param name="triggerType">Workflow trigger type</param>
        public static string GetWorkflowTriggerTypeString(WorkflowTriggerTypeEnum triggerType)
        {
            switch (triggerType)
            {
                case WorkflowTriggerTypeEnum.Creation:
                    return ResHelper.GetAPIString("ma.trigger.creation", "Creation");

                case WorkflowTriggerTypeEnum.Change:
                    return ResHelper.GetAPIString("ma.trigger.change", "Change");

                default:
                    throw new Exception("[WorkflowHelper.GetWorkflowTriggerTypeString]: Unknown trigger type.");
            }
        }


        /// <summary>
        /// Gets default e-mail template name.
        /// </summary>
        /// <param name="emailType">Type of workflow e-mail</param>
        public static string GetDefaultEmailTemplateName(WorkflowEmailTypeEnum emailType)
        {
            switch (emailType)
            {
                case WorkflowEmailTypeEnum.Approved:
                    return "Workflow.Approved";

                case WorkflowEmailTypeEnum.ReadyForApproval:
                    return "Workflow.ReadyForApproval";

                case WorkflowEmailTypeEnum.Rejected:
                    return "Workflow.Rejected";

                case WorkflowEmailTypeEnum.Published:
                    return "Workflow.Published";

                case WorkflowEmailTypeEnum.Archived:
                    return "Workflow.Archived";

                case WorkflowEmailTypeEnum.Notification:
                    return "Workflow.Notification";

                default:
                    throw new Exception("[WorkflowHelper.GetDefaultEmailTemplateName]: Unknown e-mail type.");
            }
        }


        /// <summary>
        /// Gets process status string representation.
        /// </summary>
        /// <param name="processStatus">Process status</param>
        public static string GetProcessStatusString(ProcessStatusEnum processStatus)
        {
            switch (processStatus)
            {
                case ProcessStatusEnum.Processing:
                    return ResHelper.GetAPIString("ma.automationstate.status.processing", "Processing");

                case ProcessStatusEnum.Pending:
                    return ResHelper.GetAPIString("ma.automationstate.status.pending", "Pending");

                case ProcessStatusEnum.Finished:
                    return ResHelper.GetAPIString("ma.automationstate.status.finished", "Finished");

                default:
                    return ResHelper.GetAPIString("general.unknown", "Unknown");
            }
        }

        #endregion
    }
}
