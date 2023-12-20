using CMS.Base;

namespace CMS.DocumentEngine.Internal
{
    /// <summary>
    /// Encapsulates arguments for <see cref="LogDocumentChangeHandler"/>
    /// </summary>
    public sealed class LogDocumentChangeCloneEventArgs : CMSEventArgs
    {
        /// <summary>
        /// Original node.
        /// </summary>
        public TreeNode Original { get; set; }

        /// <summary>
        /// Cloned node.
        /// </summary>
        public TreeNode Clone { get; set; }

        /// <summary>
        /// Creates an instance of the <see cref="LogDocumentChangeCloneEventArgs"/> class.
        /// </summary>
        /// <param name="original">Original node.</param>
        /// <param name="clone">Cloned node.</param>
        public LogDocumentChangeCloneEventArgs(TreeNode original, TreeNode clone)
        {
            Original = original;
            Clone = clone;
        }
    }
}
