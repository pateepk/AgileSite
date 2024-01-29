using System;
using System.Net;

using CMS.DataEngine;

namespace CMS.SocialMarketing.URLShortening
{
    /// <summary>
    /// Represents a provider for TinyURL
    /// </summary>
    internal sealed class TinyURLShortener : IURLShortener
    {
        #region "Properties, constants & fields"

        private const string TINYURL_URL_FORMAT = "http://tinyurl.com/api-create.php?url={0}";

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
        public TinyURLShortener()
        {
            
        }


        /// <summary>
        /// Internal constructor for testing purposes
        /// </summary>
        /// <param name="env">Environment mock</param>
        internal TinyURLShortener(IURLShortenerHelperEnvironment env)
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
            return true;
        }


        /// <summary>
        /// Shortens the given URL via TinyURL
        /// </summary>
        /// <param name="url">URL to be shortened</param>
        /// <param name="site">Context to get settings for</param>
        /// <returns>Shortened URL on succes, <paramref name="url"/> otherwise.</returns>
        public string Shorten(string url, SiteInfoIdentifier site)
        {
            string shortenerURL = String.Format(TINYURL_URL_FORMAT, ShortenerEnvronment.URLEncode(url));
            WebClient client = new WebClient();
            try
            {
                return client.DownloadString(shortenerURL);
            }
            catch(Exception ex)
            {
                ShortenerEnvronment.LogWarning("TinyURL_Shortener", "ShorteningFailed", ex, site);
                return url;
            }
        }

        #endregion
    }
}
