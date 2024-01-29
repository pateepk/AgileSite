using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CMS.Chat
{
    /// <summary>
    /// Helper class for managing chat users in rooms (ChatRoomUserInfo).
    /// </summary>
    public static class ChatRoomUserHelper
    {
        #region "Public static methods"

        /// <summary>
        /// Sets admin level of specified user in specified room to specified level.
        /// </summary>
        /// <param name="chatRoomID">Room ID</param>
        /// <param name="chatUserID">Chat user ID</param>
        /// <param name="adminLevel">New admin level</param>
        public static void SetChatAdminLevel(int chatRoomID, int chatUserID, AdminLevelEnum adminLevel)
        {
            ChatRoomUserInfoProvider.SetChatAdminLevel(chatRoomID, chatUserID, adminLevel);

            InvalidateAdminCache(chatUserID);
        }


        /// <summary>
        /// Increases admin level of specified user in specified room to specified level.
        /// 
        /// This means that admin level is changed only if current level of this user is lower than new one.
        /// </summary>
        /// <param name="chatRoomID">Room ID</param>
        /// <param name="chatUserID">Chat user ID</param>
        /// <param name="adminLevel">New admin level</param>
        public static void IncreaseChatAdminLevel(int chatRoomID, int chatUserID, AdminLevelEnum adminLevel)
        {
            ChatRoomUserInfoProvider.IncreaseChatAdminLevel(chatRoomID, chatUserID, adminLevel);

            InvalidateAdminCache(chatUserID);
        }

        /// <summary>
        /// Kicks user from this room. His KickExpiration is set to DateTime.Now + number of seconds specified in settings.
        /// </summary>
        /// <param name="roomID">Room ID</param>
        /// <param name="chatUserToKick">Chat user to kick</param>
        /// <param name="kicker">Kicker (used to insert system message)</param>
        public static void KickUserFromRoom(int roomID, ChatUserInfo chatUserToKick, ChatUserInfo kicker)
        {
            RoomState room = ChatGlobalData.Instance.Sites.Current.Rooms.GetRoom(roomID);

            ChatRoomInfo chatRoomInfo = room.RoomInfo;

            DateTime kickExpiration = DateTime.Now.AddSeconds(ChatSettingsProvider.KickLastingIntervalSetting);

            ChatRoomUserInfoProvider.KickUser(chatUserToKick.ChatUserID, chatRoomInfo.ChatRoomID, kickExpiration);

            ChatMessageHelper.InsertSystemMessage(roomID, ChatMessageTypeEnum.Kicked, chatUserToKick.ChatUserNickname, kicker.ChatUserNickname);
            
            ChatNotificationHelper.InsertChatNotification(ChatNotificationTypeEnum.Kicked, kicker.ChatUserID, chatUserToKick.ChatUserID, roomID, chatRoomInfo.ChatRoomSiteID);

            room.InvalidateKickedUsers();
        }


        /// <summary>
        /// Kicks user permanently from this room.
        /// 
        /// He is deleted from db and from memory. He can enter again only after receiving another invitation.
        /// </summary>
        /// <param name="roomID">Room ID</param>
        /// <param name="chatUserToKick">Chat user to kick</param>
        /// <param name="kicker">Kicking chat user</param>
        public static void KickUserPermanentlyFromRoom(int roomID, ChatUserInfo chatUserToKick, ChatUserInfo kicker)
        {
            ChatRoomUserInfoProvider.KickPermanentlyFromRoom(roomID, chatUserToKick.ChatUserID);

            ChatMessageHelper.InsertSystemMessage(roomID, ChatMessageTypeEnum.KickedPermanently, chatUserToKick.ChatUserNickname, kicker.ChatUserNickname);
            ChatNotificationHelper.InsertChatNotification(ChatNotificationTypeEnum.KickedPermanently, kicker.ChatUserID, chatUserToKick.ChatUserID, roomID, null);

            InvalidateAdminCache(chatUserToKick.ChatUserID);
        }


        /// <summary>
        /// Removes (leaves) chat user from this room. 
        /// </summary>
        /// <param name="roomID">Room ID</param>
        /// <param name="chatUserID">Chat user ID</param>
        public static void LeaveRoom(int roomID, int chatUserID)
        {
            string leaveMessageFormat = ChatMessageHelper.GetSystemMessageText(ChatMessageTypeEnum.LeaveRoom, "{nickname}");

            ChatRoomUserInfoProvider.LeaveRoom(roomID, chatUserID, leaveMessageFormat, ChatMessageTypeEnum.LeaveRoom);
        }


        /// <summary>
        /// Removes user permanently from this room. This means, that he has to be invited again to enter it.
        /// 
        /// In public rooms it does the same as classic LeaveRoom().
        /// </summary>
        /// <param name="roomID">Room ID</param>
        /// <param name="chatUser">User to remove</param>
        public static void LeaveRoomPermanently(int roomID, ChatUserInfo chatUser)
        {
            ChatRoomUserHelper.SetChatAdminLevel(roomID, chatUser.ChatUserID, AdminLevelEnum.None);

            string leaveMessageFormat = ChatMessageHelper.GetSystemMessageText(ChatMessageTypeEnum.LeaveRoomPermanently, "{nickname}");

            ChatRoomUserInfoProvider.LeaveRoom(roomID, chatUser.ChatUserID, leaveMessageFormat, ChatMessageTypeEnum.LeaveRoomPermanently);

            InvalidateAdminCache(chatUser.ChatUserID);
        }


        /// <summary>
        /// Adds (joins) user to this room. If user is already online, it does nothing.
        /// 
        /// It sets user's LastChecking to Now.
        /// </summary>
        /// <param name="roomID">Room ID</param>
        /// <param name="chatUser">Chat user to add</param>
        /// <param name="password">Entrance password to this room (checked if room has password set).</param>
        /// <param name="hashPassword">True if password should be hashed before joining. If false, password is already hashed.</param>
        public static void JoinUserToRoom(int roomID, ChatUserInfo chatUser, string password, bool hashPassword)
        {
            if (chatUser == null)
            {
                throw new ChatServiceException(ChatResponseStatusEnum.NotLoggedIn);
            }

            RoomState roomState;

            if (ChatGlobalData.Instance.Sites.Current.Rooms.ForceTryGetRoom(roomID, out roomState))
            {
                // Hash password or get passed value if hashing is not needed
                string hashedPassword = hashPassword ? ChatRoomHelper.GetRoomPasswordHash(password, roomState.RoomInfo.ChatRoomGUID) : password;

                // Actual JoinRoom (setting JoinTime, LastPing in DB) is executed always, to make sure that user is really joined
                JoinRoomResultEnum result = ChatRoomUserInfoProvider.JoinRoom(roomID, chatUser.ChatUserID, hashedPassword);

                switch (result)
                {
                    case JoinRoomResultEnum.Joined: // user was joined by this query - needs to insert system message
                        ChatMessageHelper.InsertSystemMessage(roomState.RoomInfo.ChatRoomID, ChatMessageTypeEnum.EnterRoom, chatUser.ChatUserNickname);

                        return;
                    case JoinRoomResultEnum.AlreadyIn: // user is already joined in this room - do nothing
                        return;
                    case JoinRoomResultEnum.WrongPassword:
                        throw new ChatServiceException(ChatResponseStatusEnum.WrongPassword); // wrong password
                    case JoinRoomResultEnum.RoomDisabled:
                    default:
                        throw new ChatServiceException(ChatResponseStatusEnum.RoomNotFound);
                }
            }
            else
            {
                throw new ChatServiceException(ChatResponseStatusEnum.RoomNotFound);
            }
        }

        #endregion


        #region "Private static methods"

        /// <summary>
        /// Invalidates admin cache for one user.
        /// </summary>
        /// <param name="chatUserID">This user's admin cache will be invalidated</param>
        private static void InvalidateAdminCache(int chatUserID)
        {
            ChatGlobalData.Instance.UsersRoomAdminStates.Invalidate(chatUserID);
        }

        #endregion
    }
}
