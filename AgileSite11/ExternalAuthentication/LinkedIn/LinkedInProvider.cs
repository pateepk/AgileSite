using System;
using System.Collections.Generic;
using System.Threading;

using CMS.EventLog;
using CMS.Helpers;

using CMS.ExternalAuthentication.LinkedIn;

namespace CMS.ExternalAuthentication
{
    /// <summary>
    /// LinkedInProvider class
    /// </summary>
    public static class LinkedInProvider
    {
        #region "Constants"

        /// <summary>
        /// Access token page.
        /// </summary>
        public const string ACCESS_TOKEN_PAGE = "~/CMSModules/Membership/Pages/LinkedIn/LinkedInAccessTokenPage.aspx";

        #endregion


        #region "Properties"

        /// <summary>
        /// Token manager
        /// </summary>
        private static TokenManager TokenManager
        {
            get
            {
                // Try to get TokenManager from session and determine if this is a new authorization process
                TokenManager tokenManager = SessionHelper.GetValue("MembershipProviderLinkedInTokenManager") as TokenManager;
                var needNewTokenManager = RequestContext.CurrentQueryString.Contains("apiKey") && RequestContext.CurrentQueryString.Contains("apiSecret");
                
                if (tokenManager == null || needNewTokenManager)
                {
                    string requestUrl = RequestContext.URL.ToString();
                    string consumerKey = URLHelper.GetQueryValue(requestUrl, "apiKey");
                    string consumerSecret = URLHelper.GetQueryValue(requestUrl, "apiSecret");

                    if (string.IsNullOrEmpty(consumerKey) == false)
                    {
                        tokenManager = new TokenManager(consumerKey, consumerSecret);
                        SessionHelper.SetValue("MembershipProviderLinkedInTokenManager", tokenManager);
                    }
                }
                return tokenManager;
            }
        }

        #endregion


        #region "Public methods"

        /// <summary>
        /// Authorizes client application for using user data.
        /// </summary>
        /// <param name="txtToken">Token client ID.</param>
        public static Dictionary<string, string> Authorize(string txtToken)
        {
            string accessToken = null;
            Dictionary<string, string> tokens = new Dictionary<string, string>();

            var redirectUrl = new Uri(URLHelper.GetAbsoluteUrl(ACCESS_TOKEN_PAGE + "?txtToken=" + txtToken));
            var authorization = new LinkedInAuthorization(TokenManager, accessToken, redirectUrl);

            if (String.IsNullOrEmpty(URLHelper.GetQueryValue(RequestContext.CurrentURL, "code")))
            {              
                try
                {
                    // Redirect to LinkedIn OAuth authorization page
                    authorization.BeginAuthorize(redirectUrl);
                }
                catch (ThreadAbortException)
                {
                    // Reset exception - this exception is expected because client is redirected to external page.
                    Thread.ResetAbort();
                }
                catch (Exception ex)
                {
                    // Log exception
                    if (ex is DotNetOpenAuth.Messaging.ProtocolException)
                    {
                        EventLogProvider.LogException("MembershipProvider", "LinkedInProvider", ex.InnerException);
                    }
                    else
                    {
                        EventLogProvider.LogException("MembershipProvider", "LinkedInProvider", ex);
                    }

                    // Destroy token manager from session
                    SessionHelper.Remove("MembershipProviderLinkedInTokenManager");
                }
            }
            else
            {
                // Complete authorization
                accessToken = authorization.CompleteAuthorize();
                if (!String.IsNullOrEmpty(accessToken))
                {
                    // Return access token and access token secret
                    tokens["AccessToken"] = accessToken;

                    // Destroy token manager from session
                    SessionHelper.Remove("MembershipProviderLinkedInTokenManager");
                }
            }

            return tokens;
        }

        #endregion
    }
}
