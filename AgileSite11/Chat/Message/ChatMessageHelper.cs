using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

using CMS.Helpers;
using CMS.Base;
using CMS.SiteProvider;
using CMS.Membership;
using CMS.Protection;

namespace CMS.Chat
{
    /// <summary>
    /// Helper class for chat messages.
    /// </summary>
    public static class ChatMessageHelper
    {   
        #region "Public methods"

        /// <summary>
        /// Inserts system message to the room with default translation and optional params specified as args.
        /// </summary>
        /// <param name="roomID">System message will be inserted to this room.</param>
        /// <param name="messageType">Type of system message</param>
        /// <param name="args">Arguments to insert into the message text at the placeholders ({0}, {1}, etc.)</param>
        public static void InsertSystemMessage(int roomID, ChatMessageTypeEnum messageType, params string[] args)
        {
            ChatMessageInfo newSystemMessage = BuildNewSystemMessage(roomID, messageType, args);

            ChatMessageInfoProvider.SetChatMessageInfo(newSystemMessage);
        }


        /// <summary>
        /// Gets latest messages from DB.
        /// </summary>
        /// <param name="roomId">Room ID</param>
        /// <param name="messageParam">Params to take into consideration when filtering messages (max count and since)</param>
        public static IEnumerable<MessageData> GetLatestMessages(int roomId, MessageCacheParams messageParam)
        {
            return ChatMessageInfoProvider.GetLatestMessages(roomId, messageParam.MaxCount, messageParam.SinceWhen);
        }


        /// <summary>
        /// Rejects passed message.
        /// </summary>
        /// <param name="message">Message to reject</param>
        public static void RejectMessage(ChatMessageInfo message)
        {
            if (message.ChatMessageRejected != true)
            {
                message.ChatMessageRejected = true;
                ChatMessageInfoProvider.SetChatMessageInfo(message);
            }
        }


        /// <summary>
        /// Gets default text representation of the system message. It is a macro which needs to be resolved before sending to the client.
        /// 
        /// Exception is thrown if <paramref name="messageType"/> is not a system message.
        /// 
        /// Example of a returned value: {$chat.system.userhaschangednickname|(replace){0}(with)guest_42|(replace){1}(with)lalalal$}
        /// </summary>
        /// <param name="messageType">Type of the system message.</param>
        /// <param name="args">Optional params which will be included in the macro in form of |replace{{i}}(with)param. i is counter and starts from 0</param>
        /// <returns>Macro containing text of the system message</returns>
        public static string GetSystemMessageText(ChatMessageTypeEnum messageType, params string[] args)
        {
            if (!messageType.IsSystemMessage())
            {
                throw new Exception("Can't get default text for non-system message");
            }

            string resourceString = messageType.ToStringValue((int)ChatMessageTypeStringValueUsageEnum.LiveSiteMessage);

            string replaceFormat =  "msg = Replace(msg, \"{{{0}}}\", \"{1}\");";
            StringBuilder replaces = new StringBuilder();

            int i = 0;

            foreach (string arg in args)
            {
                replaces.AppendFormat(replaceFormat, i++, HTMLHelper.HTMLEncode(arg));
            }

            return string.Format("{{% msg = GetResourceString(\"{0}\"); {1} return msg; %}}", resourceString, replaces.ToString());
        }


