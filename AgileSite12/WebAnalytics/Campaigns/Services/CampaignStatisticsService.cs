using System.Linq;

using CMS.Activities;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.WebAnalytics.Internal;

namespace CMS.WebAnalytics
{
    /// <summary>
    /// Provides methods for campaign statistics computation.
    /// </summary>
    internal class CampaignStatisticsService : ICampaignStatisticsService
    {
        /// <summary>
        /// This method computes the sum of hits of all conversions within the campaign.
        /// </summary>
        /// <param name="campaignId">Campaign identifier.</param>
        /// <returns>Total conversions hits within the campaign.</returns>
        public int ComputeConversionsCount(int campaignId)
        {
            // Count number of non-funnel conversions only.
            // Funnel step conversions represent some sort of sub-conversions so they are not important for statistics.
            var statistics = CampaignConversionInfoProvider.GetCampaignConversions()
                                                           .WhereEquals("CampaignConversionCampaignID", campaignId)
                                                           .WhereFalse("CampaignConversionIsFunnelStep")
                                                           .Column(new AggregatedColumn(AggregationType.Sum, "CampaignConversionHits").As("Conversions"))
                                                           .Result.Tables[0].Rows[0];

            return ValidationHelper.GetInteger(statistics["Conversions"], 0);
        }


        /// <summary>
        /// Returns statistics related to the campaign objective.
        /// </summary>
        /// <param name="campaignId">ID of the campaign to get objective for.</param>
        public CampaignObjectiveStatistics GetObjectiveStatistics(int campaignId)
        {
            var objective = GetObjective(campaignId);
            if ((objective == null) || (objective.CampaignObjectiveValue == 0))
            {
                return null;
            }

            var objectiveConversion = CampaignConversionInfoProvider.GetCampaignConversionInfo(objective.CampaignObjectiveCampaignConversionID);
            if (objectiveConversion == null)
            {
                return null;
            }

            var typeName = GetConversionTypeName(objectiveConversion);
            var paramName = objectiveConversion.CampaignConversionDisplayName;

            return new CampaignObjectiveStatistics
            {
                Target = objective.CampaignObjectiveValue,
                Actual = objectiveConversion.CampaignConversionHits,
                Name = string.IsNullOrEmpty(paramName) ? typeName : string.Format("{0}: {1}", typeName, paramName)
            };
        }


        private string GetConversionTypeName(CampaignConversionInfo conversion)
        {
            var activityType = ActivityTypeInfoProvider.GetActivityTypeInfo(conversion.CampaignConversionActivityType);
            if (activityType != null)
            {
                return activityType.ActivityTypeDisplayName;
            }

            return string.Empty;
        }


        private CampaignObjectiveInfo GetObjective(int campaignId)
        {
            return CampaignObjectiveInfoProvider.GetCampaignObjectives()
                                                .WhereEquals("CampaignObjectiveCampaignID", campaignId)
                                                .TopN(1)
                                                .FirstOrDefault();
        }
    }
}
