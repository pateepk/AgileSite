using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;

using CMS.DataEngine;
using CMS.Helpers;
using CMS.LicenseProvider;
using CMS.Base;
using CMS.SiteProvider;

namespace CMS.Membership
{
    /// <summary>
    /// Class provides session management.
    /// </summary>
    public static class SessionManager
    {
        private static readonly object mRemoveExpiredSessionLock = new object();


        #region "Public properties"

        /// <summary>
        /// Returns number of public users on current site.
        /// </summary>
        public static int PublicUsers
        {
            get
            {
                // Get the site name
                string siteName = SiteContext.CurrentSiteName;
                if (!String.IsNullOrEmpty(siteName))
                {
                    return SessionsCachingWorker.GetPublicUsersCount(siteName);
                }

                return 0;
            }
        }


        /// <summary>
        /// Returns number of authenticated users on current site.
        /// </summary>
        public static int AuthenticatedUsers
        {
            get
            {
                // Get the site name
                string siteName = SiteContext.CurrentSiteName;
                if (!String.IsNullOrEmpty(siteName))
                {
                    return SessionsCachingWorker.GetAuthenticatedUsersCount(siteName);
                }

                return 0;
            }
        }


        /// <summary>
        /// Returns number of hidden users on current site.
        /// </summary>
        public static int HiddenUsers
        {
            get
            {
                // Get the site name
                string siteName = SiteContext.CurrentSiteName;
                if (!String.IsNullOrEmpty(siteName))
                {
                    return SessionsCachingWorker.GetHiddenUsersCount(siteName);
                }

                return 0;
            }
        }


        /// <summary>
        /// Gets the value that indicates whether online users should be saved in the database.
        /// </summary>
        public static bool StoreOnlineUsersInDatabase
        {
            get
            {
                return SettingsKeyInfoProvider.GetBoolValue("CMSSessionUseDBRepository");
            }
        }


        /// <summary>
        /// Gets the value that indicates whether online users feature is enabled.
        /// </summary>
        public static bool OnlineUsersEnabled
        {
            get
            {
                return SettingsKeyInfoProvider.GetBoolValue("CMSUseSessionManagement");
            }
        }

        #endregion


        #region "Public methods"

        #region "General session operations"

