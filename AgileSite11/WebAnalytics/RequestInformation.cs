using CMS.Helpers;

namespace CMS.WebAnalytics
{
    /// <summary>
    /// Provides information about current request.
    /// </summary>
    internal class RequestInformation : IRequestInformation
    {
        /// <summary>
        /// Checks whether current request was mad by search crawler.
        /// </summary>
        /// <returns><c>True</c> if request was made by crawler otherwise <c>false</c></returns>
        public bool IsCrawlerRequest()
        {
            return BrowserHelper.IsCrawler();
        }


        /// <summary>
        /// Returns information about browser as name and version in string representation.
        /// </summary>
        /// <returns>Browser information or <c>null</c> if no request identified</returns>
        public string GetBrowserInformation()
        {
            return BrowserHelper.GetBrowser();
        }
    }
}