using System;
using System.Linq;
using System.Text;

using CMS.DataEngine;
using CMS.Helpers;
using CMS.SiteProvider;
using CMS.WebAnalytics;

namespace CMS.BannerManagement
{
    /// <summary>
    /// Banner management event handlers.
    /// </summary>
    internal class BannerHandlers
    {
        #region "Methods"

        /// <summary>
        /// Initializes the events handlers.
        /// </summary>
        public static void Init()
        {
            WebAnalyticsEvents.GenerateStatistics.After += GenerateStatistics_After;
        }


        /// <summary>
        /// Generates statistics "Banner hits" and "Banner impressions".
        /// </summary>
        private static void GenerateStatistics_After(object sender, GenerateStatisticsEventArgs e)
        {
            var bannerCategories = BannerCategoryInfoProvider.GetBannerCategories();
            var random = new Random();
            var currentSite = SiteContext.CurrentSite;

            var where = new WhereCondition()
                .WhereEquals("StatisticsSiteID", currentSite.SiteID)
                .WhereIn("StatisticsCode", new [] {"bannerhit, bannerclick"});

            StatisticsInfoProvider.RemoveAnalyticsData(DateTimeHelper.ZERO_TIME, DateTimeHelper.ZERO_TIME, currentSite.SiteID, where.ToString(true));
            
            foreach (var bannerCategory in bannerCategories)
            {
                foreach (
                    var banner in
                        BannerInfoProvider.GetBanners().WhereEquals("BannerCategoryID", bannerCategory.BannerCategoryID))
                {
                    int impressedVisitors = (int)(e.Visitors.Sum(visitor => visitor.Value) * 0.8 + 0.1 * random.NextDouble());
                    int clickingVisitors = (int)(e.Visitors.Sum(visitor => visitor.Value) * 0.05 + 0.05 * random.NextDouble());

                    HitLogProcessor.SaveLogToDatabase(new LogRecord
                    {
                        CodeName = "bannerhit",
                        Hits = impressedVisitors,
                        Value = 0,
                        LogTime = e.Date,
                        ObjectName = null,
                        ObjectId = banner.BannerID,
                        SiteName = SiteContext.CurrentSiteName,
                        Culture = null
                    });

                    HitLogProcessor.SaveLogToDatabase(new LogRecord
                    {
                        CodeName = "bannerclick",
                        Hits = clickingVisitors,
                        Value = 0,
                        LogTime = e.Date,
                        ObjectName = null,
                        ObjectId = banner.BannerID,
                        SiteName = SiteContext.CurrentSiteName,
                        Culture = null
                    });
                }
            }
        }

        #endregion
    }
}
