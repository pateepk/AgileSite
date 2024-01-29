using CMS.Base;

namespace CMS.WebAnalytics
{
    /// <summary>
    /// Event arguments for handler <see cref="CheckAnalyticsConsentHandler"/>.
    /// </summary>
    public class CheckAnalyticsConsentEventArgs : CMSEventArgs
    {
        internal const bool DEFAULT_HAS_CONSENT = true;

        /// <summary>
        /// Gets the contact identifier.
        /// </summary>
        public int ContactId
        {
            get;
        }


        /// <summary>
        /// Indicates whether the analytics has consent to log data into system. Default is <c>true</c>.
        /// </summary>
        public bool HasConsent
        {
            get;
            set;
        } = DEFAULT_HAS_CONSENT;


        /// <summary>
        /// Creates an instance of <see cref="CheckAnalyticsConsentEventArgs"/>.
        /// </summary>
        public CheckAnalyticsConsentEventArgs(int contactId)
        {
            ContactId = contactId;
        }
    }
}
