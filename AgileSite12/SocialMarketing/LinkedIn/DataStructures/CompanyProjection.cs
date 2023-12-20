using Newtonsoft.Json;

namespace CMS.SocialMarketing.LinkedInInternal
{
    /// <summary>
    /// Represents a LinkedIn company projection.
    /// </summary>
    [JsonObject]
    internal class CompanyProjection
    {
        /// <summary>
        /// Company.
        /// </summary>
        [JsonProperty("organization~", Required = Required.Always)]
#pragma warning disable CS0618 // Type or member is obsolete
        public Company Company
#pragma warning restore CS0618 // Type or member is obsolete
        {
            get;
            set;
        }


        /// <summary>
        /// Company URN.
        /// </summary>
        [JsonProperty("organization", Required = Required.Always)]
        public string CompanyURN
        {
            get;
            set;
        }
    }
}
