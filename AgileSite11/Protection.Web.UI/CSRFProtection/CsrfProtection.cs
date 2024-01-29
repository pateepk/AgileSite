using System;
using System.Linq;
using System.Security.Cryptography;
using System.Web;
using System.Web.Security;
using System.Web.UI;

using CMS.Helpers;
using CMS.Base;
using CMS.OutputFilter;

namespace CMS.Protection.Web.UI
{
    /// <summary>
    /// Implementation of the stateless CSRF defense based on comparing hidden field value with cookie value.
    /// </summary>
    internal sealed class CsrfProtection
    {
        #region "Variables"

        /// <summary>
        /// Defines the name of the hidden field used to store CSRF token.
        /// </summary>
        internal const string HIDDEN_FIELD_NAME = "__CMSCsrfToken";


        /// <summary>
        /// Defines the key derivation string used by MachineKey.Protect and MachineKey.Unprotect methods.
        /// The 'purposes' parameter helps ensure that some protected data can be consumed only by the
        /// component that originally generated it. Applications should take care to ensure that each
        /// subsystem uses a unique 'purposes' list.
        /// </summary>
        internal const string PURPOSES = "CSRF token protection";

        // Singleton instance locking object
        private static readonly object instanceLock = new object();

        // Singleton instance 
        private static CsrfProtection mInstance;

        #endregion


        #region "Properties"

        /// <summary>
        /// Singleton instance of <see cref="CsrfProtection"/> class.
        /// </summary>
        private static CsrfProtection Instance
        {
            get
            {
                if (mInstance == null)
                {
                    lock (instanceLock)
                    {
                        if (mInstance == null)
                        {
                            mInstance = new CsrfProtection();
                        }
                    }
                }
                return mInstance;
            }
        }

        #endregion


        #region "Application events"

        /// <summary>
        /// Registers event handler.
        /// </summary>
        internal static void PreInit()
        {
            ApplicationEvents.PreInitialized.Execute += (sender, e) =>
            {
                // CSRF protection must not be disabled in application config
                if (ValidationHelper.GetBoolean(SettingsHelper.AppSettings["CMSEnableCsrfProtection"], true))
                {
                    RequestEvents.PostMapRequestHandler.Execute += Instance.OnPostMapRequestHandlerExecute;

                    OutputFilterEvents.SaveOutputToCache.Before += Instance.CreateSubstitutionForCsrfToken;
                    ResponseOutputFilter.OnResolveSubstitution += Instance.EnsureCorrectCsrfToken;
                }
            };
        }


        private void EnsureCorrectCsrfToken(object sender, SubstitutionEventArgs e)
        {
            if (!e.Match)
            {
                if (e.Expression == HIDDEN_FIELD_NAME)
                {
                    var token = EnsureCsrfToken();
                    // Add correct CSRF token
                    e.Result = GetProtectedCsrfToken(token);
                    e.Match = true;
                }
            }
        }


        private void CreateSubstitutionForCsrfToken(object sender, OutputCacheEventArgs e)
        {
            string csrfFieldPattern = @"(<input type=""hidden"" name=""{0}"" id=""{0}"" value="")(.+)("" />)";
            var regex = RegexHelper.GetRegex(String.Format(csrfFieldPattern, HIDDEN_FIELD_NAME));

            e.Output.OutputData.Html = regex.Replace(e.Output.OutputData.Html, "$1{~" + HIDDEN_FIELD_NAME + "~}$3");
        }


        private void OnPostMapRequestHandlerExecute(object sender, EventArgs eventArgs)
        {
            // Apply CSRF protection only within the main application
            if ((SystemContext.IsCMSRunningAsMainApplication) && (CMSHttpContext.Current.Handler != null))
            {
                Page page = CMSHttpContext.Current.Handler as Page;

                // Apply CSRF protection only if CSRF protection is not disabled for the current page
                if (IsCsrfProtectionEnabledOnPage(page))
                {
                    // Ensure a valid CSRF token exists in the cookie, otherwise create a new one
                    var csrfToken = EnsureCsrfToken();
                    page.PreInit += (s, e) => ValidateTokenInPage(csrfToken);
                    page.PreRender += (s, e) => IncludeTokenInPage(page, csrfToken);
                }
            }
        }


