using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CMS.Chat
{
    /// <summary>
    /// Upper class for storing information about admin rights of users in rooms.
    /// 
    /// Information are stored in a hierarchy:
    /// 
    /// UsersRoomAdminStates has dictionary indexed by ints - those are chat user IDs. Values are UserRoomAdminState.
    /// UserRoomAdminStates  has dictionary indexed by ints - those are room IDs. Values are RoomAdminState - admin state in room.
    /// </summary>
    public class UsersRoomAdminStates
    {
        #region "Private fields"

        private ChatCacheDictionaryWrapper<int, UserRoomAdminState> usersAdminStatesCache;

        private string uniqueName;

        #endregion


        #region "Constructor"

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="parentName">Unique name of parent (for caching)</param>
        /// <param name="maxDelay">Maximum delay of retrieving info from cache</param>
        public UsersRoomAdminStates(string parentName, TimeSpan maxDelay)
        {
            uniqueName = parentName + "|AdminStates";

            // Init cache - every item can be retrieved separatelly
            usersAdminStatesCache = new ChatCacheDictionaryWrapper<int, UserRoomAdminState>(
                uniqueName,
                (chatUserID) => ChatRoomUserInfoProvider.GetAdminStates(chatUserID),
                maxDelay,
                null
            );
        }

        #endregion


        #region "Public methods"

        /// <summary>
        /// Gets rooms where admin rights were changed for specified user.
        /// </summary>
        /// <param name="chatUserID">Chat user ID</param>
        /// <param name="sinceWhen">Rights were changed since this time</param>
        /// <returns>RoomAdminStates</returns>
        public IEnumerable<RoomAdminState> GetRoomsWithChangedRights(int chatUserID, DateTime sinceWhen)
        {
            UserRoomAdminState userAdminState = usersAdminStatesCache.GetItem(chatUserID);

            if (userAdminState != null)
            {
                return userAdminState.GetChangedRooms(sinceWhen);
            }

            return Enumerable.Empty<RoomAdminState>();
        }


        /// <summary>
        /// Gets non one to one room where specified user has more rights than 'None'.
        /// </summary>
        /// <param name="chatUserID">Chat user ID</param>
        /// <returns>IDs of rooms with raised privileges</returns>
        public IEnumerable<int> GetRoomsWithJoinRights(int chatUserID)
        {
            UserRoomAdminState userAdminState = usersAdminStatesCache.GetItem(chatUserID);

            if (userAdminState != null)
            {
                return userAdminState.GetRoomsWithJoinRights();
            }

            return Enumerable.Empty<int>();
        }


        /// <summary>
        /// Gets admin level of user in a room. None is returned if room was not found.
        /// </summary>
        /// <param name="chatUserID">Chat user ID</param>
        /// <param name="chatRoomID">Room ID</param>
        public AdminLevelEnum GetAdminLevelInRoom(int chatUserID, int chatRoomID)
        {
            UserRoomAdminState userAdminState = usersAdminStatesCache.GetItem(chatUserID);

            if (userAdminState != null)
            {
                return userAdminState.GetAdminLevelInRoom(chatRoomID);
            }

            return AdminLevelEnum.None;
        }


        /// <summary>
        /// Checks if user is Admin (or creator) in room.
        /// </summary>
        /// <param name="chatUserID">Chat user ID</param>
        /// <param name="chatRoomID">Room ID</param>
        public bool IsUserAdmin(int chatUserID, int chatRoomID)
        {
            return GetAdminLevelInRoom(chatUserID, chatRoomID) >= AdminLevelEnum.Admin;
        }


        /// <summary>
        /// Invalidates admins states cache for specified user.
        /// </summary>
        /// <param name="chatUserID">Chat user ID</param>
        public void Invalidate(int chatUserID)
        {
            usersAdminStatesCache.InvalidateItem(chatUserID);
        }

        #endregion
    }
}
