using CMS.DataEngine;
using CMS.WorkflowEngine;

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Base document workflow action
    /// </summary>
    public abstract class DocumentWorkflowAction : BaseWorkflowAction<TreeNode, BaseInfo, WorkflowActionEnum>
    {
        #region "Properties"

        /// <summary>
        /// Current document.
        /// </summary>
        protected TreeNode Node
        {
            get
            {
                return (TreeNode)InfoObject;
            }
        }


        /// <summary>
        /// Workflow manager
        /// </summary>
        protected WorkflowManager WorkflowManager
        {
            get
            {
                return Manager as WorkflowManager;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Refreshes node instance
        /// </summary>
        protected void RefreshNode()
        {
            InfoObject = DocumentHelper.GetDocument(Node.DocumentID, Node.TreeProvider);
        }
        
        #endregion
    }
}
