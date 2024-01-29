using CMS;
using CMS.Helpers.Internal;

[assembly: RegisterImplementation(typeof(ICrawlerChecker), typeof(DefaultCrawlerChecker), Priority = CMS.Core.RegistrationPriority.Fallback)]

namespace CMS.Helpers.Internal
{
    /// <summary>
    /// Provides method for checking whether the current visitor is crawler.
    /// </summary>
    public interface ICrawlerChecker
    {
        /// <summary>
        /// Checks whether the current request comes from the crawler.
        /// </summary>
        /// <returns>True, if current request comes from the crawler; otherwise, false</returns>
        bool IsCrawler();
    }
}