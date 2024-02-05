using Newtonsoft.Json;

namespace CMS.ExternalAuthentication
{
    /// <summary>
    /// Captures the result of an access token request
    /// (see RFC 6749 section 5 "Issuing an access token" for more info).
    /// </summary>
    internal class OAuth2AccessTokenData
    {
        /// <summary>
        /// The access token issued by the authorization server.
        /// </summary>
        [JsonProperty("access_token")]
        public string AccessToken
        {
            get;
            set;
        }


        /// <summary>
        /// The type of the access token.
        /// </summary>
        [JsonProperty("token_type")]
        public string TokenType
        {
            get;
            set;
        }


        /// <summary>
        /// The duration in seconds of the access token lifetime.
        /// </summary>
        [JsonProperty("expires_in")]
        public string Expires
        {
            get;
            set;
        }


        /// <summary>
        /// The refresh token, which can be used to obtain new access tokens.
        /// </summary>
        [JsonProperty("refresh_token")]
        public string State
        {
            get;
            set;
        }


        /// <summary>
        /// The scope of the access request expressed as a list of space-delimited, case sensitive strings.
        /// </summary>
        [JsonProperty("scope")]
        public string Scope
        {
            get;
            set;
        }
    }
}
