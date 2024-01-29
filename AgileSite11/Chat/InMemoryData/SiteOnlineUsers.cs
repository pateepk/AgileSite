using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CMS.Chat
{
    /// <summary>
    /// Class holding cached online users and notification (only times) on one site.
    /// </summary>
    public class SiteOnlineUsers
    {
        #region "Private fields"

        private readonly ChatIncrementalCacheWithCurrentStateWrapper<OnlineUserData, int> onlineUsersCache;
        
        private readonly ChatCacheWrapper<Dictionary<int, DateTime>> lastNotificationChangesCache;

        private readonly int siteID;
        private readonly string uniqueName;

        #endregion


        #region "Constructor"

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="parentName">Unique name of parent</param>
        /// <param name="siteID">Site ID</param>
        public SiteOnlineUsers(string parentName, int siteID)
        {
            this.siteID = siteID;
            this.uniqueName = parentName + "|SiteOnlineUsers";

            // Init online users cache
            onlineUsersCache = new ChatIncrementalCacheWithCurrentStateWrapper<OnlineUserData, int>(
                uniqueName + "|OU",
                () => ChatOnlineUserInfoProvider.GetAllChatOnlineUsers(siteID),
                (since) => ChatOnlineUserInfoProvider.GetChangedChatOnlineUsers(siteID, since),
                TimeSpan.FromSeconds(9) // Online users will be refreshed after 9 seconds (less than half interval of Ping)
                );

            
            // Init notification times cache
            lastNotificationChangesCache = new ChatCacheWrapper<Dictionary<int, DateTime>>(
                uniqueName + "|NotificationChanges",
                () => ChatNotificationInfoProvider.GetLastNotificationChanges(siteID),
                TimeSpan.FromSeconds(20),
                null
                );
        }

        #endregion


        #region "Public methods"

        /// <summary>
        /// Gets online users from Cache.
        /// </summary>
        /// <param name="sinceWhen">If null, all online users on this site will be returned. If not null users changed since this time will be returned.</param>
        public OnlineUsersData GetOnlineUsers(DateTime? sinceWhen)
        {
            ICacheWrapperResponse<OnlineUserData> response;

            // Return all online users
            if (!sinceWhen.HasValue)
            {
                response = onlineUsersCache.GetCurrentStateWithLastChange();
            }
            else
            {
                response = onlineUsersCache.GetLatestData(sinceWhen.Value);
            }

            if (response == null)
            {
                return null;
            }

            return new OnlineUsersData()
            {
                List = response.Items.Where(ou => !ou.IsHidden), // Filter hidden users
                LastChange = response.LastChange.Ticks
            };
        }


        /// <summary>
        /// Searches online users. 
        /// </summary>
        /// <param name="nickname">Users with containing this substring wil be returned. Comparision is incasesensitive.</param>
        /// <param name="topN">Maximum number of records.</param>
        /// <param name="invitedToRoomID">If not null, only users who can be invited to this room will be returned</param>
        public IEnumerable<OnlineUserData> SearchOnlineUsers(string nickname, int topN, int? invitedToRoomID)
        {
            IEnumerable<OnlineUserData> users = onlineUsersCache.CurrentState.Values.Where(ou => !ou.IsHidden); // Search only in not hidden users

            // Return only users who can be invited to this room (if set)
            if (invitedToRoomID.HasValue)
            {
                RoomState room = ChatGlobalData.Instance.Sites.Current.Rooms.GetRoom(invitedToRoomID.Value);

                if (room != null)
                {
                    var usersInRoom = room.OnlineUsers;

                    // Filter out anonymous users if room does not allow anonymous users to access it
                    if (!room.RoomInfo.ChatRoomAllowAnonym)
                    {
                        users = users.Where(u => !u.IsAnonymous);
                    }

                    // Leave only users who are not in a room or who are offline in a public room (admins)
                    users = users.Where(u => !usersInRoom.ContainsKey(u.ChatUserID) || (!room.RoomInfo.ChatRoomPrivate && !usersInRoom[u.ChatUserID].IsOnline));
                }
            }

            if (!string.IsNullOrEmpty(nickname))
            {
                // string.Contains can't find strings case insensitive - hence the use of IndexOf
                users = users.Where(cud => cud.Nickname.IndexOf(nickname, StringComparison.OrdinalIgnoreCase) >= 0);
            }
            
            if (topN > 0)
            {
                users = users.Take(topN);
            }

            return users.ToList();
        }

        
        /// <summary>
        /// Gets new notifications for user.
        /// </summary>
        /// <param name="chatUserID">Chat user ID</param>
        /// <param name="sinceWhen">Notification changed (read or send) after this time will be returned. If null, all unread notifications will be returned.</param>
        public ChatNotificationsData GetNotifications(int chatUserID, DateTime? sinceWhen)
        {
            DateTime lastNotificationChange;

            if (
                !sinceWhen.HasValue ||
                !lastNotificationChangesCache.Data.TryGetValue(chatUserID, out lastNotificationChange) ||
                lastNotificationChange > sinceWhen.Value
                )
            {
                return ChatNotificationHelper.GetNotifications(sinceWhen, chatUserID, siteID);
            }

            return null;
        }


        /// <summary>
        /// Checks if chat user with specified ID is online.
        /// </summary>
        /// <param name="chatUserID">Id of chat user</param>
        /// <returns>True (is online) or false (is not online)</returns>
        public bool IsChatUserOnline(int chatUserID)
        {
            return onlineUsersCache.CurrentState.ContainsKey(chatUserID);
        }


        /// <summary>
        /// Gets online user. Caller of this method assumes, that user is online. So if user is not found in cache,
        /// database will be queried.
        /// </summary>
        /// <param name="chatUserID">Chat user ID</param>
        /// <returns>OnlineUserData or null if user is not online</returns>
        public OnlineUserData GetOnlineUser(int chatUserID)
        {
            OnlineUserData onlineUser;

            return onlineUsersCache.ForceGetItem(chatUserID, out onlineUser) ? onlineUser : null;
        }


        /// <summary>
        /// Invalidates online users cache.  current state. Before next fetch of current state data (property CurrentState, method UpdateAndTryGetItem(), etc.)
        /// data will be reloaded from data source.
        /// </summary>
        public void Invalidate()
        {
            onlineUsersCache.InvalidateCurrentState();
        }

        #endregion
    }
}
