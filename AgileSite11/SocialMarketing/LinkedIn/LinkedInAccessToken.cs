using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CMS.SocialMarketing
{
    /// <summary>
    /// Represents information about a LinkedIn user access token.
    /// </summary>
    public struct LinkedInAccessToken
    {

        /// <summary>
        /// Access token.
        /// </summary>
        public readonly string AccessToken;


        /// <summary>
        /// Access token secret.
        /// </summary>
        public readonly string AccessTokenSecret;


        /// <summary>
        /// Date and time when the access token expires (UTC).
        /// </summary>
        public readonly DateTime? Expiration;


        /// <summary>
        /// Initializes a new instance of the LinkedInAccessToken structure.
        /// </summary>
        /// <param name="accessToken">Access token.</param>
        /// <param name="accessTokenSecret">Access token secret.</param>
        /// <param name="expiration">Date and time when the access token expires.</param>
        public LinkedInAccessToken(string accessToken, string accessTokenSecret, DateTime? expiration)
        {
            AccessToken = accessToken;
            AccessTokenSecret = accessTokenSecret;
            Expiration = expiration;
        }

    }
}
