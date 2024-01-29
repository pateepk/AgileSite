using System;
using System.Collections.Specialized;
using System.Net;
using System.Web;

using CMS.Helpers;
using CMS.IO;

using Newtonsoft.Json.Linq;

namespace CMS.ExternalAuthentication.LinkedIn
{
    /// <summary>
    /// Provides authorization methods for LinkedIn API.
    /// </summary>
    [Serializable]
    public class LinkedInAuthorization : IDisposable
    {
        private readonly TokenManager mTokenManager;
        private readonly Uri mReturnUrl;


        /// <summary>
        /// Access token.
        /// </summary>
        public string AccessToken
        {
            get;
            private set;
        }


        /// <summary>
        /// Creates new instance of <see cref="LinkedInAuthorization"/>.
        /// </summary>
        /// <param name="tokenManager">Token manager.</param>
        /// <param name="accessToken">Access token.</param>
        /// <param name="returnUrl">Return url.</param>
        public LinkedInAuthorization(TokenManager tokenManager, string accessToken, Uri returnUrl = null) 
        {
            mTokenManager = tokenManager;
            AccessToken = accessToken;
            mReturnUrl = returnUrl;
        }


        /// <summary>
        /// Prepares GET request that has OAuth authorization already attached to it.
        /// </summary>
        /// <param name="requestUrl">Request URL</param>
        [Obsolete]

        public HttpWebRequest PrepareGetRequest(Uri requestUrl)
        {
            return null;
        }


        /// <summary>
        /// Prepares POST request that has OAuth authorization already attached to it.
        /// </summary>
        /// <param name="requestUrl">Request URL</param>
        [Obsolete]
        public HttpWebRequest PreparePostRequest(Uri requestUrl)
        {
            return null;
        }


        /// <summary>
        /// Prepares a request for user authorization from an authorization server
        /// </summary>
        /// <param name="returnUrl">Return URL.</param>
        public void BeginAuthorize(Uri returnUrl)
        {
            var url = LinkedInHelper.AUTHORIZATION_ENDPOINT;
            url = URLHelper.AddParameterToUrl(url, "response_type", "code");
            url = URLHelper.AddParameterToUrl(url, "client_id", mTokenManager.ConsumerKey);
            url = URLHelper.AddParameterToUrl(url, "redirect_uri", HttpUtility.UrlEncode(returnUrl.ToString()));
            url = URLHelper.AddParameterToUrl(url, "state", Guid.NewGuid().ToString());

            URLHelper.Redirect(url);
        }


        /// <summary>
        /// Completes process authorization,
        /// </summary>
        /// <returns>Access token</returns>
        public string CompleteAuthorize()
        {
            var data = HttpUtility.ParseQueryString(String.Empty);
            data.Add(new NameValueCollection
            {
                { "grant_type", "authorization_code" },
                { "code", QueryHelper.GetString("code", String.Empty) },
                { "redirect_uri", mReturnUrl.ToString() },
                { "client_id", mTokenManager.ConsumerKey },
                { "client_secret", mTokenManager.ConsumerSecret }
            });

            var webRequest = (HttpWebRequest)WebRequest.Create(LinkedInHelper.TOKEN_ENDPOINT);
            webRequest.Method = "POST";
            webRequest.ContentType = "application/x-www-form-urlencoded";

            using (var stream = webRequest.GetRequestStream())
            {
                using (var writer = StreamWriter.New(stream))
                {
                    writer.Write(data.ToString());
                }
            }

            using (var webResponse = webRequest.GetResponse())
            {
                var responseStream = webResponse.GetResponseStream();
                if (responseStream == null)
                {
                    return null;
                }

                using (var reader = StreamReader.New(responseStream))
                {
                    var response = reader.ReadToEnd();
                    var json = JObject.Parse(response);
                    var accessToken = json.Value<string>("access_token");
                    AccessToken = accessToken;

                    return accessToken;
                }
            }
        }


        /// <summary>
        /// Disposes the object.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }


        /// <summary>
        /// Disposes the object.
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
        }
    }
}
