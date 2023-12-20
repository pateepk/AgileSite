using System;
using System.Web;

using CMS.EventLog;
using CMS.Helpers;

using Microsoft.IdentityModel.Protocols.WSFederation;

namespace CMS.WIFIntegration
{
    /// <summary>
    /// Support base class for other WIF classes.
    /// </summary>
    internal abstract class AbstractWIFAuthentication
    {
        #region "Variables"

        private Settings mSettings = null;

        #endregion


        #region "Private properties"

        /// <summary>
        /// Module settings.
        /// </summary>
        protected Settings Settings
        {
            get { return mSettings ?? (mSettings = new Settings()); }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Logs authentication fail and redirects to error page to let user know that something went wrong.
        /// </summary>
        /// <param name="message">Error message.</param>
        protected void LogErrorAndRedirect(string message)
        {
            EventLogProvider.LogEvent(EventType.ERROR, "WIF Integration", "WIF_AUTH_FAIL", message);

            var context = CMSHttpContext.Current;
            if (context != null)
            {
                context.Server.Transfer(AdministrationUrlHelper.GetErrorPageUrl("wif.error.title", "wif.error.text"));
            }
        }
        

        /// <summary>
        /// Sets referrer HTTP header.
        /// </summary>
        protected static void SetReferrer()
        {
            var response = CMSHttpContext.Current.Response;
            response.Headers["Referrer"] = URLHelper.GetApplicationUrl();
        }


        /// <summary>
        /// Creates sign in URL using the specified base URI, wtrealm parameter and wreply parameter.
        /// </summary>
        /// <param name="baseUrl">The Base URL to identity provider.</param>
        /// <param name="realm">The value of the wtrealm message parameter.</param>
        /// <param name="reply">The URI to which to reply. (The value of the wreply message parameter.)</param>
        protected static string GetSignInUrl(Uri baseUrl, string realm, string reply)
        {
            return GetRedirectUrlToIdentityProvider(baseUrl, "wsignin1.0", realm, reply);
        }


        /// <summary>
        /// Creates sign out URL using the specified base URI, wtrealm parameter and wreply parameter.
        /// </summary>
        /// <param name="baseUrl">The Base URL to identity provider.</param>
        /// <param name="realm">The value of the wtrealm message parameter.</param>
        /// <param name="reply">The URI to which to reply. (The value of the wreply message parameter.)</param>
        protected static string GetSignOutUrl(Uri baseUrl, string realm, string reply)
        {
            return GetRedirectUrlToIdentityProvider(baseUrl, "wsignout1.0", realm, reply);
        }


        /// <summary>
        /// Creates URL using the specified base URI, wa parameter, wtrealm parameter and wreply parameter.
        /// </summary>
        /// <param name="baseUrl">The Base URL to identity provider.</param>
        /// <param name="action">The action to execute ("wsignin1.0", "wsignout1.0").</param>
        /// <param name="realm">The value of the wtrealm message parameter.</param>
        /// <param name="reply">The URI to which to reply. (The value of the wreply message parameter.)</param>
        private static string GetRedirectUrlToIdentityProvider(Uri baseUrl, string action, string realm, string reply)
        {
            if (baseUrl == null)
            {
                throw new ArgumentNullException("baseUrl");
            }

            if (String.IsNullOrEmpty(realm) && String.IsNullOrEmpty(reply))
            {
                throw new WSFederationMessageException("[AbstractWIFAuthentication.GetRedirectUrlToIdentityProvider] WS-Federation SignIn request must specify a 'wtrealm' or 'wreply' parameter.");
            }

            var url = baseUrl.AbsoluteUri;

            url = URLHelper.AddParameterToUrl(url, "wa", action);
            if (!String.IsNullOrEmpty(realm))
            {
                url = URLHelper.AddParameterToUrl(url, "wtrealm", HttpUtility.HtmlAttributeEncode(realm));
            }
            if (!String.IsNullOrEmpty(reply))
            {
                url = URLHelper.AddParameterToUrl(url, "wreply", HttpUtility.HtmlAttributeEncode(reply));
            }

            return url;
        }

        #endregion
    }
}
