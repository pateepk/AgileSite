using System;
using System.Data;

using CMS.Search;

namespace CMS.CustomSearchProvider
{
    /// <summary>
    /// Custom class providing searching.
    /// </summary>
    public class SearchProvider : ISearchProvider
    {
        /// <summary>
        /// Searches data and returns results.
        /// </summary>
        /// <param name="siteName">Site name</param>
        /// <param name="searchNodePath">Search node path</param>
        /// <param name="cultureCode">Culture code</param>
        /// <param name="searchExpression">Search expression</param>
        /// <param name="searchMode">Search mode</param>
        /// <param name="searchChildNodes">Search child nodes</param>
        /// <param name="classNames">Class names</param>
        /// <param name="filterResultsByReadPermission">Filter results by read permission?</param>
        /// <param name="searchOnlyPublished">Search only published?</param>
        /// <param name="whereCondition">Where condition</param>
        /// <param name="orderBy">Order by value</param>
        /// <param name="combineWithDefaultCulture">Specifies if return the default culture document when specified culture not found</param>
        public DataSet Search(string siteName, string searchNodePath, string cultureCode, string searchExpression, SearchModeEnum searchMode, bool searchChildNodes, string classNames, bool filterResultsByReadPermission, bool searchOnlyPublished, string whereCondition, string orderBy, bool combineWithDefaultCulture)
        {
            // this is a sample code that uses standard search provider and returns standard search results without any modifications
            SearchProviderSQL.SearchProvider standardSearchProvider = new SearchProviderSQL.SearchProvider();
            DataSet ds = standardSearchProvider.Search(siteName, searchNodePath, cultureCode, searchExpression, searchMode, searchChildNodes, classNames, filterResultsByReadPermission, searchOnlyPublished, whereCondition, orderBy, combineWithDefaultCulture);
            return ds;
        }
    }
}