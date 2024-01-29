using System;
using System.Collections.Generic;

using DotNetOpenAuth.OAuth.ChannelElements;
using DotNetOpenAuth.OAuth.Messages;

namespace CMS.ExternalAuthentication
{
    /// <summary>
    /// TokenManager class
    /// </summary>
    [Serializable]
    public class TokenManager : IConsumerTokenManager
    {
        #region "Private variables"

        private Dictionary<string, string> mTokens;

        #endregion


        #region "Properties"

        /// <summary>
        /// Consumer key
        /// </summary>
        public string ConsumerKey
        {
            get;
            private set;
        }


        /// <summary>
        /// Consumer secret
        /// </summary>
        public string ConsumerSecret
        {
            get;
            private set;
        }


        /// <summary>
        /// All tokens and their secrets contained in token manager
        /// </summary>
        public Dictionary<string, string> Tokens
        {
            get
            {
                if (mTokens == null)
                {
                    mTokens = new Dictionary<string, string>();
                }

                return mTokens;
            }
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// TokenManager constructor
        /// </summary>
        /// <param name="consumerKey">Consumer key.</param>
        /// <param name="consumerSecret">Consumer secret.</param>
        public TokenManager(string consumerKey, string consumerSecret)
        {
            ConsumerKey = consumerKey;
            ConsumerSecret = consumerSecret;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Adds token and its secret to the manager.
        /// </summary>
        /// <param name="token">Token.</param>
        /// <param name="tokenSecret">Token secret.</param>
        public void AddToken(string token, string tokenSecret)
        {
            Tokens.Add(token, tokenSecret);
        }

        #endregion


        #region "ITokenManager Members"

        /// <summary>
        /// Gets the Token Secret given a request or access token.
        /// </summary>
        /// <param name="token">The request or access token.</param>
        public string GetTokenSecret(string token)
        {
            return Tokens[token];
        }


        /// <summary>
        /// Stores a newly generated unauthorized request token, secret, and optional
        /// application-specific parameters for later recall.
        /// </summary>
        /// <param name="request">The request message that resulted in the generation of a new unauthorized request token.</param>
        /// <param name="response">The response message that includes the unauthorized request token.</param>
        public void StoreNewRequestToken(UnauthorizedTokenRequest request, ITokenSecretContainingMessage response)
        {
            Tokens[response.Token] = response.TokenSecret;
        }


        /// <summary>
        /// Deletes a request token and its associated secret and stores a new access
        /// token and secret.
        /// </summary>
        /// <param name="consumerKey">The Consumer that is exchanging its request token for an access token.</param>
        /// <param name="requestToken">The Consumer's request token that should be deleted/expired.</param>
        /// <param name="accessToken">The new access token that is being issued to the Consumer.</param>
        /// <param name="accessTokenSecret">The secret associated with the newly issued access token.</param>
        public void ExpireRequestTokenAndStoreNewAccessToken(string consumerKey, string requestToken, string accessToken, string accessTokenSecret)
        {
            Tokens.Remove(requestToken);
            Tokens[accessToken] = accessTokenSecret;
        }


        /// <summary>
        /// Classifies a token as a request token or an access token.
        /// </summary>
        /// <param name="token">The token to classify.</param>
        public TokenType GetTokenType(string token)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
