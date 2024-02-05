using System;

using CMS.WebAnalytics.Internal;

namespace CMS.WebAnalytics
{
    /// <summary>
    /// Provides information whether current request was made by search engine
    /// based on current request and ExcludeSearchEngines settings.
    /// </summary>
    internal class SearchEnginesDetector : ISearchEnginesDetector
    {
        private readonly IRequestInformation mRequestInformation;
        private readonly IWebAnalyticsSettingsProvider mWebAnalyticsSettingsProvider;


        /// <summary>
        /// Creates new instance of <see cref="SearchEnginesDetector"/>.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="requestInformation"/> or <paramref name="webAnalyticsSettingsProvider"/> is <c>null</c></exception>
        public SearchEnginesDetector(IRequestInformation requestInformation, IWebAnalyticsSettingsProvider webAnalyticsSettingsProvider)
        {
            if (requestInformation == null)
            {
                throw new ArgumentNullException("requestInformation");
            }

            if (webAnalyticsSettingsProvider == null)
            {
                throw new ArgumentNullException("webAnalyticsSettingsProvider");
            }

            mRequestInformation = requestInformation;
            mWebAnalyticsSettingsProvider = webAnalyticsSettingsProvider;
        }


        /// <summary>
        /// Returns whether request was made by search engine with respect to site settings.
        /// </summary>
        /// <param name="siteName">Name of site</param>
        /// <returns><c>true</c> if request was made by search engine and exclude search engines setting is active otherwise <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="siteName"/> is <c>null</c>.</exception>
        public bool IsSearchEngine(string siteName)
        {
            if (siteName == null)
            {
                throw new ArgumentNullException("siteName");
            }

            if (mRequestInformation.IsCrawlerRequest())
            {
                return mWebAnalyticsSettingsProvider.ExcludeSearchEngines(siteName);
            }

            return false;
        }
    }
}