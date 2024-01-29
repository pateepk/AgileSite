using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CMS.DataEngine;

namespace CMS.Search
{
    /// <summary>
    /// Exception thrown during smart search operations on indexes.  
    /// </summary>
    public class SearchIndexException : SearchException
    {

        #region "Properties"

        /// <summary>
        /// Search index info
        /// </summary>
        public ISearchIndexInfo SearchIndexInfo
        {
            get;
            set;
        }

        /// <summary>
        /// Exception message
        /// </summary>
        public override string Message
        {
            get
            {
                return String.Format("Error while processing index {0}: {1}", SearchIndexInfo.IndexCodeName, base.Message);
            }
        }

        #endregion 


        #region "Constructors"

        /// <summary>
        /// Contructor
        /// </summary>
        /// <param name="indexInfo">Search index info</param>
        public SearchIndexException(ISearchIndexInfo indexInfo)
            : this(indexInfo, null)
        {         
        }


        /// <summary>
        /// Contructor
        /// </summary>
        /// <param name="indexInfo">Search index info</param>
        /// <param name="message">Exception message</param>
        public SearchIndexException(ISearchIndexInfo indexInfo, string message)
            : this(indexInfo, message, null)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="indexInfo">Search index info</param>
        /// <param name="message">Exception message</param>
        /// <param name="innerException">Exception that caused the current exception</param>
        public SearchIndexException(ISearchIndexInfo indexInfo, string message, Exception innerException)
            : base(message, innerException)
        {
            SearchIndexInfo = indexInfo;
        }

        #endregion
    }
}
