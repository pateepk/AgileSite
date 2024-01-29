using System;
using System.Collections.Concurrent;

namespace CMS.Membership
{
    /// <summary>
    /// Class contains hashtables for session management.
    /// This class serves internally for the sessions management and related on-line users feature.
    /// </summary>
    public class SessionsInfo
    {
        #region "Private variables"

        private ConcurrentDictionary<string, SessionInfo> mSessions = new ConcurrentDictionary<string, SessionInfo>(4, 8, StringComparer.OrdinalIgnoreCase);
        private ConcurrentDictionary<int, string> mAuthenticatedSessions = new ConcurrentDictionary<int, string>(4, 8);
        private ConcurrentDictionary<int, string> mHiddenSessions = new ConcurrentDictionary<int, string>(4, 8);

        #endregion


        #region "Public properties"

        /// <summary>
        /// Collection of sessions indexed by string session identifiers.
        /// </summary>
        public ConcurrentDictionary<string, SessionInfo> Sessions
        {
            get
            {
                return mSessions;
            }
            set
            {
                mSessions = value;
            }
        }


        /// <summary>
        /// Collection of string session identifiers indexed by integer authenticated users identifiers.
        /// </summary>
        public ConcurrentDictionary<int, string> AuthenticatedSessions
        {
            get
            {
                return mAuthenticatedSessions;
            }
            set
            {
                mAuthenticatedSessions = value;
            }
        }


        /// <summary>
        /// Collection of string session identifiers indexed by integer hidden users identifiers.
        /// </summary>
        public ConcurrentDictionary<int, string> HiddenSessions
        {
            get
            {
                return mHiddenSessions;
            }
            set
            {
                mHiddenSessions = value;
            }
        }


        /// <summary>
        /// Number of public users.
        /// </summary>
        public int PublicUsers
        {
            get
            {
                return Math.Max(mSessions.Count - AuthenticatedUsers - HiddenUsers, 0);
            }
        }


        /// <summary>
        /// Number of authenticated users.
        /// </summary>
        public int AuthenticatedUsers
        {
            get
            {
                return Math.Max(mAuthenticatedSessions.Count - HiddenUsers, 0);
            }
        }


        /// <summary>
        /// Number of hidden users.
        /// </summary>
        public int HiddenUsers
        {
            get
            {
                return mHiddenSessions.Count;
            }
        }

        #endregion
    }
}