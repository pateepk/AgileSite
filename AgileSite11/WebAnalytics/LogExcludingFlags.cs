using System;

namespace CMS.WebAnalytics
{
    /// <summary>
    /// Log excluding flags. Indicates which check actions should be skipped
    /// </summary>
    [Flags]
    public enum LogExcludingFlags : int
    {
        /// <summary>
        /// Check all
        /// </summary> 
        CheckAll = 0,

        /// <summary>
        /// Skip IP address check
        /// </summary>
        SkipIpCheck = 1,

        /// <summary>
        /// Skip file extension check
        /// </summary>
        SkipFileExtensionCheck = 2,

        /// <summary>
        ///  Skip excluded URL check
        /// </summary>
        SkipUrlCheck = 4,

        /// <summary>
        ///  Skip Crawler check
        /// </summary>
        SkipCrawlerCheck = 8
    }
}