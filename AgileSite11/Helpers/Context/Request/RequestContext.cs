using System;
using System.Net;

using CMS.Base;
using CMS.IO;
using CMS.Helpers;

[assembly: ThreadRequiredContext(typeof(RequestContext))]

namespace CMS.Helpers
{
    /// <summary>
    /// Request context properties
    /// </summary>
    public sealed class RequestContext : AbstractContext<RequestContext>
    {
#pragma warning disable BH1005 // 'Request.UserHostAddress' should not be used. Use 'RequestContext.UserHostAddress' instead.
#pragma warning disable BH1006 // 'Request.Url' should not be used. Use 'RequestContext.Url' instead.

        #region "Variables"

        private bool mLogCurrentError = true;
        private bool mUseGZip;
        private bool mResponseIsCompressed;
        private bool mLogPageHit;
        private bool mUseNoCacheForRedirect;

        private bool? mIsContentPage;
        private bool? mIsSsl;
        private bool? mIsUserAuthenticated;

        private ExcludedSystemEnum mCurrentExcludedStatus = ExcludedSystemEnum.Unknown;
        private RequestStatusEnum mCurrentStatus = RequestStatusEnum.Unknown;

        private string mCurrentUrlLangPrefix = String.Empty;
        private string mCurrentUrlPathPrefix = String.Empty;

        private string mRawUrl;
        private string mCurrentUrl;
        private string mCurrentUrlExtension;

        private string mCurrentRelativePath;

        private string mCurrentDomain;
        private string mFullDomain;

        private Uri mUrl;

        private string mServerHostAddress;
        private string mUserHostAddress;
        private string mUserName;
        private string mUserAgent;
        private string mUrlReferrer;

        private SafeDictionary<string, object> mClientApplication;
        private EventList mRequestEvents;

        #endregion


        #region "Properties"

        /// <summary>
        /// If true (default), the current application error should be logged
        /// </summary>
        public static bool LogCurrentError
        {
            get
            {
                return Current.mLogCurrentError;
            }
            set
            {
                Current.mLogCurrentError = value;
            }
        }


        /// <summary>
        /// Returns current excluded status of the request path
        /// </summary>
        public static ExcludedSystemEnum CurrentExcludedStatus
        {
            get
            {
                return Current.mCurrentExcludedStatus;
            }
            set
            {
                Current.mCurrentExcludedStatus = value;
            }
        }


        /// <summary>
        /// Returns current request URL rewriting status.
        /// </summary>
        public static RequestStatusEnum CurrentStatus
        {
            get
            {
                return Current.mCurrentStatus;
            }
            set
            {
                Current.mCurrentStatus = value;
            }
        }


        /// <summary>
        /// Indicates that the current request is a request to the content page.
        /// </summary>
        public static bool IsContentPage
        {
            get
            {
                var c = Current;

                var contentPage = c.mIsContentPage;
                if (contentPage == null)
                {
                    switch (CurrentStatus)
                    {
                        case RequestStatusEnum.PathRewritten:
                        case RequestStatusEnum.MVCPage:
                        case RequestStatusEnum.SentFromCache:
                            contentPage = true;
                            break;

                        default:
                            contentPage = false;
                            break;

                    }

                    c.mIsContentPage = contentPage;
                }

                return contentPage.Value;
            }
        }


        /// <summary>
        /// If true, the filter uses GZip compression on output.
        /// </summary>
        public static bool UseGZip
        {
            get
            {
                return Current.mUseGZip;
            }
            set
            {
                Current.mUseGZip = value;
            }
        }


        /// <summary>
        /// Indicates whether the current response contains compressed data.
        /// </summary>
        public static bool ResponseIsCompressed
        {
            get
            {
                return Current.mResponseIsCompressed;
            }
            set
            {
                Current.mResponseIsCompressed = value;
            }
        }


        /// <summary>
        /// Indicates if page hit should be logged.
        /// </summary>
        public static bool LogPageHit
        {
            get
            {
                return Current.mLogPageHit;
            }
            set
            {
                Current.mLogPageHit = value;
            }
        }


        /// <summary>
        /// Gets or sets the value that contains current language prefix for current URL.
        /// </summary>
        public static string CurrentURLLangPrefix
        {
            get
            {
                return Current.mCurrentUrlLangPrefix;
            }
            set
            {
                Current.mCurrentUrlLangPrefix = value;
            }
        }