        /// <summary>
        /// Updates current user session info from the context.
        /// </summary>
        /// <param name="siteName">Site name</param>
        public static void UpdateCurrentSession(string siteName)
        {
            using (var h = SessionEvents.UpdateSession.StartEvent())
            {
                // Update session event handlers can cancel the update processing
                if (h.CanContinue())
                {
                    // Check license for online users
                    if (!LicenseHelper.CheckFeature(RequestContext.CurrentDomain, FeatureEnum.OnlineUsers))
                    {
                        return;
                    }

                    siteName = siteName ?? SiteContext.CurrentSiteName;
                    if (String.IsNullOrEmpty(siteName))
                    {
                        // Do not process session update without any site
                        return;
                    }

                    // Try to get current SessionInfo form request
                    SessionInfo si = MembershipContext.CurrentSession;
                    bool isNewSession = (si == null);

                    // If fails create new SessionInfo 
                    if (isNewSession)
                    {
                        si = new SessionInfo();
                        si.SessionIdentifier = SessionHelper.GetSessionID();
                    }

                    // Backup old userID
                    int oldUserID = si.SessionUserID;

                    // Setup values
                    si.LoadFromContext();

                    si.SessionLastActive = DateTime.Now;
                    si.SessionExpires = si.SessionLastActive.AddMinutes(SessionHelper.SessionTimeout);
                    si.SessionExpires = si.SessionExpires.AddMinutes(SettingsKeyInfoProvider.GetIntValue("CMSSessionManagerSchedulerInterval"));
                    si.SessionExpired = false;
                    si.SessionLastLogon = DateTimeHelper.ZERO_TIME;

                    // Fire the update data event
                    SessionEvents.UpdateSessionData.StartEvent(si);

                    // Get current user
                    bool isAuthenticated = AuthenticationHelper.IsAuthenticated();

                    IUserInfo user = isAuthenticated ? CMSActionContext.CurrentUser : null;

                    bool isKicked = isAuthenticated && AuthenticationHelper.UserKicked(user.UserID);
                    bool isHidden = isAuthenticated && user.UserIsHidden;

                    // Update session info with user information
                    si.SessionUserIsHidden = !isAuthenticated || user.UserIsHidden;

                    // Delete the old record from authenticated users when user logs in from one account to another without logging out
                    if (oldUserID != si.SessionUserID)
                    {
                        // Remove from hash tables
                        RemoveAuthenticatedUser(siteName, oldUserID, true);
                    }

                    if (!isKicked)
                    {
                        if (isAuthenticated || (StoreOnlineUsersInDatabase && isNewSession))
                        {
                            si.SessionLastLogon = si.SessionLastActive;
                        }

                        // Update DB
                        if (StoreOnlineUsersInDatabase)
                        {
                            var updater = new SessionDatabaseUpdater();
                            updater.UpdateItem(si);
                        }

                        // Update hash tables
                        UpsertSiteSession(siteName, isAuthenticated, isHidden, user?.UserID, si);
                    }
                    else
                    {
                        // Sign user out
                        SessionHelper.Clear();
                        AuthenticationHelper.SignOut();

                        // Remove from DB
                        if (StoreOnlineUsersInDatabase)
                        {
                            var updater = new SessionDatabaseUpdater();
                            updater.DeleteItem(si);
                        }

                        // Remove from hash tables
                        RemoveAuthenticatedUser(siteName, si.SessionUserID, true);

                        // Redirect to kicked page
                        URLHelper.Redirect(URLHelper.ResolveUrl("~/CMSMessages/KickedUser.aspx"));
                    }

                    // Add current SessionInfo object to membership context
                    MembershipContext.CurrentSession = si;
                }

                h.FinishEvent();
            }
        }


        /// <summary>
        /// Removes authenticated user from current sessions.
        /// </summary>
        /// <param name="siteName">Site name</param>
        /// <param name="userId">User identifier</param>
        /// <param name="createTask">Indicates whether web farm task should be created</param>
        public static void RemoveAuthenticatedUser(string siteName, int userId, bool createTask)
        {
            if (siteName != null)
            {
                SessionsCachingWorker.UpsertSiteSessionAsync(siteName, false, false, userId, null);

                // Create web farm task if it is required
                if (createTask)
                {
                    WebFarmHelper.CreateTask(SessionTaskType.RemoveAuthenticatedUser, "", siteName, userId.ToString());
                }
            }
        }


        /// <summary>
        /// Remove user from sessions hashtables.
        /// </summary>
        /// <param name="userId">User id</param>
        /// <param name="createTask">Indicates whether web farm task should be created</param>
        public static void RemoveUser(int userId, bool createTask)
        {
            if (userId > 0)
            {
                // Remove from hash tables
                SessionsCachingWorker.RemoveSessionsAsync(userId);

                //Create web farm task if needed
                if (createTask && WebFarmHelper.WebFarmEnabled)
                {
                    WebFarmHelper.CreateTask(SessionTaskType.RemoveUser, null, userId.ToString());
                }
            }
        }


        /// <summary>
        /// Removes user from sessions hashtables.
        /// </summary>
        /// <param name="userId">User id</param>
        public static void RemoveUser(int userId)
        {
            RemoveUser(userId, true);
        }


        /// <summary>
        /// Re-indexes sessions hashtable - changes key from old site name to new site name.
        /// </summary>
        /// <param name="oldSiteName">Old site name</param>
        /// <param name="newSiteName">New site name</param>
        public static void ReindexSessionsInfosHashtable(string oldSiteName, string newSiteName)
        {
            if ((oldSiteName != null) && (newSiteName != null))
            {
                SessionsCachingWorker.ChangeSiteNameAsync(oldSiteName, newSiteName);
            }
        }


