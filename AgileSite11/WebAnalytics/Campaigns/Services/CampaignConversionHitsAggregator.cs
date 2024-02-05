using System.Collections.Generic;
using System.Linq;

using CMS.Base;
using CMS.DataEngine;

namespace CMS.WebAnalytics
{
    /// <summary>
    /// Aggregates the number of campaign conversion hits from different sources into one number stored in <see cref="CampaignConversionInfo.CampaignConversionHits"/> property.
    /// </summary>
    internal class CampaignConversionHitsAggregator : ICampaignConversionHitsAggregator
    {
        /// <summary>
        /// Counts and updates <see cref="CampaignConversionInfo.CampaignConversionHits"/> using aggregated value of related <see cref="CampaignConversionHitsInfo.CampaignConversionHitsCount"/>.
        /// </summary>
        /// <param name="conversions">List of <see cref="CampaignConversionInfo"/> objects which should be updated.</param>
        public void AggregateHits(IEnumerable<CampaignConversionInfo> conversions)
        {
            var conversionIDs = conversions.Select(conversion => conversion.CampaignConversionID).ToList();

            var hitsByConversionID = CampaignConversionHitsInfoProvider.GetCampaignConversionHits()
                                                                       .Column("CampaignConversionHitsConversionID")
                                                                       .AddColumn(new AggregatedColumn(AggregationType.Sum, "CampaignConversionHitsCount").As("CampaignConversionHitsCount"))
                                                                       .WhereIn("CampaignConversionHitsConversionID", conversionIDs)
                                                                       .GroupBy("CampaignConversionHitsConversionID")
                                                                       .ToDictionary(item => item.CampaignConversionHitsConversionID, item => item.CampaignConversionHitsCount);

            using (new CMSActionContext() { LogEvents = false, LogSynchronization = false, LogIntegration = false })
            {
                foreach (var conversion in conversions)
                {
                    var conversionID = conversion.CampaignConversionID;

                    if (hitsByConversionID.ContainsKey(conversionID))
                    {
                        conversion.CampaignConversionHits = hitsByConversionID[conversionID];
                        CampaignConversionInfoProvider.SetCampaignConversionInfo(conversion);
                    }
                }
            }
        }
    }
}
