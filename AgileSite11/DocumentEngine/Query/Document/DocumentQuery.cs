using System.Linq;

using CMS.DataEngine;

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Queries particular database data or defines parameters for data selection
    /// </summary>
    public class DocumentQuery<TDocument> : DocumentQueryBase<DocumentQuery<TDocument>, TDocument>
        where TDocument : TreeNode, new()
    {
        #region "Constructors"

        /// <summary>
        /// Constructor
        /// </summary>
        public DocumentQuery()
            : base(null)
        {
            InitFromType();
        }

        #endregion
        

        #region "Operators"

        /// <summary>
        /// Implicit operator for conversion from typed DocumentQuery class to typed InfoDataSet
        /// </summary>
        /// <param name="query">Query object</param>
        public static explicit operator InfoDataSet<TDocument>(DocumentQuery<TDocument> query)
        {
            if (query == null)
            {
                return null;
            }

            return query.TypedResult;
        }


        /// <summary>
        /// Implicit operator for conversion from typed DocumentQuery class to typed InfoDataSet
        /// </summary>
        /// <param name="query">Query object</param>
        public static implicit operator TDocument(DocumentQuery<TDocument> query)
        {
            if (query == null)
            {
                return null;
            }

            return query.FirstOrDefault();
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Initializes the query from the given type
        /// </summary>
        internal void InitFromType()
        {
            Document = DocumentFactory<TDocument>.New();
        }

        #endregion
    }


    /// <summary>
    /// Predefined query returning given object type.
    /// </summary>
    public class DocumentQuery : DocumentQueryBase<DocumentQuery, TreeNode>
    {
        #region "Constructors"

        /// <summary>
        /// Constructor
        /// </summary>
        public DocumentQuery()
            : base(null)
        {
        }
        

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="className">Class name</param>
        public DocumentQuery(string className)
            : base(className)
        {
        }

        #endregion
    }
}
