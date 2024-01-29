using System;

using CMS.DataEngine;
using System.Collections.Generic;
using System.Linq;


namespace CMS.Search.Azure
{
    internal static class SearchTaskEngineUtils
    {
        private const string FIELD_DOCUMENTCATEGORYIDS = "DocumentCategoryIDs";
        private const string FIELD_NODEALIASPATHPREFIXES = "NodeAliasPathPrefixes";


        /// <summary>
        /// Gets all document IDs from the search index by given <paramref name="siteCodeName"/>.
        /// </summary>
        /// <param name="indexName">Index name in Azure.</param>
        /// <param name="siteCodeName">Site code name.</param>
        /// <param name="searchService">Search service.</param>
        /// <returns>List of Azure document IDs.</returns>
        public static List<string> GetAllDocumentIdsBySite(string indexName, string siteCodeName, SearchServiceManager searchService)
        {
            var siteColumn = NamingHelper.GetValidFieldName(SearchFieldsConstants.SITE);
            var siteCodeNameEscaped = SearchValueHelper.EscapeSearchFilterValue(siteCodeName).ToLowerInvariant();

            var filter = $"{siteColumn} eq '{siteCodeNameEscaped}'";

            return GetAllDocumentIdsByFilter(indexName, filter, searchService);
        }


        /// <summary>
        /// Gets all document IDs from the search index by given <paramref name="categoryId"/>.
        /// </summary>
        /// <param name="indexName">Index name in Azure.</param>
        /// <param name="categoryId">Category ID.</param>
        /// <param name="searchService">Search service.</param>
        /// <returns>List of Azure document IDs.</returns>
        public static List<string> GetAllDocumentIdsByCategoryId(string indexName, string categoryId, SearchServiceManager searchService)
        {
            var documentCategoryIdsColumn = NamingHelper.GetValidFieldName(FIELD_DOCUMENTCATEGORYIDS);
            var categoryIdEscaped = SearchValueHelper.EscapeSearchFilterValue(categoryId);

            var filter = $"{documentCategoryIdsColumn}/any(c: c eq '{categoryIdEscaped}')";

            return GetAllDocumentIdsByFilter(indexName, filter, searchService);
        }


        /// <summary>
        /// Gets all document IDs from the search index for the <paramref name="nodeAliasPath"/> page and its subtree on the specified site.
        /// </summary>
        /// <param name="indexName">Index name.</param>
        /// <param name="nodeAliasPath">Node alias path.</param>
        /// <param name="siteCodeName">Site code name.</param>
        /// <param name="searchService">Search service.</param>
        /// <returns>List of Azure document IDs.</returns>
        public static List<string> GetAllDocumentIdsInSubTree(string indexName, string siteCodeName, string nodeAliasPath, SearchServiceManager searchService)
        {
            var siteColumn = NamingHelper.GetValidFieldName(SearchFieldsConstants.SITE);
            var nodeAliasPathPrefixesColumn = NamingHelper.GetValidFieldName(FIELD_NODEALIASPATHPREFIXES);

            var siteCodeNameEscaped = SearchValueHelper.EscapeSearchFilterValue(siteCodeName).ToLowerInvariant();
            var nodeAliasPathEscaped = SearchValueHelper.EscapeSearchFilterValue(nodeAliasPath);

            var filter = $"{siteColumn} eq '{siteCodeNameEscaped}' and {nodeAliasPathPrefixesColumn}/any(p: p eq '{nodeAliasPathEscaped}')";

            return GetAllDocumentIdsByFilter(indexName, filter, searchService);
        }


        /// <summary>
        /// Gets all document IDs from the search index matching the <paramref name="filter"/>.
        /// </summary>
        /// <param name="indexName">Index name.</param>
        /// <param name="filter">Filter expression.</param>
        /// <param name="searchService">Search service.</param>
        /// <returns>List of Azure document IDs.</returns>
        private static List<string> GetAllDocumentIdsByFilter(string indexName, string filter, SearchServiceManager searchService)
        {
            var idColumn = NamingHelper.GetValidFieldName(SearchFieldsConstants.ID);
            var searchParameters = new Microsoft.Azure.Search.Models.SearchParameters
            {
                Select = new[] { idColumn },
                Top = SearchEngineConfiguration.Instance.DocumentSearchPageSize,
                Filter = filter,
                OrderBy = new[] { idColumn }
            };

            return searchService.GetAllDocuments(indexName, null, searchParameters)
                .Select(x => x[idColumn] as string)
                .ToList();
        }


        /// <summary>
        /// Returns true, if task was created at or after index rebuild time.
        /// </summary>
        /// <param name="taskCreated">Task creation time.</param>
        /// <param name="index">Index info.</param>
        public static bool TaskWasCreatedAfterIndexRebuild(DateTime taskCreated, SearchIndexInfo index)
        {
            return taskCreated >= index.ActualRebuildTime;
        }
    }
}
