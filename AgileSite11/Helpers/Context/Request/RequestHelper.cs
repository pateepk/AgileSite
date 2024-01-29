using System;
using System.Configuration;
using System.Web.Security;
using System.Security.Principal;

using CMS.Core;
using CMS.IO;
using CMS.Base;

namespace CMS.Helpers
{
    /// <summary>
    /// Request helping methods.
    /// </summary>
    public static class RequestHelper
    {
        #region "Constants"

        private const string AUTH_TYPE_DEFAULT = "default";
        private const string AUTH_TYPE_FORMS = "forms";
        private const string AUTH_TYPE_WINDOWS = "windows";
        private const string AUTH_TYPE_BOTH = "both";
        private const string REQUEST_PROPFIND = "PROPFIND";

        #endregion


        #region "Variables"

        private static bool? mAllowGZip;

        private static readonly Lazy<IPerformanceCounter> mTotalPageRequests = new Lazy<IPerformanceCounter>(Service.Resolve<IPerformanceCounter>);
        private static readonly Lazy<IPerformanceCounter> mTotalSystemPageRequests = new Lazy<IPerformanceCounter>(Service.Resolve<IPerformanceCounter>);
        private static readonly Lazy<IPerformanceCounter> mTotalGetFileRequests = new Lazy<IPerformanceCounter>(Service.Resolve<IPerformanceCounter>);
        private static readonly Lazy<IPerformanceCounter> mTotalNonPageRequests = new Lazy<IPerformanceCounter>(Service.Resolve<IPerformanceCounter>);
        private static readonly Lazy<IPerformanceCounter> mTotalPageNotFoundRequests = new Lazy<IPerformanceCounter>(Service.Resolve<IPerformanceCounter>);
        private static readonly Lazy<IPerformanceCounter> mPendingRequests = new Lazy<IPerformanceCounter>(Service.Resolve<IPerformanceCounter>);
        private static readonly Lazy<IPerformanceCounter> mTotalPageRobotTxtRequests = new Lazy<IPerformanceCounter>(Service.Resolve<IPerformanceCounter>);

        private static string mLogURLsFile;
        private static string mAuthenticationTypeCheck;
        private static bool? mGetFileEndRequest;

        #endregion


        #region "Properties"

        /// <summary>
        /// Gets the value that sets the type of current authentication
        /// AUTH_TYPE_DEFAULT  - Use default
        /// AUTH_TYPE_FORMS    - Forcibly sets Forms authentication 
        /// AUTH_TYPE_WINDOWS  - Forcibly sets Windows authentication 
        /// AUTH_TYPE_BOTH     - Forcibly sets Windows authentication  and Forms authentication
        /// </summary>
        private static string AuthenticationTypeCheck
        {
            get
            {
                // Check whether authentication type check is set
                if (mAuthenticationTypeCheck == null)
                {
                    // Get authentication type from settings
                    mAuthenticationTypeCheck = ValidationHelper.GetString(SettingsHelper.AppSettings["CMSAuthenticationType"], AUTH_TYPE_DEFAULT).ToLowerInvariant();
                }

                return mAuthenticationTypeCheck;
            }
        }


        /// <summary>
        /// Counter of Total Content page requests.
        /// </summary>
        public static IPerformanceCounter TotalPageRequests
        {
            get
            {
                return mTotalPageRequests.Value;
            }
        }


        /// <summary>
        /// Counter of Total System page requests.
        /// </summary>
        public static IPerformanceCounter TotalSystemPageRequests
        {
            get
            {
                return mTotalSystemPageRequests.Value;
            }
        }


        /// <summary>
        /// Counter of GetFile requests.
        /// </summary>
        public static IPerformanceCounter TotalGetFileRequests
        {
            get
            {
                return mTotalGetFileRequests.Value;
            }
        }


        /// <summary>
        /// Counter of NonPage requests.
        /// </summary>
        public static IPerformanceCounter TotalNonPageRequests
        {
            get
            {
                return mTotalNonPageRequests.Value;
            }
        }