        /// <summary>
        /// Gets or sets the value that contains current path prefix for current URL.
        /// </summary>
        public static string CurrentURLPathPrefix
        {
            get
            {
                return Current.mCurrentUrlPathPrefix;
            }
            set
            {
                Current.mCurrentUrlPathPrefix = value;
            }
        }


        /// <summary>
        /// Current request RawUrl.
        /// </summary>
        public static string RawURL
        {
            get
            {
                var c = Current;

                string url = c.mRawUrl;
                if (url == null)
                {
                    // Load standard RawUrl if not found
                    if (CMSHttpContext.Current != null)
                    {
                        url = URLHelper.ResolveUrl(CMSHttpContext.Current.Request.RawUrl);
                    }

                    c.mRawUrl = url;
                }

                return url;
            }
            set
            {
                value = URLHelper.ResolveUrl(value);

                Current.mRawUrl = value;
            }
        }


        /// <summary>
        /// Current page relative path. e.g. "~/Home.aspx" has relative path "/Home.aspx"
        /// </summary>
        public static string CurrentRelativePath
        {
            get
            {
                var c = Current;

                string path = c.mCurrentRelativePath;
                if (path == null)
                {
                    if (CMSHttpContext.Current != null)
                    {
                        // Load the relative path if not found
                        path = CMSHttpContext.Current.Request.AppRelativeCurrentExecutionFilePath;
                        if (path.StartsWithCSafe("~/"))
                        {
                            path = path.Substring(1); //'~/path' -> '/path'
                        }
                    }

                    c.mCurrentRelativePath = path;
                }

                return path;
            }
            set
            {
                Current.mCurrentRelativePath = value;
            }
        }


        /// <summary>
        /// Returns the current request URL query string.
        /// </summary>
        public static string CurrentQueryString
        {
            get
            {
                return URL.Query;
            }
        }


        /// <summary>
        /// Returns the current request URL scheme.
        /// </summary>
        public static string CurrentScheme
        {
            get
            {
                // Try to get from URL if available
                var url = URL;
                if (url != null)
                {
                    return url.Scheme;
                }

                // Otherwise return default scheme from web config - default scheme is 'http'
                return ValidationHelper.GetString(SettingsHelper.AppSettings["CMSUrlDefaultScheme"], "http");
            }
        }


        /// <summary>
        /// Returns the current request URL port.
        /// </summary>
        [Obsolete("Property will be removed. Use URL.Port instead.")]
        public static int CurrentPort
        {
            get
            {
                return URL.Port;
            }
        }


        /// <summary>
        /// Returns the current request URL.
        /// </summary>
        public static string CurrentURL
        {
            get
            {
                var c = Current;

                string url = c.mCurrentUrl;
                if (url == null)
                {
                    if (CMSHttpContext.Current == null)
                    {
                        return null;
                    }

                    url = RawURL;

                    c.mCurrentUrl = url;
                }

                return url;
            }
            set
            {
                Current.mCurrentUrl = value;
            }
        }


        /// <summary>
        /// Gets information about URL of current request.
        /// </summary>
        public static Uri URL
        {
            get
            {
                var c = Current;

                var url = c.mUrl;
                if (url == null)
                {
                    var context = CMSHttpContext.Current;
                    if (context == null)
                    {
                        return null;
                    }

                    // Try to get custom port
                    url = context.Request.Url;
                    if (url != null)
                    {
                        // Ensure correct uri scheme when the IsSSL property was manually set
                        if (IsSSL && (url.Scheme == "http"))
                        {
                            UriBuilder uriBuilder = new UriBuilder(url);
                            uriBuilder.Scheme = "https";
                            url = uriBuilder.Uri;
                        }

                        url = URLHelper.EnsureProperPort(IsSSL,url);
                    }

                    c.mUrl = url;
                }

                return c.mUrl;
            }
        }


        /// <summary>
        /// Indicates whether current request is SSL request.
        /// </summary>
        public static bool IsSSL
        {
            get
            {
                var c = Current;

                bool? isSSL = c.mIsSsl;

                if (isSSL == null)
                {
                    isSSL = CurrentURL.StartsWithCSafe("https://");

                    // If protocol is not https check IsSecureConnection property
                    if (!isSSL.Value)
                    {
                        var context = CMSHttpContext.Current;
                        if (context != null)
                        {
                            isSSL = context.Request.IsSecureConnection;
                        }
                    }

                    c.mIsSsl = isSSL;
                }

                return isSSL.Value;
            }
            set
            {
                Current.mIsSsl = value;
            }
        }


