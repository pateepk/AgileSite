namespace CMS.WebAnalytics
{
    /// <summary>
    /// Arguments for event handlers related to <see cref="CampaignUTMChangedHandler"/>.
    /// </summary>
    public class CampaignUTMChangedData
    {
        /// <summary>
        /// ID of issue (for A/B must be master email ID).
        /// </summary>
        public int OriginalEmailID
        {
            get;
            set;
        }


        /// <summary>
        /// Campaign info.
        /// </summary>
        public CampaignInfo Campaign
        {
            get;
            set;
        }


        /// <summary>
        /// New UTM source to set.
        /// </summary>
        public string NewUTMSource
        {
            get;
            set;
        }
    }
}
