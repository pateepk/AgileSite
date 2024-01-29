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
    /// Arguments of event represented by <see cref="CreateFieldsHandler"/>.
    /// </summary>
    public class CreateFieldsEventArgs : CMSEventArgs
    {
        /// <summary>
        /// List of Azure Search fields resulting from processing of <see cref="SearchFields"/>.
        /// </summary>
        public List<Field> Fields
        {
            get;
            set;
        }


        /// <summary>
        /// List of search fields being processed to Azure Search <see cref="Fields"/>.
        /// </summary>
        public IEnumerable<ISearchField> SearchFields
        {
            get;
            set;
        }


        /// <summary>
        /// Searchable for which the collection of <see cref="Fields"/> is being created.
        /// </summary>
        public ISearchable Searchable
        {
            get;
            set;
        }


        /// <summary>
        /// Index for which the fields are being created.
        /// </summary>
        public ISearchIndexInfo SearchIndex
        {
            get;
            set;
        }
    }
}
