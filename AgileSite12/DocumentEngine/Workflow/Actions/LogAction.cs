using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using CMS.EventLog;
using CMS.Helpers;
using CMS.Base;
using CMS.WorkflowEngine;

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Class for log action - for debug purposes
    /// </summary>
    public class LogAction : DocumentWorkflowAction
    {
        private List<string> mManagers = null;

        /// <summary>
        /// Managers who can approve
        /// </summary>
        public List<string> Managers
        {
            get
            {
                if (mManagers == null)
                {
                    mManagers = GetResolvedParameter<string>("Managers", "").Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                }

                return mManagers;
            }
        }


        /// <summary>
        /// Executes action
        /// </summary>
        public override void Execute()
        {
            bool update = false;
            StringBuilder message = new StringBuilder();

            // Log event
            if (ValidationHelper.GetBoolean(Parameters["LogEventToCurrent"], false))
            {
                // Set comment
                Comment = "Comment for next action (" + Guid.NewGuid() + ")";

                message.AppendFormat("Time: {0}", DateTime.Now);
                message.AppendFormat("<br/>Thread: {0}", CMSThread.GetCurrentThreadId());
                message.AppendFormat("<br/>Current step: {0}", ActionStep.StepDisplayName);
                message.AppendFormat("<br/>Initial step: {0}", InitialStep.StepDisplayName);
                message.AppendFormat("<br/>Document: {0}", InfoObject.Generalized.ObjectDisplayName);
                message.AppendFormat("<br/>Workflow: {0}", Workflow.WorkflowDisplayName);
                message.AppendFormat("<br/>Comment: {0}", Comment);
                message.AppendFormat("<br/>___________________________________________");
                message.AppendFormat("<br/>Parameters:<br/>");

                // Get list of parameters
                foreach (string p in Parameters.ColumnNames)
                {
                    if (!(p.StartsWithCSafe("##") && p.EndsWithCSafe("##")))
                    {
                        message.AppendFormat("<br/>{0}: {1}", p, Parameters[p]);
                    }
                }

                message.AppendFormat("<br/>___________________________________________<br/>");
                message.AppendFormat("<br/>Action definition: {0}", ActionDefinition.ActionDisplayName);
                message.AppendFormat("<br/>Assembly info: {0} ({1})", ActionDefinition.ActionAssemblyName, ActionDefinition.ActionClass);
                message.AppendFormat("<br/>___________________________________________<br/>");
                message.AppendFormat("<br/>Macros: {0}", GetResolvedParameter<string>("Macros", ""));

                // Log event
                LogContext.LogEventToCurrent(EventType.INFORMATION, "Workflow Log Action", "WORKFLOWLOGACTION", message.ToString(),
                                    RequestContext.RawURL, User.UserID, User.UserName,
                                    0, null, RequestContext.UserHostAddress, InfoObject.Generalized.ObjectSiteID, SystemContext.MachineName, RequestContext.URLReferrer, RequestContext.UserAgent, DateTime.Now);
            }

            // Throw exception
            if (ValidationHelper.GetBoolean(Parameters["ThrowException"], false))
            {
                message.Append("<br /> EXCEPTION THROWN MANUALLY");
                throw new Exception(message.ToString());
            }

            // Sleep
            int sleep = ValidationHelper.GetInteger(Parameters["ThreadSleep"], 0);
            if (sleep > 0)
            {
                Thread.Sleep(sleep);
            }

            // Update counter
            if (ValidationHelper.GetBoolean(Parameters["UseCounter"], false))
            {
                long counter = ValidationHelper.GetInteger(Node.NodeCustomData["ActionCounter"], 0);
                using (CMSActionContext ctx = new CMSActionContext())
                {
                    ctx.DisableAll();

                    Node.DocumentCustomData["ActionCounter"] = ++counter;
                    update = true;
                }

                Arguments.Comment += string.Format(" | This step was passed {0} times.", counter);
            }

            if (Managers.Count > 0)
            {
                // Get list    
                List<string> approved = ValidationHelper.GetString(Node.DocumentCustomData["Managers"], "").Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries).ToList();

                // Add current manager
                string userName = User.UserName;
                if (!approved.Contains(userName))
                {
                    approved.Add(userName);
                }

                // Store manager
                Node.DocumentCustomData["Managers"] = approved.Join(";");
                update = true;

                // There are still managers to approve
                var missingManagers = Managers.Except(approved);
                if (missingManagers.Count() != 0)
                {
                    Arguments.StopProcessing = true;
                    using (new WorkflowActionContext { CheckStepPermissions = false })
                    {
                        Arguments.Comment += string.Format(" | Page needs approval also from these manager(s): {0}", missingManagers.Join(", "));
                        WorkflowManager.MoveToSpecificStep(Node, Arguments.OriginalStep, Arguments.Comment, WorkflowTransitionTypeEnum.Automatic);
                    }
                }
            }

            // Update document
            if (update)
            {
                using (CMSActionContext ctx = new CMSActionContext())
                {
                    ctx.DisableAll();
                    Node.Update();
                }
            }
        }
    }
}
