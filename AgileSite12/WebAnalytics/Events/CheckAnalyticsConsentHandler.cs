using CMS.Base;

namespace CMS.WebAnalytics
{
    /// <summary>
    /// Handler for retrieving the consent status during logging web analytics data.
    /// </summary>
    public class CheckAnalyticsConsentHandler : SimpleHandler<CheckAnalyticsConsentHandler, CheckAnalyticsConsentEventArgs>
    {
        /// <summary>
        /// Starts the event with given <paramref name="contactId"/>.
        /// </summary>
        public CheckAnalyticsConsentEventArgs StartEvent(int contactId)
        {
            var args = new CheckAnalyticsConsentEventArgs(contactId);

            return StartEvent(args);
        }
    }
}