        private void ValidateTokenInPage(byte[] csrfToken)
        {
            HttpRequestBase currentRequest = CMSHttpContext.Current.Request;

            // If it is a POST request and the request is not an AJAX request
            if (IsVulnerableRequest(currentRequest))
            {
                // Get the hidden field value
                string hiddenFieldBase64Value = currentRequest.Form[HIDDEN_FIELD_NAME];

                if (String.IsNullOrEmpty(hiddenFieldBase64Value))
                {
                    ThrowCsrfException("The CSRF hidden field was missing.");
                }

                byte[] hiddenFieldToken = null;

                try
                {
                    hiddenFieldToken = MachineKey.Unprotect(Convert.FromBase64String(hiddenFieldBase64Value), PURPOSES);
                }
                catch (Exception exception)
                {
                    // Either the hidden field value was tampered with or the application is running on web farms 
                    // that are not configured properly (i.e. auto-generated encryption keys (machine keys)).
                    ThrowCsrfException("The CSRF hidden field was malformed.", exception);
                }

                ValidateCsrfTokens(csrfToken, hiddenFieldToken);
            }
        }

        #endregion


        #region "Public methods"

        /// <summary>
        /// Validates the hidden field and cookie tokens.
        /// </summary>
        /// <param name="cookieToken">Cookie token</param>
        /// <param name="hiddenFieldToken">Hidden field token</param>
        /// <exception cref="CsrfException">Thrown when <paramref name="cookieToken"/> or <paramref name="hiddenFieldToken"/> is null or tokens do not match.</exception>
        public void ValidateCsrfTokens(byte[] cookieToken, byte[] hiddenFieldToken)
        {
            if (cookieToken == null)
            {
                throw new ArgumentNullException(nameof(cookieToken));
            }

            if (hiddenFieldToken == null)
            {
                throw new ArgumentNullException(nameof(hiddenFieldToken));
            }

            // This is thrown in case the tokens do not match. The reasons can be:
            //  - the request was made from a page which loaded while an older session was still active (i.e. old browser tabs)
            //  - CSRF attempts using valid protected CSRF tokens (i.e. spoofed hidden field value using older tokens)
            if ((cookieToken.Length != hiddenFieldToken.Length) || !cookieToken.SequenceEqual(hiddenFieldToken))
            {
                ThrowCsrfException("The CSRF hidden field value didn't match the CSRF cookie value.");
            }
        }


        /// <summary>
        /// Ensures the CSRF token exists. If not it will create a new one.
        /// </summary>
        /// <exception cref="CsrfException">Thrown when the current request is vulnerable and the CSRF cookie is missing or malformed.</exception> 
        public byte[] EnsureCsrfToken()
        {
            byte[] csrfToken;
            HttpRequestBase currentRequest = CMSHttpContext.Current.Request;

            string csrfCookieValue = CookieHelper.GetValue(CookieName.CsrfCookie);

            if (String.IsNullOrEmpty(csrfCookieValue))
            {
                if (IsVulnerableRequest(currentRequest))
                {
                    ThrowCsrfException("The CSRF cookie was missing.");
                }

                csrfToken = EnsureCsrfTokenInternal();
            }
            else
            {
                try
                {
                    csrfToken = Convert.FromBase64String(csrfCookieValue);
                }
                catch (FormatException ex)
                {
                    if (IsVulnerableRequest(currentRequest))
                    {
                        ThrowCsrfException("The CSRF cookie was malformed.", ex);
                    }

                    csrfToken = EnsureCsrfTokenInternal();
                }
            }

            return csrfToken;
        }


        /// <summary>
        /// Sets a newly created CSRF token into the corresponding cookie. Returns the CSRF token.
        /// </summary>
        internal byte[] EnsureCsrfTokenInternal()
        {
            var csrfToken = CreateCsrfToken();

            // Create a session cookie and set token as a cookie value
            CookieHelper.SetValue(CookieName.CsrfCookie, Convert.ToBase64String(csrfToken), DateTime.MinValue);

            return csrfToken;
        }


