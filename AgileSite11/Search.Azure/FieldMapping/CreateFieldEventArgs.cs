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
    /// Arguments of event represented by <see cref="CreateFieldHandler"/>.
    /// </summary>
    public class CreateFieldEventArgs : CMSEventArgs
    {
        /// <summary>
        /// Azure Search field which is being created from <see cref="SearchField"/>.
        /// </summary>
        public Field Field
        {
            get;
            set;
        }


        /// <summary>
        /// Search field for which Azure Search <see cref="Field"/> is being created.
        /// </summary>
        public ISearchField SearchField
        {
            get;
            set;
        }


        /// <summary>
        /// Searchable for which the field is being created.
        /// </summary>
        public ISearchable Searchable
        {
            get;
            set;
        }


        /// <summary>
        /// Index for which the field is being created.
        /// </summary>
        public ISearchIndexInfo SearchIndex
        {
            get;
            set;
        }
    }
}