        /// <summary>
        /// Inserts new message to room. Message is of type ClassicMessage.
        /// </summary>
        /// <param name="messageText">Text of message</param>
        /// <param name="roomID">Room ID</param>
        /// <param name="senderChatUser">Sender of this message</param>
        /// <returns>MessageData of the new message</returns>
        public static MessageData PostMessage(string messageText, int roomID, ChatUserInfo senderChatUser)
        {
            VerifyMessageIsValid(ref messageText);

            RoomState room;

            
            if (!ChatGlobalData.Instance.Sites.Current.Rooms.ForceTryGetRoom(roomID, out room))
            {
                throw new ChatServiceException(ChatResponseStatusEnum.RoomNotFound);
            }

            // If room is whisper (one on one), check if second user is online. If he is not online, send him notification.
            if (room.RoomInfo.IsWhisperRoom)
            {
                RoomOnlineUserData secondUserData = room.OnlineUsers.Where(pair => pair.Key != senderChatUser.ChatUserID).Select(pair => pair.Value).SingleOrDefault();
                
                // If second chat user was not found or is offline - send him notification
                if ((secondUserData == null) || (!secondUserData.IsOnline))
                {
                    // ID of the second chat user
                    int secondChatUserID = 0;

                    if (secondUserData == null)
                    {
                        // Get him from the database
                        ChatRoomUserInfo roomUserInfo = ChatRoomUserInfoProvider.GetSecondChatUserIDInOneToOneRoom(senderChatUser.ChatUserID, roomID);

                        if (roomUserInfo != null)
                        {
                            secondChatUserID = roomUserInfo.ChatRoomUserChatUserID;
                        }
                    }
                    else
                    {
                        // Second user is already retrieved from memory - no need to get him from the DB
                        secondChatUserID = secondUserData.ChatUser.ChatUserID;
                    }

                    // If second user was found - send him a notification
                    if (secondChatUserID != 0)
                    {
                        ChatNotificationHelper.InsertChatNotification(ChatNotificationTypeEnum.Invitation, senderChatUser.ChatUserID, secondChatUserID, roomID, room.RoomInfo.ChatRoomSiteID);
                    }
                }
            }

            ChatMessageInfo chatMessage = ChatMessageHelper.BuildNewChatMessage(messageText, roomID, senderChatUser.ChatUserID);
            PerformBadWordsCheck(chatMessage);

            // Save the message if it wasn't saved while performing bad words check
            if (chatMessage.ChatMessageID <= 0)
            {
                ChatMessageInfoProvider.SetChatMessageInfo(chatMessage);
            }

            return new MessageData()
            {
                IsRejected = false,
                LastModified = chatMessage.ChatMessageLastModified,
                MessageID = chatMessage.ChatMessageID,
                MessageText = chatMessage.ChatMessageText,
                Nickname = senderChatUser.ChatUserNickname,
                PostedTime = chatMessage.ChatMessageCreatedWhen,
                Recipient = "",
                SystemMessageType = chatMessage.ChatMessageSystemMessageType,
                AuthorID = senderChatUser.ChatUserID,
                RecipientID = null,
            };
        }


        /// <summary>
        /// Inserts new whisper message to room.
        /// </summary>
        /// <param name="messageText">Text of the message</param>
        /// <param name="roomID">Room ID</param>
        /// <param name="senderChatUser">Sender</param>
        /// <param name="recepientChatUser">Receiver</param>
        /// <returns>MessageData of the new message</returns>
        public static MessageData PostMessageToUser(string messageText, int roomID, ChatUserInfo senderChatUser, ChatUserInfo recepientChatUser)
        {
            // Build message for saving
            ChatMessageInfo chatMessage = ChatMessageHelper.BuildNewChatMessage(messageText, roomID, senderChatUser.ChatUserID, ChatMessageTypeEnum.Whisper, recepientChatUser.ChatUserID);
            PerformBadWordsCheck(chatMessage);

            // Save the message if it wasn't saved while performing bad words check
            if (chatMessage.ChatMessageID <= 0)
            {
                ChatMessageInfoProvider.SetChatMessageInfo(chatMessage);
            }

            return new MessageData()
            {
                IsRejected = false,
                LastModified = chatMessage.ChatMessageLastModified,
                MessageID = chatMessage.ChatMessageID,
                MessageText = chatMessage.ChatMessageText,
                Nickname = senderChatUser.ChatUserNickname,
                PostedTime = chatMessage.ChatMessageCreatedWhen,
                Recipient = recepientChatUser.ChatUserNickname,
                SystemMessageType = chatMessage.ChatMessageSystemMessageType,
                AuthorID = senderChatUser.ChatUserID,
                RecipientID = recepientChatUser.ChatUserID,
            };
        }


        /// <summary>
        /// Perform bad words check on message text
        /// </summary>
        /// <param name="messageInfo">Message info</param>
        /// <exception cref="ChatBadWordsException"></exception>
        public static void PerformBadWordsCheck(ChatMessageInfo messageInfo)
        {
            if (BadWordsHelper.PerformBadWordsCheck(SiteContext.CurrentSiteName) && !BadWordInfoProvider.CanUseBadWords(MembershipContext.AuthenticatedUser, SiteContext.CurrentSiteName))
            {
                Dictionary<string, int> columns = new Dictionary<string, int>();
                columns.Add("ChatMessageText", 0);

                ChatRoomInfo room = ChatRoomInfoProvider.GetChatRoomInfo(messageInfo.ChatMessageRoomID);
                string reportTitle = String.Format("{0}: {1}", room != null ? room.ChatRoomDisplayName : "", messageInfo.ChatMessageText);
                string url = RequestHelper.GetHeader("referer", null);
                string badMessage = BadWordsHelper.CheckBadWords(messageInfo, columns, null, null, reportTitle, url, MembershipContext.AuthenticatedUser.UserID, null);
                if (!String.IsNullOrEmpty(badMessage))
                {
                    throw new ChatBadWordsException(badMessage);
                }
            }
        }

