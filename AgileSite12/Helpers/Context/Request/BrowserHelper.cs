using System;
using System.Collections.Generic;

using CMS.Base;

#pragma warning disable BH1007

namespace CMS.Helpers
{
    /// <summary>
    /// Browser helper methods.
    /// </summary>
    [Serializable]
    public class BrowserHelper : IDataContainer
    {
        #region "Constants"

        private static string SAFARI_CODE = "Safari";

        private static string CHROME_CODE = "Chrome";

        private static string IE_CODE = "IE";

        private static string IE_LONGCODE = "Internet Explorer";

        private static string GECKO_CODE = "Gecko";

        private static string OPERA_CODE = "Opera";

        #endregion


        #region "Properties"

        /// <summary>
        /// Gets or sets the screen resolution.
        /// </summary>
        public string ScreenResolution
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the screen color depth.
        /// </summary>
        public string ScreenColorDepth
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the operating system of the user.
        /// </summary>
        public string OperatingSystem
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the search keywords used to access the site.
        /// </summary>
        public string SearchKeywords
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates whether Silverlight is installed.
        /// </summary>
        public bool IsSilverlightInstalled
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates whether Java is installed.
        /// </summary>
        public bool IsJavaInstalled
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates whether Flash is installed.
        /// </summary>
        public bool IsFlashInstalled
        {
            get;
            set;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Gets the browser specific CSS class name.
        /// </summary>
        public static string GetBrowserClass()
        {
            if (IsCrawler())
            {
                return null;
            }

            return CMSHttpContext.Current?.Request?.Browser?.Browser;
        }


        /// <summary>
        /// Indicates if browser is Internet explorer.
        /// </summary>        
        public static bool IsIE()
        {
            var browserCapabilities = CMSHttpContext.Current?.Request?.Browser;
            return (browserCapabilities != null) && !string.IsNullOrEmpty(browserCapabilities.Browser) &&
                       ((browserCapabilities.Browser == IE_CODE) || (browserCapabilities.Browser == IE_LONGCODE));
        }



        /// <summary>
        /// Indicates if browser is based on Gecko.
        /// </summary>
        public static bool IsGecko()
        {
            string browserClass = GetBrowserClass();
            return ((browserClass != null) && (browserClass.Contains(GECKO_CODE)));
        }


        /// <summary>
        /// Indicates if browser is Opera.
        /// </summary>
        public static bool IsOpera()
        {
            string browserClass = GetBrowserClass();
            return ((browserClass != null) && (browserClass.Contains(OPERA_CODE)));
        }


        /// <summary>
        /// Indicates if client is a Win-32 based comupter.
        /// </summary>
        public static bool IsWin32()
        {
            var browserCapabilities = CMSHttpContext.Current?.Request?.Browser;
            return (browserCapabilities != null) && browserCapabilities.Win32;
        }


        /// <summary>
        /// Indicates if browser is mobile device.
        /// </summary>
        public static bool IsMobileDevice()
        {
            return IsMobileDevice(false);
        }


        /// <summary>
        /// Indicates if browser is mobile device.
        /// </summary>
        /// <param name="detectLargeDevice">Indicates if large devices should be detected as mobile</param>
        public static bool IsMobileDevice(bool detectLargeDevice)
        {
            var request = CMSHttpContext.Current?.Request;
            var oBrowser = request?.Browser;
            if (oBrowser != null)
            {
                // Check if visitor use mobile device
                bool userAgentMobileCheck = false;
                string userAgent = request.UserAgent;

                if (!string.IsNullOrEmpty(userAgent))
                {
                    userAgent = userAgent.ToLowerInvariant();
                    userAgentMobileCheck = userAgent.Contains("iphone") ||
                                           userAgent.Contains("android") ||
                                           userAgent.Contains("midp") ||
                                           userAgent.Contains("cldc") ||
                                           userAgent.Contains("blackberry");

                    // If user agent is iPad and detect large device is true, return it as mobile device
                    if (userAgent.Contains("ipad") && detectLargeDevice)
                    {
                        return true;
                    }
                }

                return (oBrowser.IsMobileDevice ||
                    ValidationHelper.GetBoolean(oBrowser["BlackBerry"], false) ||
                    userAgentMobileCheck);
            }
            return false;
        }


        /// <summary>
        /// Returns mobile browser agent.
        /// </summary>
        public static string GetUserAgent()
        {
            return CMSHttpContext.Current?.Request?.UserAgent;
        }


        /// <summary>
        /// Returns UrlReferrer from request.
        /// </summary>
        public static string GetUrlReferrer()
        {
            return CMSHttpContext.Current?.Request?.UrlReferrer?.ToString();
        }


        /// <summary>
        /// Returns list of browser user languages from request.
        /// </summary>
        public static string[] GetUserLanguages()
        {
            return CMSHttpContext.Current?.Request?.UserLanguages;
        }


        /// <summary>
        /// Returns true if current browser is Safari.
        /// </summary>
        public static bool IsSafari()
        {
            return (CMSHttpContext.Current?.Request?.Browser?.Browser.IndexOfCSafe(SAFARI_CODE, true) >= 0);
        }


        /// <summary>
        /// Returns true if current browser is Chrome.
        /// </summary>
        public static bool IsChrome()
        {
            return (CMSHttpContext.Current?.Request?.UserAgent?.IndexOfCSafe(CHROME_CODE, true) >= 0);
        }


        /// <summary>
        /// Indicates if browser is based on WebKit.
        /// </summary>
        public static bool IsWebKit()
        {
            return IsChrome() || IsSafari();
        }


        /// <summary>
        /// Gets the browser string from current request.
        /// </summary>
        public static string GetBrowser()
        {
            var request = CMSHttpContext.Current?.Request;
            var browser = request?.Browser;

            if (request == null || browser == null)
            {
                return null;
            }

            string version = GetRendererVersion(browser.Browser, request.UserAgent) ?? browser.Version;
            return $"{browser.Browser}{version}";
        }


        /// <summary>
        /// Returns version based on rendering engine rather then on version declared by the browser itself for supported browsers. 
        /// If browser or its rendering engine version is not supported, <c>null</c> is returned.
        /// </summary>
        /// <param name="browser">Browser string that was sent by the browser in the User-Agent request header.</param>
        /// <param name="userAgent">Complete user-agent string of the client.</param>
        private static string GetRendererVersion(string browser, string userAgent)
        {
            if (browser == "Internet Explorer")
            {
                return GetInternetExplorerVersionFromRendererVersion(userAgent);
            }

            return null;
        }


        /// <summary>
        /// Gets version of Internet explorer from the rendering engine version. According to the server configuration, when browsing intranet sites
        /// Internet explorer automatically switches to the compatible mode, which is in general lower version of the browser. This is suitable for
        /// rendering but not for analytics, therefore real version has to be obtained based on the <paramref name="userAgent"/> string specifying version of the Trident 
        /// rendering engine. Version cannot be obtained using .NET browser capabilities object. 
        /// </summary>
        /// <remarks>
        /// More information can be found on MSDN: https://msdn.microsoft.com/en-us/library/ms537503(v=vs.85).aspx.
        /// </remarks>
        /// <param name="userAgent">Browser user agent, that will be used for obtaining version of Trident rendering engine</param>
        /// <returns>Version obtained from given <paramref name="userAgent"/>. If no version can be determined, returns <c>null</c>.</returns>
        internal static string GetInternetExplorerVersionFromRendererVersion(string userAgent)
        {
            if (userAgent.Contains("Trident/7.0"))
            {
                return "11.0";
            }
            if (userAgent.Contains("Trident/6.0"))
            {
                return "10.0";
            }
            if (userAgent.Contains("Trident/5.0"))
            {
                return "9.0";
            }
            if (userAgent.Contains("Trident/4.0"))
            {
                return "8.0";
            }

            return null;
        }


        /// <summary>
        /// Gets browser major version.
        /// </summary>
        public static int GetMajorVersion()
        {
            // Get major version safely
            int majorVersion = 0;
            try
            {
                majorVersion = CMSHttpContext.Current.Request.Browser.MajorVersion;
            }
            catch
            {
            }

            return majorVersion;
        }


        /// <summary>
        /// Gets browser minor version.
        /// </summary>
        public static double GetMinorVersion()
        {
            // Get minor version safely
            double minorVersion = 0;
            try
            {
                minorVersion = CMSHttpContext.Current.Request.Browser.MinorVersion;
                minorVersion = GetMinorVersionFixed(minorVersion);
            }
            catch
            {
            }

            return minorVersion;
        }


        /// <summary>
        /// Returns whether browsing device is search engine crawler (spider, bot).
        /// </summary>       
        public static bool IsCrawler()
        {
            var browserCapabilities = CMSHttpContext.Current?.Request?.Browser;
            return (browserCapabilities != null) && browserCapabilities.Crawler;
        }


        /// <summary>
        /// Get browser minor version. Different .Net versions return different ways of minor version numbers.
        /// This method fixes differences.
        /// </summary>
        private static double GetMinorVersionFixed(double minor)
        {
            while (minor >= 1)
            {
                minor = minor / 10;
            }

            return minor;
        }

        #endregion


        #region "IDataContainer Members"

        /// <summary>
        /// Get the list of supported properties.
        /// </summary>
        public List<string> ColumnNames
        {
            get
            {
                return new List<string> {
                    "Browser", "BrowserClass", "BrowserMajorVersion", "BrowserMinorVersion", "UserAgent", "UrlReferrer",
                    "IsCrawler", "IsGecko", "IsChrome", "IsIE", "IsMobileDevice", "IsOpera", "IsSafari", "IsWebKit",
                    "IsWin32", "GECKO_CODE", "IE_CODE", "IE_LONGCODE", "CHROME_CODE", "OPERA_CODE", "SAFARI_CODE",
                    "OperatingSystem", "IsSilverlightInstalled", "IsJavaInstalled", "IsFlashInstalled",
                    "ScreenColorDepth", "ScreenResolution", "UserLanguages", "UserLanguage", "SearchKeywords"
                    };
            }
        }


        /// <summary>
        /// Returns given property from BrowserHelper
        /// </summary>
        /// <param name="columnName">Property name</param>
        /// <param name="value">Value will be returned here, if exists</param>
        public bool TryGetValue(string columnName, out object value)
        {
            value = null;
            switch (columnName.ToLowerInvariant())
            {
                case "browser":
                    value = GetBrowser();
                    return true;

                case "browserclass":
                    value = GetBrowserClass();
                    return true;

                case "browsermajorversion":
                    value = GetMajorVersion();
                    return true;

                case "browserminorversion":
                    value = GetMinorVersionFixed(GetMinorVersion());
                    return true;

                case "useragent":
                    value = GetUserAgent();
                    return true;

                case "urlreferrer":
                    value = GetUrlReferrer();
                    return true;

                case "iscrawler":
                    value = IsCrawler();
                    return true;

                case "ischrome":
                    value = IsChrome();
                    return true;

                case "isgecko":
                    value = IsGecko();
                    return true;

                case "isie":
                    value = IsIE();
                    return true;

                case "ismobiledevice":
                    value = IsMobileDevice(true);
                    return true;

                case "isopera":
                    value = IsOpera();
                    return true;

                case "issafari":
                    value = IsSafari();
                    return true;

                case "iswebkit":
                    value = IsWebKit();
                    return true;

                case "iswin32":
                    value = IsWin32();
                    return true;

                case "gecko_code":
                    value = GECKO_CODE;
                    return true;

                case "ie_code":
                    value = IE_CODE;
                    return true;

                case "ie_longcode":
                    value = IE_LONGCODE;
                    return true;

                case "chrome_code":
                    value = CHROME_CODE;
                    return true;

                case "opera_code":
                    value = OPERA_CODE;
                    return true;

                case "safari_code":
                    value = SAFARI_CODE;
                    return true;

                case "operatingsystem":
                    value = OperatingSystem;
                    return true;

                case "screencolordepth":
                    value = ScreenColorDepth;
                    return true;

                case "screenresolution":
                    value = ScreenResolution;
                    return true;

                case "issilverlightinstalled":
                    value = IsSilverlightInstalled;
                    return true;

                case "isflashinstalled":
                    value = IsFlashInstalled;
                    return true;

                case "isjavainstalled":
                    value = IsJavaInstalled;
                    return true;

                case "userlanguages":
                    value = GetUserLanguages();
                    return true;

                case "userlanguage":
                    string[] languages = GetUserLanguages();
                    if ((languages != null) && (languages.Length > 0))
                    {
                        value = languages[0];
                        return true;
                    }
                    break;

                case "searchkeywords":
                    value = SearchKeywords;
                    return true;
            }
            return false;
        }


        /// <summary>
        /// Returns true if given column exists.
        /// </summary>
        /// <param name="columnName">Name of the column</param>
        public bool ContainsColumn(string columnName)
        {
            foreach (string column in ColumnNames)
            {
                if (column.EqualsCSafe(columnName, true))
                {
                    return true;
                }
            }
            return false;
        }

        #endregion


        #region "ISimpleDataContainer Members"

        /// <summary>
        /// Returns given property from BrowserHelper. Setter is not implemented
        /// </summary>
        /// <param name="columnName">Property name</param>
        public object this[string columnName]
        {
            get
            {
                TryGetValue(columnName, out var value);
                return value;
            }
            set
            {
                throw new NotImplementedException();
            }
        }


        /// <summary>
        /// Returns given property from BrowserHelper. Setter is not implemented
        /// </summary>
        /// <param name="columnName">Property name</param>
        public object GetValue(string columnName)
        {
            TryGetValue(columnName, out var value);
            return value;
        }


        /// <summary>
        /// Not implemented.
        /// </summary>
        /// <param name="columnName">Not implemented.</param>
        /// <param name="value">Not implemented.</param>
        public bool SetValue(string columnName, object value)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}

#pragma warning restore BH1007