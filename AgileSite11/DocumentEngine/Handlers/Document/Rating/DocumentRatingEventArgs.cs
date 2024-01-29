using CMS.Base;

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Document rating event arguments
    /// </summary>
    public class DocumentRatingEventArgs : CMSEventArgs
    {
        /// <summary>
        /// Document node
        /// </summary>
        public TreeNode Node
        {
            get;
            internal set;
        }
    }
}