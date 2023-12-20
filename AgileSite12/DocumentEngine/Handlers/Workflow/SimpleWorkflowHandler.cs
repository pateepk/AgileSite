using CMS.Base;

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Simple Workflow handler
    /// </summary>
    public class SimpleWorkflowHandler : SimpleHandler<SimpleWorkflowHandler, WorkflowEventArgs>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public SimpleWorkflowHandler()
        {
        }


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="parentHandler">Parent handler</param>
        public SimpleWorkflowHandler(SimpleWorkflowHandler parentHandler)
        {
            Parent = parentHandler;
        }


        /// <summary>
        /// Initiates the event handling
        /// </summary>
        /// <param name="document">Document</param>
        public WorkflowEventArgs StartEvent(TreeNode document)
        {
            var e = new WorkflowEventArgs
                {
                    Document = document
                };

            return StartEvent(e);
        }
    }
}