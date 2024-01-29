using System;
using System.Collections.Generic;

using CMS.DataEngine;
using CMS.SiteProvider;

using DotNetOpenAuth.OAuth.ChannelElements;
using DotNetOpenAuth.OAuth.Messages;

namespace CMS.ExternalAuthentication
{
    /// <summary>
    /// InMemoryTokenManager class.
    /// </summary>
    [Serializable]
    public class InMemoryTokenManager : IConsumerTokenManager
    {
        #region "Private variables"

        private Dictionary<string, string> tokensAndSecrets = new Dictionary<string, string>();
        private string mConsumerKey = "";
        private string mConsumerSecret = "";

        #endregion


        #region "Public properties"

        /// <summary>
        /// LinkedIn Api key.
        /// </summary>
        public string ConsumerKey
        {
            get
            {
                if (String.IsNullOrEmpty(mConsumerKey))
                {
                    mConsumerKey = SettingsKeyInfoProvider.GetValue(SiteContext.CurrentSiteName + ".CMSLinkedInApiKey");
                }
                return mConsumerKey;
            }
        }


        /// <summary>
        /// LinkedIn application secret.
        /// </summary>
        public string ConsumerSecret
        {
            get
            {
                if (String.IsNullOrEmpty(mConsumerSecret))
                {
                    mConsumerSecret = SettingsKeyInfoProvider.GetValue(SiteContext.CurrentSiteName + ".CMSLinkedInApplicationSecret");
                }
                return mConsumerSecret;
            }
        }

        #endregion


        #region "Private methods"

        /// <summary>
        /// Returns consumer secret.
        /// </summary>
        /// <param name="consumerKey">Api key</param>
        public string GetConsumerSecret(string consumerKey)
        {
            if (consumerKey == ConsumerKey)
            {
                return ConsumerSecret;
            }
            else
            {
                return "";
            }
        }


        /// <summary>
        /// Returns token secret.
        /// </summary>
        /// <param name="token">Auth token</param>
        public string GetTokenSecret(string token)
        {
            return tokensAndSecrets[token];
        }


        /// <summary>
        /// Stores new request token.
        /// </summary>
        /// <param name="request">Request as UnauthorizedTokenRequest object</param>
        /// <param name="response">Response as ITokenSecretContainingMessage object</param>
        public void StoreNewRequestToken(UnauthorizedTokenRequest request, ITokenSecretContainingMessage response)
        {
            tokensAndSecrets[response.Token] = response.TokenSecret;
        }


        /// <summary>
        /// Expires request token and stores new access token.
        /// </summary>
        /// <param name="consumerKey">Api key</param>
        /// <param name="requestToken">Request token</param>
        /// <param name="accessToken">Access token</param>
        /// <param name="accessTokenSecret">Acess token secret</param>
        public void ExpireRequestTokenAndStoreNewAccessToken(string consumerKey, string requestToken, string accessToken, string accessTokenSecret)
        {
            tokensAndSecrets.Remove(requestToken);
            tokensAndSecrets[accessToken] = accessTokenSecret;
        }


        /// <summary>
        /// Classifies a token as a request token or an access token. Returns Request or Access token, or invalid if the token is not recognized.
        /// </summary>
        /// <param name="token">The token to classify</param>
        public TokenType GetTokenType(string token)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}