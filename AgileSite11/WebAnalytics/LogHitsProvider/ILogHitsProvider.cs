using System;
using System.ComponentModel;

namespace CMS.WebAnalytics.Internal
{
    /// <summary>
    /// Defines methods which have to be implemented in order to be able to perform logging of visitor hits.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public interface ILogHitsProvider
    {
        /// <summary>
        /// Performs logging of general hit.
        /// </summary>
        /// <param name="logHitParameters">Parameters required for hit logging</param>
        /// <exception cref="ArgumentException">Attempt to found page according to the given <paramref name="logHitParameters"/> was not successful (no page was found).</exception>
        /// <exception cref="InvalidOperationException">Javascript logging is not enabled on the current site.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="logHitParameters"/> is null</exception>
        void LogHit(LogHitParameters logHitParameters);


        /// <summary>
        /// Performs logging of banner hit.
        /// </summary>
        /// <param name="bannerID">ID of the banner the visitor clicked on</param>
        /// <exception cref="InvalidOperationException">Javascript logging is not enabled on the current site.</exception>
        void LogBannerHit(int bannerID);


        /// <summary>
        /// Performs logging of search event hit. 
        /// </summary>
        /// <param name="logSearchHitParameters">Parameters required for hit logging</param>
        /// <exception cref="ArgumentException">Attempt to found page according to the given <paramref name="logSearchHitParameters"/> was not successful (no page was found).</exception>
        /// <exception cref="InvalidOperationException">Javascript logging is not enabled on the current site.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="logSearchHitParameters"/> is null</exception>
        void LogSearchHit(LogSearchHitParameters logSearchHitParameters);
    }
}