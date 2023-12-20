using System.Collections.Specialized;

using CMS.Helpers;

namespace CMS.DocumentEngine.PageBuilder
{
    /// <summary>
    /// Provides helper methods for a Page builder feature.
    /// </summary>
    public static class PageBuilderHelper
    {
        /// <summary>
        /// Query string key for editing instance identifier.
        /// </summary>
        public const string EDITING_INSTANCE_QUERY_NAME = "instance";

        
        /// <summary>
        /// Query string key for edit mode detection.
        /// </summary>
        public const string EDIT_MODE_QUERY_NAME = "editmode";


        /// <summary>
        /// Query string key for clear cache request.
        /// </summary>
        public const string CLEAR_PAGE_CACHE_QUERY_NAME = "invalidatecache";


        private const string EDIT_MODE_QUERY_VALUE = "1";


        /// <summary>
        /// Adds query string parameter to URL to identify the edit mode of Page builder feature.
        /// </summary>
        /// <param name="url">URL to modify.</param>
        public static string AddEditModeParameter(string url)
        {
            return URLHelper.AddParameterToUrl(url, EDIT_MODE_QUERY_NAME, EDIT_MODE_QUERY_VALUE);
        }


        /// <summary>
        /// Adds query string parameter to URL to identify the clear page cache request.
        /// </summary>
        /// <param name="url">URL to modify.</param>
        public static string AddClearPageCacheParameter(string url)
        {
            return URLHelper.AddParameterToUrl(url, CLEAR_PAGE_CACHE_QUERY_NAME, "1");
        }


        /// <summary>
        /// Generates preview link for specified page and user.
        /// </summary>
        /// <param name="page">Page for which the preview link should be generated.</param>
        /// <param name="userName">User name.</param>
        /// <param name="queryString">Optional additional query string parameters.</param>
        public static string GetPreviewLink(TreeNode page, string userName, NameValueCollection queryString = null)
        {
            return new PreviewLinkGenerator(page).Generate(userName, readonlyMode: false, embededInAdministration: true, queryString: queryString);
        }
    }
}