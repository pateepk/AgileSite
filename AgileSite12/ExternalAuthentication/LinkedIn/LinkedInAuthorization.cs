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
    internal class LinkedInAuthorization
    {
        /// <summary>
        /// The authorization endpoint.
        /// </summary>
        private const string AUTHORIZATION_ENDPOINT = "https://www.linkedin.com/oauth/v2/authorization";


        /// <summary>
        /// The access token endpoint.
        /// </summary>
        private const string TOKEN_ENDPOINT = "https://www.linkedin.com/oauth/v2/accessToken";


        private readonly ILinkedInData mData;


        /// <summary>
        /// Creates a new instance of <see cref="LinkedInAuthorization"/>.
        /// </summary>
        /// <param name="data">Data required for LinkedIn authorization.</param>
        /// <throws>
        /// <see cref="ArgumentNullException"/> when data is null.
        /// </throws>
        public LinkedInAuthorization(ILinkedInData data)
        {
            mData = data ?? throw new ArgumentNullException(nameof(data));
        }


        /// <summary>
        /// Performs authorization request to LinkedIn API.
        /// </summary>
        /// <param name="returnUrl">Url used for redirect once the LinkedIn access token request is received.</param>
        /// <seealso cref="OpenAuthorizationPage"/>
        /// <returns>Instance of <see cref="LinkedInAccessToken"/> if request is valid.</returns>
        /// <throws>Exception when request in invalid.</throws>
        public LinkedInAccessToken Authorize(Uri returnUrl)
        {
            var data = HttpUtility.ParseQueryString(String.Empty);
            data.Add(new NameValueCollection
            {
                { "grant_type", "authorization_code" },
                { "code", mData.Code },
                { "redirect_uri", returnUrl.ToString() },
                { "client_id", mData.ApiKey },
                { "client_secret", mData.ApiSecret }
            });

            var webRequest = (HttpWebRequest)WebRequest.Create(TOKEN_ENDPOINT);
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
                    var expiresIn = json.Value<int>("expires_in");

                    return new LinkedInAccessToken(accessToken, DateTime.Now.AddSeconds(expiresIn));
                }
            }
        }


        /// <summary>
        /// Performs redirect to LinkedIn login page.
        /// </summary>
        /// <param name="returnUrl">Url used for redirect once the LinkedIn login form is submitted.</param>
        public void OpenAuthorizationPage(Uri returnUrl)
        {
            var url = AUTHORIZATION_ENDPOINT;
            url = URLHelper.AddParameterToUrl(url, "response_type", "code");
            url = URLHelper.AddParameterToUrl(url, "client_id", mData.ApiKey);
            url = URLHelper.AddParameterToUrl(url, "redirect_uri", HttpUtility.UrlEncode(returnUrl.ToString()));
            url = URLHelper.AddParameterToUrl(url, "state", Guid.NewGuid().ToString());

            foreach (var parameter in mData.AdditionalQueryParameters)
            {
                url = URLHelper.AddParameterToUrl(url, parameter.Key, HttpUtility.UrlEncode(parameter.Value));
            }

            URLHelper.Redirect(url);
        }
    }
}
