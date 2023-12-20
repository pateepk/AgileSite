using System;

namespace CMS.Search
{
    internal static class SearchIndexInfoExtensions
    {
        /// <summary>
        /// Determines if the index is Lucene index.
        /// </summary>
        /// <param name="infoObj">Search index info.</param>
        /// <returns>True, if the index is Lucene index. Otherwise false.</returns>
        public static bool IsLuceneIndex(this SearchIndexInfo infoObj)
        {
            return String.IsNullOrEmpty(infoObj.IndexProvider) || infoObj.IndexProvider.Equals(SearchIndexInfo.LUCENE_SEARCH_PROVIDER, StringComparison.OrdinalIgnoreCase);
        }


        /// <summary>
        /// Determines if the index is Azure index.
        /// </summary>
        /// <param name="infoObj">Search index info.</param>
        /// <returns>True, if the index is Azure index. Otherwise false.</returns>
        public static bool IsAzureIndex(this SearchIndexInfo infoObj)
        {
            return infoObj.IndexProvider.Equals(SearchIndexInfo.AZURE_SEARCH_PROVIDER, StringComparison.OrdinalIgnoreCase);
        }
    }
}
