using CMS;
using CMS.WebAnalytics;

[assembly: RegisterImplementation(typeof(IAnalyticsConsentProvider), typeof(AnalyticsConsentProvider), Priority = CMS.Core.RegistrationPriority.SystemDefault)]

namespace CMS.WebAnalytics
{
    /// <summary>
    /// Contains methods for retrieving existing consents for logging analytics data.
    /// </summary>
    public interface IAnalyticsConsentProvider
    {
        /// <summary>
        /// Returns <c>true</c> when exist consent to log analytics data to system.
        /// Returns <c>false</c> otherwise.
        /// </summary>
        bool HasConsentForLogging();
    }
}