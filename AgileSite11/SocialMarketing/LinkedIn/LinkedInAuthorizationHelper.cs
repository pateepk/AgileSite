using System;

using CMS.Base;
using CMS.SocialMarketing.LinkedInInternal;

namespace CMS.SocialMarketing
{
    /// <summary>
    /// Provides methods for obtaining authorization for LinkedIn API querying.
    /// </summary>
    internal class LinkedInAuthorizationHelper : AbstractHelper<LinkedInAuthorizationHelper>
    {
        #region "Public methods"

        /// <summary>
        /// Creates authorization for LinkedIn API querying. The necessary OAuth keys may be retrieved from <see cref="LinkedInApplicationInfo"/> and <see cref="LinkedInAccountInfo"/>,
        /// or directly from LinkedIn developer's site.
        /// </summary>
        /// <param name="consumerKey">Consumer key (obtained from <see cref="LinkedInApplicationInfo"/> or directly from LinkedIn - also known as API Key)</param>
        /// <param name="consumerSecret">Consumer key (obtained from <see cref="LinkedInApplicationInfo"/> or directly from LinkedIn - also known as Secret Key)</param>
        /// <param name="accessToken">Access token (obtained from <see cref="LinkedInAccountInfo"/> or directly from LinkedIn - also known as OAuth User Token)</param>
        /// <param name="accessTokenSecret">Access token secret (obtained from <see cref="LinkedInAccountInfo"/> or directly from LinkedIn - also known as OAuth User Secret)</param>
        /// <returns>Authorization object created from given OAuth keys.</returns>
        /// <exception cref="ArgumentNullException">Thrown when accessToken is null.</exception>
        public static LinkedInAuthorization CreateAuthorization(string consumerKey, string consumerSecret, string accessToken, string accessTokenSecret)
        {
            return HelperObject.CreateAuthorizationInternal(consumerKey, consumerSecret, accessToken, accessTokenSecret);
        }


        /// <summary>
        /// Gets authorization for LinkedIn API querying.
        /// </summary>
        /// <param name="accountInfo"><see cref="LinkedInAccountInfo"/></param>
        /// <returns>Authorization object, or null if accountInfo is null or it's <see cref="LinkedInAccountInfo.LinkedInAccountLinkedInApplicationID"/> is not valid.</returns>
        /// <exception cref="ArgumentNullException">Thrown when access token of corresponding account is null.</exception>
        public static LinkedInAuthorization GetAuthorizationByAccountInfo(LinkedInAccountInfo accountInfo)
        {
            return HelperObject.GetAuthorizationByAccountInfoInternal(accountInfo);
        }

        #endregion


        #region "Protected methods"

        /// <summary>
        /// Creates authorization for LinkedIn API querying. The necessary OAuth keys may be retrieved from <see cref="LinkedInApplicationInfo"/> and <see cref="LinkedInAccountInfo"/>,
        /// or directly from LinkedIn developer's site.
        /// </summary>
        /// <param name="consumerKey">Consumer key (obtained from <see cref="LinkedInApplicationInfo"/> or directly from LinkedIn - also known as API Key)</param>
        /// <param name="consumerSecret">Consumer key (obtained from <see cref="LinkedInApplicationInfo"/> or directly from LinkedIn - also known as Secret Key)</param>
        /// <param name="accessToken">Access token (obtained from <see cref="LinkedInAccountInfo"/> or directly from LinkedIn - also known as OAuth User Token)</param>
        /// <param name="accessTokenSecret">Access token secret (obtained from <see cref="LinkedInAccountInfo"/> or directly from LinkedIn - also known as OAuth User Secret)</param>
        /// <returns>Authorization object created from given OAuth keys.</returns>
        /// <exception cref="ArgumentNullException">Thrown when accessToken is null.</exception>
        protected virtual LinkedInAuthorization CreateAuthorizationInternal(string consumerKey, string consumerSecret, string accessToken, string accessTokenSecret)
        {
            TokenManager tokenManager = new TokenManager(consumerKey, consumerSecret);
            tokenManager.AddToken(accessToken, accessTokenSecret);
            LinkedInAuthorization auth = new LinkedInAuthorization(tokenManager, accessToken);

            return auth;
        }


        /// <summary>
        /// Gets authorization for LinkedIn API querying by <see cref="LinkedInAccountInfo.LinkedInAccountID"/>.
        /// </summary>
        /// <param name="accountInfo"><see cref="LinkedInAccountInfo"/></param>
        /// <returns>Authorization object, or null if accountInfo is null or it's <see cref="LinkedInAccountInfo.LinkedInAccountLinkedInApplicationID"/> is not valid.</returns>
        /// <exception cref="ArgumentNullException">Thrown when access token of corresponding account is null.</exception>
        protected virtual LinkedInAuthorization GetAuthorizationByAccountInfoInternal(LinkedInAccountInfo accountInfo)
        {
            if (accountInfo == null)
            {
                return null;
            }
            LinkedInApplicationInfo applicationInfo = LinkedInApplicationInfoProvider.GetLinkedInApplicationInfo(accountInfo.LinkedInAccountLinkedInApplicationID);
            if (applicationInfo == null)
            {
                return null;
            }

            return CreateAuthorization(applicationInfo.LinkedInApplicationConsumerKey, applicationInfo.LinkedInApplicationConsumerSecret, accountInfo.LinkedInAccountAccessToken, accountInfo.LinkedInAccountAccessTokenSecret);
        }

        #endregion
    }
}
