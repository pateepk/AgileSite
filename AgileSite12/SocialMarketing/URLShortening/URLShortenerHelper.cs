using System;
using System.Collections.Generic;
using System.Linq;

using CMS.Base;
using CMS.DataEngine;
using CMS.SocialMarketing.URLShortening;


namespace CMS.SocialMarketing
{

    /// <summary>
    /// Provides methods for shortening URLs using URL shortening services.
    /// </summary>
    public class URLShortenerHelper : AbstractHelper<URLShortenerHelper>
    {

        #region "Private constants, fields & properties"

        private const string URLSHORTENER_SETTINGSKEY_FORMAT = "CMSSocialMarketingURLShortening{0}";
        private const string URL_DEFAULTPROTOCOL = @"http://";

        /// <summary>
        /// Protocol extraction regexp
        /// </summary>
        /// <remarks>
        /// Static because of backward compatibility and public accessor being static
        /// </remarks>
        private IURLShortener mBitLy = null;
        private IURLShortener mTinyUrl = null;
        private IURLShortenerHelperEnvironment mEnvironment = null;
        private URLParser mURLParser = null;
        private readonly string[] mURLShortenerDomains = new string[] {
            "ow.ly",
            "bit.ly",
            "goo.gl",
            "deck.ly",
            "su.pr",
            "tinyurl.com",
            "is.gd",
            "ez.com",
            "budurl.com",
            "cli.gs",
            "on.fb.me",
            "fb.me",
        };


        private IURLShortener BitLy
        {
            get
            {
                if (mBitLy == null)
                {
                    mBitLy = new BitLyShortener();
                }
                return mBitLy;
            }
        }


        private IURLShortener TinyURL
        {
            get
            {
                if (mTinyUrl == null)
                {
                    mTinyUrl = new TinyURLShortener();
                }
                return mTinyUrl;
            }
        }

        private IURLShortenerHelperEnvironment HelperEnvironment
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

