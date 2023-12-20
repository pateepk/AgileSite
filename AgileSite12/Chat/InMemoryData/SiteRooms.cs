using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using CMS.Base;

namespace CMS.Chat
{
    /// <summary>
    /// This class holds rooms (RoomStates) for site.
    /// </summary>
    public class SiteRooms
    {
        #region "Private fields"

        private ChatCacheWrapper<RoomsContainer> roomsContainerCache;

        private string uniqueName;
    
        private ChatCacheWrapper<SiteRoomsOnlineUsersCounts> onlineUsersCountCache;

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor. <paramref name="parentName"/> is used as cache key.
        /// </summary>
        /// <param name="parentName">Unique name of parent object</param>
        /// <param name="siteID">Site ID</param>
        /// <param name="maxDelay">Max delay</param>
        public SiteRooms(string parentName, int siteID, TimeSpan maxDelay)
        {
            uniqueName = parentName + "|Rooms";

            roomsContainerCache = new ChatCacheWrapper<RoomsContainer>(
                uniqueName + "|RC",
                () => new RoomsContainer(uniqueName, siteID, maxDelay),
                null,
                null);

            onlineUsersCountCache = new ChatCacheWrapper<SiteRoomsOnlineUsersCounts>(
                uniqueName + "|OUC",
                () => new SiteRoomsOnlineUsersCounts(ChatRoomUserInfoProvider.GetOnlineUsersCounts(siteID)),
                TimeSpan.FromSeconds(15),
                null
                );
        }

        #endregion


        #region "Public methods"

        /// <summary>
        /// Gets RoomState. Room is specified by <paramref name="chatRoomID"/>.
        /// 
        /// Rooms are updated before retrieving if needed.
        /// </summary>
        /// <param name="chatRoomID">ID of a chat room</param>
        /// <returns>RoomState or null if room was not found</returns>
        public RoomState GetRoom(int chatRoomID)
        {
            return roomsContainerCache.Data.GetRoom(chatRoomID);
        }


        /// <summary>
        /// Gets changed rooms by specified chat user.
        /// </summary>
        /// <param name="sinceWhen">Rooms changed since this time will be returned. If null, only accessible rooms will be returned</param>
        /// <param name="chatUser">Chat user whose accessible rooms should be returned. If null, only public rooms will be returned.</param>
        public ChatRoomsData GetChangedRooms(DateTime? sinceWhen, ChatUserInfo chatUser)
        {
            return roomsContainerCache.Data.GetChangedRooms(sinceWhen, chatUser);
        }


        /// <summary>
        /// Gets counts of users in rooms when this count has changed.
        /// </summary>
        /// <param name="sinceWhen">Last change this client got</param>
        /// <param name="chatUser">Current chat user</param>
        /// <param name="changedRooms">IDs of rooms which were changed since last request. Users counts in those rooms will be always send back to client (if the user is logged in and sinceWhen is not null).</param>
        public UsersInRoomsCountsData GetUsersInRoomsCounts(DateTime? sinceWhen, ChatUserInfo chatUser, IEnumerable<int> changedRooms)
        {
            roomsContainerCache.Data.UpdateRoomsIfNeeded();

            // If chat user is not logged in - return only public rooms
            if (chatUser == null)
            {
                return GetUsersInRoomsCountsForLoggedOut(sinceWhen);
            }

            // If sinceWhen is null, return all accessible rooms
            if (sinceWhen == null)
            {
                return GetUsersInRoomsCountsFirstRequest(chatUser);
            }
            else
            {
                return GetUsersInRoomsCountsNotFirstRequest(sinceWhen.Value, chatUser, changedRooms);
            }

        }


        /// <summary>
        /// Gets room from cache. If room is not found, new rooms from database are retrieved and then the finding is made again.
        /// </summary>
        /// <param name="roomID">Room ID</param>
        /// <param name="roomState">Output RoomState</param>
        public bool ForceTryGetRoom(int roomID, out RoomState roomState)
        {
            return roomsContainerCache.Data.ForceTryGetRoom(roomID, out roomState);
        }


        /// <summary>
        /// Updates rooms.
        /// </summary>
        public void ForceUpdate()
        {
            roomsContainerCache.Data.ForceUpdate();
        }

        #endregion


        #region "Private methods"
        
        #region "Getting users in rooms counts"

        private UsersInRoomsCountsData GetUsersInRoomsCountsForLoggedOut(DateTime? sinceWhen)
        {
            // Online users counts from cache
            SiteRoomsOnlineUsersCounts siteOnlineUsersInRoomsCounts = onlineUsersCountCache.Data;

            // Get counts of users in public rooms which are enabled and available for anonymous users
            List<OnlineUsersCountData> onlineUsersCounts = GetOnlineUsersCounts(sinceWhen, null, roomsContainerCache.Data.PublicRooms);


            if (onlineUsersCounts.Count == 0)
            {
                return null;
            }

            return new UsersInRoomsCountsData()
            {
                List = onlineUsersCounts,
                LastChange = onlineUsersCounts.Max(ouc => ouc.LastChange.Ticks),
            };
        }


