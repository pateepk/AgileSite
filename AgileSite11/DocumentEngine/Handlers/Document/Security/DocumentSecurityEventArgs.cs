using CMS.DataEngine;

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Document security event arguments
    /// </summary>
    public class DocumentSecurityEventArgs : DocumentSecurityEventArgs<TreeNode>
    {
    }


    /// <summary>
    /// Document security event arguments
    /// </summary>
    public class DocumentSecurityEventArgs<TDocument> : ObjectSecurityEventArgs<TDocument>
    {
        /// <summary>
        /// Document to check
        /// </summary>
        public TDocument Node
        {
            get
            {
                return Object;
            }
            set
            {
                Object = value;
            }
        }
    }
}