        /// <summary>
        /// Clears all information from session management about specified site.
        /// </summary>
        /// <param name="siteName">Site name</param>
        public static void Clear(string siteName)
        {
            // Remove sessions from hash table
            SessionsCachingWorker.ClearAsync(siteName);

            // Remove sessions from database
            if (StoreOnlineUsersInDatabase)
            {
                SiteInfo si = SiteInfoProvider.GetSiteInfo(siteName);
                if (si != null)
                {
                    // Prepare the parameters
                    QueryDataParameters parameters = new QueryDataParameters();
                    parameters.Add("@SessionSiteID", si.SiteID);

                    ConnectionHelper.ExecuteQuery("cms.user.deletesessionbysite", parameters);
                }
            }
        }


        /// <summary>
        /// Gets sessions from hashtables and updates in the database.
        /// </summary>
        public static void UpdateDatabaseSession()
        {
            var updater = new SessionDatabaseUpdater();
            updater.UpdateItems(SessionsCachingWorker.GetSessions());
        }

        #endregion


        #region "Expired"

        /// <summary>
        /// Removes the expired sessions from the hash table.
        /// </summary>
        public static void RemoveExpiredSessions()
        {
            bool lockTaken = false;
            try
            {
                lockTaken = Monitor.TryEnter(mRemoveExpiredSessionLock);
                if (lockTaken)
                {
                    var expirationDate = DateTime.Now;

                    foreach (var session in SessionsCachingWorker.GetSessions(onlyAuthenticated: false)
                        .Where(sess => sess.SessionExpired || (sess.SessionExpires < expirationDate)))
                    {
                        SessionsCachingWorker.RemoveSessionsAsync(session);
                    }
                }
            }
            finally
            {
                if (lockTaken)
                {
                    Monitor.Exit(mRemoveExpiredSessionLock);
                }
            }
        }


        /// <summary>
        /// Removes the expired sessions from the database.
        /// </summary>
        public static void RemoveExpiredSessionsFromDB()
        {
            ConnectionHelper.ExecuteQuery("cms.user.deleteexpiredsession", null);
        }


        /// <summary>
        /// Removes expired sessions.
        /// </summary>
        public static void RemoveExpiredSessions(bool database, bool hashtables)
        {
            // Delete from database
            if (database)
            {
                RemoveExpiredSessionsFromDB();
            }

            // Delete from hash tables
            if (hashtables)
            {
                RemoveExpiredSessions();
            }

            // Remove expired sessions from kicked hashtable
            AuthenticationHelper.RemoveExpiredKickedUsers();
        }

        #endregion


        #region "Kicked"

        /// <summary>
        /// Performs the necessary actions to kick the user.
        /// <param name="userID">User id to kick</param>
        /// </summary>
        public static void KickUser(int userID)
        {
            AuthenticationHelper.AddUserToKicked(userID);

            // Create webfarm task if needed
            WebFarmHelper.CreateTask(SessionTaskType.AddUserToKicked, null, userID.ToString());

            if (StoreOnlineUsersInDatabase)
            {
                var parameters = new QueryDataParameters();
                parameters.Add("@SessionUserID", userID);
                ConnectionHelper.ExecuteQuery("cms.user.deletesessionsforuser", parameters);
            }
        }


        /// <summary>
        /// Removes user from kicked.
        /// <param name="userID">User id to kick</param>
        /// </summary>
        public static void RemoveUserFromKicked(int userID)
        {
            AuthenticationHelper.RemoveUserFromKicked(userID);

            // Create webfarm task if needed
            WebFarmHelper.CreateTask(SessionTaskType.RemoveUserFromKicked, null, userID.ToString());
        }