        private UsersInRoomsCountsData GetUsersInRoomsCountsFirstRequest(ChatUserInfo chatUser)
        {
            RoomsContainer roomsContainer = roomsContainerCache.Data;

            // Admin states
            UsersRoomAdminStates roomAdminStates = ChatGlobalData.Instance.UsersRoomAdminStates;

            // Rooms where our user can access
            IEnumerable<int> accessiblePrivate = roomAdminStates.GetRoomsWithJoinRights(chatUser.ChatUserID);

            // Public rooms and private rooms where current user has access
            IEnumerable<RoomState> publicAndAccessiblePrivateRooms = roomsContainer.PublicRooms.Concat(accessiblePrivate.Select(roomID => roomsContainer.GetPrivateRoom(roomID)));


            // Get counts of users in public rooms which are enabled and available for anonymous users
            List<OnlineUsersCountData> onlineUsersCounts = GetOnlineUsersCounts(null, chatUser, publicAndAccessiblePrivateRooms);


            if (onlineUsersCounts.Count == 0)
            {
                return null;
            }

            return new UsersInRoomsCountsData()
            {
                List = onlineUsersCounts,
                LastChange = onlineUsersCounts.Max(ouc => ouc.LastChange.Ticks),
            };
        }


        /// <summary>
        /// This method gets users counts in rooms changed since <paramref name="sinceWhen"/> (the number of users has to be changed - not the room itself).
        /// 
        /// Rooms passed in <paramref name="changedRooms"/> are always appended to the response.
        /// </summary>
        /// <param name="sinceWhen">Time of the previous update of users count by this user</param>
        /// <param name="chatUser">Current chat user</param>
        /// <param name="changedRooms">IDs of rooms which were changed since last request. Users counts in these rooms will be always sent</param>
        private UsersInRoomsCountsData GetUsersInRoomsCountsNotFirstRequest(DateTime? sinceWhen, ChatUserInfo chatUser, IEnumerable<int> changedRooms)
        {
            RoomsContainer roomsContainer = roomsContainerCache.Data;

            // Admin states
            UsersRoomAdminStates roomAdminStates = ChatGlobalData.Instance.UsersRoomAdminStates;

            // Rooms where our user has access
            IEnumerable<int> accessiblePrivate = roomAdminStates.GetRoomsWithJoinRights(chatUser.ChatUserID);

            // Public rooms and private rooms where current user has access
            IEnumerable<RoomState> publicAndAccessiblePrivateRooms = roomsContainer.PublicRooms.Concat(accessiblePrivate.Select(roomID => roomsContainer.GetPrivateRoom(roomID)));


            // Get counts of users in public rooms which are enabled and available for anonymous users
            List<OnlineUsersCountData> onlineUsersCounts = GetOnlineUsersCounts(sinceWhen, chatUser, publicAndAccessiblePrivateRooms);

            // If any room was changed in this request, add also number of users counts in this rooms to this request
            if (changedRooms != null)
            {
                SiteRoomsOnlineUsersCounts siteOnlineUsersInRoomsCountsCache = onlineUsersCountCache.Data;

                // Add every number of users to the response
                foreach (OnlineUsersCountData count in changedRooms.Select(roomId => siteOnlineUsersInRoomsCountsCache[roomId]).Where(count => count != null))
                {
                    onlineUsersCounts.Add(count);
                }
            }

            if (onlineUsersCounts.Count == 0)
            {
                return null;
            }

            return new UsersInRoomsCountsData()
            {
                List = onlineUsersCounts,
                LastChange = onlineUsersCounts.Max(ouc => ouc.LastChange.Ticks),
            };
        }


        private List<OnlineUsersCountData> GetOnlineUsersCounts(DateTime? sinceWhen, ChatUserInfo chatUser, IEnumerable<RoomState> roomsToSelectFrom)
        {
            // Online users counts from cache
            SiteRoomsOnlineUsersCounts siteOnlineUsersInRoomsCounts = onlineUsersCountCache.Data;

            bool isUserAnonymous = (chatUser == null) || chatUser.IsAnonymous;

            return (from roomState in roomsToSelectFrom // Get RoomState 
                    where roomState != null
                    let roomInfo = roomState.RoomInfo // Get ChatRoomInfo from RoomState
                    where roomInfo.ChatRoomEnabled && (!isUserAnonymous || roomInfo.ChatRoomAllowAnonym) // Filter rooms to those which are enabled and allow anonymous (if user is anonymous)
                    let onlineUsersCount = siteOnlineUsersInRoomsCounts[roomInfo.ChatRoomID] // Get count of online users
                    where (onlineUsersCount != null) && (!sinceWhen.HasValue || (onlineUsersCount.LastChange > sinceWhen.Value)) // Filter it to those who has changed
                    select onlineUsersCount).ToList(); // Convert counts to list
        }


        #endregion

        #endregion
    }
}
