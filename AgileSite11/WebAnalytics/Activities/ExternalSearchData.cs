using System;

namespace CMS.WebAnalytics
{
    /// <summary>
    /// Provides information about search engine associated with current request.
    /// </summary>
    public class ExternalSearchData
    {
        /// <summary>
        /// Request search engine name
        /// </summary>
        public string SearchEngineName
        {
            get;
            private set;
        }


        /// <summary>
        /// Search keyword
        /// </summary>
        public string Keyword
        {
            get;
            private set;
        }


        private ExternalSearchData(string searchEngineName, string keyword)
        {
            SearchEngineName = searchEngineName;
            Keyword = keyword;
        }


        /// <summary>
        /// Returns new <see cref="ExternalSearchData"/> with information about search engine from current request.
        /// </summary>
        /// <param name="referer">Url referer for current request.</param>
        /// <returns>Returns <see cref="ExternalSearchData"/> instance with information about search engine or <c>null</c> if no search engine detected.</returns>
        public static ExternalSearchData Get(Uri referer)
        {
            if (referer == null)
            {
                return null;
            }

            // Try get search engine name for referring URL
            string searchKeyword;
            var searchEngineInfo = SearchEngineAnalyzer.GetSearchEngineFromUrl(referer.AbsoluteUri, out searchKeyword);
            if (searchEngineInfo != null)
            {
                return new ExternalSearchData(searchEngineInfo.SearchEngineDisplayName, searchKeyword);
            }

            return null;
        }
    }
}