using CMS;
using CMS.WebAnalytics;
using CMS.WebAnalytics.Internal;

[assembly: RegisterImplementation(typeof(ICampaignStatisticsService), typeof(CampaignStatisticsService), Priority = CMS.Core.RegistrationPriority.Fallback)]

namespace CMS.WebAnalytics.Internal
{
    /// <summary>
    /// Provides methods for campaign statistics computation.
    /// </summary>
    public interface ICampaignStatisticsService
    {
        /// <summary>
        /// This method computes the sum of hits of all conversions within the campaign.
        /// </summary>
        /// <param name="campaignId">Campaign identifier.</param>
        /// <returns>Total conversions hits within the campaign.</returns>
        int ComputeConversionsCount(int campaignId);


        /// <summary>
        /// Returns statistics related to the campaign objective. 
        /// </summary>
        /// <param name="campaignId">ID of the campaign to get objective for.</param>
        CampaignObjectiveStatistics GetObjectiveStatistics(int campaignId);
    }
}