        /// <summary>
        /// Counter of Page not found requests.
        /// </summary>
        public static IPerformanceCounter TotalPageNotFoundRequests
        {
            get
            {
                return mTotalPageNotFoundRequests.Value;
            }
        }


        /// <summary>
        /// Counter of currently pending requests.
        /// </summary>
        public static IPerformanceCounter PendingRequests
        {
            get
            {
                return mPendingRequests.Value;
            }
        }


        /// <summary>
        /// Counter of Total page Robots.txt requests.
        /// </summary>
        public static IPerformanceCounter TotalPageRobotsTxtRequests
        {
            get
            {
                return mTotalPageRobotTxtRequests.Value;
            }
        }


        /// <summary>
        /// Allow GZip of the output.
        /// </summary>
        public static bool AllowGZip
        {
            get
            {
                if (mAllowGZip == null)
                {
                    mAllowGZip = CoreServices.Settings["CMSAllowGZip"].ToBoolean(false);
                }

                return mAllowGZip.Value;
            }
            set
            {
                mAllowGZip = value;
            }
        }


        /// <summary>
        /// Gets whether the resource compression is enabled.
        /// </summary>
        public static bool AllowResourceCompression
        {
            get
            {
                return CoreServices.Settings["CMSResourceCompressionEnabled"].ToBoolean(false);
            }
        }


        /// <summary>
        /// Gets whether the request is completed or ended when request processing is finished.
        /// </summary>
        public static bool GetFileEndRequest
        {
            get
            {
                if (mGetFileEndRequest == null)
                {
                    mGetFileEndRequest = ValidationHelper.GetBoolean(SettingsHelper.AppSettings["CMSGetFileEndRequest"], true);
                }

                return mGetFileEndRequest.Value;
            }
        }


        /// <summary>
        /// Gets the HTTP data transfer method (such as GET, POST) used by client.
        /// </summary>
        public static string HttpMethod
        {
            get
            {
                return CMSHttpContext.Current?.Request.HttpMethod ?? String.Empty;
            }
        }

        #endregion


        #region "Debug properties"