        /// <summary>
        /// Returns WHERE condition for kicked users.
        /// </summary>
        /// <returns>Returns WHERE condition</returns>
        public static string GetKickedUsersWhere()
        {
            string kicked = AuthenticationHelper.GetKickedUsers();
            if (!String.IsNullOrEmpty(kicked))
            {
                return "UserID IN( " + kicked + " )";
            }
            return null;
        }

        #endregion


        #region "On-line users"

        /// <summary>
        /// Returns UserInfos of online users according to WHERE condition ordered by ORDER BY expression.
        /// </summary>
        /// <param name="where">WHERE condition</param>
        /// <param name="orderBy">ORDER BY expression</param>   
        /// <param name="topN">TOP N expression</param>
        /// <param name="location">Location (alias path) of users</param>
        /// <param name="siteName">Site name</param>  
        /// <param name="includeHidden">Include hidden</param>
        /// <param name="includeKicked">Include kicked</param>
        public static DataSet GetOnlineUsers(string where, string orderBy, int topN, string location, string siteName, bool includeHidden, bool includeKicked)
        {
            return GetOnlineUsers(where, orderBy, topN, null, location, siteName, includeHidden, includeKicked);
        }


        /// <summary>
        /// Returns UserInfos of online users according to WHERE condition ordered by ORDER BY expression.
        /// </summary>
        /// <param name="where">WHERE condition</param>
        /// <param name="orderBy">ORDER BY expression</param>   
        /// <param name="topN">TOP N expression</param>
        /// <param name="columns">Columns condition</param>
        /// <param name="location">Location (aliaspath) of users</param>
        /// <param name="siteName">Site name</param>  
        /// <param name="includeHidden">Include hidden</param>
        /// <param name="includeKicked">Include kicked</param>
        public static DataSet GetOnlineUsers(string where, string orderBy, int topN, string columns, string location, string siteName, bool includeHidden, bool includeKicked)
        {
            // Check license
            if (!LicenseHelper.CheckFeature(RequestContext.CurrentDomain, FeatureEnum.OnlineUsers))
            {
                return null;
            }

            string whereUsers = GetUsersWhereCondition(location, siteName, includeHidden, includeKicked);

            // If no user was found return
            if (String.IsNullOrEmpty(whereUsers))
            {
                return null;
            }

            if (!String.IsNullOrEmpty(where))
            {
                whereUsers += " AND (" + where + ")";
            }

            // Select all online users
            if (StoreOnlineUsersInDatabase)
            {
                return ConnectionHelper.ExecuteQuery("cms.user.selectusersession", null, whereUsers, orderBy, topN, columns);
            }

            var whereCondition = new WhereCondition(whereUsers);
            var query = UserInfoProvider.GetUsersDataWithSettings().Where(whereCondition).TopN(topN).Columns(columns);

            if (!string.IsNullOrEmpty(orderBy))
            {
                string direction;
                var column = SqlHelper.GetOrderByColumnName(orderBy, out direction);

                query = SqlHelper.ORDERBY_DESC.Equals(direction, StringComparison.OrdinalIgnoreCase)
                    ? query.OrderByDescending(column)
                    : query.OrderByAscending(column);
            }

            return query.Result;
        }


