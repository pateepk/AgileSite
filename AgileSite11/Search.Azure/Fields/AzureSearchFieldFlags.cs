using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMS.Search.Azure
{
    /// <summary>
    /// Contains Azure Search specific flag names.
    /// </summary>
    public static class AzureSearchFieldFlags
    {
        /// <summary>
        /// Name of flag indicating whether field is considered a content field.
        /// </summary>
        public const string CONTENT = "AzureContent";


        /// <summary>
        /// Name of flag indicating whether field is retrievable.
        /// </summary>
        public const string RETRIEVABLE = "AzureRetrievable";


        /// <summary>
        /// Name of flag indicating whether field is searchable.
        /// </summary>
        public const string SEARCHABLE = "AzureSearchable";


        /// <summary>
        /// Name of flag indicating whether field is facetable.
        /// </summary>
        public const string FACETABLE = "AzureFacetable";


        /// <summary>
        /// Name of flag indicating whether field is filterable.
        /// </summary>
        public const string FILTERABLE = "AzureFilterable";


        /// <summary>
        /// Name of flag indicating whether field is sortable.
        /// </summary>
        public const string SORTABLE = "AzureSortable";
    }
}
