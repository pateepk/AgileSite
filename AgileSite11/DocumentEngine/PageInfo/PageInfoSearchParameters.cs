using System;

using CMS.DataEngine;
using CMS.Helpers;

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Encapsulates parameters which are used when searching <see cref="PageInfo"/> candidate.
    /// </summary>
    internal class PageInfoSearchParameters
    {
        private string mCulturePath;


        public string SiteName
        {
            get;
            private set;
        }


        public string RelativeUrlPath
        {
            get;
            private set;
        }


        public string UrlExtension
        {
            get;
            private set;
        }


        public string CultureCode
        {
            get;
            private set;
        }


        public bool CombineWithDefaultCulture
        {
            get;
            private set;
        }


        public bool UrlIsConfirmedAsPage
        {
            get;
            private set;
        }


        /// <summary>
        /// URL with language prefix if language prefixes are allowed for current site.
        /// </summary>
        public string RelativeUrlPathWithLanguagePrefix
        {
            get
            {
                if (string.IsNullOrEmpty(mCulturePath))
                {
                    mCulturePath = IsSearchPathWithLanguagePrefixAllowed(SiteName) ? "/" + RequestContext.CurrentURLLangPrefix + RelativeUrlPath : null;
                }

                return mCulturePath;
            }
        }


        /// <summary>
        /// Indicates whether <see cref="PageInfo"/> can be searched by URL path with language prefix (<seealso cref="RelativeUrlPathWithLanguagePrefix"/>).
        /// </summary>
        public bool CheckUrlPathWithLanguagePrefix
        {
            get
            {
                return !string.IsNullOrEmpty(RelativeUrlPathWithLanguagePrefix) && !String.Equals(RelativeUrlPathWithLanguagePrefix, RelativeUrlPath, StringComparison.InvariantCultureIgnoreCase);
            }
        }


        /// <summary>
        /// Creates an instance of the class <see cref="PageInfoSearchParameters"/>.
        /// </summary>
        public PageInfoSearchParameters(string siteName, string relativeUrlPath, string urlExtension, string cultureCode, bool combineWithDefaultCulture, bool urlIsConfirmedAsPage)
        {
            SiteName = siteName;
            RelativeUrlPath = relativeUrlPath;
            UrlExtension = urlExtension;
            CultureCode = cultureCode;
            CombineWithDefaultCulture = combineWithDefaultCulture;
            UrlIsConfirmedAsPage = urlIsConfirmedAsPage;
        }


        private static bool IsSearchPathWithLanguagePrefixAllowed(string siteName)
        {
            return URLHelper.UseLangPrefixForUrls(siteName) && SettingsKeyInfoProvider.GetBoolValue(siteName + ".CMSAllowUrlsWithoutLanguagePrefixes") && !String.IsNullOrEmpty(RequestContext.CurrentURLLangPrefix);
        }
    }
}