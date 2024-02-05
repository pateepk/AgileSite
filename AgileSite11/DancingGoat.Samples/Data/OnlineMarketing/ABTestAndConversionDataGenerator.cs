using System;
using System.Linq;

using CMS.DataEngine;
using CMS.Helpers;
using CMS.OnlineMarketing;
using CMS.SiteProvider;
using CMS.WebAnalytics;

namespace CMS.DancingGoat.Samples
{
    /// <summary>
    /// Class generating and deleting sample statistics for A/B tests and campaigns.
    /// </summary>
    internal class ABTestAndConversionDataGenerator
    {
        #region "Constants"

        private const string COFFEE_SAMPLE_ORDER_CONVERSION_NAME = "CoffeeSampleOrder";
        private const string COLOMBIA_LANDING_PAGE_AB_TEST_NAME = "ColombiaLandingPageA_BTest";

        private const string COLOMBIA_LANDING_PAGE_ORIGINAL_VARIANT_NAME = "Original";
        private const string COLOMBIA_LANDING_PAGE_B_VARIANT_NAME = "BVariant";

        private const string CAFE_SAMPLE_PROMOTION_CAMPAIGN_NAME = "CafeSamplePromotion";

        private const int CONVERSION_VALUE = 10;

        #endregion


        #region "Variables"

        private readonly int[] mFirstConversionsForOriginalPage = { 57, 69, 60, 42, 28, 17, 11, 5, 3, 2, 2, 1, 1, 1, 0 };
        private readonly int[] mFirstConversionsForBPage = { 35, 43, 38, 27, 15, 9, 5, 4, 4, 2, 1, 1, 1, 0, 0 };

        private readonly int[] mRecurringConversionsForOriginalPage = { 1, 2, 2, 1, 0, 0, 1, 2, 1, 0, 0, 0, 0, 1, 0 };
        private readonly int[] mRecurringConversionsForBPage = { 35, 43, 38, 27, 15, 9, 5, 4, 4, 2, 1, 1, 1, 0, 0 };

        private readonly int[] mConversionsForOriginalPage = { 58, 71, 62, 43, 28, 17, 12, 7, 4, 2, 2, 1, 1, 2, 0 };
        private readonly int[] mConversionsForBPage = { 35, 45, 40, 30, 16, 9, 5, 5, 4, 2, 3, 1, 1, 0, 0 };

        private readonly int[] mFirstVisitorsForOriginalPage = { 65, 76, 72, 52, 43, 38, 22, 12, 8, 5, 5, 3, 3, 2, 1 };
        private readonly int[] mFirstVisitorsForBPage = { 66, 75, 72, 52, 44, 37, 21, 13, 9, 4, 4, 4, 3, 2, 2 };

        private readonly int[] mReturningVisitorsForOriginalPage = { 17, 19, 23, 25, 19, 15, 15, 17, 18, 20, 26, 22, 33, 35, 33 };
        private readonly int[] mReturningVisitorsForBPage = { 18, 22, 19, 22, 17, 17, 15, 19, 20, 26, 24, 26, 29, 31, 30 };

        private readonly SiteInfo mSite;
        private ABTestInfo mABTest;
        private ConversionInfo mConversion;
        private ABVariantInfo mOriginalVariant;
        private ABVariantInfo mBVariant;
        private CampaignInfo mCampaign;

        #endregion


        #region "Constructor"

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="site">Site to generate data for</param>
        public ABTestAndConversionDataGenerator(SiteInfo site)
        {
            mSite = site;
        }

        #endregion


        #region "Properties"

        private ABTestInfo ABTest
        {
            get
            {
                return mABTest ?? (mABTest = ABTestInfoProvider.GetABTests().TopN(1)
                                                               .OnSite(mSite.SiteID)
                                                               .WhereEquals("ABTestName", COLOMBIA_LANDING_PAGE_AB_TEST_NAME).FirstOrDefault());
            }
        }


        private ConversionInfo Conversion
        {
            get
            {
                return mConversion ?? (mConversion = ConversionInfoProvider.GetConversions().TopN(1)
                                                                           .OnSite(mSite.SiteID)
                                                                           .WhereEquals("ConversionName", COFFEE_SAMPLE_ORDER_CONVERSION_NAME).FirstOrDefault());
            }
        }


        private ABVariantInfo OriginalVariant
        {
            get
            {
                if (mOriginalVariant == null)
                {
                    var variants = ABCachedObjects.GetVariants(ABTest);
                    mOriginalVariant =
                        variants.Single(variant => variant.ABVariantName == COLOMBIA_LANDING_PAGE_ORIGINAL_VARIANT_NAME);
                }

                return mOriginalVariant;
            }
        }


        private ABVariantInfo BVariant
        {
            get
            {
                if (mBVariant == null)
                {
                    var variants = ABCachedObjects.GetVariants(ABTest);
                    mBVariant =
                        variants.Single(variant => variant.ABVariantName == COLOMBIA_LANDING_PAGE_B_VARIANT_NAME);
                }

                return mBVariant;
            }
        }


