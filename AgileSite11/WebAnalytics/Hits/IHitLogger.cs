using CMS;
using CMS.WebAnalytics;

[assembly: RegisterImplementation(typeof(IHitLogger), typeof(HitLogger), Priority = CMS.Core.RegistrationPriority.Fallback)]

namespace CMS.WebAnalytics
{
    /// <summary>
    /// Service used to log page hits.
    /// </summary>
    internal interface IHitLogger
    {
        /// <summary>
        /// Logs information about browser hit.
        /// </summary>
        /// <param name="siteName">Name of hit site</param>
        /// <param name="browserInformation">Browser information</param>
        void LogBrowserInformation(string siteName, string browserInformation);
    }
}