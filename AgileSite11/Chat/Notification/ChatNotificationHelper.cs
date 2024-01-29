using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace CMS.Chat
{
    /// <summary>
    /// Helper class for chat notifications.
    /// </summary>
    public static class ChatNotificationHelper
    {
        #region "Public methods"

        /// <summary>
        /// Inserts new notification to the database. If there is already an unread notification with the same sender, receiver and roomID, the new one is not inserted!
        /// </summary>
        /// <param name="type">Type of chat notification</param>
        /// <param name="senderID">Sender (chat user ID)</param>
        /// <param name="receiverID">Receiver (chat user ID)</param>
        /// <param name="roomID">Room ID (in case of join room, leave room, etc. notifications). Null, if notification is not room related.</param>
        /// <param name="siteID">Notification will be assigned to this site (null is global)</param>
        public static void InsertChatNotification(ChatNotificationTypeEnum type, int senderID, int receiverID, int? roomID, int? siteID)
        {
            ChatNotificationInfoProvider.InsertNotification(type, senderID, receiverID, roomID, siteID);
        }


        /// <summary>
        /// Closes (marks as read) the passed notification unless it is already read.
        /// </summary>
        /// <param name="chatNotification">Chat notification to close.</param>
        public static void MarkAsReadChatNotification(ChatNotificationInfo chatNotification)
        {
            if (!chatNotification.ChatNotificationIsRead)
            {
                ChatNotificationInfoProvider.MarkAsReadChatNotification(chatNotification.ChatNotificationID);
            }
        }


        /// <summary>
        /// Closes all current user's notifications (current user is receiver) which arrived before specified time.
        /// </summary>
        /// <param name="receiverID">Receiver of notifications</param>
        /// <param name="untilWhen">Notifications arrived before this time</param>
        public static void CloseAllChatNotifications(int receiverID, DateTime untilWhen)
        {
            IEnumerable <ChatNotificationInfo> notificationsToClose = ChatNotificationInfoProvider.GetUnreadNotificationsUntil(receiverID, untilWhen);

            foreach (ChatNotificationInfo notification in notificationsToClose)
            {
                if (notification.ChatNotificationType == ChatNotificationTypeEnum.Invitation)
                {
                    DeclineInvitation(notification);
                }
                else
                {
                    MarkAsReadChatNotification(notification);
                }
            }
        }


        /// <summary>
        /// Inserts notification of type Invitation.
        /// </summary>
        /// <param name="room">Invitation to this room will be send</param>
        /// <param name="currentChatUser">Current chat user ID (sender of notification)</param>
        /// <param name="chatUserToInviteID">ID of chat user to invite (receiver of notification)</param>
        public static void SendInvitationToRoom(ChatRoomInfo room, ChatUserInfo currentChatUser, int chatUserToInviteID)
        {
            ChatUserInfo chatUserToInvite = ChatUserInfoProvider.GetChatUserInfo(chatUserToInviteID);

            if (chatUserToInvite == null)
            {
                throw new ChatServiceException(ChatResponseStatusEnum.ChatUserNotFound);
            }

            SendInvitationToRoom(room, currentChatUser, chatUserToInvite);
        }


        /// <summary>
        /// Inserts notification of type Invitation.
        /// </summary>
        /// <param name="room">Invitation to this room will be send</param>
        /// <param name="currentChatUser">Current chat user ID (sender of notification)</param>
        /// <param name="chatUserToInvite">Chat user to invite (receiver of notification)</param>
        public static void SendInvitationToRoom(ChatRoomInfo room, ChatUserInfo currentChatUser, ChatUserInfo chatUserToInvite)
        {
            if (chatUserToInvite == null)
            {
                throw new ChatServiceException(ChatResponseStatusEnum.BadRequest);
            }

            if (chatUserToInvite.IsAnonymous && !room.ChatRoomAllowAnonym)
            {
                throw new ChatServiceException(ChatResponseStatusEnum.AnonymsDisallowed);
            }

            // Insert invite to notifications
            InsertChatNotification(ChatNotificationTypeEnum.Invitation, currentChatUser.ChatUserID, chatUserToInvite.ChatUserID, room.ChatRoomID, room.ChatRoomSiteID);

            // Insert system message to room
            ChatMessageHelper.InsertSystemMessage(room.ChatRoomID, ChatMessageTypeEnum.UserInvited, currentChatUser.ChatUserNickname, chatUserToInvite.ChatUserNickname);
        }


        /// <summary>
        /// Accepts invitation.
        /// 
        /// Sets notification as read. Inserts notification about accepting invitation and gives Join right to room (if room is private).
        /// </summary>
        /// <param name="notification">Invitation to accept</param>
        /// <returns>Invitation to this room was accepted (or null if it is one to one). This is used to display room immediatelly on the client side.</returns>
        public static ChatRoomData AcceptInvitation(ChatNotificationInfo notification)
        {
            // Notification has wrong type
            if (notification.ChatNotificationType != ChatNotificationTypeEnum.Invitation)
            {
                throw new ChatServiceException(ChatResponseStatusEnum.BadRequest);
            }

            RoomState roomState = ChatGlobalData.Instance.Sites.Current.Rooms.GetRoom(notification.ChatNotificationRoomID.Value);

            // Room not found
            if ((roomState == null) || !roomState.RoomInfo.ChatRoomEnabled)
            {
                throw new ChatServiceException(ChatResponseStatusEnum.RoomNotFound);
            }

            ChatUserInfo currentChatUser = ChatOnlineUserHelper.GetLoggedInChatUser();


            // Increase admin level only if room is private
            if (roomState.RoomInfo.ChatRoomPrivate)
            {
                ChatRoomUserHelper.IncreaseChatAdminLevel(notification.ChatNotificationRoomID.Value, notification.ChatNotificationReceiverID, AdminLevelEnum.Join);
            }
            
            
            MarkAsReadChatNotification(notification);

            // Insert notification about accepting invitation
            InsertChatNotification(ChatNotificationTypeEnum.InvitationAccepted, currentChatUser.ChatUserID, notification.ChatNotificationSenderID, notification.ChatNotificationRoomID, roomState.RoomInfo.ChatRoomSiteID);

            // If room is not one to one, return it to the client. One to one rooms are not supposed to be displayed on the client side, hence they are not returned.
            if (!roomState.RoomInfo.ChatRoomIsOneToOne)
            {
                return ChatRoomHelper.ConvertRoomToData(roomState);
            }

            return null;
        }


        /// <summary>
        /// Declines invitation.
        /// </summary>
        /// <param name="notification">Invitation</param>
        public static void DeclineInvitation(ChatNotificationInfo notification)
        {
            MarkAsReadChatNotification(notification);

            // Insert notification about declining invitation
            InsertChatNotification(ChatNotificationTypeEnum.InvitationDeclined, ChatOnlineUserHelper.GetLoggedInChatUser().ChatUserID, notification.ChatNotificationSenderID, notification.ChatNotificationRoomID, notification.ChatNotificationSiteID);
        }


        /// <summary>
        /// Gets notification belonging to specified user changed since <paramref name="since"/>.
        /// </summary>
        /// <param name="since">Notification changed (read or send) after this time will be returned. If null, all unread notifications will be returned.</param>
        /// <param name="receiverID">Receiver</param>
        /// <param name="siteID">Only notification assigned to this site will be returned</param>
        public static ChatNotificationsData GetNotifications(DateTime? since, int receiverID, int siteID)
        {
            List<ChatNotificationData> notifications = ChatNotificationInfoProvider.GetChatNotifications(receiverID, since, since.HasValue ? (bool?)null : false, siteID).ToList();

            if (notifications.Count == 0)
            {
                return null;
            }

            long lastChange = notifications.Max(n => Math.Max(n.NotificationDateTime.Ticks, (n.ReadDateTime ?? DateTime.MinValue).Ticks));

            return new ChatNotificationsData()
            {
                List = notifications,
                LastChange = lastChange,
            };
        }

        #endregion
    }
}