        /// <summary>
        /// Returns where condition which could be used for loading online users from database.
        /// </summary>
        /// <param name="location">Location (aliaspath) of users</param>
        /// <param name="siteName">Site name</param>  
        /// <param name="includeHidden">Include hidden</param>
        /// <param name="includeKicked">Include kicked</param>
        public static string GetUsersWhereCondition(string location, string siteName, bool includeHidden, bool includeKicked)
        {
            bool locationSpecified = !String.IsNullOrEmpty(location);
            bool wildcardLocation = locationSpecified && location.EndsWith("%", StringComparison.Ordinal);

            if (wildcardLocation)
            {
                location = location.TrimEnd('%');
            }

            if (String.IsNullOrEmpty(siteName))
            {
                // Get global statistics
                siteName = null;
            }

            string whereUsers = String.Empty;

            if (StoreOnlineUsersInDatabase)
            {
                // Filter users by location
                if (locationSpecified)
                {
                    // Avoid sql injection
                    location = SqlHelper.EscapeQuotes(location.Trim());

                    // Wildcard search
                    if (wildcardLocation)
                    {
                        whereUsers += "( CMS_Session.SessionLocation LIKE N'" + SqlHelper.EscapeLikeText(location) + "%')";
                    }
                    // Single path search
                    else
                    {
                        whereUsers += "( CMS_Session.SessionLocation = N'" + location + "')";
                    }
                }

                // Filter users by site
                if (!String.IsNullOrEmpty(siteName))
                {
                    int siteId = SiteInfoProvider.GetSiteID(siteName);
                    if (siteId != 0)
                    {
                        if (!String.IsNullOrEmpty(whereUsers))
                        {
                            whereUsers += " AND ";
                        }

                        whereUsers += "( CMS_Session.SessionSiteID = " + siteId + ")";
                    }
                }

                // Add condition to non-hidden if needed
                if (!includeHidden)
                {
                    if (!String.IsNullOrEmpty(whereUsers))
                    {
                        whereUsers += " AND ";
                    }

                    whereUsers += "(CMS_Session.SessionUserIsHidden = 0)";
                }

                // Complete SELECT from CMS_Session table
                if (String.IsNullOrEmpty(whereUsers))
                {
                    whereUsers = " 1 = 1 ";
                }
                whereUsers = " UserID IN ( SELECT SessionUserID FROM CMS_Session WHERE " + whereUsers + " {0}) ";

                // Add condition to kicked users if needed
                string kickedCondition = "";
                if (includeKicked)
                {
                    string kicked = AuthenticationHelper.GetKickedUsers();
                    if (!String.IsNullOrEmpty(kicked))
                    {
                        kickedCondition = "UNION SELECT UserID FROM CMS_User WHERE UserID IN( " + kicked + " )";
                    }
                }

                // Join to query                
                whereUsers = String.Format(whereUsers, kickedCondition);
            }
            else
            {
                var sessions = SessionsCachingWorker.GetSessions(siteName);
                var hiddenUsers = SessionsCachingWorker.GetHiddenUsers(siteName);

                string strKicked = AuthenticationHelper.GetKickedUsers();
                List<int> kicked = String.IsNullOrEmpty(strKicked) ? new List<int>() : strKicked.Split(',').Select(s => ValidationHelper.GetInteger(s, 0)).ToList();

                foreach (SessionInfo session in sessions)
                {
                    if (session != null)
                    {
                        bool validLocation = (!locationSpecified
                                             || (wildcardLocation && (session.SessionLocation != null) && session.SessionLocation.StartsWith(location, StringComparison.InvariantCultureIgnoreCase)))
                                             || String.Equals(session.SessionLocation, location, StringComparison.InvariantCultureIgnoreCase);

                        if (validLocation)
                        {
                            // Don't add hidden or kicked users (kicked session is removed from authenticated sessions during the first session update after session has been kicked, so kicked user must be checked)
                            if ((includeHidden || !hiddenUsers.Contains(session.SessionUserID))
                                && (includeKicked || !kicked.Contains(session.SessionUserID)))
                            {
                                whereUsers += session.SessionUserID + ",";
                            }
                        }
                    }
                }

                // Include kicked users
                if (includeKicked && kicked.Any())
                {
                    whereUsers += strKicked + ",";
                }

                if (!String.IsNullOrEmpty(whereUsers))
                {
                    whereUsers = whereUsers.TrimEnd(',');
                    whereUsers = "UserID IN(" + whereUsers + ")";
                }
            }

            return whereUsers;
        }