        /// <summary>
        /// Returns current URL domain, without trailing slashes.
        /// </summary>
        public static string CurrentDomain
        {
            get
            {
                var c = Current;

                string domain = c.mCurrentDomain;
                if (domain == null)
                {
                    // Get domain
                    var context = CMSHttpContext.Current;
                    if ((context == null) || (context.Request == null) || (context.Request.Url == null))
                    {
                        return String.Empty;
                    }
                    else
                    {
                        domain = context.Request.Url.Host;
                    }

                    c.mCurrentDomain = domain;
                }

                return domain;
            }
        }


        /// <summary>
        /// Returns current URL domain with port if exist.
        /// </summary>
        public static string FullDomain
        {
            get
            {
                var c = Current;

                string domain = c.mFullDomain;
                if (domain == null)
                {
                    if (CMSHttpContext.Current == null)
                    {
                        domain = String.Empty;
                    }
                    else
                    {
                        // Get the full domain
                        domain = URL.Authority;
                        if (domain.EndsWithCSafe(":80"))
                        {
                            domain = domain.Substring(0, domain.Length - 3);
                        }
                    }

                    c.mFullDomain = domain;
                }

                return domain;
            }
        }


        /// <summary>
        /// Indicates whether current redirect should add no-cache directive to the response header
        /// </summary>
        public static bool UseNoCacheForRedirect
        {
            get
            {
                return Current.mUseNoCacheForRedirect;
            }
            set
            {
                Current.mUseNoCacheForRedirect = value;
            }
        }


        /// <summary>
        /// Returns extension (part after last dot) contained in current URL.
        /// Extension starting with . (.ext) or empty extension if there is no extension is returned.
        /// </summary>
        public static string CurrentUrlExtension
        {
            get
            {
                var c = Current;

                string extension = c.mCurrentUrlExtension;
                if (extension == null)
                {
                    // Get current URL (regular or handled) and remove query
                    string url = URLHelper.RemoveQuery(CurrentURL);

                    if (!String.IsNullOrEmpty(url))
                    {
                        // Remove application path
                        url = URLHelper.RemoveApplicationPath(url);

                        // Remove trailing slash
                        url = url.TrimEnd('/');

                        // Get extension
                        extension = Path.GetExtension(url);
                    }
                    else
                    {
                        extension = String.Empty;
                    }

                    c.mCurrentUrlExtension = extension;
                }

                return extension;
            }
            set
            {
                Current.mCurrentUrlExtension = value;
            }
        }


        /// <summary>
        /// Returns true, if the user is authenticated
        /// </summary>
        public static bool IsUserAuthenticated
        {
            get
            {
                // Try to get from virtual context first
                string virtualUserName = (string)VirtualContext.GetItem(VirtualContext.PARAM_USERNAME);
                if (virtualUserName != null)
                {
                    return !String.IsNullOrEmpty(virtualUserName);
                }

                // Get from context
                var c = Current;

                bool? isAuthenticated = c.mIsUserAuthenticated;
                if (isAuthenticated == null)
                {
                    // Initialize based on whether request user is available or not
                    if ((CMSHttpContext.Current == null) || (CMSHttpContext.Current.User == null))
                    {
                        return false;
                    }

                    isAuthenticated = CMSHttpContext.Current.User.Identity.IsAuthenticated;

                    c.mIsUserAuthenticated = isAuthenticated;
                }

                return isAuthenticated.Value;
            }
        }


        /// <summary>
        /// Gets or sets user host address. Returns empty string if HttpContext is not initialized and other value is not set.
        /// </summary>
        public static string UserHostAddress
        {
            get
            {
                var c = Current;

                string address = c.mUserHostAddress;
                if (address == null)
                {
                    // Try to get testing user host address from app settings
                    address = ValidationHelper.GetString(SettingsHelper.AppSettings["CMSUserHostAddress"], String.Empty);

                    if (String.IsNullOrEmpty(address) && (CMSHttpContext.Current != null))
                    {
                        // Get user host address from HTTP request
                        address = CMSHttpContext.Current.Request.UserHostAddress;
                    }

                    c.mUserHostAddress = address;
                }

                return address;
            }
            set
            {
                Current.mUserHostAddress = value;
            }
        }


        /// <summary>
        /// Gets or sets server host address.
        /// </summary>
        [Obsolete("Property will be removed. Use custom logic instead.")]
        public static string ServerHostAddress
        {
            get
            {
                var c = Current;

                string address = c.mServerHostAddress;
                if (address == null)
                {
                    // Get host address
                    string strHostName = Dns.GetHostName();
                    IPHostEntry ipHostInfo = Dns.GetHostEntry(strHostName);

                    address = ipHostInfo.AddressList[0].ToString();

                    c.mServerHostAddress = address;
                }

                return address;
            }
            set
            {
                Current.mServerHostAddress = value;
            }
        }


