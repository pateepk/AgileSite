using System;
using System.Net;
using System.Net.Cache;
using System.Text;

using CMS.DataEngine;

using Newtonsoft.Json.Linq;

namespace CMS.SocialMarketing.URLShortening
{
    internal sealed class GooGlShortener : IURLShortener
    {
        #region "Properties, constants & fields"

        private const string GOOGL_APIKEY_SETTINGSKEY = "CMSGooglAPIKey";
        private const string GOOGL_URL_FORMAT = "https://www.googleapis.com/urlshortener/v1/url?key={0}";

        private IURLShortenerHelperEnvironment mEnvironment = null;
        private IURLShortenerHelperEnvironment ShortenerEnvronment
        {
            get
            {
                if (mEnvironment == null)
                {
                    mEnvironment = new URLShortenerHelperEnvironment();
                }

                return mEnvironment;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Public default constructor
        /// </summary>
        public GooGlShortener()
        {
            
        }


        /// <summary>
        /// Internal constructor for testing purposes
        /// </summary>
        /// <param name="env">Environment mock</param>
        internal GooGlShortener(IURLShortenerHelperEnvironment env)
        {
            mEnvironment = env;
        }


        /// <summary>
        /// Determines whether shortener is available
        /// </summary>
        /// <param name="site">Site for whom availability you want to know</param>
        /// <returns>True if available.</returns>
        public bool IsAvailable(SiteInfoIdentifier site)
        {
            string key = ShortenerEnvronment.GetStringSettingValue(GOOGL_APIKEY_SETTINGSKEY, site);
            if (!String.IsNullOrEmpty(key))
            {
                return true;
            }

            return false;
        }


        /// <summary>
        /// Shortens the given URL via Goo.gl
        /// </summary>
        /// <param name="url">URL to be shortened</param>
        /// <param name="site">Context to get settings for</param>
        /// <returns>Shortened URL on success, <paramref name="url"/> otherwise.</returns>
        public string Shorten(string url, SiteInfoIdentifier site)
        {
            string apiKey = ShortenerEnvronment.GetStringSettingValue(GOOGL_APIKEY_SETTINGSKEY, site);
            string message = new JObject{ {"longUrl", url} }.ToString();

            try
            {
                WebClient webClient = new WebClient
                {
                    CachePolicy = new RequestCachePolicy(RequestCacheLevel.NoCacheNoStore),
                    Encoding = Encoding.ASCII,
                    Headers = new WebHeaderCollection()
                    {
                        {"Content-Type","application/json"},
                    },
                };
                string response = webClient.UploadString(String.Format(GOOGL_URL_FORMAT, apiKey), WebRequestMethods.Http.Post, message);
                string shortUrl = (string)JObject.Parse(response)["id"];

                return shortUrl;
            }
            catch (Exception ex)
            {
                ShortenerEnvronment.LogWarning("Goo.gl_URLShortener", "ShorteningFailed", ex, site);
                return url;
            }
        }

        #endregion
    }
}
