using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CMS.Base;
using CMS.DataEngine;

using Microsoft.Azure.Search.Models;

namespace CMS.Search.Azure
{
    /// <summary>
    /// Arguments of event represented by <see cref="CreateDocumentHandler"/>.
    /// </summary>
    public class CreateDocumentEventArgs : CMSEventArgs
    {
        /// <summary>
        /// Search document for which Azure Search <see cref="Document"/> is being created.
        /// </summary>
        public SearchDocument SearchDocument
        {
            get;
            set;
        }


        /// <summary>
        /// Azure Search document which is being created from <see cref="SearchDocument"/>.
        /// </summary>
        public Document Document
        {
            get;
            set;
        }


        /// <summary>
        /// Searchable for which the document is being created.
        /// </summary>
        public ISearchable Searchable
        {
            get;
            set;
        }


        /// <summary>
        /// Index for which the document is being created.
        /// </summary>
        public ISearchIndexInfo SearchIndex
        {
            get;
            set;
        }
    }
}
