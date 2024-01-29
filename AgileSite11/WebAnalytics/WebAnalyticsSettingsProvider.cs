using System;

using CMS.DataEngine;

namespace CMS.WebAnalytics
{
    /// <summary>
    /// Provides access to WebAnalytics settings.
    /// </summary>
    internal class WebAnalyticsSettingsProvider : IWebAnalyticsSettingsProvider
    {
        /// <summary>
        /// Defines whether search engines should be excluded.
        /// </summary>
        /// <param name="siteName">Site name</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="siteName"/> is <c>null</c>.</exception>
        /// <returns><c>True</c> if search engines should be excluded otherwise <c>false</c>.</returns>
        public bool ExcludeSearchEngines(string siteName)
        {
            if (siteName == null)
            {
                throw new ArgumentNullException("siteName");
            }

            return SettingsKeyInfoProvider.GetBoolValue(siteName + ".CMSAnalyticsExcludeSearchEngines");
        }


        /// <summary>
        /// Defines whether tracking of browser types is enabled.
        /// </summary>
        /// <param name="siteName">Site name</param>
        /// <returns><c>True</c> if browser types should be tracked otherwise <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="siteName"/> is <c>null</c>.</exception>
        public bool TrackBrowserTypesEnabled(string siteName)
        {
            if (siteName == null)
            {
                throw new ArgumentNullException("siteName");
            }

            return SettingsKeyInfoProvider.GetBoolValue(siteName + ".CMSAnalyticsTrackBrowserTypes");
        }
    }
}