using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

namespace CMS.SocialMarketing
{
    /// <summary>
    /// Represents a response for an OAuth access token request.
    /// </summary>
    /// <remarks>
    /// The response has be following format:
    /// <code>
    /// {
    ///     "access_token": {access-token}, 
    ///     "token_type": {type},
    ///     "expires_in":  {seconds-til-expiration}
    /// }
    /// </code>
    /// See https://developers.facebook.com/docs/facebook-login/manually-build-a-login-flow#exchangecode for details.
    /// </remarks>
    public class OAuthAccessToken
    {
        /// <summary>
        /// Gets or sets the access token.
        /// </summary>
        [JsonProperty(propertyName: "access_token")]
        public string AccessToken
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the access token type.
        /// </summary>
        [JsonProperty(propertyName: "token_type")]
        public string TokenType
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the number of seconds till the access token expires.
        /// </summary>
        /// <remarks>
        /// The token expiration value is 0 for access tokens which never expire.
        /// </remarks>
        [JsonProperty(propertyName: "expires_in")]
        public int ExpiresIn
        {
            get;
            set;
        }
    }
}
