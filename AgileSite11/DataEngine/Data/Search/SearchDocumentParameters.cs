using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CMS.DataEngine
{
    /// <summary>
    /// Initialization parameters used to create new SearchDocument.
    /// </summary>
    public class SearchDocumentParameters
    {
        #region "Public properties"

        /// <summary>
        /// Index the documents belongs to. Null when document is not result of index search. 
        /// </summary>
        public ISearchIndexInfo Index
        {
            get;
            set;
        }


        /// <summary>
        /// Type of document
        /// </summary>
        public string Type
        {
            get;
            set;
        }


        /// <summary>
        /// ID value
        /// </summary>
        public string Id
        {
            get;
            set;
        }


        /// <summary>
        /// Document created
        /// </summary>
        public DateTime Created
        {
            get;
            set;
        }


        /// <summary>
        /// Site name
        /// </summary>
        public string SiteName
        {
            get;
            set;
        }


        /// <summary>
        /// Document culture
        /// </summary>
        public string Culture
        {
            get;
            set;
        }

        #endregion

    }
}
