using System;
using System.Net;
using System.Xml;

using CMS.DataEngine;

namespace CMS.SocialMarketing.URLShortening
{
    internal sealed class BitLyShortener : IURLShortener
    {
        #region "Properties, constants & fields"

        private const string BITLY_URL_FORMAT = "http://api.bitly.com/v3/shorten?login={0}&apiKey={1}&longURL={2}&format=xml";
        private const string BITLY_LOGIN_SETTINGSKEY = "CMSBitlyLogin";
        private const string BITLY_APIKEY_SETTINGSKEY = "CMSBitlyAPIKey";

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
        public BitLyShortener()
        {

        }


        /// <summary>
        /// Internal constructor for testing purposes
        /// </summary>
        /// <param name="env">Environment mock</param>
        internal BitLyShortener(IURLShortenerHelperEnvironment env)
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
            string login = ShortenerEnvronment.GetStringSettingValue(BITLY_LOGIN_SETTINGSKEY, site);
            if (!String.IsNullOrEmpty(login))
            {
                string key = ShortenerEnvronment.GetStringSettingValue(BITLY_APIKEY_SETTINGSKEY, site);
                if (!String.IsNullOrEmpty(key))
                {
                    return true;
                }
            }

            return false;
        }


        /// <summary>
        /// Shortens the given URL via bit.ly.
        /// </summary>
        /// <param name="url">URL to be shortened</param>
        /// <param name="site">Context to get settings for</param>
        /// <returns>Shortened URL on success, <paramref name="url"/> otherwise.</returns>
        public string Shorten(string url, SiteInfoIdentifier site)
        {
            string login = ShortenerEnvronment.GetStringSettingValue(BITLY_LOGIN_SETTINGSKEY, site);
            string apiKey = ShortenerEnvronment.GetStringSettingValue(BITLY_APIKEY_SETTINGSKEY, site);

            string shortenerURL = String.Format(BITLY_URL_FORMAT, login, apiKey, ShortenerEnvronment.URLEncode(url));
            try
            {
                WebClient webClient = new WebClient();
                XmlDocument document = new XmlDocument();
                document.LoadXml(webClient.DownloadString(shortenerURL));
                XmlElement responseElement = document["response"];
                if (responseElement["status_code"].InnerText != "200")
                {
                    return url;
                }

                return responseElement["data"]["url"].InnerText;
            }
            catch (Exception ex)
            {
                ShortenerEnvronment.LogWarning("Bit.Ly_URLShortener", "ShorteningFailed", ex, site);
                return url;
            }
        }

        #endregion
    }
}
