using System;

namespace CMS.WebAnalytics
{
    /// <summary>
    /// Service used to log page hits.
    /// </summary>
    internal class HitLogger : IHitLogger
    {
        /// <summary>
        /// Logs information about browser hit.
        /// </summary>
        /// <param name="siteName">Name of hit site</param>
        /// <param name="browserInformation">Browser information</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="siteName"/> or <paramref name="browserInformation"/> is <c>null</c>.</exception>
        public void LogBrowserInformation(string siteName, string browserInformation)
        {
            if (siteName == null)
            {
                throw new ArgumentNullException("siteName");
            }

            if (browserInformation == null)
            {
                throw new ArgumentNullException("browserInformation");
            }

            HitLogProvider.LogHit(HitLogProvider.BROWSER_TYPE, siteName, null, browserInformation, 0);
        }
    }
}