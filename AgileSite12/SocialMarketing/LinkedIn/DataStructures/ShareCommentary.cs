using Newtonsoft.Json;

namespace CMS.SocialMarketing.LinkedInInternal
{
    /// <summary>
    /// Represents the message content of the share.
    /// </summary>
    [JsonObject]
    internal class ShareCommentary
    {
        /// <summary>
        /// Text of share commentary.
        /// </summary>
        [JsonProperty("text", Required = Required.Always)]
        public string Text 
        { 
            get; 
            set; 
        }
    }
}
