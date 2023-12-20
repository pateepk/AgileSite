using System;
using System.Linq;
using System.Text;

namespace CMS.Search
{
    /// <summary>
    /// Interface for the index searcher
    /// </summary>
    public interface IIndexSearcher
    {
        /// <summary>
        /// Returns true if the index is optimized
        /// </summary>
        bool IsOptimized();


        /// <summary>
        /// Searches the given query
        /// </summary>
        /// <param name="query">Query to search</param>
        /// <param name="a">Search analyzer</param>
        /// <param name="filter">Search filter</param>
        ISearchHits Search(string query, ISearchAnalyzer a, ISearchFilter filter = null);


        /// <summary>
        /// Deletes the items with matching field name and value
        /// </summary>
        /// <param name="name">Field name</param>
        /// <param name="value">Value</param>
        void Delete(string name, string value);


        /// <summary>
        /// Commits the changes to the searcher
        /// </summary>
        void Commit();


        /// <summary>
        /// Closes the searcher
        /// </summary>
        void Close();


        /// <summary>
        /// Returns true if the searcher is valid
        /// </summary>
        bool IsValid();


        /// <summary>
        /// Returns the number of documents available in the searcher
        /// </summary>
        int NumberOfDocuments();
    }
}
