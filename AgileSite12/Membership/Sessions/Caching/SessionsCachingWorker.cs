using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

using CMS.Base;
using CMS.Core;

namespace CMS.Membership
{
    /// <summary>
    /// Provides thread-safe operations on user sessions cache by organizing operations FIFO queue.
    /// </summary>
    internal class SessionsCachingWorker : ThreadQueueWorker<SessionsCachingAction, SessionsCachingWorker>
    {
        #region "Private variables"

        /// <summary>
        /// This collection and the individual <see cref="SessionsInfo"/> objects must be modified from the context of this worker thread only
        /// (in order to maintain consistency of the collections inside <see cref="SessionsInfo"/>).
        /// Since the collections can be read by different threads via <see cref="GetSessions"/> or <see cref="GetSession"/> methods they are concurrent.
        /// </summary>
        private static readonly ConcurrentDictionary<string, SessionsInfo> mSiteSessions = new ConcurrentDictionary<string, SessionsInfo>(4, 8, StringComparer.OrdinalIgnoreCase);

        // Contains UserIds for sessions that are going to be removed by worker 
        private static readonly ConcurrentDictionary<int, byte> mRemoveUserSession = new ConcurrentDictionary<int, byte>();

        #endregion


        #region "ThreadQueueWorker implementation"

        /// <summary>
        /// Gets the interval in milliseconds for the worker (default 100ms)
        /// </summary>
        protected override int DefaultInterval
        {
            get
            {
                return 100;
            }
        }


        /// <summary>
        /// Triggered when worker is being finalized. This is a hash table worker, so no actions are needed.
        /// </summary>
        protected override void Finish()
        {
        }


        /// <summary>
        /// Process item in the queue.
        /// </summary>
        /// <param name="item">Queued item.</param>
        protected override void ProcessItem(SessionsCachingAction item)
        {
            try
            {
                switch (item?.Action)
                {
                    case SessionsCachingActionEnum.UpsertSession:
                        UpsertSiteSessionInternal(item);
                        break;
                    case SessionsCachingActionEnum.RemoveSession:
                        RemoveSessionInternal(item);
                        if (item.UserID.GetValueOrDefault() > 0)
                        {
                            mRemoveUserSession.Remove(item.UserID.Value);
                        }
                        break;
                    case SessionsCachingActionEnum.ChangeSiteName:
                        ChangeSiteNameInternal(item);
                        break;
                    case SessionsCachingActionEnum.Clear:
                        ClearInternal(item);
                        break;
                }
            }
            catch (Exception ex)
            {
                CoreServices.EventLog.LogException("SessionsCaching", "EXCEPTION", ex);
            }
        }

        #endregion


        #region "Queue operations"

        /// <summary>
        /// Update or insert user's site session.
        /// </summary>
        /// <param name="siteName">Name of the site visited by user</param>
        /// <param name="isAuthenticated">Users has been authenticated and will be stored in collection of authenticated users</param>
        /// <param name="isHidden">Users is hidden and will be stored in collection of hidden users</param>
        /// <param name="userId">Session user identifier</param>
        /// <param name="session">Session info to store in memory</param>
        internal static void UpsertSiteSessionAsync(string siteName, bool isAuthenticated, bool isHidden, int? userId, SessionInfo session)
        {
            Current.Enqueue(SessionsCachingAction.GetUpsertAction(siteName, isAuthenticated, isHidden, userId, session));
        }


        /// <summary>
        /// Remove user session from all sites in sessions dictionaries.
        /// </summary>
        /// <param name="userId">User identifier</param>
        internal static void RemoveSessionsAsync(int userId)
        {
            if (mRemoveUserSession.TryAdd(userId, byte.MinValue))
            {
                Current.Enqueue(SessionsCachingAction.GetRemoveAction(userId));
            }
        }


        /// <summary>
        /// Remove user session from all sites in sessions hashtables.
        /// </summary>
        /// <param name="session">User session info.</param>
        internal static void RemoveSessionsAsync(SessionInfo session)
        {
            Current.Enqueue(SessionsCachingAction.GetRemoveAction(session));
        }


        /// <summary>
        /// Re-indexes sessions dictionary - changes key from old site name to new site name.
        /// </summary>
        /// <param name="oldSiteName">Old site name</param>
        /// <param name="newSiteName">New site name</param>
        internal static void ChangeSiteNameAsync(string oldSiteName, string newSiteName)
        {
            Current.Enqueue(SessionsCachingAction.GetRenameAction(oldSiteName, newSiteName));
        }


        /// <summary>
        /// Clears all information from session management about specified site.
        /// </summary>
        /// <param name="siteName">Site name</param>
        internal static void ClearAsync(string siteName)
        {
            Current.Enqueue(SessionsCachingAction.GetClearAction(siteName));
        }

        #endregion


        #region "Unqueued operations"

