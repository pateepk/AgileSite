using CMS.DocumentEngine;

namespace CMS.Base.Web.UI
{
    /// <summary>
    /// Simple document manager event arguments
    /// </summary>
    public class SimpleDocumentManagerEventArgs : ManagerEventArgs
    {
        #region "Properties"

        /// <summary>
        /// Document node
        /// </summary>
        public TreeNode Node
        {
            get;
            set;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="node">Document node</param>
        /// <param name="mode">Manager mode</param>
        public SimpleDocumentManagerEventArgs(TreeNode node, FormModeEnum mode)
            : base(mode, null)
        {
            Node = node;
        }


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="node">Document node</param>
        /// <param name="mode">Manager mode</param>
        /// <param name="actionName">Action name</param>
        public SimpleDocumentManagerEventArgs(TreeNode node, FormModeEnum mode, string actionName)
            : base(mode, actionName)
        {
            Node = node;
        }

        #endregion
    }
}
