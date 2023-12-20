using System;
using System.Threading;

using CMS.EventLog;
using CMS.Membership;
using CMS.Base;
using CMS.WorkflowEngine;

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Class for asynchronous workflow e-mail sending.
    /// </summary>
    public class WorkflowEmailSender : AbstractWorker
    {
        #region "Properties"

        /// <summary>
        /// Tree node.
        /// </summary>
        public TreeNode Node
        {
            get;
            set;
        }


        /// <summary>
        /// User.
        /// </summary>
        public UserInfo User
        {
            get;
            set;
        }


        /// <summary>
        /// Original step.
        /// </summary>
        public WorkflowStepInfo OriginalStep
        {
            get;
            set;
        }


        /// <summary>
        /// Current step.
        /// </summary>
        public WorkflowStepInfo CurrentStep
        {
            get;
            set;
        }


        /// <summary>
        /// Workflow action.
        /// </summary>
        public WorkflowActionEnum Action
        {
            get;
            set;
        }


        /// <summary>
        /// Action comment.
        /// </summary>
        public string Comment
        {
            get;
            set;
        }


        /// <summary>
        /// Application URL for e-mail macro resolving
        /// </summary>
        public string ApplicationUrl
        {
            get;
            set;
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="node">Tree node</param>
        /// <param name="user">User</param>
        /// <param name="originalStep">Original workflow step</param>
        /// <param name="currentStep">Current workflow step</param>
        /// <param name="action">Workflow action</param>
        /// <param name="comment">Comment</param>
        /// <param name="applicationUrl">Application URL for macro resolver</param>
        public WorkflowEmailSender(TreeNode node, UserInfo user, WorkflowStepInfo originalStep, WorkflowStepInfo currentStep, WorkflowActionEnum action, string comment, string applicationUrl)
        {
            Node = node;
            User = user;
            OriginalStep = originalStep;
            CurrentStep = currentStep;
            Action = action;
            Comment = comment;
            ApplicationUrl = applicationUrl;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Runs the action.
        /// </summary>
        public override void Run()
        {
            try
            {
                using (CMSActionContext context = new CMSActionContext())
                {
                    context.AllowAsyncActions = false;

                    WorkflowManager wm = WorkflowManager.GetInstance(Node.TreeProvider);
                    wm.ApplicationUrl = ApplicationUrl;
                    wm.SendWorkflowEmails(Node, User, OriginalStep, CurrentStep, Action, Comment);
                }
            }
            catch (ThreadAbortException ex)
            {
                // Thread was aborted
                if (!CMSThread.Stopped(ex))
                {
                    EventLogProvider.LogException("WorkflowEmails", "WorkflowEmail", ex);
                }
            }
            catch (Exception ex)
            {
                // Log the exception
                EventLogProvider.LogException("WorkflowEmails", "WorkflowEmail", ex);
            }
        }

        #endregion
    }
}