namespace CMS.WebAnalytics
{
    /// <summary>
    /// Identifies the campaign conversion type for the <see cref="CampaignConversionHitsProcessor"/>.
    /// </summary>
    internal class CampaignConversionType
    {
        /// <summary>
        /// Activity type for which is report calculated.
        /// </summary>
        public string ActivityType
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates whether conversion hits should include all activities of <see cref="ActivityType"/>
        /// or only activities with relevant item (specific page, product etc.).
        /// </summary>
        public bool IsForAll
        {
            get;
            set;
        }


        public CampaignConversionType(string activityType, bool isForAll)
        {
            ActivityType = activityType;
            IsForAll = isForAll;
        }
    }
}
