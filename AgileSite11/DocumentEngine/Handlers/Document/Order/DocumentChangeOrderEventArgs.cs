using CMS.Base;

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Document change order event arguments
    /// </summary>
    public class DocumentChangeOrderEventArgs : DocumentChangeOrderEventArgs<TreeNode>
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public DocumentChangeOrderEventArgs()
        {
        }


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="node">Document</param>
        /// <param name="newOrder">Specific new document order index to be set</param>
        /// <param name="relativeOrder">Indicates if the NewOrder index is relative to current document position.</param>
        public DocumentChangeOrderEventArgs(TreeNode node, int newOrder, bool relativeOrder = false)
            : base(node, newOrder, relativeOrder)
        {
        }


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="node">Document</param>
        /// <param name="orderType">Defines the order type (First, Last, Alphabetical) if the NewOrder is not specified</param>
        public DocumentChangeOrderEventArgs(TreeNode node, DocumentOrderEnum orderType)
            : base(node, orderType)
        {
        }
    }


    /// <summary>
    /// Document change order event arguments
    /// </summary>
    public class DocumentChangeOrderEventArgs<TDocument> : CMSEventArgs<TDocument>
    {
        /// <summary>
        /// Document
        /// </summary>
        public TDocument Node
        {
            get;
            set;
        }


        /// <summary>
        /// Specific new document order index to be set. If not defined, the OrderType is used instead.
        /// </summary>
        public int? NewOrder
        {
            get;
            protected set;
        }


        /// <summary>
        /// Indicates if the NewOrder index is relative to current document position.
        /// </summary>
        public bool RelativeOrder
        {
            get;
            protected set;
        }


        /// <summary>
        /// Defines the order type (First, Last, Alphabetical) if the NewOrder is not specified.
        /// </summary>
        public DocumentOrderEnum OrderType
        {
            get;
            protected set;
        }


        /// <summary>
        /// Constructor.
        /// </summary>
        public DocumentChangeOrderEventArgs()
        {
        }


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="node">Document</param>
        /// <param name="newOrder">Specific new document order index to be set</param>
        /// <param name="relativeOrder">Indicates if the NewOrder index is relative to current document position.</param>
        public DocumentChangeOrderEventArgs(TDocument node, int newOrder, bool relativeOrder = false)
        {
            Node = node;
            NewOrder = newOrder;
            RelativeOrder = relativeOrder;
        }


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="node">Document</param>
        /// <param name="orderType">Defines the order type (First, Last, Alphabetical) if the NewOrder is not specified</param>
        public DocumentChangeOrderEventArgs(TDocument node, DocumentOrderEnum orderType)
        {
            Node = node;
            OrderType = orderType;
        }
    }
}