        /// <summary>
        /// Get authenticated sessions registered in dictionaries.
        /// </summary>
        /// <param name="siteName">Get session from specific site or from all sites.</param>
        /// <param name="onlyAuthenticated">Get only authenticated sessions</param>
        internal static IEnumerable<SessionInfo> GetSessions(string siteName = null, bool onlyAuthenticated = true)
        {
            var sites = (siteName != null) ? new[] { siteName } : mSiteSessions.Keys;
            foreach (string site in sites)
            {
                SessionsInfo sessions;
                if (mSiteSessions.TryGetValue(site, out sessions))
                {
                    if (onlyAuthenticated)
                    {
                        SessionInfo session;
                        foreach (var kvpAuth in sessions.AuthenticatedSessions)
                        {
                            if (sessions.Sessions.TryGetValue(kvpAuth.Value, out session))
                            {
                                yield return session;
                            }
                        }
                    }
                    else
                    {
                        foreach (var kvpSession in sessions.Sessions)
                        {
                            yield return kvpSession.Value;
                        }
                    }
                }
            }
        }


        /// <summary>
        /// Get specific session info.
        /// </summary>
        /// <param name="siteName">Site name where the session resides.</param>
        /// <param name="sessionId">Session identifier by which it is indexed in dictionaries.</param>
        internal static SessionInfo GetSession(string siteName, string sessionId)
        {
            if ((siteName != null) && (sessionId != null))
            {
                SessionsInfo sessions;
                if (mSiteSessions.TryGetValue(siteName, out sessions))
                {
                    SessionInfo session;
                    if (sessions.Sessions.TryGetValue(sessionId, out session))
                    {
                        return session;
                    }
                }
            }

            return null;
        }


        /// <summary>
        /// Get the number of authenticated users. Do not include hidden users.
        /// </summary>
        /// <param name="siteName">Get the number for specific site or for all sites.</param>
        internal static int GetAuthenticatedUsersCount(string siteName = null)
        {
            if (siteName == null)
            {
                return ProcessSessions(null, sessions => sessions.SelectMany(s => s.AuthenticatedSessions.Keys.Except(s.HiddenSessions.Keys))
                                                                 .Distinct()
                                                                 .Count());
            }


            return ProcessSessions(siteName, sessions => sessions.Aggregate(0, (a, b) => a + b.AuthenticatedUsers));
        }


        /// <summary>
        /// Get the number of hidden users.
        /// </summary>
        /// <param name="siteName">Get the number for specific site or for all sites.</param>
        internal static int GetHiddenUsersCount(string siteName = null)
        {
            if (siteName == null)
            {
                return ProcessSessions(null, sessions => sessions.SelectMany(s => s.HiddenSessions.Keys).Distinct().Count());
            }

            return ProcessSessions(siteName, sessions => sessions.Aggregate(0, (a, b) => a + b.HiddenUsers));
        }


        /// <summary>
        /// Get the number of public users.
        /// </summary>
        /// <param name="siteName">Get the number for specific site or for all sites.</param>
        internal static int GetPublicUsersCount(string siteName = null)
        {
            return ProcessSessions(siteName, sessions => sessions.Aggregate(0, (a, b) => a + b.PublicUsers));
        }


        /// <summary>
        /// Get IDs of authenticated users on given site. Includes IDs of hidden users.
        /// </summary>
        /// <param name="siteName">Site to check user on, or NULL if all sites shall be checked.</param>
        internal static List<int> GetAuthenticatedUsers(string siteName = null)
        {
            return ProcessSessions(siteName, sessions => sessions.SelectMany(siteSessions => siteSessions.AuthenticatedSessions.Keys.Cast<int>()).ToList());
        }


        /// <summary>
        /// Finds out whether the user is authenticated on given site.
        /// </summary>
        /// <param name="siteName">Name of user's site.</param>
        /// <param name="userId">User session identifier.</param>
        /// <param name="allowHidden">Look for hidden users too.</param>
        internal static bool IsUserAuthenticated(string siteName, int userId, bool allowHidden = false)
        {
            return ProcessSessions(
                siteName,
                sessions => sessions.Any(s => s.AuthenticatedSessions.Keys.Cast<int>().Contains(userId) && (allowHidden || !s.HiddenSessions.Keys.Cast<int>().Contains(userId)))
            );
        }


        /// <summary>
        /// Get IDs of hidden users on given site.
        /// </summary>
        /// <param name="siteName">Site to check user on, or NULL if all sites shall be checked.</param>
        internal static List<int> GetHiddenUsers(string siteName = null)
        {
            return ProcessSessions(siteName, sessions => sessions.SelectMany(siteSessions => siteSessions.HiddenSessions.Keys.Cast<int>()).ToList());
        }

        #endregion


        #region "Private methods"
        
