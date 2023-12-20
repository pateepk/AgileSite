using Newtonsoft.Json;

namespace CMS.SocialMarketing.LinkedInInternal
{
    [JsonObject]
    internal class NumberOfFollowersResponse
    {
        [JsonProperty("firstDegreeSize")]
        public int NumberOfFollowers { get; set; }
    }
}
