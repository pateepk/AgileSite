using System;

namespace CMS.ExternalAuthentication.LinkedIn
{
    /// <summary>
    /// Represents information about a LinkedIn user access token and its expiration date.
    /// </summary>
    public sealed class LinkedInAccessToken
    {
        /// <summary>
        /// Access token.
        /// </summary>
        public string AccessToken
        {
            get;
        }


        /// <summary>
        /// Date and time when the access token expires (UTC).
        /// </summary>
        public DateTime? Expiration
        {
            get;
        }


        /// <summary>
        /// Initializes a new instance of the LinkedInAccessToken structure.
        /// </summary>
        /// <param name="accessToken">Access token.</param>
        /// <param name="expiration">Date and time when the access token expires.</param>
        public LinkedInAccessToken(string accessToken, DateTime? expiration)
        {
            AccessToken = accessToken;
            Expiration = expiration;
        }
    }
}
