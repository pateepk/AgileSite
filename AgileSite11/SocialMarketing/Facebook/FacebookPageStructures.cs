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


    /// <summary>
    /// Represents information about a Facebook page.
    /// </summary>
    [Obsolete("Structure will be removed in the next version. The Facebook page ID has to be set manually.")]
    public struct FacebookPageIdentityData
    {

        /// <summary>
        /// Page URL.
        /// </summary>
        public readonly string PageUrl;


        /// <summary>
        /// Page identifier.
        /// </summary>
        public readonly string PageId;


        /// <summary>
        /// Initializes a new instance of the PageIdentityData structure.
        /// </summary>
        /// <param name="url">Page URL.</param>
        /// <param name="id">Page identifier.</param>
        public FacebookPageIdentityData(string url, string id)
        {
            PageUrl = url;
            PageId = id;
        }

    }
}
