using CMS.Helpers;
using CMS.Membership;

namespace CMS.WorkflowEngine
{
    /// <summary>
    /// Interface for workflow action arguments.
    /// </summary>
    public interface IWorkflowActionEventArgs
    {
        #region "Properties"

        /// <summary>
        /// Action definition.
        /// </summary>
        WorkflowActionInfo ActionDefinition
        {
            get;
        }


        /// <summary>
        /// Parameters of action.
        /// </summary>
        ObjectParameters Parameters
        {
            get;
        }


        /// <summary>
        /// User running action.
        /// </summary>
        UserInfo User
        {
            get;
        }


        /// <summary>
        /// Current step.
        /// </summary>
        WorkflowStepInfo ActionStep
        {
            get;
        }


        /// <summary>
        /// Current step.
        /// </summary>
        WorkflowStepInfo InitialStep
        {
            get;
        }


        /// <summary>
        /// Current step.
        /// </summary>
        WorkflowStepInfo OriginalStep
        {
            get;
        }


        /// <summary>
        /// Current workflow.
        /// </summary>
        WorkflowInfo Workflow
        {
            get;
        }


        /// <summary>
        /// Comment used when action moves to next step.
        /// </summary>
        string Comment
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates if the process should be stopped. Process is not moved to the next step.
        /// </summary>
        bool StopProcessing
        {
            get;
            set;
        }
        
        #endregion
    }
}