        /// <summary>
        /// Update or insert user's site session.
        /// </summary>
        private static void UpsertSiteSessionInternal(SessionsCachingAction action)
        {
            // Ensure sessions object
            SessionsInfo sessions;
            if (!mSiteSessions.TryGetValue(action.SiteName, out sessions))
            {
                sessions = new SessionsInfo();
                mSiteSessions[action.SiteName] = sessions;
            }

            if (action.UserID.GetValueOrDefault() > 0)
            {
                // Check if user is authenticated
                if (action.IsAuthenticated)
                {
                    sessions.AuthenticatedSessions[action.UserID.Value] = action.Session.SessionIdentifier;

                    // If user is hidden put it into hidden dictionary
                    if (action.IsHidden)
                    {
                        sessions.HiddenSessions[action.UserID.Value] = action.Session.SessionIdentifier;
                    }
                    else
                    {
                        sessions.HiddenSessions.Remove(action.UserID.Value);
                    }
                }
                else
                {
                    // Remove user from all dictionaries
                    sessions.AuthenticatedSessions.Remove(action.UserID.Value);
                    sessions.HiddenSessions.Remove(action.UserID.Value);
                }
            }

            if (action.Session != null)
            {
                // Add session info
                sessions.Sessions[action.Session.SessionIdentifier] = action.Session;
            }
        }


        /// <summary>
        /// Remove user session from all sites in sessions dictionary.
        /// </summary>
        private static void RemoveSessionInternal(SessionsCachingAction action)
        {
            // Remove users session from all sites
            foreach (SessionsInfo sessions in mSiteSessions.Values.Where(sess => sess != null))
            {
                if (action.UserID.GetValueOrDefault() > 0)
                {
                    RemoveSessionFromSessionCollections(sessions.Sessions, sessions.HiddenSessions, action.UserID.Value);
                    RemoveSessionFromSessionCollections(sessions.Sessions, sessions.AuthenticatedSessions, action.UserID.Value);
                }

                else if (sessions.Sessions.ContainsKey(action.Session.SessionIdentifier))
                {
                    sessions.Sessions.Remove(action.Session.SessionIdentifier);
                }
            }
        }


        /// <summary>
        /// Removes <see cref="SessionInfo"/> for user with <paramref name="userId"/> from <paramref name="sessions"/> if found in <paramref name="specializedSessionCollection"/>.
        /// </summary>
        /// <param name="sessions">
        /// Collection of users <see cref="SessionInfo"/>s. Dictionary keys are string session identifiers.
        /// </param>
        /// <param name="specializedSessionCollection">
        /// Specialized collection contains user id as key and value is session identifier used in <paramref name="sessions"/>.
        /// Expected input is <see cref="SessionsInfo.HiddenSessions"/> or <see cref="SessionsInfo.AuthenticatedSessions"/> collection.
        /// </param>
        /// <param name="userId">
        /// User for whom to remove <see cref="SessionInfo"/> from <paramref name="sessions"/>. 
        /// If <paramref name="userId"/> is not found in <paramref name="specializedSessionCollection"/>, 
        /// then <see cref="SessionInfo"/> is not removed from <paramref name="sessions"/>.
        /// </param>
        private static void RemoveSessionFromSessionCollections(ConcurrentDictionary<string, SessionInfo> sessions, ConcurrentDictionary<int, string> specializedSessionCollection, int userId)
        {
            string key;
            if (specializedSessionCollection.TryGetValue(userId, out key))
            {
                specializedSessionCollection.Remove(userId);
                sessions.Remove(key);
            }
        }
    

        /// <summary>
        /// Re-indexes sessions dictionary - changes key from old site name to new site name.
        /// </summary>
        private static void ChangeSiteNameInternal(SessionsCachingAction action)
        {
            SessionsInfo sessions;
            if (mSiteSessions.TryGetValue(action.OldSiteName, out sessions))
            {
                mSiteSessions[action.SiteName] = sessions;
                mSiteSessions.Remove(action.OldSiteName);
            }
        }


        /// <summary>
        /// Clears all information from session management about specified site.
        /// </summary>
        private static void ClearInternal(SessionsCachingAction action)
        {
            mSiteSessions.Remove(action.SiteName);
        }


        /// <summary>
        /// Runs given function against session collections.
        /// </summary>
        /// <remarks>
        /// In order to keep shared sessions dictionary thread-safe, always materialize enumerable collections in <paramref name="collectionProcessor"/>.
        /// </remarks>
        /// <typeparam name="T">Return type of passed function</typeparam>
        /// <param name="siteName">When specified, given function is run only against session collection that belongs to specified site.</param>
        /// <param name="collectionProcessor">Function to process session collections.</param>
        /// <returns>Function result</returns>
        private static T ProcessSessions<T>(string siteName, Func<IEnumerable<SessionsInfo>, T> collectionProcessor)
        {
            var sites = (siteName != null) ? new[] { siteName } : mSiteSessions.Keys;
            
            var sessions = sites.Select(site =>
            {
                SessionsInfo localSessions;
                mSiteSessions.TryGetValue(site, out localSessions);
            
                return localSessions;
            })
            .Where(session => session != null);
            
            return collectionProcessor(sessions);
        }

        #endregion
    }
}
