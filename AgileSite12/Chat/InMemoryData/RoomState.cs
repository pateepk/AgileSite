using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections;

using CMS.Base;
using CMS.DataEngine;
using CMS.SiteProvider;

namespace CMS.Chat
{
    /// <summary>
    /// Class holding caches of messages and online users in one room.
    /// </summary>
    public class RoomState
    {
        #region "Private fields"

        private ChatRoomInfo chatRoomInfo;

        private readonly ChatParametrizedCacheWrapper<MessageData, MessageCacheParams> messagesCache;

        private readonly ChatIncrementalCacheWithCurrentStateWrapper<RoomOnlineUserData, int> roomOnlineUsersCache;

        private readonly ChatCacheWrapper<KickedUsers> kickedUsersCache;

        private readonly string uniqueName;
        private readonly int roomID;

        #endregion


        #region "Public properties"

        /// <summary>
        /// Gets ChatRoomInfo of this room. Properties of this room can't be change via this instance (use ReloadChatRoomInfo() instead).
        /// </summary>
        public ChatRoomInfo RoomInfo
        {
            get
            {
                return chatRoomInfo;
            }
        }


        /// <summary>
        /// List of online users in this room. Contains not only online users, but also users with Admin rights, Join rights (private rooms), etc.
        /// </summary>
        public Dictionary<int, RoomOnlineUserData> OnlineUsers
        {
            get
            {
                return roomOnlineUsersCache.CurrentState;
            }
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor. <paramref name="parentName"/> is used to create cache keys.
        /// </summary>
        /// <param name="parentName">Unique name of a parent object</param>
        /// <param name="chatRoom">ChatRoom</param>
        public RoomState(string parentName, ChatRoomInfo chatRoom)
        {
            roomID = chatRoom.ChatRoomID;
            uniqueName = string.Format("{0}|{1}|{2}", parentName, "RS", roomID);

            chatRoomInfo = chatRoom;

            // Init online users cache
            roomOnlineUsersCache = new ChatIncrementalCacheWithCurrentStateWrapper<RoomOnlineUserData, int>(
                uniqueName + "|ROU",
                () => ChatRoomUserInfoProvider.GetAllOnlineUsersInRoom(roomID),
                (since) => ChatRoomUserInfoProvider.GetRoomOnlineUsers(roomID, since),
                TimeSpan.FromSeconds(5) // Online users cache will be refreshed every 5 seconds
            );

            // Init messages cache
            messagesCache = new ChatParametrizedCacheWrapper<MessageData, MessageCacheParams>(
                uniqueName + "|MES",
                (messageParam) => ChatMessageHelper.GetLatestMessages(roomID, messageParam),
                TimeSpan.FromSeconds(3) // Messages cache will be refreshed every 3 seconds
            );

            // Init kicked users cache
            kickedUsersCache = new ChatCacheWrapper<KickedUsers>(
                uniqueName + "|KickedUsers",
                () => ChatRoomUserInfoProvider.GetKickedUsers(roomID),
                TimeSpan.FromMinutes(20), // Kicked users will be refreshed every 20 minutes (it is ok, because they will be invalidated after kicking)
                null
            );
        }

        #endregion


        #region "Public methods"

        /// <summary>
        /// Gets newest messages.
        /// </summary>
        /// <param name="count">Max count</param>
        /// <param name="sinceWhen">Since when</param>
        /// <param name="isFirstRequest">True if this is the first request (client does not have sinceWhen yet)</param>
        /// <param name="chatUserID">Current user ID (whisper messages will be filtered for this user)</param>
        public MessagesData GetMessages(int? count, DateTime? sinceWhen, bool isFirstRequest, int chatUserID)
        {
            // If count is 0 on the first request, it means, that client does not want any messages now, but he will want any message which arrives since now
            if (count.HasValue && isFirstRequest && (count.Value == 0))
            {
                // Get time of last message from DB
                DateTime? lastChange = ChatMessageInfoProvider.GetNewestMessageTime(roomID);
                
                return new MessagesData()
                {
                    LastChange = (lastChange.HasValue ? lastChange.Value : DateTime.Now).Ticks,
                    List = null
                };
            }

            // Params for getting messages from cache
            MessageCacheParams messageParam = new MessageCacheParams();

            messageParam.MaxCount = count;
            messageParam.SinceWhen = sinceWhen;

            // Message types which will be included
            HashSet<ChatMessageTypeEnum> messageTypesFilter = null;

            // If it is the first request (client does not have any messages yet), settings for loading messages at first load has to be applied
            if (isFirstRequest)
            {
                string siteName = SiteContext.CurrentSiteName;

                // ClassicMessage, Whisper, RequestDeclined are added by default
                messageTypesFilter = new HashSet<ChatMessageTypeEnum>()
                {
                    ChatMessageTypeEnum.ClassicMessage, 
                    ChatMessageTypeEnum.Whisper
                };

                if (RoomInfo.IsOneToOneSupport)
                {
                    messageTypesFilter.Add(ChatMessageTypeEnum.ChatRequestDeclined);
                }

                if (SettingsKeyInfoProvider.GetBoolValue(siteName + ".CMSChatDisplayKickAtFirstLoad"))
                {
                    messageTypesFilter.Add(ChatMessageTypeEnum.Kicked);
                    messageTypesFilter.Add(ChatMessageTypeEnum.KickedPermanently);
                }
                if (SettingsKeyInfoProvider.GetBoolValue(siteName + ".CMSChatDisplayEnterAtFirstLoad"))
                {
                    messageTypesFilter.Add(ChatMessageTypeEnum.EnterRoom);
                }
                if (SettingsKeyInfoProvider.GetBoolValue(siteName + ".CMSChatDisplayLeaveAtFirstLoad"))
                {
                    messageTypesFilter.Add(ChatMessageTypeEnum.LeaveRoom);
                    messageTypesFilter.Add(ChatMessageTypeEnum.LeaveRoomPermanently);
                }
                if (SettingsKeyInfoProvider.GetBoolValue(siteName + ".CMSChatDisplayChangeNicknameAtFirstLoad"))
                {
                    messageTypesFilter.Add(ChatMessageTypeEnum.ChangeNickname);
                }
                if (SettingsKeyInfoProvider.GetBoolValue(siteName + ".CMSChatDisplaySupportGreetingAtFirstLoad"))
                {
                    messageTypesFilter.Add(ChatMessageTypeEnum.SupportGreeting);
                }
                if (SettingsKeyInfoProvider.GetBoolValue(siteName + ".CMSChatDisplayInvitedAtFirstLoad"))
                {
                    messageTypesFilter.Add(ChatMessageTypeEnum.UserInvited);
                }
                if (SettingsKeyInfoProvider.GetBoolValue(siteName + ".CMSChatDisplayAnnouncementAtFirstLoad"))
                {
                    messageTypesFilter.Add(ChatMessageTypeEnum.Announcement);
                }
            }

            // Get data from cache. Apply filtering for whispers (remove whispers for other users) and for message types (is ingnored on non first request)
            ICacheWrapperResponse<MessageData> response = messagesCache.GetData(messageParam, (m) => WhisperFilter(m, chatUserID) && MessageTypeFilter(m, messageTypesFilter));

            // No new messages
            if (response == null)
            {
                return null;
            }

            return new MessagesData()
            {
                List = response.Items,
                LastChange = response.LastChange.Ticks,
            };
        }


        /// <summary>
        /// Gets online users in this room. Changed or all (sinceWhen is null).
        /// </summary>
        /// <param name="sinceWhen">If null, all online users in this room will be returned. If not null users changed since this time will be returned.</param>
        public RoomOnlineUsersData GetOnlineUsers(DateTime? sinceWhen)
        {
            ICacheWrapperResponse<RoomOnlineUserData> response;

            if (!sinceWhen.HasValue)
            {
                // All users have to be returned, so cached all users version can be used
                response = roomOnlineUsersCache.GetCurrentStateWithLastChange();
            }
            else
            {
                // Get changed data from cache
                response = roomOnlineUsersCache.GetLatestData(sinceWhen.Value);
            }

            if (response == null)
            {
                return null;
            }

            return new RoomOnlineUsersData()
            {
                List = response.Items,
                LastChange = response.LastChange.Ticks,
            };
        }



        /// <summary>
        /// Checks if the chat users specified by <paramref name="chatUserID"/> is kicked from this room.
        /// </summary>
        /// <param name="chatUserID">ID of a chat user</param>
        /// <returns>True if chat user was kicked.</returns>
        public bool IsUserKicked(int chatUserID)
        {
            int dummy;
            return IsUserKicked(chatUserID, out dummy);
        }


        /// <summary>
        /// Checks if the chat users specified by <paramref name="chatUserID"/> is kicked from this room.
        /// </summary>
        /// <param name="chatUserID">ID of a chat room</param>
        /// <param name="forHowLong">Specified how long must user wait till he can access room again.</param>
        /// <returns>True if chat user was kicked.</returns>
        public bool IsUserKicked(int chatUserID, out int forHowLong)
        {
            return kickedUsersCache.Data.IsUserKicked(chatUserID, out forHowLong);
        }


        /// <summary>
        /// Invalidate list of kicked users in this room.
        /// </summary>
        public void InvalidateKickedUsers()
        {
            kickedUsersCache.Invalidate();
        }
        

        /// <summary>
        /// Checks if user is online. Caller of this method expects that user is online. If user is not online in cache, this method checks database.
        /// </summary>
        /// <param name="chatUserID">Chat user ID</param>
        /// <returns>True if he is online</returns>
        public bool ForceIsUserOnline(int chatUserID)
        {
            RoomOnlineUserData onlineUser;

            if (roomOnlineUsersCache.ForceGetItem(chatUserID, out onlineUser))
            {
                if (!onlineUser.IsOnline)
                {
                    return roomOnlineUsersCache.UpdateAndTryGetItem(chatUserID, out onlineUser) && onlineUser.IsOnline;
                }
                return true;
            }

            return false;
        }


        /// <summary>
        /// Gets online user. Caller expects that user is present in list of online users.
        /// </summary>
        /// <param name="chatUserID">Chat user ID</param>
        /// <param name="onlineUser">Output Online User</param>
        /// <returns>True if user was found</returns>
        public bool ForceTryGetUser(int chatUserID, out RoomOnlineUserData onlineUser)
        {
            return roomOnlineUsersCache.ForceGetItem(chatUserID, out onlineUser);
        }


        /// <summary>
        /// Gets online user. 
        /// </summary>
        /// <param name="chatUserID">Chat user ID</param>
        /// <param name="onlineUser">Output Online User</param>
        /// <returns>True if user was found</returns>
        public bool TryGetUser(int chatUserID, out RoomOnlineUserData onlineUser)
        {
            return roomOnlineUsersCache.CurrentState.TryGetValue(chatUserID, out onlineUser);
        }


        /// <summary>
        /// Reloads info about this chat room. Should be called every time after room is changed.
        /// </summary>
        /// <param name="newChatRoomInfo">New info about this chat room.</param>
        public void ReloadChatRoomInfo(ChatRoomInfo newChatRoomInfo)
        {
            chatRoomInfo = newChatRoomInfo;
        }

        #endregion


        #region "Private methods"

        private bool WhisperFilter(MessageData messageData, int chatUserID)
        {
            return (messageData.SystemMessageType != ChatMessageTypeEnum.Whisper) || (messageData.AuthorID.Value == chatUserID) || (messageData.RecipientID.Value == chatUserID);
        }


        private bool MessageTypeFilter(MessageData messageData, HashSet<ChatMessageTypeEnum> types)
        {
            return (types == null) || types.Contains(messageData.SystemMessageType);
        }

        #endregion
    }
}
