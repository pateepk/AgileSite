using System;

using CMS.Base;

namespace CMS.Search
{
    /// <summary>
    /// Search handler
    /// </summary>
    public class SearchHandler : AdvancedHandler<SearchHandler, SearchEventArgs>
    {
        /// <summary>
        /// Initiates the event handling
        /// </summary>
        /// <param name="parameters">Search parameters</param>
        /// <param name="results">Search results</param>
        public SearchHandler StartEvent(SearchParameters parameters, SearchResults results)
        {
            SearchEventArgs e = new SearchEventArgs()
            {
                Parameters = parameters,
                Results = results
            };

            return StartEvent(e, true);
        }
    }
}