using System;

using CMS.Core;

namespace CMS.Search
{
    /// <summary>
    /// Provides the connection between search engine and specific search index implementation
    /// </summary>
    public class SearchManager : StaticWrapper<ISearchManager>
    {
        private static int mTaskProcessingBatchSize = 10;


        /// <summary>
        /// Maximum amount of search tasks that system retrieves from database in single query during search tasks processing.
        /// </summary>
        /// <remarks>Default value is 10.</remarks>
        public static int TaskProcessingBatchSize
        {
            get
            {
                return mTaskProcessingBatchSize;
            }
            set
            {
                mTaskProcessingBatchSize = value;
            }
        }


        #region "Methods"

        /// <summary>
        /// Forcibly unlock current index.
        /// </summary>
        /// <param name="path">Index path to unlock</param>
        public static void Unlock(string path)
        {
            Implementation.Unlock(path);
        }


        /// <summary>
        /// Returns SQl Fulltext query.
        /// </summary>
        /// <param name="searchFor">Search query</param>
        public static SearchQueryClauses GetQueryClauses(string searchFor)
        {
            return Implementation.GetQueryClauses(searchFor);
        }


        /// <summary>
        /// Creates the index searcher
        /// </summary>
        /// <param name="path">Index path</param>
        public static IIndexSearcher CreateIndexSearcher(string path)
        {
            return Implementation.CreateIndexSearcher(path);
        }


        /// <summary>
        /// Creates the index writer for the given path and Analyzer
        /// </summary>
        /// <param name="path">Index path</param>
        /// <param name="analyzer">Analyzer</param>
        /// <param name="create">If true, the writer is created</param>
        public static IIndexWriter CreateIndexWriter(string path, ISearchAnalyzer analyzer, bool create)
        {
            return Implementation.CreateIndexWriter(path, analyzer, create);
        }


        /// <summary>
        /// Returns current object analyzer.
        /// </summary>
        /// <param name="sii">Search index info</param>
        /// <param name="isSearch">Indicates whether analyzer should be used for search or indexing</param>
        public static ISearchAnalyzer CreateAnalyzer(SearchIndexInfo sii, bool isSearch)
        {
            return Implementation.CreateAnalyzer(sii, isSearch);
        }


        /// <summary>
        /// Returns analyzer that can be used on searching over multiple indexes
        /// </summary>
        /// <param name="indexes">Search index infos</param>
        public static ISearchAnalyzer CreateAnalyzer(params SearchIndexInfo[] indexes)
        {
            return Implementation.CreateAnalyzer(indexes);
        }


        /// <summary>
        /// Creates the defined search filter
        /// </summary>
        /// <param name="fieldName">Name of the field</param>
        /// <param name="match">Match value</param>
        /// <param name="condition">Filter condition</param>
        public static ISearchFilter CreateFilter(string fieldName, string match, Func<string, string, bool> condition)
        {
            return Implementation.CreateFilter(fieldName, match, condition);
        }


        /// <summary>
        /// Adds the index search results to the search result
        /// </summary>
        /// <param name="parameters">Search parameters</param>
        /// <param name="results">Search results</param>
        /// <exception cref="SearchException">Thrown when an error during search occurs.</exception>
        public static void AddResults(SearchParameters parameters, SearchResults results)
        {
            Implementation.AddResults(parameters, results);
        }

        #endregion
    }
}