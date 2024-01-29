using System.ComponentModel;

using CMS;
using CMS.WebAnalytics;
using CMS.WebAnalytics.Internal;

[assembly: RegisterImplementation(typeof(ISearchEnginesDetector), typeof(SearchEnginesDetector), Priority = CMS.Core.RegistrationPriority.Fallback)]

namespace CMS.WebAnalytics.Internal
{
    /// <summary>
    /// Provides information whether current request was made by search engine
    /// based on current request and ExcludeSearchEngines settings.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public interface ISearchEnginesDetector
    {
        /// <summary>
        /// Returns whether request was made by search engine with respect to site settings.
        /// </summary>
        /// <param name="siteName">Name of site</param>
        /// <returns><c>true</c> if request was made by search engine and exclude search engines setting is active otherwise <c>false</c>.</returns>
        bool IsSearchEngine(string siteName);
    }
}