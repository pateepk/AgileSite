using CMS.DataEngine;

namespace CMS.WebAnalytics
{
    /// <summary>
    /// Default implementation of <see cref="IAnalyticsConsentProvider"/>.
    /// </summary>
    internal class AnalyticsConsentProvider : IAnalyticsConsentProvider
    {
        /// <summary>
        /// Returns <c>true</c> when exist the consent to log analytics data to system; otherwise <c>false</c>.
        /// </summary>
        public bool HasConsentForLogging()
        {
            if (!WebAnalyticsEvents.CheckAnalyticsConsent.IsBound)
            {
                return CheckAnalyticsConsentEventArgs.DEFAULT_HAS_CONSENT;
            }

            using (var args = WebAnalyticsEvents.CheckAnalyticsConsent.StartEvent(ModuleCommands.OnlineMarketingGetCurrentContactID()))
            {
                return args.HasConsent;
            }
        }
    }
}