        private CampaignInfo Campaign
        {
            get
            {
                return mCampaign ?? (mCampaign = CampaignInfoProvider.GetCampaigns().TopN(1)
                                                                     .OnSite(mSite.SiteID)
                                                                     .WhereEquals("CampaignName", CAFE_SAMPLE_PROMOTION_CAMPAIGN_NAME).FirstOrDefault());
            }
        }

        #endregion


        #region "Public methods"

        /// <summary>
        /// Generates statistics for AB tests on the current site.
        /// </summary>
        public void Generate()
        {
            if (ABTest == null || Conversion == null || Campaign == null)
            {
                return;
            }

            DateTime to = DateTime.Now;
            DateTime from = to.AddDays(-14);

            ABTest.ABTestOpenFrom = from;
            ABTest.ABTestOpenTo = to;

            ClearStatisticsData();

            for (DateTime startTime = from; startTime < to.AddDays(1); startTime = startTime.AddDays(1))
            {
                int daysFromStart = (startTime - from).Days;

                LogABConversionHits(daysFromStart, startTime);
                LogABVisitHits(daysFromStart, startTime);
            }

            ABTest.ABTestWinnerGUID = OriginalVariant.ABVariantGUID;
            ABTestInfoProvider.SetABTestInfo(ABTest);
        }

        #endregion


        #region "Private methods"

        /// <summary>
        /// Removes all previously logged data related to the Dancing Goat default A/B test and Campaign.
        /// </summary>
        private void ClearStatisticsData()
        {
            var where = new WhereCondition().WhereContains("StatisticsCode", ABTest.ABTestName)
                                            .Or()
                                            .WhereEquals("StatisticsObjectName", Campaign.CampaignUTMCode)
                                            .Or()
                                            .WhereEquals("StatisticsObjectName", Conversion.ConversionName);

            StatisticsInfoProvider.RemoveAnalyticsData(DateTimeHelper.ZERO_TIME, DateTimeHelper.ZERO_TIME, mSite.SiteID,
                where.ToString(true));
        }


        /// <summary>
        /// Performs logging of A/B conversion hits.
        /// </summary>
        /// <param name="daysFromStart">Determines for which day should be the logging performed (selects correct values from the data arrays)</param>
        /// <param name="logDate">Date the hit is assigned to</param>
        private void LogABConversionHits(int daysFromStart, DateTime logDate)
        {
            Action<string, int[], int[]> logABConversionHit = (type, originalVariantData, bVariantData) =>
            {
                LogHit(GetABHitCodename(type, ABTest, OriginalVariant), originalVariantData[daysFromStart], originalVariantData[daysFromStart] * CONVERSION_VALUE,
                    logDate, Conversion.ConversionName);

                LogHit(GetABHitCodename(type, ABTest, BVariant), bVariantData[daysFromStart], bVariantData[daysFromStart] * CONVERSION_VALUE,
                    logDate, Conversion.ConversionName);
            };

            logABConversionHit("absessionconversionfirst", mFirstConversionsForOriginalPage, mFirstConversionsForBPage);
            logABConversionHit("absessionconversionrecurring", mRecurringConversionsForOriginalPage, mRecurringConversionsForBPage);
            logABConversionHit("abconversion", mConversionsForOriginalPage, mConversionsForBPage);
        }


        /// <summary>
        /// Performs logging of A/B visit hits.
        /// </summary>
        /// <param name="daysFromStart">Determines for which day should be the logging performed (selects correct values from the data arrays)</param>
        /// <param name="logDate">Date the hit is assigned to</param>
        private void LogABVisitHits(int daysFromStart, DateTime logDate)
        {
            Action<string, int[], int[]> logABVisitHit = (type, originalVariantData, bVariantData) =>
            {
                LogHit(GetABHitCodename(type, ABTest, OriginalVariant), originalVariantData[daysFromStart], 0, logDate, OriginalVariant.ABVariantPath);

                LogHit(GetABHitCodename(type, ABTest, BVariant), bVariantData[daysFromStart], 0, logDate, BVariant.ABVariantPath);
            };

            logABVisitHit("abvisitfirst", mFirstVisitorsForOriginalPage, mFirstVisitorsForBPage);
            logABVisitHit("abvisitreturn", mReturningVisitorsForOriginalPage, mReturningVisitorsForBPage);
        }


        /// <summary>
        /// Constructs proper statistics code name for the A/B hit.
        /// </summary>
        private string GetABHitCodename(string statisticsType, ABTestInfo abTest, ABVariantInfo variant)
        {
            return statisticsType + ";" + abTest.ABTestName + ";" + variant.ABVariantName;
        }
        

        /// <summary>
        /// Performs logging of the hit.
        /// </summary>
        private void LogHit(string codeName, int visits, int value, DateTime logTime, string objectName)
        {
            HitLogProcessor.SaveLogToDatabase(new LogRecord
            {
                CodeName = codeName,
                Hits = visits,
                Value = value,
                LogTime = logTime,
                ObjectName = objectName,
                SiteName = mSite.SiteName,
                Culture = "en-US"
            });
        }

        #endregion
    }
}