        #endregion


        #region "Private methods"

        /// <summary>
        /// Checks if message is valid.
        /// </summary>
        /// <param name="messageText">Text of a new message</param>
        private static void VerifyMessageIsValid(ref string messageText)
        {
            // Empty message
            if (messageText == null)
            {
                throw new ChatServiceException(ChatResponseStatusEnum.MessageCanNotBeEmpty);
            }

            messageText = messageText.Trim();

            if (messageText.Length == 0)
            {
                throw new ChatServiceException(ChatResponseStatusEnum.MessageCanNotBeEmpty);
            }


            // Too long message
            if (messageText.Length > ChatSettingsProvider.MaximumMessageLengthSetting)
            {
                throw new ChatServiceException(ChatResponseStatusEnum.MessageTooLong);
            }
        }


        private static ChatMessageInfo BuildNewSystemMessage(int roomID, ChatMessageTypeEnum messageType, params string[] args)
        {
            if (!messageType.IsSystemMessage())
            {
                throw new Exception("Message must be system message");
            }

            return BuildNewChatMessage(GetSystemMessageText(messageType, args), roomID, null, messageType);
        }


        /// <summary>
        /// Creates a non-system ChatMessageInfo from the specified parameters. Message is of the type ChatSystemMessageTypeEnum.ClassicMessage.
        /// </summary>
        /// <param name="messageText">Text of the message</param>
        /// <param name="roomID">Room ID</param>
        /// <param name="chatUserID">Sender of this messages</param>
        /// <returns>New ChatMessageInfo</returns>
        private static ChatMessageInfo BuildNewChatMessage(string messageText, int roomID, int chatUserID)
        {
            return BuildNewChatMessage(messageText, roomID, chatUserID, ChatMessageTypeEnum.ClassicMessage);
        }


        /// <summary>
        /// Creates new ChatMessage info from the specified parameters. This message is not whisper.
        /// </summary>
        /// <param name="messageText">Message text</param>
        /// <param name="roomID">Room id</param>
        /// <param name="chatUserID">Sender of this message (null if message is system).</param>
        /// <param name="systemMessageType">Type of this message</param>
        /// <returns>ChatMessageInfo with properties set. ReceipientID is null.</returns>
        public static ChatMessageInfo BuildNewChatMessage(string messageText, int roomID, int? chatUserID, ChatMessageTypeEnum systemMessageType)
        {
            return BuildNewChatMessage(messageText, roomID, chatUserID, systemMessageType, null);
        }


        /// <summary>
        /// Creates new ChatMessage info from the specified parameters.
        /// </summary>
        /// <param name="messageText">Message text</param>
        /// <param name="roomID">Room id</param>
        /// <param name="chatUserID">Sender of this message (null if message is system).</param>
        /// <param name="systemMessageType">Type of this message</param>
        /// <param name="recipient">Recipient of the message (makes sense only if whispering)</param>
        /// <returns>ChatMessageInfo with properties set</returns>
        private static ChatMessageInfo BuildNewChatMessage(string messageText, int roomID, int? chatUserID, ChatMessageTypeEnum systemMessageType, int? recipient)
        {
            ChatMessageInfo chatMessage = new ChatMessageInfo()
            {
                ChatMessageText = messageText,
                ChatMessageRoomID = roomID,
                ChatMessageIPAddress = RequestContext.UserHostAddress,
                ChatMessageCreatedWhen = DateTime.Now, // GETDATE() will be used on SQL Server side
                ChatMessageRejected = false,
                ChatMessageUserID = chatUserID,
                ChatMessageSystemMessageType = systemMessageType,
                ChatMessageRecipientID = recipient
            };

            return chatMessage;
        }

        #endregion
    }
}
