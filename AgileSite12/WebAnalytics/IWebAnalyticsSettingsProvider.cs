using CMS;
using CMS.WebAnalytics;

[assembly: RegisterImplementation(typeof(IWebAnalyticsSettingsProvider), typeof(WebAnalyticsSettingsProvider), Priority = CMS.Core.RegistrationPriority.Fallback)]

namespace CMS.WebAnalytics
{
    /// <summary>
    /// Provides access to WebAnalytics settings.
    /// </summary>
    internal interface IWebAnalyticsSettingsProvider
    {
        /// <summary>
        /// Defines whether search engines should be excluded.
        /// </summary>
        /// <param name="siteName">Site name</param>
        /// <returns><c>True</c> if search engines should be excluded otherwise <c>false</c>.</returns>
        bool ExcludeSearchEngines(string siteName);


        /// <summary>
        /// Defines whether tracking of browser types is enabled.
        /// </summary>
        /// <param name="siteName">Site name</param>
        /// <returns><c>True</c> if browser types should be tracked otherwise <c>false</c>.</returns>
        bool TrackBrowserTypesEnabled(string siteName);
    }
}