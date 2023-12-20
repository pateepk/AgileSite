using System;

namespace CMS.SocialMarketing
{
    /// <summary>
    /// Represents information about a Facebook page access token.
    /// </summary>
    public struct FacebookPageAccessTokenData
    {
        /// <summary>
        /// Access token.
        /// </summary>
        public readonly string AccessToken;


        /// <summary>
        /// Date and time when the access token expires (UTC).
        /// </summary>
        public readonly DateTime? Expiration;


        /// <summary>
        /// Initializes a new instance of the PageAccessTokenData structure.
        /// </summary>
        /// <param name="accessToken">Access token.</param>
        /// <param name="expiration">Date and time when the access token expires.</param>
        public FacebookPageAccessTokenData(string accessToken, DateTime? expiration)
        {
            AccessToken = accessToken;
            Expiration = expiration;
        }

    }
}
