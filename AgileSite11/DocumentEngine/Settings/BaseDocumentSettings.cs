using System;

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Container for shared settings used by operations with documents.
    /// </summary>
    public abstract class BaseDocumentSettings
    {
        #region "Properties"

        /// <summary>
        /// Document node to process.
        /// </summary>
        public TreeNode Node
        {
            get;
            set;
        }


        /// <summary>
        /// TreeProvider to use.
        /// </summary>
        public TreeProvider Tree
        {
            get;
            set;
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Parametric constructor.
        /// </summary>
        /// <param name="node">Document node to process.</param>
        /// <param name="tree"><see cref="TreeProvider"/> to use.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="node"/> is null.</exception>
        protected BaseDocumentSettings(TreeNode node, TreeProvider tree)
        {
            if (node == null)
            {
                throw new ArgumentNullException(nameof(node));
            }

            Node = node;
            Tree = tree ?? node.TreeProvider;
        }

        #endregion
    }
}