        /// <summary>
        /// Returns true if user with specified userID is online.
        /// </summary>
        /// <param name="siteName">Site name</param>
        /// <param name="userID">User id</param>
        /// <param name="includeHidden">Include hidden</param>        
        public static bool IsUserOnline(string siteName, int userID, bool includeHidden)
        {
            siteName = siteName ?? SiteContext.CurrentSiteName;

            if (SessionsCachingWorker.IsUserAuthenticated(siteName, userID, includeHidden))
            {
                return true;
            }

            // If DB repository is enabled try to find in database
            if (StoreOnlineUsersInDatabase)
            {
                var sessionUsersQuery = new DataQuery().Column("SessionUserID").From("CMS_Session").WhereEquals("SessionUserID", userID);

                if (!includeHidden)
                {
                    sessionUsersQuery = sessionUsersQuery.WhereFalse("SessionUserIsHidden");
                }

                // Build condition
                DataQuery usersQuery = new DataQuery()
                    .Column("UserID").Distinct()
                    .From("View_CMS_User")
                    .WhereIn("UserID", sessionUsersQuery);

                // Return result
                return usersQuery.HasResults();
            }

            return false;
        }


        /// <summary>
        /// Returns number of online users.
        /// </summary>
        /// <param name="siteName">Site name</param>
        /// <param name="location">Location</param>
        /// <param name="includeHidden">Include hidden users</param>
        /// <param name="includeKicked">Include kicked users - kicked users are count through all sites</param>
        /// <param name="publicUsers">Number of public users</param>
        /// <param name="authenticatedUsers">Bumber of authenticated users</param>
        public static void GetUsersNumber(string siteName, string location, bool includeHidden, bool includeKicked, out int publicUsers, out int authenticatedUsers)
        {
            if (StoreOnlineUsersInDatabase)
            {
                GetUsersNumberFromDB(siteName, location, includeHidden, out publicUsers, out authenticatedUsers);
            }
            else
            {
                GetUsersNumberFromHashTables(siteName, location, includeHidden, includeKicked, out publicUsers, out authenticatedUsers);
            }
        }

        #endregion


        #region "Contacts"

        /// <summary>
        /// Removes object dependencies.
        /// </summary>
        /// <param name="contactID">Contact being deleted</param>
        public static void RemoveContactDependencies(int contactID)
        {
            if (StoreOnlineUsersInDatabase)
            {
                var parameters = new QueryDataParameters();
                parameters.Add("@SessionContactID", contactID);
                ConnectionHelper.ExecuteQuery("cms.user.removesessioncontact", parameters);
            }
        }

        #endregion

        #endregion


        #region "Private methods"

        /// <summary>
        /// Gets number of users online.
        /// </summary>
        /// <param name="siteName">Name of the site</param>
        /// <param name="location">Location on the site</param>
        /// <param name="includeHidden">Include hidden users</param>
        /// <param name="publicUsers">Number of public users</param>
        /// <param name="authenticatedUsers">Number of public</param>
        private static void GetUsersNumberFromDB(string siteName, string location, bool includeHidden, out int publicUsers, out int authenticatedUsers)
        {
            publicUsers = 0;
            authenticatedUsers = 0;

            var users = OnlineUserInfoProvider.GetOnlineUsers().Columns("SessionUserID");

            if (!String.IsNullOrEmpty(siteName))
            {
                users.WhereEquals("SessionSiteID", SiteInfoProvider.GetSiteID(siteName));
            }

            if (!String.IsNullOrEmpty(location))
            {
                if (location.EndsWith("%", StringComparison.OrdinalIgnoreCase))
                {
                    users.WhereStartsWith("SessionLocation", location.TrimEnd('%'));
                }
                else
                {
                    users.WhereEquals("SessionLocation", location.TrimEnd('/'));
                }
            }

            if (!includeHidden)
            {
                users.Where("SessionUserIsHidden = 0 OR SessionUserID IS NULL");
            }

            DataSet result = users.Result;
            if (!DataHelper.DataSourceIsEmpty(result))
            {
                publicUsers = result.Tables[0].Select("SessionUserID IS NULL").Length;
                authenticatedUsers = result.Tables[0].Select("SessionUserID > 0").Length;
            }
        }