        /// <summary>
        /// Gets user name or empty string if HttpContext is not initialized. The user name is in the exact format in which the user is authenticated (may not be safe name).
        /// </summary>
        public static string UserName
        {
            get
            {
                // Try to get from virtual context first
                string virtualUserName = (string)VirtualContext.GetItem(VirtualContext.PARAM_USERNAME);
                if (virtualUserName != null)
                {
                    return virtualUserName;
                }

                // Get from context
                var c = Current;

                string userName = c.mUserName;
                if (userName == null)
                {
                    // Initialize based on whether request user is available or not
                    if ((CMSHttpContext.Current == null) || (CMSHttpContext.Current.User == null))
                    {
                        return "";
                    }

                    userName = CMSHttpContext.Current.User.Identity.Name;

                    c.mUserName = userName;
                }

                return userName;
            }
        }


        /// <summary>
        /// Returns user agent or empty string if HttPContext is not initialized.
        /// </summary>
        public static string UserAgent
        {
            get
            {
                // Get from context
                var c = Current;

                string agent = c.mUserAgent;
                if (agent == null)
                {
                    // Initialize based on whether request user is available or not
                    if (CMSHttpContext.Current == null)
                    {
                        return "";
                    }

                    agent = CMSHttpContext.Current.Request.UserAgent;

                    c.mUserAgent = agent;
                }

                return agent;
            }
        }


        /// <summary>
        /// Returns URL referrer or empty string if HttPContext is not initialized.
        /// </summary>
        public static string URLReferrer
        {
            get
            {
                // Get from context
                var c = Current;

                string url = c.mUrlReferrer;
                if (url == null)
                {
                    // Initialize based on whether request user is available or not
                    if ((CMSHttpContext.Current == null) || (CMSHttpContext.Current.Request.UrlReferrer == null))
                    {
                        return "";
                    }

                    url = CMSHttpContext.Current.Request.UrlReferrer.ToString();

                    c.mUrlReferrer = url;
                }

                return url;
            }
        }


        /// <summary>
        /// Dictionary to be used to collect data from server for client application.
        /// </summary>
        public static SafeDictionary<string, object> ClientApplication
        {
            get
            {
                // Get from context
                var c = Current;

                var app = c.mClientApplication;
                if (app == null)
                {
                    app = new SafeDictionary<string, object>();

                    c.mClientApplication = app;
                }

                return app;
            }
        }

        
        /// <summary>
        /// Events that are fired and survive within current request.
        /// </summary>
        internal static EventList RequestEvents
        {
            get
            {
                var c = Current;

                // Ensure the event list
                EventList list = c.mRequestEvents;
                if (list == null)
                {
                    list = new EventList
                    {
                        Name = "ComponentEvents.RequestEvents",
                        IsStatic = false
                    };

                    c.mRequestEvents = list;
                }

                return list;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Ensures the context source items to be always distributed to thread in async mode
        /// </summary>
        private static void EnsureSourceItems()
        {
            Ensure(RawURL);
            Ensure(CurrentDomain);
            Ensure(FullDomain);
            Ensure(IsUserAuthenticated);

            // Items to ensure for event log
            Ensure(UserHostAddress);
            Ensure(UserAgent);
            Ensure(URLReferrer);
            Ensure(UserName);
        }


        /// <summary>
        /// Clears the cached values related to URL of the request
        /// </summary>
        public static void ClearCachedUrlValues()
        {
            var c = Current;

            // Do not clear public facing properties - We want to keep their original values
            //c.mRawUrl = null;
            //c.mCurrentUrlExtension = null;

            c.mUrl = null;
            c.mCurrentUrl = null;

            // Clear relative path only for virtual context due to output filter processing
            if (VirtualContext.IsInitialized)
            {
                c.mCurrentRelativePath = null;
            }
        }


        /// <summary>
        /// Clones the object for new thread
        /// </summary>
        public override object CloneForNewThread()
        {
            EnsureSourceItems();

            return base.CloneForNewThread();
        }

        #endregion

#pragma warning restore BH1005 // 'Request.UserHostAddress' should not be used. Use 'RequestContext.UserHostAddress' instead.
#pragma warning restore BH1006 // 'Request.Url' should not be used. Use 'RequestContext.Url' instead.
    }
}
