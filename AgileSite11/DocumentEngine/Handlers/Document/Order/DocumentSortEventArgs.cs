using CMS.Base;

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Documents sorting event arguments
    /// </summary>
    public class DocumentSortEventArgs : DocumentSortEventArgs<TreeNode>
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public DocumentSortEventArgs()
        {
            
        }


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="parentNodeId">Node ID of a document which sub-section should be sorted</param>
        /// <param name="ascending">Indicates if the sort should be ascending</param>
        /// <param name="sortType">Defines the sort type (by alphabet or date)</param>
        public DocumentSortEventArgs(int parentNodeId, bool ascending, DocumentSortEnum sortType)
            : base(parentNodeId, ascending, sortType)
        {
        }
    }


    /// <summary>
    /// Document change order event arguments
    /// </summary>
    public class DocumentSortEventArgs<TDocument> : CMSEventArgs<TDocument>
    {
        /// <summary>
        /// Node ID of a document which sub-section should be sorted.
        /// </summary>
        public int ParentNodeId
        {
            get;
            protected set;
        }


        /// <summary>
        /// Indicates if the sort should be ascending.
        /// </summary>
        public bool Ascending
        {
            get;
            protected set;
        }


        /// <summary>
        /// Defines the sort type (by alphabet or date).
        /// </summary>
        public DocumentSortEnum SortType
        {
            get;
            protected set;
        }


        /// <summary>
        /// Constructor.
        /// </summary>
        public DocumentSortEventArgs()
        {
        }


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="parentNodeId">Node ID of a document which sub-section should be sorted</param>
        /// <param name="ascending">Indicates if the sort should be ascending</param>
        /// <param name="sortType">Defines the sort type (by alphabet or date)</param>
        public DocumentSortEventArgs(int parentNodeId, bool ascending, DocumentSortEnum sortType)
        {
            ParentNodeId = parentNodeId;
            Ascending = ascending;
            SortType = sortType;
        }
    }
}