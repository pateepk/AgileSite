using Newtonsoft.Json;

namespace CMS.SocialMarketing.LinkedInInternal
{
    /// /// <summary>
    /// Represents a company update statistic.
    /// See https://docs.microsoft.com/en-us/linkedin/marketing/integrations/community-management/organizations/share-statistics#schema for more information.
    /// </summary>
    [JsonObject]
    internal class CompanyShareStatisticsRecord
    {
        /// <summary>
        /// The organizational entity URN for which the statistics represents.
        /// </summary>
        [JsonProperty("organizationalEntity")]
        public string CompanyURN 
        { 
            get; 
            set; 
        }


        /// <summary>
        /// The share URN for describing individual share statistics. Is blank for aggregate share statistics.
        /// </summary>
        [JsonProperty("share")]
        public string ShareURN
        {
            get;
            set;
        }


        /// <summary>
        /// The UGC Post URN for describing individual share statistics.
        /// </summary>
        [JsonProperty("ugcPost")]
        public string UgcPost
        {
            get;
            set;
        }


        /// <summary>
        /// Time the statistics record is related to.
        /// </summary>
        [JsonProperty("timeRange")]
        public TimeRange TimeRange
        {
            get;
            set;
        }


        /// <summary>
        /// Share statistics during the specified time range.
        /// </summary>
        [JsonProperty("totalShareStatistics")]
        public ShareStatisticsData ShareStatisticsData
        {
            get;
            set;
        }
    }
}
