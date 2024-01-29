using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CMS.Chat
{
    /// <summary>
    /// Class holding counts of online users in rooms.
    /// </summary>
    public class SiteRoomsOnlineUsersCounts
    {
        #region "Private fields"

        private Dictionary<int, OnlineUsersCountData> onlineUsersCountsDict;

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="onlineUsersCounts">List of online users</param>
        public SiteRoomsOnlineUsersCounts(IEnumerable<OnlineUsersCountData> onlineUsersCounts)
        {
            // Convert list to dictionary with RoomID as key
            onlineUsersCountsDict = onlineUsersCounts.ToDictionary(ouc => ouc.RoomID, ouc => ouc);
        }

        #endregion


        #region "Public properties"

        /// <summary>
        /// Gets online users counts in one room.
        /// </summary>
        /// <param name="roomID">Room ID</param>
        public OnlineUsersCountData this[int roomID]
        {
            get
            {
                OnlineUsersCountData result;

                if (onlineUsersCountsDict.TryGetValue(roomID, out result))
                {
                    return result;
                }

                return null;
            }
        }

        #endregion
    }
}
