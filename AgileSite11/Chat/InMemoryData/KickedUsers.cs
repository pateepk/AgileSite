using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CMS.Chat
{
    /// <summary>
    /// Class holding kicked users. It should be stored in cache.
    /// </summary>
    public class KickedUsers
    {
        #region "Private fields"

        private readonly Dictionary<int, DateTime> kickedUsers = new Dictionary<int, DateTime>();

        #endregion


        #region "Constructor"

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="kickedUsers">Kicked users in form Chat user id -> Kick expiration</param>
        public KickedUsers(Dictionary<int, DateTime> kickedUsers)
        {
            this.kickedUsers = kickedUsers;
        }

        #endregion


        #region "Public methods"

        /// <summary>
        /// Checks if user is kicked now.
        /// </summary>
        /// <param name="chatUserID">Chat user ID</param>
        /// <param name="forHowLong">Output parameter. After how many seconds user will be able to join room.</param>
        public bool IsUserKicked(int chatUserID, out int forHowLong)
        {
            DateTime kickExpiration;

            forHowLong = -1;

            if (kickedUsers != null && kickedUsers.TryGetValue(chatUserID, out kickExpiration) && kickExpiration > DateTime.Now)
            {
                forHowLong = (int)(kickExpiration - DateTime.Now).TotalSeconds;

                return true;
            }

            return false;
        }

        #endregion
    }
}
