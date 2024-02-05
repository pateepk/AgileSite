using System;
using System.Collections.Generic;
using System.Web;

using CMS.Base;

namespace CMS.Helpers
{
    /// <summary>
    /// Browser helper methods.
    /// </summary>
    [Serializable]
    public class BrowserHelper : IDataContainer
    {
#pragma warning disable BH1007 // 'Request.Browser' should not be used. Use 'BrowserHelper.GetBrowser()' instead.
        
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
            if (HttpContext.Current?.Request.Browser != null)
            {
                var browserCapabilities = HttpContext.Current.Request.Browser;
                if (!browserCapabilities.Crawler)
                {
                    return browserCapabilities.Browser;
                }
            }

            return null;
        }


        /// <summary>
        /// Indicates if browser is Internet explorer.
        /// </summary>        
        public static bool IsIE()
        {
            if (HttpContext.Current != null)
            {
                HttpRequest currentRequest = HttpContext.Current.Request;
                return (currentRequest.Browser != null) && !string.IsNullOrEmpty(currentRequest.Browser.Browser) &&
                       ((currentRequest.Browser.Browser == IE_CODE) || (currentRequest.Browser.Browser == IE_LONGCODE));
            }

            return false;
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
            if (HttpContext.Current != null)
            {
                HttpRequest currentRequest = HttpContext.Current.Request;
                return (currentRequest.Browser != null) && currentRequest.Browser.Win32;
            }

            return false;
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
            if ((HttpContext.Current != null) && (HttpContext.Current.Request != null))
            {
                // Get request and browser
                HttpRequest request = HttpContext.Current.Request;
                HttpBrowserCapabilities oBrowser = request.Browser;
                if (oBrowser != null)
                {
                    // Check if visitor use mobile device
                    bool userAgentMobileCheck = false;
                    string userAgent = request.UserAgent;

                    if (!string.IsNullOrEmpty(userAgent))
                    {
                        userAgent = userAgent.ToLowerCSafe();
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
            }
            return false;
        }


        /// <summary>
        /// Returns mobile browser agent.
        /// </summary>
        public static string GetUserAgent()
        {
            if ((HttpContext.Current != null) && (HttpContext.Current.Request != null))
            {
                // Get request and browser
                return HttpContext.Current.Request.UserAgent;
            }
            return null;
        }


        /// <summary>
        /// Returns UrlReferrer from request.
        /// </summary>
        public static string GetUrlReferrer()
        {
            if ((HttpContext.Current != null) && (HttpContext.Current.Request != null) && (HttpContext.Current.Request.UrlReferrer != null))
            {
                return HttpContext.Current.Request.UrlReferrer.ToString();
            }
            return null;
        }


        /// <summary>
        /// Returns list of browser user languages from request.
        /// </summary>
        public static string[] GetUserLanguages()
        {
            if ((HttpContext.Current != null) && (HttpContext.Current.Request != null))
            {
                return HttpContext.Current.Request.UserLanguages;
            }
            return null;
        }


        /// <summary>
        /// Returns true if current browser is Safari.
        /// </summary>
        public static bool IsSafari()
        {
            if ((HttpContext.Current == null) || (HttpContext.Current.Request == null) || (HttpContext.Current.Request.Browser == null))
            {
                return false;
            }

            // Check if browser is Safari
            return (HttpContext.Current.Request.Browser.Browser.IndexOfCSafe(SAFARI_CODE, true) >= 0);
        }


        /// <summary>
        /// Returns true if current browser is Chrome.
        /// </summary>
        public static bool IsChrome()
        {
            if ((HttpContext.Current == null) || (HttpContext.Current.Request == null))
            {
                return false;
            }

            // Handle browsers explicitly
            if (HttpContext.Current.Request.UserAgent != null)
            {
                string agent = HttpContext.Current.Request.UserAgent;

                // Chrome
                if (agent.IndexOfCSafe(CHROME_CODE, true) >= 0)
                {
                    return true;
                }
            }

            return false;
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
            if ((CMSHttpContext.Current == null) || (CMSHttpContext.Current.Request == null) || (CMSHttpContext.Current.Request.Browser == null))
            {
                return null;
            }
            HttpRequestBase request = CMSHttpContext.Current.Request;
            HttpBrowserCapabilitiesBase browser = request.Browser;


            string version = GetRendererVersion(browser.Browser, request.UserAgent) ?? browser.Version;
            return String.Format("{0}{1}", browser.Browser, version);
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
                majorVersion = HttpContext.Current.Request.Browser.MajorVersion;
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
                minorVersion = HttpContext.Current.Request.Browser.MinorVersion;
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
            if (HttpContext.Current != null)
            {
                HttpRequest currentRequest = HttpContext.Current.Request;
                return (currentRequest.Browser != null) && currentRequest.Browser.Crawler;
            }

            return false;
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
            switch (columnName.ToLowerCSafe())
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
                object value = null;
                TryGetValue(columnName, out value);
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
            object value = null;
            TryGetValue(columnName, out value);
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

#pragma warning restore BH1007 // 'Request.Browser' should not be used. Use 'BrowserHelper.GetBrowser()' instead.
    }
}