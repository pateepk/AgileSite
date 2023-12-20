using System;

using CMS;
using CMS.WebAnalytics;

[assembly: RegisterImplementation(typeof(ICampaignAssetsPublisher), typeof(CampaignAssetsPublisher), Priority = CMS.Core.RegistrationPriority.Fallback)]

namespace CMS.WebAnalytics
{
    /// <summary>
    /// Publishes assets added to the campaign.
    /// </summary>
    public interface ICampaignAssetsPublisher
    {
        /// <summary>
        /// Publishes page assets and file assets added to the given campaign.
        /// </summary>
        /// <param name="campaign">Campaign whose assets are published.</param>
        void PublishPagesAndFiles(CampaignInfo campaign);
    }
}
