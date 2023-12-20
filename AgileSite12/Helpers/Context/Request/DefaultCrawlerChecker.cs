using System;
using System.Linq;
using System.Text;

namespace CMS.Helpers.Internal
{
    /// <summary>
    /// Provides method for checking whether the current visitor is crawler.
    /// </summary>
    internal class DefaultCrawlerChecker : ICrawlerChecker
    {
        /// <summary>
        /// Checks whether the current request comes from the crawler.
        /// </summary>
        /// <returns>True, if current request comes from the crawler; otherwise, false</returns>
        public bool IsCrawler()
        {
            return BrowserHelper.IsCrawler();
        }
    }
}