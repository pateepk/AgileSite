using System.Collections.Generic;

using CMS;
using CMS.WebAnalytics;

[assembly: RegisterImplementation(typeof(ICampaignConversionHitsAggregator), typeof(CampaignConversionHitsAggregator), Priority = CMS.Core.RegistrationPriority.Fallback)]

namespace CMS.WebAnalytics
{
    /// <summary>
    /// Aggregates the number of campaign conversion hits from different sources into one number stored in <see cref="CampaignConversionInfo.CampaignConversionHits"/> property.
    /// </summary>
    public interface ICampaignConversionHitsAggregator
    {
        /// <summary>
        /// Counts and updates <see cref="CampaignConversionInfo.CampaignConversionHits"/> using aggregated value of related <see cref="CampaignConversionHitsInfo.CampaignConversionHitsCount"/>.
        /// </summary>
        /// <param name="conversions">List of <see cref="CampaignConversionInfo"/> objects which should be updated.</param>
        void AggregateHits(IEnumerable<CampaignConversionInfo> conversions);
    }
}
