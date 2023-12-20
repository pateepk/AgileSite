using Newtonsoft.Json;

namespace CMS.SocialMarketing.LinkedInInternal
{
    /// <summary>
    /// Represents the time range for statistics.
    /// </summary>
    [JsonObject]
    internal class TimeRange
    {
        /// <summary>
        /// Exclusive starting timestamp of when the query should begin (milliseconds since epoch) Queries from beginning of time when not set.
        /// </summary>
        [JsonProperty("start")]
        public long Start
        {
            get;
            set;
        }


        /// <summary>
        /// Inclusive ending timestamp of when the query should end (milliseconds since epoch). Queries until current time when not set.
        /// </summary>
        [JsonProperty("end")]
        public long End
        {
            get;
            set;
        }
    }
}
