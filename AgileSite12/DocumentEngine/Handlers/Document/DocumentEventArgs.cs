using CMS.Base;

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Document event arguments
    /// </summary>
    public class DocumentEventArgs : CMSEventArgs
    {
        /// <summary>
        /// Tree provider
        /// </summary>
        public TreeProvider TreeProvider
        {
            get;
            set;
        }


        /// <summary>
        /// Document node
        /// </summary>
        public TreeNode Node
        {
            get;
            set;
        }


        /// <summary>
        /// Attachment, where is makes sense: SaveAttachment, DeleteAttachment
        /// </summary>
        public DocumentAttachment Attachment
        {
            get;
            set;
        }


        /// <summary>
        /// Target parent node ID, initialized for operations where it makes sense: Copy, Move, Insert
        /// </summary>
        public int TargetParentNodeID
        {
            get
            {
                return TargetParentNode != null ? TargetParentNode.NodeID : 0;
            }
        }


        /// <summary>
        /// Target parent node, initialized for operations where it makes sense: Copy, Move, Insert
        /// </summary>
        public TreeNode TargetParentNode
        {
            get;
            set;
        }


        /// <summary>
        /// If true the children are included within the operation where it makes sense: Copy
        /// </summary>
        public bool IncludeChildren
        {
            get;
            set;
        }
    }
}