        /// <summary>
        /// Gets number of users online.
        /// </summary>
        /// <param name="siteName">Name of the site</param>
        /// <param name="location">Location on the site</param>
        /// <param name="includeHidden">Include hidden users</param>
        /// <param name="includeKicked">Include kicked users</param>
        /// <param name="publicUsers">Number of public users</param>
        /// <param name="authenticatedUsers">Number of public</param>
        private static void GetUsersNumberFromHashTables(string siteName, string location, bool includeHidden, bool includeKicked, out int publicUsers, out int authenticatedUsers)
        {
            if (String.IsNullOrEmpty(siteName))
            {
                // Get global statistics
                siteName = null;
            }

            if (String.IsNullOrEmpty(location))
            {
                // Get values either global or for specific site
                publicUsers = SessionsCachingWorker.GetPublicUsersCount(siteName);
                authenticatedUsers = SessionsCachingWorker.GetAuthenticatedUsersCount(siteName);

                if (includeHidden)
                {
                    authenticatedUsers += SessionsCachingWorker.GetHiddenUsersCount(siteName);
                }
            }
            else
            {
                publicUsers = 0;
                authenticatedUsers = 0;

                // Get desired sessions
                var sessions = SessionsCachingWorker.GetSessions(siteName, false);
                var authenticated = SessionsCachingWorker.GetAuthenticatedUsers(siteName);
                var hidden = SessionsCachingWorker.GetHiddenUsers(siteName);

                // Get kicked users
                string strKicked = AuthenticationHelper.GetKickedUsers();
                List<int> kicked = new List<int>();

                if (!String.IsNullOrEmpty(strKicked))
                {
                    kicked.AddRange(strKicked.Split(',').Select(s => ValidationHelper.GetInteger(s, 0)));
                }

                // Count session statistics
                foreach (SessionInfo session in sessions.Where(s => s != null))
                {
                    bool validLocation = session.SessionLocation != null &&
                        (session.SessionLocation.Equals(location.TrimEnd('/'), StringComparison.InvariantCultureIgnoreCase) ||
                        (location.EndsWith("%", StringComparison.InvariantCultureIgnoreCase) && session.SessionLocation.StartsWith(location.TrimEnd('%'), StringComparison.InvariantCultureIgnoreCase)));

                    if (validLocation)
                    {
                        if (session.SessionUserID == 0)
                        {
                            // Public user
                            publicUsers++;
                        }
                        else if (authenticated.Contains(session.SessionUserID))
                        {
                            if (includeHidden || !hidden.Contains(session.SessionUserID))
                            {
                                // Authenticated user
                                authenticatedUsers++;
                            }
                        }
                        else if (includeKicked && kicked.Contains(session.SessionUserID))
                        {
                            // Kicked user
                            authenticatedUsers++;
                        }
                    }
                }
            }
        }

        #endregion


        #region "Internal methods"

        /// <summary>
        /// Returns current session info form hashtable.
        /// </summary>        
        internal static SessionInfo GetCurrentSessionInfoFromHashtable()
        {
            return SessionsCachingWorker.GetSession(SiteContext.CurrentSiteName, SessionHelper.GetSessionID());
        }


        /// <summary>
        /// Update or insert user's site session.
        /// </summary>
        /// <param name="siteName">Name of the site visited by user</param>
        /// <param name="isAuthenticated">Users has been authenticated and will be stored in collection of authenticated users</param>
        /// <param name="isHidden">Users is hidden and will be stored in collection of hidden users</param>
        /// <param name="userId">Session user identifier</param>
        /// <param name="session">Session info to store in memory</param>
        internal static void UpsertSiteSession(string siteName, bool isAuthenticated, bool isHidden, int? userId, SessionInfo session)
        {
            SessionsCachingWorker.UpsertSiteSessionAsync(siteName, isAuthenticated, isHidden, userId, session);
        }

        #endregion
    }
}