        /// <summary>
        /// Logs request URLs file.
        /// </summary>
        public static string LogURLsFile
        {
            get
            {
                var context = CMSHttpContext.Current;
                if ((mLogURLsFile == null) && (context != null) && (context.Server != null))
                {
                    mLogURLsFile = SystemContext.WebApplicationPhysicalPath + "\\App_Data\\logRequestsURLs.log";

                    // Ensure the directory
                    DirectoryHelper.EnsureDiskPath(mLogURLsFile, SystemContext.WebApplicationPhysicalPath);
                }

                return mLogURLsFile;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Sets all the performance settings to null and causes them to be reloaded.
        /// </summary>
        public static void ResetPerformanceSettings()
        {
            mAllowGZip = null;
        }


        /// <summary>
        /// Returns true if current request is postback.
        /// </summary>
        public static bool IsPostBack()
        {
            return "POST".Equals(HttpMethod, StringComparison.OrdinalIgnoreCase);
        }


        /// <summary>
        /// Returns true if current request is AJAX request.
        /// </summary>
        public static bool IsAJAXRequest()
        {
            var context = CMSHttpContext.Current;
            return (context != null) && ((context.Request.Headers["x-microsoftajax"] != null) || (context.Request.Headers["X-Requested-With"] == "XMLHttpRequest"));
        }


        /// <summary>
        /// Returns true if current request is callback.
        /// </summary>
        public static bool IsCallback()
        {
            var context = CMSHttpContext.Current;
            return (context != null) && (context.Request.Form["__CALLBACKID"] != null);
        }


        /// <summary>
        /// Returns true if current request is an asynchronous postback.
        /// </summary>
        public static bool IsAsyncPostback()
        {
            return IsPostBack() && (IsAJAXRequest() || IsCallback());
        }


        /// <summary> 
        /// Returns true if GZip is supported by the client browser
        /// </summary> 
        public static bool IsGZipSupported()
        {
            string accept = CMSHttpContext.Current.Request.Headers["Accept-Encoding"];
            if (!String.IsNullOrEmpty(accept) && accept.Contains("deflate"))
            {
                return !IsAsyncPostback();
            }
            
            return false;
        }


        /// <summary>
        /// Returns the value of specified HTTP header or default value if header is not defined.
        /// </summary>
        /// <param name="name">HTTP header name</param>
        /// <param name="defaultValue">Default value to use if header not found</param>
        /// <returns>Header value or default value if not found</returns>
        public static string GetHeader(string name, string defaultValue)
        {
            return ValidationHelper.GetString(CMSHttpContext.Current.Request.Headers[name], defaultValue);
        }


        /// <summary>
        /// Sets the 400 status (Bad Request) and ends the response.
        /// </summary>
        /// <param name="explanationMessage">The optional message to include in the response.</param>
        public static void Respond400(string explanationMessage)
        {
            var response = CMSHttpContext.Current.Response;

            response.Clear();
            response.StatusCode = 400;
            if (!string.IsNullOrEmpty(explanationMessage))
            {
                response.Write(explanationMessage);
            }

            RequestDebug.LogRequestOperation("400BadRequest", explanationMessage, 1);
            response.End();
        }


        /// <summary>
        /// Sets the 403 status (Forbidden) and ends the response.
        /// </summary>
        public static void Respond403()
        {
            var response = CMSHttpContext.Current.Response;

            response.Clear();
            response.StatusCode = 403;

            RequestDebug.LogRequestOperation("403Forbidden", null, 1);
            response.End();
        }


        /// <summary>
        /// Sets the 404 status (Not Found) and ends the response.
        /// </summary>
        public static void Respond404()
        {
            var response = CMSHttpContext.Current.Response;

            response.Clear();
            response.StatusCode = 404;

            RequestDebug.LogRequestOperation("404PageNotFound", null, 1);
            response.End();
        }


        /// <summary>
        /// Sets the 500 status (Internal Server Error) and ends the response.
        /// </summary>
        public static void Respond500()
        {
            var response = CMSHttpContext.Current.Response;

            response.Clear();
            response.StatusCode = 500;

            RequestDebug.LogRequestOperation("500ServerError", null, 1);
            response.End();
        }


        /// <summary>
        /// Sets the 503 status (Service Unavailable) and ends the response.
        /// </summary>
        public static void Respond503()
        {
            var response = CMSHttpContext.Current.Response;

            response.Clear();
            response.StatusCode = 503;

            RequestDebug.LogRequestOperation("503ServerError", null, 1);
            response.End();
        }


        /// <summary>
        /// Ends current response (calls Response.End).
        /// </summary>
        public static void EndResponse()
        {
            RequestDebug.LogRequestOperation("ResponseEnd", null, 1);
            CMSHttpContext.Current.Response.End();
        }


        /// <summary>
        /// Completes current request (calls ApplicationInstance.CompleteRequest).
        /// </summary>
        public static void CompleteRequest()
        {
            RequestDebug.LogRequestOperation("CompleteRequest", null, 1);
            CMSHttpContext.Current.ApplicationInstance.CompleteRequest();
        }


        /// <summary>
        /// Set script timeout due to compilation delay
        /// </summary>
        public static void EnsureScriptTimeout()
        {
            var context = CMSHttpContext.Current;
            if ((context != null) && (context.Server.ScriptTimeout < 240))
            {
                context.Server.ScriptTimeout = 240;
            }
        }

        #endregion


        #region "Browser methods"

        /// <summary>
        /// Gets the browser string from current request.
        /// </summary>
        [Obsolete("Method will be removed. Use BrowserHelper.GetBrowser() instead.")]
        public static string GetBrowser()
        {
            return BrowserHelper.GetBrowser();
        }


        /// <summary>
        /// Returns true if current browser is Chrome.
        /// </summary>
        [Obsolete("Method will be removed. Use BrowserHelper.IsChrome() instead.")]
        public static bool IsChrome()
        {
            return BrowserHelper.IsChrome();
        }


        /// <summary>
        /// Returns true if current browser is Safari.
        /// </summary>
        [Obsolete("Method will be removed. Use BrowserHelper.IsSafari() instead.")]
        public static bool IsSafari()
        {
            return BrowserHelper.IsSafari();
        }

        #endregion


        #region "Authentication methods"

        /// <summary>
        /// Returns true if the authentication mode is Windows authentication.
        /// </summary>
        public static bool IsWindowsAuthentication()
        {
            // Default checking
            if (AuthenticationTypeCheck == AUTH_TYPE_DEFAULT)
            {
                // Get authentication mode from current request
                var context = CMSHttpContext.Current;
                if ((context != null) && (context.User != null))
                {
                    var identity = context.User.Identity;
                    return (identity.AuthenticationType != "Forms") 
                        && !String.IsNullOrEmpty(identity.AuthenticationType)
                        && (identity is WindowsIdentity);
                }

                // Return false if context is not available
                return false;
            }

            // Return true if type of authentication check is set to Windows or Both
            return ((AuthenticationTypeCheck == AUTH_TYPE_WINDOWS) || (AuthenticationTypeCheck == AUTH_TYPE_BOTH));
        }


        /// <summary>
        /// Returns true if the authentication mode is Forms authentication.
        /// </summary>
        public static bool IsFormsAuthentication()
        {
            if (AuthenticationTypeCheck == AUTH_TYPE_DEFAULT)
            {
                // Get authentication mode from current request
                var context = CMSHttpContext.Current;
                if ((context != null) && (context.User != null))
                {
                    return (context.User.Identity.AuthenticationType == "Forms") || String.IsNullOrEmpty(context.User.Identity.AuthenticationType);
                }

                // Return false if context is not available
                return false;
            }

            // Return true if type of authentication check is set to Forms or Both
            return ((AuthenticationTypeCheck == AUTH_TYPE_FORMS) || (AuthenticationTypeCheck == AUTH_TYPE_BOTH));
        }


        /// <summary>
        /// If true, mixed authentication is used.
        /// </summary>
        public static bool IsMixedAuthentication()
        {
            try
            {
                return (Membership.Providers["CMSADProvider"] != null) && (Roles.Providers["CMSADRoleProvider"] != null);
            }
            catch(ConfigurationException ex)
            {
                CoreServices.EventLog.LogException("MixedAuthentication", "CHECK", ex);
                return false;
            }
        }

        #endregion


        #region "WebDAV methods"

        ///<summary>
        /// Determines whether current request is WebDAV request
        ///</summary>
        ///<returns>TRUE if request is WebDAV request</returns>
        public static bool IsWebDAVRequest()
        {
            var context = CMSHttpContext.Current;
            if (context == null)
            {
                return false;
            }

            string userAgent = context.Request.Headers["User-Agent"];

            // Check 'microsoft-webdav-miniredir'
            if (!String.IsNullOrEmpty(userAgent))
            {
                return userAgent.ToLowerInvariant().Contains("microsoft-webdav-miniredir");
            }

            return false;
        }


        /// <summary>
        /// Determines whether current request is WebDAV PROPFIND request and url is application path.
        /// </summary>
        public static bool IsWebDAVPropfindRequest()
        {
            // Check PROPFIND method 
            if (HttpMethod.Equals(REQUEST_PROPFIND, StringComparison.OrdinalIgnoreCase))
            {
                // Check if raw URL is the same as application path and request is from WebDAV
                return RequestContext.RawURL.Equals(SystemContext.ApplicationPath, StringComparison.InvariantCultureIgnoreCase) && IsWebDAVRequest();
            }

            return false;
        }

        #endregion
    }
}