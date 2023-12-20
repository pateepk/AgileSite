using System;

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Container for settings used when moving document.
    /// </summary>
    public class MoveDocumentSettings : BaseDocumentSettings
    {
        #region "Properties"

        /// <summary>
        /// Target node
        /// </summary>
        public TreeNode TargetNode
        {
            get;
            internal set;
        }


        /// <summary>
        /// Indicates if the document permissions should be kept
        /// </summary>
        public bool KeepPermissions
        {
            get;
            set;
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Default constructor
        /// </summary>
        public MoveDocumentSettings()
            : base(null, null)
        {
        }


        /// <summary>
        /// Parametric constructor
        /// </summary>
        /// <param name="node">Document node to move</param>
        /// <param name="target">Target node</param>
        /// <param name="tree">Tree provider to use</param>
        public MoveDocumentSettings(TreeNode node, TreeNode target, TreeProvider tree = null)
            : base(node, tree)
        {
            TargetNode = target;
        }

        #endregion
    }
}
