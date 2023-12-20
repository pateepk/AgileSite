using System.Collections.Generic;

using CMS.Base;
using CMS.DataEngine;

using Microsoft.Azure.Search.Models;

namespace CMS.Search.Azure
{
    /// <summary>
    /// Arguments of event represented by <see cref="AddDocumentValueHandler"/>.
    /// </summary>
    public class AddDocumentValueEventArgs : CMSEventArgs
    {
        /// <summary>
        /// Name of field in <see cref="SearchDocument"/> for which Azure Search <see cref="Document"/> value is being created.
        /// </summary>
        public string Name
        {
            get;
            set;
        }


        /// <summary>
        /// Name of resulting field in Azure Search <see cref="Document"/>.
        /// </summary>
        public string AzureName
        {
            get;
            set;
        }


        /// <summary>
        /// Enumeration of Azure Search fields corresponding to the <see cref="Document"/>.
        /// </summary>
        public IEnumerable<Field> Fields
        {
            get;
            set;
        }


        /// <summary>
        /// Azure Search <see cref="Document"/> value to be assigned.
        /// </summary>
        public object Value
        {
            get;
            set;
        }


        /// <summary>
        /// Search document for which the document value is being created.
        /// </summary>
        public SearchDocument SearchDocument
        {
            get;
            set;
        }


        /// <summary>
        /// Azure Search document for which the document value is being created.
        /// </summary>
        public Document Document
        {
            get;
            set;
        }


        /// <summary>
        /// Searchable for which the document value is being created.
        /// </summary>
        public ISearchable Searchable
        {
            get;
            set;
        }


        /// <summary>
        /// Index for which the document value is being created.
        /// </summary>
        public ISearchIndexInfo SearchIndex
        {
            get;
            set;
        }
    }
}
