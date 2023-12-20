using Newtonsoft.Json;

namespace CMS.SocialMarketing.LinkedInInternal
{
    /// <summary>
    /// Provides additional options while defining the content of the share.
    /// </summary>
    [JsonObject]
    internal class SpecificContent
    {
        /// <summary>
        /// The content of the share.
        /// </summary>
        [JsonProperty("com.linkedin.ugc.ShareContent", Required = Required.Always)]
        public ShareContent ShareContent
        {
            get;
            set;
        }
    }
}
