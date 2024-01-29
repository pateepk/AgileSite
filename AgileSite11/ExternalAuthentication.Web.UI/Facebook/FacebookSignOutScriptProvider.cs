using System;
using System.Web.UI;

using CMS.Base.Web.UI;
using CMS.Membership;
using CMS.Membership.Web.UI;
using CMS.SiteProvider;

namespace CMS.ExternalAuthentication.Web.UI
{
    /// <summary>
    /// Class whose instance should be registered as a custom logout script provider
    /// </summary>
    internal class FacebookSignOutScriptProvider : ICustomSignOutScriptProvider
    {
        /// <summary>
        /// Builds and returns Facebook logout script which calls the given callback.
        /// </summary>
        /// <param name="finishCallBack">Callback method to be called when this Facebook script finishes</param>
        /// <param name="page">Page to which helper scripts can be registered</param>
        /// <returns>Facebook logout script which calls the given callback or null if no script is required by this provider</returns>
        public string GetSignOutScript(string finishCallBack, Page page)
        {
            if (FacebookConnectHelper.FacebookIsAvailable(SiteContext.CurrentSiteName) && (MembershipContext.AuthenticatedUser != null))
            {
                bool includeLogoutScript = (!String.IsNullOrWhiteSpace(MembershipContext.AuthenticatedUser.UserSettings.UserFacebookID));
                if (!includeLogoutScript && MembershipContext.CurrentUserIsImpersonated())
                {
                    // Make sure impersonator gets logged out of Facebook
                    UserInfo originalUser = MembershipContext.GetImpersonatingUser();

                    includeLogoutScript = (originalUser != null) && (!String.IsNullOrWhiteSpace(originalUser.UserSettings.UserFacebookID));
                }

                if (includeLogoutScript)
                {
                    ScriptHelper.RegisterStartupScript(page, typeof(string), "FacebookInitScript", FacebookConnectScriptHelper.GetFacebookInitScriptForSite(SiteContext.CurrentSiteName));

                    return FacebookConnectScriptHelper.GetFacebookLogoutScriptForSignOut(finishCallBack + "();");
                }
            }

            return null;
        }
    }
}