        private URLParser Parser
        {
            get
            {
                if (mURLParser == null)
                {
                    mURLParser = new URLParser();
                }

                return mURLParser;
            }
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Default constructor used to create default HelperObject.
        /// </summary>
        public URLShortenerHelper()
        {

        }


        /// <summary>
        /// Constructor fopr testing purposes
        /// </summary>
        /// <param name="environment">Environment behaviour mock</param>
        /// <param name="bitLy">bit.ly shortener mock</param>
        /// <param name="tinyUrl">tinyurl shortener mock</param>
        internal URLShortenerHelper(IURLShortenerHelperEnvironment environment, IURLShortener bitLy, IURLShortener tinyUrl)
        {
            this.mEnvironment = environment;
            this.mBitLy = bitLy;
            this.mTinyUrl = tinyUrl;
        }

        #endregion


        #region "Public methods"

        /// <summary>
        /// Retrieves the URL shortener for the specified social network posts, and returns it.
        /// </summary>
        /// <param name="socialNetwork">The type of the social network.</param>
        /// <param name="siteIdentifier">Determines which site's settings are used. Provide null to use global settings.</param>
        /// <returns>The URL shortener for the specified social network posts.</returns>
        /// <remarks>
        /// If the URL shortener is not set or the value in the settings is not valid or the social network with the specified name does not exist,
        /// a special value URLShortenerTypeEnum.None is returned.
        /// </remarks>
        public static URLShortenerTypeEnum GetDefaultURLShortenerForSocialNetwork(SocialNetworkTypeEnum socialNetwork, SiteInfoIdentifier siteIdentifier)
        {
            return HelperObject.GetDefaultURLShortenerForSocialNetworkInternal(socialNetwork, siteIdentifier);
        }


        /// <summary>
        /// Creates a collection of available URL shorteners, and returns it.
        /// </summary>
        /// <param name="siteIdentifier">Determines which site's URL shortener settings are used. Provide null to use global settings.</param>
        /// <returns>A collection of available URL shorteners.</returns>
        /// <remarks>
        /// The collection contains URL shorteners that either require no configuration, or where the configuration is present.
        /// </remarks>
        public static List<URLShortenerTypeEnum> GetAvailableURLShorteners(SiteInfoIdentifier siteIdentifier)
        {
            return HelperObject.GetAvailableURLShortenersInternal(siteIdentifier);
        }


        /// <summary>
        /// Provides information about availability of the URL shortener.
        /// If specified shortener is configured for given site, true is returned. False otherwise.
        /// If shortener is URLShortenerTypeEnum.None, true is returned as this shortener is always available.
        /// </summary>
        /// <param name="shortener">Shortener whose availability you're interested in.</param>
        /// <param name="siteIdentifier">Determines which site's URL shortener settings are used. Provide null to use global settings.</param>
        /// <returns>
        /// If specified shortener is configured for given site, true is returned. False otherwise.
        /// If shortener is URLShortenerTypeEnum.None, true is returned as this shortener is always available.
        /// </returns>
        public static bool IsURLShortenerAvailable(URLShortenerTypeEnum shortener, SiteInfoIdentifier siteIdentifier)
        {
            return HelperObject.IsURLShortenerAvailableInternal(shortener, siteIdentifier);
        }


        /// <summary>
        /// Shortens URLs in given text using selected URL shortener. Throws an exception if shortener is not available for the given site.
        /// </summary>
        /// <param name="text">Text in that all URLs will be shortened.</param>
        /// <param name="shortener">Shortener that will be used for shortening.</param>
        /// <param name="siteIdentifier">Determines which site's URL shortener settings are used. Provide null to use global settings.</param>
        /// <returns>Text with shortened URLs.</returns>
        /// <exception cref="Exception">Shortener is not available for the given site.</exception>
        public static string ShortenURLsInText(string text, URLShortenerTypeEnum shortener, SiteInfoIdentifier siteIdentifier)
        {
            if (!HelperObject.IsURLShortenerAvailableInternal(shortener, siteIdentifier))
            {
                throw new Exception(String.Format("[URLShortenerHelper.ShortenURLsInText]: Shortener '{0}' is not available for site '{1}'.", shortener, siteIdentifier.ObjectCodeName));
            }

            return HelperObject.ShortenURLsInTextInternal(text, shortener, siteIdentifier);
        }

        #endregion


        #region "Protected methods"

        /// <summary>
        /// Retrieves the URL shortener for the specified social network posts, and returns it.
        /// </summary>
        /// <param name="socialNetwork">The type of the social network.</param>
        /// <param name="siteIdentifier">Determines which site's settings are used. Provide null to use global settings.</param>
        /// <returns>The URL shortener for the specified social network posts.</returns>
        /// <remarks>
        /// If the URL shortener is not set or the value in the settings is not valid or the social network with the specified name does not exist,
        /// a special value URLShortenerTypeEnum.None is returned.
        /// </remarks>
        protected virtual URLShortenerTypeEnum GetDefaultURLShortenerForSocialNetworkInternal(SocialNetworkTypeEnum socialNetwork, SiteInfoIdentifier siteIdentifier)
        {
            string settingName = String.Format(URLSHORTENER_SETTINGSKEY_FORMAT, socialNetwork);
            string settingValue = HelperEnvironment.GetStringSettingValue(settingName, siteIdentifier);
            if (!String.IsNullOrEmpty(settingValue))
            {
                URLShortenerTypeEnum result = URLShortenerTypeEnum.None;
                if (Enum.TryParse<URLShortenerTypeEnum>(settingValue, true, out result))
                {
                    return result;
                }
            }

            return URLShortenerTypeEnum.None;
        }


        /// <summary>
        /// Creates a collection of available URL shorteners, and returns it.
        /// </summary>
        /// <param name="siteIdentifier">Determines which site's URL shortener settings are used. Provide null to use global settings.</param>
        /// <returns>A collection of available URL shorteners.</returns>
        /// <remarks>
        /// The collection contains URL shorteners that either require no configuration, or where the configuration is present.
        /// </remarks>
        protected virtual List<URLShortenerTypeEnum> GetAvailableURLShortenersInternal(SiteInfoIdentifier siteIdentifier)
        {
            List<URLShortenerTypeEnum> result = new List<URLShortenerTypeEnum>();
            foreach (URLShortenerTypeEnum shortener in Enum.GetValues(typeof(URLShortenerTypeEnum)))
            {
                if (IsURLShortenerAvailableInternal(shortener, siteIdentifier))
                {
                    result.Add(shortener);
                }
            }
            return result;
        }


        /// <summary>
        /// Provides information about availability of the URL shortener.
        /// If specified shortener is configured for given site, true is returned. False otherwise.
        /// If shortener is URLShortenerTypeEnum.None, true is returned as this shortener is always available.
        /// </summary>
        /// <param name="shortener">Shortener whose availability you're interested in.</param>
        /// <param name="siteIdentifier">Determines which site's URL shortener settings are used. Provide null to use global settings.</param>
        /// <returns>
        /// If specified shortener is configured for given site, true is returned. False otherwise.
        /// If shortener is URLShortenerTypeEnum.None, true is returned as this shortener is always available.
        /// </returns>
        protected virtual bool IsURLShortenerAvailableInternal(URLShortenerTypeEnum shortener, SiteInfoIdentifier siteIdentifier)
        {
            switch (shortener)
            {
                case URLShortenerTypeEnum.None:
                    {
                        return true;
                    }

                case URLShortenerTypeEnum.Bitly:
                    {
                        return BitLy.IsAvailable(siteIdentifier);
                    }


                case URLShortenerTypeEnum.TinyURL:
                    {
                        return TinyURL.IsAvailable(siteIdentifier);
                    }

                default:
                    {
                        return false;
                    }
            }
        }


        /// <summary>
        /// Shortens all URLs in provided text.
        /// Returns the same text with all URLs shortened by the specified shortener.
        /// If shortener is URLShortenerTypeEnum.None, original inputText is returned.
        /// </summary>
        /// <param name="inputText">Input text</param>
        /// <param name="shortener">Shortener of your choice</param>
        /// <param name="siteIdentifier">Site name used to get URLShortener settings, since they are site-specific.</param>
        protected virtual string ShortenURLsInTextInternal(string inputText, URLShortenerTypeEnum shortener, SiteInfoIdentifier siteIdentifier)
        {
            if (shortener == URLShortenerTypeEnum.None)
            {
                return inputText;
            }

            return Parser.Replace(inputText, match => GetShortenedURLInternal(match, shortener, siteIdentifier));
        }


        /// <summary>
        /// Shortens provided URL
        /// </summary>
        /// <param name="longURLMatch">A Match containing the URL to be shortened.</param>
        /// <param name="shortener">URL shortener.</param>
        /// <param name="siteIdentifier">Site name used to get URLShortener settings, since they are site-specific.</param>
        protected virtual string GetShortenedURLInternal(URLParserMatch longURLMatch, URLShortenerTypeEnum shortener, SiteInfoIdentifier siteIdentifier)
        {
            if (IsURLShortened(longURLMatch))
            {
                return longURLMatch.URL;
            }
            string url = GetURLForShortening(longURLMatch);

            switch (shortener)
            {
                case URLShortenerTypeEnum.Bitly:
                    {
                        return BitLy.Shorten(url, siteIdentifier);
                    }

                case URLShortenerTypeEnum.TinyURL:
                    {
                        return TinyURL.Shorten(url, siteIdentifier);
                    }

                default:
                    return longURLMatch.URL;
            }
        }


        /// <summary>
        /// Determines whether an URL is shortened or to be shortened.
        /// </summary>
        /// <param name="match">URLMatch to be examined</param>
        /// <returns>True is URL looks like a shortened-one.</returns>
        protected virtual bool IsURLShortened(URLParserMatch match)
        {
            if (String.IsNullOrEmpty(match.Domain))
            {
                return false;
            }

            return mURLShortenerDomains.Contains(match.Domain.ToLowerInvariant());
        }


        /// <summary>
        /// Prepares any found URL for shortening.
        /// </summary>
        /// <param name="match">URL to be prepared.</param>
        /// <returns>String representing the URL to be shortened.</returns>
        protected virtual string GetURLForShortening(URLParserMatch match)
        {
            string result = match.URL;
            if (String.IsNullOrEmpty(match.Protocol))
            {
                result = URL_DEFAULTPROTOCOL + match.URL;
            }

            return result;
        }

        #endregion

    }
}
