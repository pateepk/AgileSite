using CMS.Activities;

namespace CMS.WebAnalytics
{
    /// <summary>
    /// Modifies activity with campaign data.
    /// </summary>
    internal class CampaignActivityModifier : IActivityModifier
    {
        private readonly ICampaignService mCampaignService;


        /// <summary>
        /// Creates new instance of <see cref="CampaignActivityModifier"/>.
        /// </summary>
        /// <param name="campaignService">Campaign service used to obtain current campaign.</param>
        public CampaignActivityModifier(ICampaignService campaignService)
        {
            mCampaignService = campaignService;
        }


        /// <summary>
        /// Sets activity campaign.
        /// <remarks>
        /// UTM source is set only if it is not empty.
        /// UTM content is set only if it is not empty.
        /// </remarks>
        /// </summary>
        public void Modify(IActivityInfo activity)
        {
            if (activity.ActivityCampaign == null)
            {
                activity.ActivityCampaign = mCampaignService.CampaignCode;

                var source = mCampaignService.CampaignSourceName;
                var content = mCampaignService.CampaignContent;

                activity.ActivityUTMSource = string.IsNullOrEmpty(source) ? null : source;
                activity.ActivityUTMContent = string.IsNullOrEmpty(content) ? null : content;
            }
        }
    }
}