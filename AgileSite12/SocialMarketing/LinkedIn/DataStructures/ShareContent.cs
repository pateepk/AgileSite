using Newtonsoft.Json;

namespace CMS.SocialMarketing.LinkedInInternal
{
    /// <summary>
    /// Represents the content of the share. 
    /// See https://docs.microsoft.com/en-us/linkedin/consumer/integrations/self-serve/share-on-linkedin#share-content for more information.
    /// </summary>
    [JsonObject]
    internal class ShareContent
    {
        /// <summary>
        /// Provides the primary content for the share.
        /// </summary>
        [JsonProperty("shareCommentary", Required = Required.Always)]
        public ShareCommentary ShareCommentary 
        { 
            get; 
            set; 
        }


        /// <summary>
        /// Represents the media assets attached to the share. 
        /// For Kentico integration, the only viable option is "NONE" - the share does not contain any media, and will only consist of text.
        /// </summary>
        [JsonProperty("shareMediaCategory", Required = Required.Always)]
        public string ShareMediaCategory
        {
            get;
            set;
        }
    }
}
