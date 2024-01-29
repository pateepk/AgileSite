namespace CMS.WorkflowEngine
{
    /// <summary>
    /// Interface for workflow actions.
    /// </summary>
    public interface IWorkflowAction
    {
        #region "Properties"

        /// <summary>
        /// Action arguments
        /// </summary>
        IWorkflowActionEventArgs Arguments
        {
            get;
        }

        #endregion


        /// <summary>
        /// Executes action.
        /// </summary>
        void Execute();


        /// <summary>
        /// Processes action.
        /// </summary>
        /// <param name="args">Arguments of action</param>
        void Process(IWorkflowActionEventArgs args);
    }
}
