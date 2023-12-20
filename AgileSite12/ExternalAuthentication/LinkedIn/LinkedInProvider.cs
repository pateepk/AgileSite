using System;
using System.Threading;

using CMS.EventLog;
using CMS.ExternalAuthentication.LinkedIn;
using CMS.Helpers;

namespace CMS.ExternalAuthentication
{
    /// <summary>
    /// LinkedInProvider class
    /// </summary>
    public static class LinkedInProvider
    {
        /// <summary>
        /// Access token page.
        /// </summary>
        public const string ACCESS_TOKEN_PAGE = "~/CMSModules/Membership/Pages/LinkedIn/LinkedInAccessTokenPage.aspx";

        /// <summary>
        /// Key for storing data in session.
        /// </summary>
        private const string SESSION_KEY = "MembershipProviderLinkedInTokenManager";


        /// <summary>
        /// Returns instance of <see cref="ILinkedInData"/> retrieved from session or created from current request.
        /// </summary>
        public static ILinkedInData GetLinkedInData()
        {
            var data = SessionHelper.GetValue(SESSION_KEY) as ILinkedInData;
            data = data ?? new LinkedInData();

            return data;
        }


        /// <summary>
        /// Attempts to authorize client application.
        /// </summary>
        /// <param name="data">Data required for LinkedIn authorization.</param>
        /// <param name="returnUrl">Url used for redirect once the LinkedIn access token request is received.</param>
        /// <param name="token">If <c>true</c> is returned instance of <see cref="LinkedInAccessToken"/> is available.</param>
        /// <returns>True when given data are valid, false otherwise</returns>
        public static bool Authorize(ILinkedInData data, Uri returnUrl, out LinkedInAccessToken token)
        {
            token = null;

            var authorization = new LinkedInAuthorization(data);

            try
            {
                token = authorization.Authorize(returnUrl);
            }
            catch (Exception exception)
            {
                LogException(exception);
            }
            finally
            {
                RemoveSessionEntry();
            }

            return token != null;
        }


        /// <summary>
        /// Performs redirect to LinkedIn login page.
        /// </summary>
        /// <param name="data">Data required for redirect.</param>
        /// <param name="returnUrl">Url used for redirect once the LinkedIn login form is submitted.</param>
        public static void OpenAuthorizationPage(ILinkedInData data, Uri returnUrl)
        {
            SessionHelper.SetValue(SESSION_KEY, data);

            try
            {
                var authorization = new LinkedInAuthorization(data);
                authorization.OpenAuthorizationPage(returnUrl);
            }
            catch (ThreadAbortException)
            {
                // Reset exception - this exception is expected because client is redirected to external page.
                Thread.ResetAbort();
            }
            catch (Exception exception)
            {
                LogException(exception);
                RemoveSessionEntry();
            }
        }


        private static void LogException(Exception exception)
        {
            EventLogProvider.LogException("MembershipProvider", nameof(LinkedInProvider), exception);
        }


        private static void RemoveSessionEntry()
        {
            SessionHelper.Remove(SESSION_KEY);
        }
    }
}
