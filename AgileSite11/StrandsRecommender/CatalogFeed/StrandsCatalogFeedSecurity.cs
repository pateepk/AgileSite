using System;
using System.Linq;
using System.Text;
using System.Web;

using CMS.Base;
using CMS.Helpers;
using CMS.SiteProvider;

namespace CMS.StrandsRecommender
{
    /// <summary>
    /// Handles credentials verifying for Strands Recommender catalog.
    /// </summary>
    internal class StrandsCatalogFeedSecurity
    {
        /// <summary>
        /// Checks whether user is properly authenticated with username and password set in Strands settings. If user is not
        /// authenticated and
        /// username or password are set in Strands settings, user is prompted to authenticate and false is returned.
        /// </summary>
        /// <param name="context">HTTP context</param>
        /// <returns>True if authentication is disabled or user is properly authenticated; otherwise false</returns>
        public static bool CheckCredentials(HttpContext context)
        {
            string siteName = SiteContext.CurrentSiteName;

            if (!StrandsSettings.IsCatalogFeedAuthenticationEnabled(siteName))
            {
                return true;
            }

            string username;
            string password;

            if (SecurityHelper.TryParseBasicAuthorizationHeader(context.Request.Headers["Authorization"], out username, out password))
            {
                if (username.EqualsCSafe(StrandsSettings.GetCatalogFeedUsername(siteName), true) &&
                    (password == StrandsSettings.GetCatalogFeedPassword(siteName)))
                {
                    return true;
                }
            }

            // When unauthorized reading prompt is made, change status code to 319 (unused by HTTP 1.1). This status code will be checked at the end of the request. 
            // If status code is 319 at that point, status code is changed to 401 and Basic Authentication headers are added. This is the workaround for automatically redirection of 401 request to Logon page.
            context.Response.StatusCode = 319;

            return false;
        }
        

        /// <summary>
        /// Checks whether current status code is 319. If true, sets response status code to 401 Unauthorized, so the browser will prompt user to enter credentials.
        /// </summary>
        public static void SetUnauthorizedStatus()
        {
            // It is possible that this code is executed more than once (when multiple requests attached to the RequestEvents.End.Before before the first one fired), so 
            // the check if status code was already set is made which will eliminate setting headers multiple times.
            var httpResponse = HttpContext.Current.Response;
            if (httpResponse.StatusCode == 319)
            {
                httpResponse.StatusCode = 401;
                httpResponse.AddHeader("WWW-Authenticate", "Basic realm=\"Provide username and password specified in Site Manager -> Settings -> Integration -> Strands Recommender settings\"");
            }
        }
    }
}
