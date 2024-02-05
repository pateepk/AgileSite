namespace CMS.WebAnalytics.Web.UI
{
    /// <summary>
    /// Service providing methods to work with campaign reports.
    /// </summary>
    public interface ICampaignReportService
    {
        /// <summary>
        /// Invalidates reports statistics computed for campaign.
        /// </summary>
        /// <param name="campaignId">Campaign identifier</param>
        void InvalidateCampaignReport(int campaignId);
    }
}
