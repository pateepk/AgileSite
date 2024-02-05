using System;
using System.Web;

using CMS.Base;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Protection;
using CMS.SiteProvider;

namespace CMS.Membership
{
    /// <summary>
    /// Membership handlers
    /// </summary>
    internal class MembershipHandlers
    {
        /// <summary>
        /// Initializes the membership handlers
        /// </summary>
        public static void Init()
        {
            CultureSiteInfo.TYPEINFO.Events.Delete.After += DeleteUserCultures;

            if (SystemContext.IsCMSRunningAsMainApplication)
            {
                ApplicationEvents.Initialized.Execute += HandleAdminEmergencyReset;

                ApplicationEvents.SessionEnd.Execute += RemoveExpiredSessions;
                ApplicationEvents.SessionStart.Execute += SessionStart;

                RequestEvents.Authenticate.Execute += HandleAutomaticSignIn;
                RequestEvents.PostAcquireRequestState.Execute += UpdateSession;
                RequestEvents.End.Execute += HandleAuthenticationRedirect;
            }
        
            SecurityEvents.Authenticate.Execute += CheckBannedIP;
        }


        /// <summary>
        /// Handles BannedIPs checking.
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Authentication event arguments</param>
        private static void CheckBannedIP(object sender, AuthenticationEventArgs e)
        {
            if (!BannedIPInfoProvider.IsAllowed(e.SiteName, BanControlEnum.Login))
            {
                e.User = null;
                MembershipContext.UserIsBanned = true;
            }
        }
        

        /// <summary>
        /// Handles admin emergency reset 
        /// </summary>
        private static void HandleAdminEmergencyReset(object sender, EventArgs e)
        {
            AuthenticationHelper.HandleAdminEmergencyReset();
        }


        /// <summary>
        /// Handles the authentication redirect within current request
        /// </summary>
        private static void HandleAuthenticationRedirect(object sender, EventArgs e)
        {
            if (AuthenticationHelper.IsAuthenticationRedirect())
            {
                var requestedUrl = URLHelper.GetAbsoluteUrl("~" + RequestContext.CurrentRelativePath, RequestContext.CurrentDomain);

                SecurityEvents.AuthenticationRequested.StartEvent(requestedUrl);
            }
        }


        /// <summary>
        /// Updates the session with current request information
        /// </summary>
        private static void UpdateSession(object sender, EventArgs e)
        {
            UpdateSession();
        }


        /// <summary>
        /// Updates the session with current request information for content pages not sent from cache
        /// </summary>
        private static void UpdateSession()
        {
            if (RequestContext.IsContentPage && (RequestContext.CurrentStatus != RequestStatusEnum.SentFromCache))
            {
                if ((HttpContext.Current.Session != null) && SessionManager.OnlineUsersEnabled && (RequestContext.CurrentExcludedStatus == ExcludedSystemEnum.NotExcluded))
                {
                    SessionManager.UpdateCurrentSession(SiteContext.CurrentSiteName);
                }
            }
        }


        /// <summary>
        /// Handles automatic sign-in for single sign-on
        /// </summary>
        private static void HandleAutomaticSignIn(object sender, EventArgs e)
        {
            // Check for automatic sign-in authentication token
            AuthenticationHelper.HandleAutomaticSignIn();
        }


        /// <summary>
        /// Session start actions
        /// </summary>
        private static void SessionStart(object sender, EventArgs e)
        {
            // Initialize cultures for windows user authentication
            AuthenticationHelper.SetWindowsUserCultures(SiteContext.CurrentSiteName);

            UpdateSession();
        }


        /// <summary>
        /// Removes the expired sessions from the database
        /// </summary>
        private static void RemoveExpiredSessions(object sender, EventArgs e)
        {
            if (DatabaseHelper.IsDatabaseAvailable && SessionManager.OnlineUsersEnabled)
            {
                using (var scope = new CMSConnectionScope(true))
                {
                    SessionManager.RemoveExpiredSessions();
                }
            }
        }


        /// <summary>
        /// Deletes the user-culture bindings when the site-culture binding is deleted
        /// </summary>
        private static void DeleteUserCultures(object sender, ObjectEventArgs e)
        {
            CultureSiteInfo csi = (CultureSiteInfo)e.Object;

            var totalRecords = 0;
            ConnectionHelper.ExecuteQuery(UserCultureInfo.OBJECT_TYPE + ".deleteall", null, "SiteID = " + csi.SiteID + " AND CultureID = " + csi.CultureID, null, -1, null, -1, -1, ref totalRecords);
        }
    }
}
