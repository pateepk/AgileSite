using System;
using System.Linq;
using System.Text;

namespace CMS.Membership
{
    /// <summary>
    /// Represents action performed on sessions cache.
    /// </summary>
    internal class SessionsCachingAction
    {
        #region "Properties"

        /// <summary>
        /// Action to perform on session cache.
        /// </summary>
        internal SessionsCachingActionEnum Action
        {
            get;
            private set;
        }


        /// <summary>
        /// Current session site name. Specify alongside with the site name when renaming site in hahstables.
        /// </summary>
        internal string OldSiteName
        {
            get;
            private set;
        }


        /// <summary>
        /// Session site name.
        /// </summary>
        internal string SiteName
        {
            get;
            private set;
        }


        /// <summary>
        /// User is authenticated.
        /// </summary>
        internal bool IsAuthenticated
        {
            get;
            private set;
        }


        /// <summary>
        /// User is hidden.
        /// </summary>
        internal bool IsHidden
        {
            get;
            private set;
        }


        /// <summary>
        /// User identifier used to index session identifiers.
        /// </summary>
        internal int? UserID
        {
            get;
            private set;
        }


        /// <summary>
        /// Session info object.
        /// </summary>
        internal SessionInfo Session
        {
            get;
            private set;
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Disable construction of caching action.
        /// </summary>
        private SessionsCachingAction()
        {
        }

        #endregion


        #region "Factory methods"

        /// <summary>
        /// Get action that inserts or updates session in hahstables.
        /// </summary>
        /// <param name="siteName">Name of the site corresponding to the session.</param>
        /// <param name="isAuthenticated">The user session is authenticated.</param>
        /// <param name="isHidden">The user session is hidden.</param>
        /// <param name="userID">User identifier.</param>
        /// <param name="session">Session info object.</param>
        internal static SessionsCachingAction GetUpsertAction(string siteName, bool isAuthenticated, bool isHidden, int? userID, SessionInfo session)
        {
            if (siteName == null)
            {
                throw new ArgumentNullException(nameof(siteName));
            }

            if ((userID == null) && (session?.SessionIdentifier == null))
            {
                throw new InvalidOperationException("Either user or session must be specified in order to perform an upsert operation.");
            }

            return new SessionsCachingAction
            {
                Action = SessionsCachingActionEnum.UpsertSession,
                SiteName = siteName,
                IsAuthenticated = isAuthenticated,
                IsHidden = isHidden,
                UserID = userID,
                Session = session
            };
        }


        /// <summary>
        /// Get action that removes all sessions of given user.
        /// </summary>
        /// <param name="userId">User identifier.</param>
        internal static SessionsCachingAction GetRemoveAction(int userId)
        {
            return new SessionsCachingAction
            {
                Action = SessionsCachingActionEnum.RemoveSession,
                UserID = userId,
            };
        }


        /// <summary>
        /// Get action for removing given <paramref name="session"/>.
        /// </summary>
        /// <param name="session">Session to be removed.</param>
        internal static SessionsCachingAction GetRemoveAction(SessionInfo session)
        {
            if (session == null)
            {
                throw new ArgumentNullException(nameof(session));
            }

            return new SessionsCachingAction
            {
                Action = SessionsCachingActionEnum.RemoveSession,
                UserID = session.SessionUserID,
                Session = session
            };
        }


        /// <summary>
        /// Get action that clears given site in hashtables.
        /// </summary>
        /// <param name="siteName">Site name to clear</param>
        internal static SessionsCachingAction GetClearAction(string siteName)
        {
            if (siteName == null)
            {
                throw new ArgumentNullException(nameof(siteName));
            }

            return new SessionsCachingAction
            {
                Action = SessionsCachingActionEnum.Clear,
                SiteName = siteName,
            };
        }


        /// <summary>
        /// Get action that changes site name in hashtables.
        /// </summary>
        /// <param name="oldSiteName">Current site name.</param>
        /// <param name="newSiteName">New site name</param>
        internal static SessionsCachingAction GetRenameAction(string oldSiteName, string newSiteName)
        {
            if (oldSiteName == null)
            {
                throw new ArgumentNullException(nameof(oldSiteName));
            }

            if (newSiteName == null)
            {
                throw new ArgumentNullException(nameof(newSiteName));
            }

            return new SessionsCachingAction
            {
                Action = SessionsCachingActionEnum.ChangeSiteName,
                OldSiteName = oldSiteName,
                SiteName = newSiteName
            };
        }

        #endregion
    }
}