        /// <summary>
        /// Creates the CSRF token.
        /// </summary>
        public byte[] CreateCsrfToken()
        {
            // Create new token
            using (var rng = new RNGCryptoServiceProvider())
            {
                // Create random key
                var buffer = new byte[30];
                rng.GetBytes(buffer);

                // Return new key
                return buffer;
            }
        }


        #endregion


        #region "Internal methods"

        /// <summary>
        /// Indicates whether specified request should be checked for CSRF attack vulnerability.
        /// </summary>
        /// <remarks>
        /// Only POST requests which do not contain XMLHttpRequest HTTP header or PostBack requests detected by <see cref="Page.IsPostBack"/> are considered as vulnerable.
        /// AJAX requests do not contain CSRF hidden field in the POST body therefore CSRF protection cannot be applied.
        /// All modern browsers deny cross-domain AJAX requests due to CORS implementation - CSRF is not possible.
        /// </remarks>
        /// <param name="httpRequest"><see cref="HttpRequestBase"/> instance.</param>
        /// <returns>True if request should be checked; otherwise false.</returns>
        internal bool IsVulnerableRequest(HttpRequestBase httpRequest)
        {
            var page = CMSHttpContext.Current?.Handler as Page;

            return ((httpRequest != null)
#pragma warning disable BH1011 // 'Page.IsPostBack' should not be used. Use 'RequestHelper.IsPostBack()' instead.
                && (httpRequest.HttpMethod.Equals("POST", StringComparison.OrdinalIgnoreCase) || ValidationHelper.GetBoolean(page?.IsPostBack, false))
#pragma warning restore BH1011 // 'Page.IsPostBack' should not be used. Use 'RequestHelper.IsPostBack()' instead.
                && (!IsAjaxRequest(httpRequest)));

        }


        /// <summary>
        /// Checks whether the current HTTP request is an AJAX request (contains "X-Requested-With: XMLHttpRequest" HTTP header).
        /// </summary>
        /// <param name="httpRequest"><see cref="HttpRequestBase"/> instance.</param>
        /// <returns>True if request is an AJAX request; otherwise false.</returns>
        private bool IsAjaxRequest(HttpRequestBase httpRequest)
        {
            if (httpRequest == null)
            {
                throw new ArgumentNullException(nameof(httpRequest));
            }

            if (httpRequest.Headers["X-Requested-With"] != null)
            {
                return httpRequest.Headers["X-Requested-With"].Equals("XMLHttpRequest", StringComparison.OrdinalIgnoreCase);
            }

            return false;
        }


        /// <summary>
        /// Registers CSRF token in hidden field and includes it into page.
        /// </summary>
        /// <param name="page">Page the CSRF token should be includes to.</param>
        /// <param name="csrfToken">CSRF token.</param>
        internal void IncludeTokenInPage(Page page, byte[] csrfToken)
        {
            // Protect the cookie token
            var token = GetProtectedCsrfToken(csrfToken);

            CsrfProtectionHelper.RegisterHiddenField(page, HIDDEN_FIELD_NAME, token);
        }


        private string GetProtectedCsrfToken(byte[] csrfToken)
        {
            var token = MachineKey.Protect(csrfToken, PURPOSES);

            return Convert.ToBase64String(token);
        }


        /// <summary>
        /// Throws a CSRF exception.
        /// </summary>
        internal void ThrowCsrfException(string message, Exception innerException = null)
        {
            throw new CsrfException(message, innerException);
        }


        /// <summary>
        /// Checks whether CSRF protection is enabled on the page.
        /// Disabling CSRF protection on the page should be used only as a workaround.
        /// </summary>
        /// <param name="page"><see cref="Page"/>instance</param>
        /// <returns>True if page context does not contain disabled CSRF protection flag, otherwise false.</returns>
        internal bool IsCsrfProtectionEnabledOnPage(Page page)
        {
            return page != null;
        }

        #endregion
    }
}