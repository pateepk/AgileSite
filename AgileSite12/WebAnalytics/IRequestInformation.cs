using CMS;
using CMS.WebAnalytics;

[assembly: RegisterImplementation(typeof(IRequestInformation), typeof(RequestInformation), Priority = CMS.Core.RegistrationPriority.Fallback)]

namespace CMS.WebAnalytics
{
    /// <summary>
    /// Provides information about current request.
    /// </summary>
    internal interface IRequestInformation
    {
        /// <summary>
        /// Checks whether current request was mad by search crawler.
        /// </summary>
        /// <returns><c>True</c> if request was made by crawler otherwise <c>false</c></returns>
        bool IsCrawlerRequest();


        /// <summary>
        /// Returns information about browser as name and version in string representation.
        /// </summary>
        /// <returns>Browser information or <c>null</c> if no request identified</returns>
        string GetBrowserInformation();
    }
}