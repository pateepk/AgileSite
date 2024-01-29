using CMS.Base;
using CMS.SiteProvider;

namespace CMS.Membership
{
    /// <summary>
    /// Authentication handler
    /// </summary>
    public class AuthenticationHandler : SimpleHandler<AuthenticationHandler, AuthenticationEventArgs>
    {
        /// <summary>
        /// Initiates the event handling
        /// </summary>
        /// <param name="userInfo">Current info of the authenticated user (through regular authentication)</param>
        /// <param name="username">Username to authenticate</param>
        /// <param name="password">Password to authenticate</param>
        /// <param name="passcode">Passcode to authenticate</param>
        /// <param name="siteName">Name of the site to which the user tries to autenticate. If the parameter is not set, current site name from context is used</param>
        public AuthenticationEventArgs StartEvent(ref UserInfo userInfo, string username, string password = "", string passcode = "", string siteName = null)
        {
            var e = new AuthenticationEventArgs
            {
                User = userInfo,
                UserName = username,
                Password = password,
                Passcode = passcode,
                SiteName = siteName ?? SiteContext.CurrentSiteName
            };

            StartEvent(e);

            userInfo = e.User;

            return e;
        }
    }
}