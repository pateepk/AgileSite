using System;
using System.Collections.Concurrent;


namespace CMS.DocumentEngine
{
    /// <summary>
    /// Contextual data object related to the current url (returns different data for different document aliases).
    /// </summary>
    public class PageInfoResult
    {
        private readonly Lazy<ConcurrentDictionary<string, PageInfoResult>> mPageResults = new Lazy<ConcurrentDictionary<string, PageInfoResult>>(() => new ConcurrentDictionary<string, PageInfoResult>(StringComparer.InvariantCultureIgnoreCase));
        private ConcurrentDictionary<string, PageInfoResult> PageResults => mPageResults.Value;


        #region "Public properties"

        /// <summary>
        /// Gets or sets the document alias action mode.
        /// </summary>
        public AliasActionModeEnum DocumentAliasActionMode
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the document alias culture.
        /// </summary>
        public string DocumentAliasCulture
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the document alias URL path.
        /// </summary>
        public string DocumentAliasUrlPath
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the page source.
        /// </summary>
        public PageInfoSource PageSource
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the wildcard query string.
        /// </summary>
        public string WildcardQueryString
        {
            get;
            set;
        }

        #endregion


        #region "Public methods"

        /// <summary>
        /// Gets the PageInfoResult object for the current url. Returns an null if there is no PageInfoResult stored for the current url.
        /// </summary>
        /// <param name="url">The URL</param>
        protected internal PageInfoResult GetCurrentResult(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                return null;
            }

            PageInfoResult result;
            PageResults.TryGetValue(url, out result);

            return result;
        }


        /// <summary>
        /// Stores the page result data for the given url.
        /// </summary>
        /// <param name="url">The URL</param>
        /// <param name="pageResult">The page result data</param>
        protected internal void SetCurrentResult(string url, PageInfoResult pageResult)
        {
            if (string.IsNullOrEmpty(url))
            {
                return;
            }

            PageResults[url] = pageResult;
        }

        #endregion
    }
}
