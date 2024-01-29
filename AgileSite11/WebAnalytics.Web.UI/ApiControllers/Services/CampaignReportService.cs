using CMS;
using CMS.WebAnalytics.Web.UI;
using CMS.Helpers;

[assembly: RegisterImplementation(typeof(ICampaignReportService), typeof(CampaignReportService), Priority = CMS.Core.RegistrationPriority.SystemDefault)]

namespace CMS.WebAnalytics.Web.UI
{
    /// <summary>
    /// Service providing methods to work with campaign reports.
    /// </summary>
    internal class CampaignReportService : ICampaignReportService
    {
        /// <summary>
        /// Invalidates reports statistics computed for campaign.
        /// </summary>
        /// <param name="campaignId">Campaign identifier</param>
        public void InvalidateCampaignReport(int campaignId)
        {
            var campaign = CampaignInfoProvider.GetCampaignInfo(campaignId);
            if (campaign == null)
            {
                return;
            }

            RemoveCampaignConversionHits(campaign);
            ResetComputedToDate(campaign);
        }
        

        private void RemoveCampaignConversionHits(CampaignInfo campaign)
        {
            CampaignConversionHitsInfoProvider.DeleteCampaignConversionHits(campaign.CampaignID);
        }


        private void ResetComputedToDate(CampaignInfo campaign)
        {
            campaign.CampaignCalculatedTo = DateTimeHelper.ZERO_TIME;
            CampaignInfoProvider.SetCampaignInfo(campaign);
        }
    }
}
