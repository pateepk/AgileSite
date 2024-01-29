using System;

using CMS;
using CMS.WebAnalytics;

[assembly: RegisterImplementation(typeof(BrowserHitLogger), typeof(BrowserHitLogger), Priority = CMS.Core.RegistrationPriority.SystemDefault)]

namespace CMS.WebAnalytics
{
    /// <summary>
    /// Represents logging of browser hits.
    /// </summary>
    internal class BrowserHitLogger
    {
        private readonly IHitLogger mHitLogger;
        private readonly IWebAnalyticsSettingsProvider mWebAnalyticsSettingsProvider;
        private readonly IRequestInformation mRequestInformation;
        private readonly IAnalyticsConsentProvider mConsentProvider;


        /// <summary>
        /// Creates new instance of <see cref="BrowserHitLogger"/>.
        /// </summary>
        /// <param name="hitLogger">Implementation of <see cref="IHitLogger"/> used to log browser hits.</param>
        /// <param name="webAnalyticsSettingsProvider">Implementation of <see cref="IWebAnalyticsSettingsProvider"/> used to check whether logging is enabled.</param>
        /// <param name="requestInformation">Implementation of <see cref="IRequestInformation"/> used to check whether request was made by search engine.</param>
        /// <param name="consentProvider">Implementation of <see cref="IAnalyticsConsentProvider"/> used to check whether there is valid consent for logging.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="hitLogger"/> or <paramref name="webAnalyticsSettingsProvider"/> 
        ///                                         or <paramref name="requestInformation"/> or <paramref name="consentProvider"/> is <c>null</c>.</exception>
        public BrowserHitLogger(IHitLogger hitLogger, IWebAnalyticsSettingsProvider webAnalyticsSettingsProvider,
            IRequestInformation requestInformation, IAnalyticsConsentProvider consentProvider)
        {
            if (hitLogger == null)
            {
                throw new ArgumentNullException(nameof(hitLogger));
            }

            if (webAnalyticsSettingsProvider == null)
            {
                throw new ArgumentNullException(nameof(webAnalyticsSettingsProvider));
            }

            if (requestInformation == null)
            {
                throw new ArgumentNullException(nameof(requestInformation));
            }

            if (consentProvider == null)
            {
                throw new ArgumentNullException(nameof(consentProvider));
            }

            mHitLogger = hitLogger;
            mWebAnalyticsSettingsProvider = webAnalyticsSettingsProvider;
            mRequestInformation = requestInformation;
            mConsentProvider = consentProvider;
        }


        /// <summary>
        /// Logs browser hit for given site name.
        /// </summary>
        /// <param name="siteName">Site name</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="siteName"/> is <c>null</c>.</exception>
        public void LogBrowser(string siteName)
        {
            if (siteName == null)
            {
                throw new ArgumentNullException(nameof(siteName));
            }

            if (IsBrowserLoggingEnabled(siteName) && mConsentProvider.HasConsentForLogging() && IsNotCrawlerRequest())
            {
                var browser = mRequestInformation.GetBrowserInformation();
                if (!String.IsNullOrEmpty(browser))
                {
                    mHitLogger.LogBrowserInformation(siteName, browser);
                }
            }
        }


        private bool IsBrowserLoggingEnabled(string siteName)
        {
            return mWebAnalyticsSettingsProvider.TrackBrowserTypesEnabled(siteName);
        }


        private bool IsNotCrawlerRequest()
        {
            return !mRequestInformation.IsCrawlerRequest();
        }
    }
}