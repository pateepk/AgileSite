using System;
using System.Web;

using CMS.Base;

namespace CMS.Helpers
{
    /// <summary>
    /// Helper methods for documentation links.
    /// </summary>
    public class DocumentationHelper : AbstractHelper<DocumentationHelper>
    {
        /// <summary>
        /// Base URL for product documentation
        /// </summary>
        private const string DOCUMENTATION_BASE_URL = "http://kentico.com/CMSPages/DocLinkMapper.ashx";


        private string mDocumentationRootUrl;
        private string mDocumentationSearchUrlPattern;


        /// <summary>
        /// Gets URL for product documentation root.
        /// </summary>
        /// <returns>URL for product documentation root.</returns>
        public static string GetDocumentationRootUrl()
        {
            return HelperObject.GetDocumentationRootUrlInternal();
        }


        /// <summary>
        /// Gets URL for product documentation topic. Topic is either a topic ID, or an absolute URL.
        /// </summary>
        /// <param name="topic">Topic identifier or permanent link to documentation topic.</param>
        /// <returns>URL for product documentation topic</returns>
        public static string GetDocumentationTopicUrl(string topic)
        {
            return HelperObject.GetDocumentationTopicUrlInternal(topic);
        }


        /// <summary>
        /// Gets URL pattern for product documentation search.
        /// Pass searched text as 0th substitution.
        /// </summary>
        /// <returns>URL pattern for product documentation search</returns>
        public static string GetDocumentationSearchUrlPattern()
        {
            return HelperObject.GetDocumentationSearchUrlPatternInternal();
        }


        /// <summary>
        /// Gets URL for product documentation root.
        /// </summary>
        /// <returns>URL for product documentation root.</returns>
        protected virtual string GetDocumentationRootUrlInternal()
        {
            if (String.IsNullOrEmpty(mDocumentationRootUrl))
            {
                string version = CMSVersion.GetVersion(true, true, false, false) + "sp";
                mDocumentationRootUrl = URLHelper.AddParameterToUrl(DOCUMENTATION_BASE_URL, "version", HttpUtility.UrlEncode(version));
            }

            return mDocumentationRootUrl;
        }


        /// <summary>
        /// Gets URL for product documentation topic. Topic is either a topic ID, or an absolute URL.
        /// </summary>
        /// <param name="topic">Topic identifier or permanent link to documentation topic.</param>
        /// <returns>URL for product documentation topic</returns>
        protected virtual string GetDocumentationTopicUrlInternal(string topic)
        {
            if (Uri.TryCreate(topic, UriKind.Absolute, out _))
            {
                return topic;
            }

            return URLHelper.AddParameterToUrl(GetDocumentationRootUrl(), "link", HttpUtility.UrlEncode(topic));
        }


        /// <summary>
        /// Gets URL pattern for product documentation search.
        /// Pass searched text as 0th substitution.
        /// </summary>
        /// <returns>URL pattern for product documentation search</returns>
        protected virtual string GetDocumentationSearchUrlPatternInternal()
        {
            if (String.IsNullOrEmpty(mDocumentationSearchUrlPattern))
            {
                mDocumentationSearchUrlPattern = URLHelper.AddParameterToUrl(GetDocumentationRootUrl(), "search", "{0}");
            }

            return mDocumentationSearchUrlPattern;
        }
    }
}
