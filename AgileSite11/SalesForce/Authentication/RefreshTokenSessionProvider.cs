﻿using System.Collections.Specialized;
using System.Net;
using System.Runtime.Serialization.Json;

using CMS.Helpers;

using SystemIO = System.IO;

namespace CMS.SalesForce
{

    /// <summary>
    /// Provides SalesForce organization sessions using the OAuth refresh token.
    /// </summary>
    public sealed class RefreshTokenSessionProvider : ISessionProvider
    {

        #region "Public properties"

        /// <summary>
        /// Gets or sets the consumer identifier associated with the remote access application in SalesForce.
        /// </summary>
        public string ClientId { get; set; }

        /// <summary>
        /// Gets or sets the consumer secret associated with the remote access application in SalesForce.
        /// </summary>
        public string ClientSecret { get; set; }

        /// <summary>
        /// Gets or sets the OAuth refresh token.
        /// </summary>
        public string RefreshToken { get; set; }

        #endregion

        #region "Public methods"

        /// <summary>
        /// Creates a new SalesForce organization session, and returns it.
        /// </summary>
        /// <returns>A new SalesForce organization session.</returns>
        public Session CreateSession()
        {
            RestContract.GetAuthenticationTokensResponse response = GetAuthenticationTokens();
            RestContract.Identity identity = GetIdentity(response);

            return new Session(identity)
            {
                AccessToken = response.AccessToken,
                OrganizationBaseUrl = response.InstanceBaseUrl
            };
        }

        #endregion

        #region "Private methods"

        private RestContract.GetAuthenticationTokensResponse GetAuthenticationTokens()
        {
            WebClient client = new WebClient();
            NameValueCollection parameters = new NameValueCollection();
            parameters.Add("grant_type", "refresh_token");
            parameters.Add("client_id", ClientId);
            parameters.Add("client_secret", ClientSecret);
            parameters.Add("refresh_token", RefreshToken);
            parameters.Add("format", "json");
            try
            {
                byte[] data = client.UploadValues("https://login.salesforce.com/services/oauth2/token", "POST", parameters);
                return Read<RestContract.GetAuthenticationTokensResponse>(data);
            }
            catch (WebException exception)
            {
                if (exception.Status == WebExceptionStatus.ProtocolError)
                {
                    using (var stream = exception.Response.GetResponseStream())
                    {
                        RestContract.AuthorizationError error = null;
                        DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(RestContract.AuthorizationError));
                        try
                        {
                            error = (RestContract.AuthorizationError)serializer.ReadObject(stream);
                        }
                        catch
                        {
                            throw new SalesForceException(ResHelper.GetStringFormat("sf.communicationexception", exception.Message));
                        }
                        throw new SalesForceException(ResHelper.GetStringFormat("sf.authorizationexception", error.Description, error.Code));
                    }
                }
                throw;
            }
        }

        private RestContract.Identity GetIdentity(RestContract.GetAuthenticationTokensResponse response)
        {
            WebClient client = new WebClient();
            client.Headers.Add("Authorization", "OAuth " + response.AccessToken);
            byte[] data = client.DownloadData(response.IdentityUrl);

            return Read<RestContract.Identity>(data);
        }

        private T Read<T>(byte[] data)
        {
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(T));
            var stream = new SystemIO.MemoryStream(data);

            return (T)serializer.ReadObject(stream);
        }

        #endregion

    }

}