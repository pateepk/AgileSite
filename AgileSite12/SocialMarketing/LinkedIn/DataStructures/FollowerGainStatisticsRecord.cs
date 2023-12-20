using Newtonsoft.Json;

namespace CMS.SocialMarketing.LinkedInInternal
{
    /// <summary>
    /// Represents Time-Bound Follower Statistics.
    /// See https://docs.microsoft.com/en-us/linkedin/marketing/integrations/community-management/organizations/follower-statistics#retrieve-time-bound-follower-statistics for more information.
    /// </summary>
    [JsonObject]
    internal class FollowerGainStatisticsRecord
    {
        /// <summary>
        /// Statistics contained in retrieved response.
        /// </summary>
        [JsonProperty("timeRange")]
        public TimeRange TimeRange
        { 
            get; 
            set; 
        }


        /// <summary>
        /// Gain statistics during the specified time range.
        /// </summary>
        [JsonProperty("followerGains")]
        public FollowerGainStatisticsData FollowerGainStatisticsData
        {
            get;
            set;
        }


        /// <summary>
        /// Company URN.
        /// </summary>
        [JsonProperty("organizationalEntity")]
        public string CompanyURN
        {
            get;
            set;
        }
    }
}
