using System;
using System.Data;

namespace CMS.Search
{
    /// <summary>
    /// Interface for search provider.
    /// </summary>
    public interface ISearchProvider
    {
        /// <summary>
        /// Searches content specified by parameters.
        /// </summary>
        /// <param name="siteName">Site code name</param>
        /// <param name="searchNodePath">Starting alias path without %</param>
        /// <param name="cultureCode">Culture code</param>
        /// <param name="searchExpression">Expression to searched for</param>
        /// <param name="searchMode">Search mode - all words, any word, exact phrase</param>
        /// <param name="searchChildNodes">Search child nodes under the given starting path</param>
        /// <param name="classNames">Class names in format cms.article;cms.news</param>
        /// <param name="filterResultsByReadPermission">Indicates if only documents the current user is allowed to read should be displayed</param>
        /// <param name="searchOnlyPublished">Indicates if only published documents should be searched</param>
        /// <param name="whereCondition">Where condition</param>
        /// <param name="orderBy">Order by clause</param>
        /// <param name="combineWithDefaultCulture">Specifies if return the default culture document when specified culture not found</param>
        DataSet Search(string siteName, string searchNodePath, string cultureCode, string searchExpression, SearchModeEnum searchMode, bool searchChildNodes, string classNames, bool filterResultsByReadPermission, bool searchOnlyPublished, string whereCondition, string orderBy, bool combineWithDefaultCulture);
    }
}