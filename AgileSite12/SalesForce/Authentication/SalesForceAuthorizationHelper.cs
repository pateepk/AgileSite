using System;
using System.Collections.Specialized;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Web;

using SystemIO = System.IO;

namespace CMS.SalesForce
{

    /// <summary>
    /// Provides helper methods for SalesForce OAuth implementation.
    /// </summary>
    public sealed class SalesForceAuthorizationHelper
    {

        #region "Private properties"

        private string mClientId;
        private string mClientSecret;
        private string mRedirectUrl;

        #endregion

        #region "Constructors"

        /// <summary>
        /// Initializes a new instance of the SalesForceAuthorizationHelper class using the specified parameters.
        /// </summary>
        /// <param name="clientId">The consumer identifier associated with the remote access application in SalesForce.</param>
        /// <param name="clientSecret">The consumer secret associated with the remote access application in SalesForce.</param>
        /// <param name="redirectUrl">The redirect URL for the OAuth 2.0 web server authentication flow.</param>
        public SalesForceAuthorizationHelper(string clientId, string clientSecret, string redirectUrl)
        {
            mClientId = clientId;
            mClientSecret = clientSecret;
            mRedirectUrl = redirectUrl;
        }

        #endregion

        #region "Public methods"

        /// <summary>
        /// Gets the authorization URL for the first step in the OAuth 2.0 web server authentication flow.
        /// </summary>
        /// <param name="state">A string value representing a state that will be kept during the authorization.</param>
        /// <returns>The authorization URL for the first step in the OAuth 2.0 web server authentication flow.</returns>
        public string GetAuthorizationUrl(string state)
        {
            return String.Format("{0}?response_type=code&client_id={1}&redirect_uri={2}&state={3}", SalesForceUrlHelper.AuthorizePath, HttpUtility.UrlEncode(mClientId), HttpUtility.UrlEncode(mRedirectUrl), HttpUtility.UrlEncode(state));
        }

        /// <summary>
        /// Authorizes remote access to SalesForce organization using the specified authorization code, and returns the result.
        /// </summary>
        /// <param name="authorizationCode">The authorization code retrieved in the first step in the OAuth 2.0 web server authentication flow.</param>
        /// <returns>The result of the remote access authorization.</returns>
        public RestContract.GetAuthenticationTokensResponse GetAuthenticationTokens(string authorizationCode)
        {
            WebClient client = new WebClient();
            NameValueCollection parameters = new NameValueCollection();
            parameters.Add("code", authorizationCode);
            parameters.Add("grant_type", "authorization_code");
            parameters.Add("client_id", mClientId);
            parameters.Add("client_secret", mClientSecret);
            parameters.Add("redirect_uri", mRedirectUrl);
            parameters.Add("format", "json");
            byte[] data = client.UploadValues(SalesForceUrlHelper.TokenPath, "POST", parameters);

            return Read<RestContract.GetAuthenticationTokensResponse>(data);
        }

        /// <summary>
        /// Retrieves the identity associated with the specified access token.
        /// </summary>
        /// <param name="response">The successful authorization response.</param>
        /// <returns>An instance of the SalesForce organization communication session.</returns>
        public RestContract.Identity GetIdentity(RestContract.GetAuthenticationTokensResponse response)
        {
            WebClient client = new WebClient();
            client.Headers.Add("Authorization", "OAuth " + response.AccessToken);
            byte[] data = client.DownloadData(response.IdentityUrl);

            return Read<RestContract.Identity>(data);
        }

        #endregion

        #region "Private methods"

        private T Read<T>(byte[] data)
        {
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(T));
            var stream = new SystemIO.MemoryStream(data);

            return (T)serializer.ReadObject(stream);
        }

        #endregion

    }

}