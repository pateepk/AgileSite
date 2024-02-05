using System;

using CMS.DataEngine;

namespace CMS.Search
{
    /// <summary>
    /// Provides an interface to the search index manager
    /// </summary>
    public interface ISearchManager
    {
        /// <summary>
        /// Creates the index writer for the given path and Analyzer
        /// </summary>
        /// <param name="path">Index path</param>
        /// <param name="analyzer">Analyzer</param>
        /// <param name="create">If true, the writer is created</param>
        IIndexWriter CreateIndexWriter(string path, ISearchAnalyzer analyzer, bool create);

        /// <summary>
        /// Creates the index searcher
        /// </summary>
        /// <param name="path">Index path</param>
        IIndexSearcher CreateIndexSearcher(string path);

        /// <summary>
        /// Returns current object analyzer.
        /// </summary>
        /// <param name="sii">Search index info</param>
        /// <param name="isSearch">Indicates whether analyzer should be used for search or indexing</param>
        ISearchAnalyzer CreateAnalyzer(SearchIndexInfo sii, bool isSearch);


        /// <summary>
        /// Returns analyzer that can be used on searching over multiple indexes
        /// </summary>
        /// <param name="indexes">Search index infos</param>
        ISearchAnalyzer CreateAnalyzer(params SearchIndexInfo[] indexes);


        /// <summary>
        /// Creates the defined search filter
        /// </summary>
        /// <param name="fieldName">Name of the field</param>
        /// <param name="match">Match value</param>
        /// <param name="condition">Filter condition</param>
        ISearchFilter CreateFilter(string fieldName, string match, Func<string, string, bool> condition);

        /// <summary>
        /// Adds the attachment results to the search results
        /// </summary>
        /// <param name="parameters">Search parameters</param>
        /// <param name="results">Search results</param>
        void AddResults(SearchParameters parameters, SearchResults results);
        
        /// <summary>
        /// Forcibly unlock current index.
        /// </summary>
        /// <param name="path">Index path to unlock</param>
        void Unlock(string path);

        /// <summary>
        /// Returns SQL Fulltext query.
        /// </summary>
        /// <param name="searchFor">Search query</param>
        SearchQueryClauses GetQueryClauses(string searchFor);
    }
}