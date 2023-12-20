using Newtonsoft.Json;

namespace CMS.SocialMarketing.LinkedInInternal
{
    /// <summary>
    /// Share statistics data. 
    /// See https://docs.microsoft.com/en-us/linkedin/marketing/integrations/community-management/organizations/share-statistics#share-statistics-data-schema for more information.
    /// </summary>
    [JsonObject]
    internal class ShareStatisticsData
    {
        /// <summary>
        /// Number of clicks.
        /// </summary>
        [JsonProperty("clickCount")]
        public int ClickCount
        {
            get;
            set;
        }


        /// <summary>
        /// Number of comments.
        /// </summary>
        [JsonProperty("commentCount")]
        public int CommentCount
        {
            get;
            set;
        }


        /// <summary>
        /// Number of mentions of the organizational entity in a comment across LinkedIn. The field does not have a value when individual share statistics are requested.
        /// </summary>
        [JsonProperty("commentMentionsCount")]
        public int CommentMentionsCount
        {
            get;
            set;
        }


        /// <summary>
        /// Engagement - Number of organic clicks, likes, comments, and shares over impressions [%].
        /// </summary>
        [JsonProperty("engagement")]
        public double Engagement
        {
            get;
            set;
        }


        /// <summary>
        /// Number of impressions.
        /// </summary>
        [JsonProperty("impressionCount")]
        public int ImpressionCount
        {
            get;
            set;
        }


        /// <summary>
        /// Number of likes. This field can become negative when members who liked a sponsored share later unlike it. The like is not counted since it is not organic, but the unlike is counted as organic.
        /// </summary>
        [JsonProperty("likeCount")]
        public int LikeCount
        {
            get;
            set;
        }


        /// <summary>
        /// Number of shares.
        /// </summary>
        [JsonProperty("shareCount")]
        public int ShareCount
        {
            get;
            set;
        }


        /// <summary>
        /// Number of mentions of the organizational entity in a share across LinkedIn. The field does not have a value when individual share statistics are requested.
        /// </summary>
        [JsonProperty("shareMentionsCount")]
        public int ShareMentionsCount
        {
            get;
            set;
        }


        /// <summary>
        /// Count of unique member impressions.
        /// </summary>
        [JsonProperty("uniqueImpressionsCount")]
        public int UniqueImpressionsCount
        {
            get;
            set;
        }
    }
}
