using Newtonsoft.Json;

namespace CMS.SocialMarketing.LinkedInInternal
{
    /// <summary>
    /// Follower gain statistics data. 
    /// See https://docs.microsoft.com/en-us/linkedin/marketing/integrations/community-management/organizations/follower-statistics#retrieve-time-bound-follower-statistics for more information.
    /// </summary>
    [JsonObject]
    internal class FollowerGainStatisticsData
    {
        /// <summary>
        /// Number of gained organic followers.
        /// </summary>
        [JsonProperty("organicFollowerGain")]
        public int OrganicFollowerGain
        { 
            get; 
            set; 
        }


        /// <summary>
        /// Number of gained paid followers.
        /// </summary>
        [JsonProperty("paidFollowerGain")]
        public int PaidFollowerGain
        {
            get;
            set;
        }
    }
}
