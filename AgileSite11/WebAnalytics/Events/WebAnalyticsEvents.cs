namespace CMS.WebAnalytics
{
    /// <summary>
    /// Web analytics events.
    /// </summary>
    public static class WebAnalyticsEvents
    {
        /// <summary>
        /// Fires when analytics log is being processed.
        /// </summary>
        public static ProcessLogRecordHandler ProcessLogRecord = new ProcessLogRecordHandler { Name = "WebAnalyticsEvents.ProcessLogRecord" };


        /// <summary>
        /// Fires when conversion is logged.
        /// </summary>
        public static LogConversionHandler LogConversion = new LogConversionHandler { Name = "WebAnalyticsEvents.LogConversion" };


        /// <summary>
        /// Fires when analytics are being logged.
        /// </summary>
        public static ProcessAnalyticsServiceHandler ProcessAnalyticsService = new ProcessAnalyticsServiceHandler { Name = "WebAnalyticsEvents.ProcessAnalyticsService" };


        /// <summary>
        /// Fires when web analytics JavaScript snippet is being inserted to the page.
        /// </summary>
        public static InsertAnalyticsJSHandler InsertAnalyticsJS = new InsertAnalyticsJSHandler { Name = "WebAnalyticsEvents.InsertAnalyticsJS" };


        /// <summary>
        /// Fires when web analytics statistics are being generated.
        /// </summary>
        public static GenerateStatisticsHandler GenerateStatistics = new GenerateStatisticsHandler { Name = "WebAnalyticsEvents.GenerateStatistics" };


        /// <summary>
        /// Fires when campaign is being launched. 
        /// </summary>
        public static CampaignLaunchedHandler CampaignLaunched = new CampaignLaunchedHandler { Name = "WebAnalyticsEvents.CampaignLaunched" };
        

        /// <summary>
        /// Fires when there is a change in campaigns UTM parameter.
        /// </summary>
        public static CampaignUTMChangedHandler CampaignUTMChanged = new CampaignUTMChangedHandler { Name = "WebAnalyticsEvents.CampaignUTMHandler" };


        /// <summary>
        /// Fires when analytics checks for data protection consent.
        /// </summary>
        public static CheckAnalyticsConsentHandler CheckAnalyticsConsent = new CheckAnalyticsConsentHandler { Name = "WebAnalyticsEvents.CheckAnalyticsConsent" };
    }
}