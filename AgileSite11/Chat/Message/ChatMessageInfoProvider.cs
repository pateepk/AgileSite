using System;
using System.Data;
using System.Linq;
using System.Collections.Generic;

using CMS.DataEngine;
using CMS.Helpers;

namespace CMS.Chat
{
    /// <summary>
    /// Class providing ChatMessageInfo management.
    /// </summary>
    public class ChatMessageInfoProvider : AbstractInfoProvider<ChatMessageInfo, ChatMessageInfoProvider>
    {
        #region "Public methods - Basic"

        /// <summary>
        /// Returns the query for all chat messages.
        /// </summary>
        public static ObjectQuery<ChatMessageInfo> GetChatMessages()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns chat message with specified ID.
        /// </summary>
        /// <param name="messageId">Chat message ID.</param>        
        public static ChatMessageInfo GetChatMessageInfo(int messageId)
        {
            return ProviderObject.GetInfoById(messageId);
        }


        /// <summary>
        /// Sets (updates or inserts) specified chat message.
        /// </summary>
        /// <param name="messageObj">Chat message to be set.</param>
        public static void SetChatMessageInfo(ChatMessageInfo messageObj)
        {
            ProviderObject.SetInfo(messageObj);
        }


        /// <summary>
        /// Deletes specified chat message.
        /// </summary>
        /// <param name="messageObj">Chat message to be deleted.</param>
        public static void DeleteChatMessageInfo(ChatMessageInfo messageObj)
        {
            ProviderObject.DeleteInfo(messageObj);
        }


        /// <summary>
        /// Deletes chat message with specified ID.
        /// </summary>
        /// <param name="messageId">Chat message ID.</param>
        public static void DeleteChatMessageInfo(int messageId)
        {
            ChatMessageInfo messageObj = GetChatMessageInfo(messageId);
            DeleteChatMessageInfo(messageObj);
        }

        #endregion


        #region "Public methods - Advanced"

        /// <summary>
        /// Gets newest messages.
        /// </summary>
        /// <param name="roomID">Room ID</param>
        /// <param name="maxCount">Maximum number of messages (number is not limited if max count is null)</param>
        /// <param name="sinceWhen">Messages posted since this time are returned (all messages are returned if null)</param>
        public static IEnumerable<MessageData> GetLatestMessages(int roomID, int? maxCount, DateTime? sinceWhen)
        {
            // Prepare the parameters
            QueryDataParameters parameters = new QueryDataParameters();
            parameters.Add("@RoomID", roomID);
            parameters.Add("@TopN", maxCount);
            parameters.Add("@ModifiedSince", sinceWhen);

            // Get the data
            DataSet ds = ConnectionHelper.ExecuteQuery("Chat.Message.selectlatestmessages", parameters);
            if (!DataHelper.DataSourceIsEmpty(ds))
            {
                return ds.Tables[0].AsEnumerable().Select(
                    dr => new MessageData
                    {
                        MessageID = ValidationHelper.GetInteger(dr["MessageID"], 0),
                        MessageText = ValidationHelper.GetString(dr["MessageText"], ""),
                        Nickname = dr.IsNull("Nickname") ? null : ValidationHelper.GetString(dr["Nickname"], ""),
                        Recipient = dr.IsNull("Recipient") ? null : ValidationHelper.GetString(dr["Recipient"], ""),
                        PostedTime = ValidationHelper.GetDateTime(dr["CreatedWhen"], DateTime.MinValue),
                        LastModified = ValidationHelper.GetDateTime(dr["MessageLastModified"], DateTime.MinValue),
                        SystemMessageType = ChatHelper.GetEnum(ValidationHelper.GetInteger(dr["SystemMessageType"], 0), ChatMessageTypeEnum.ClassicMessage),
                        IsRejected = ValidationHelper.GetBoolean(dr["IsRejected"], false),
                        AuthorID = dr.IsNull("AuthorID") ? null : (int?)ValidationHelper.GetInteger(dr["AuthorID"], 0),
                        RecipientID = dr.IsNull("RecipientID") ? null : (int?)ValidationHelper.GetInteger(dr["RecipientID"], 0),
                    });
            }

            return Enumerable.Empty<MessageData>();
        }


        /// <summary>
        /// Gets all classic messages posted in room. Oldest messages are first.
        /// </summary>
        /// <param name="roomID">Room ID</param>
        public static IEnumerable<string> GetClassicMessagesText(int roomID)
        {
            return GetChatMessages()
                    .WhereEquals("ChatMessageRoomID", roomID)
                    .WhereEquals("ChatMessageSystemMessageType", (int)ChatMessageTypeEnum.ClassicMessage)
                    .OrderBy("ChatMessageCreatedWhen")
                    .Column("ChatMessageText")
                    .Select(row => row.Field<string>("ChatMessageText"));
        }


        /// <summary>
        /// Gets ChatMessageLastModified of the newest message in room.
        /// </summary>
        /// <param name="roomID">Room ID</param>
        /// <returns>Newest ChatMessageLastModified or null if there are no messages in this room</returns>
        public static DateTime? GetNewestMessageTime(int roomID)
        {
            var message = GetChatMessages()
                            .WhereEquals("ChatMessageRoomID", roomID)
                            .Column("ChatMessageLastModified")
                            .TopN(1)
                            .OrderByDescending("ChatMessageLastModified")
                            .FirstOrDefault();

            return message?.ChatMessageLastModified;
        }

        #endregion


        #region "Internal methods - Basic"

        /// <summary>
        /// Inserts or Updates the object to the database.
        /// </summary>
        /// <param name="info">Object to insert / update</param>
        protected override void SetInfo(ChatMessageInfo info)
        {
            // Message is new, don't update timestamp automatically
            if (info.ChatMessageID == 0)
            {
                info.ChatMessageLastModified = info.ChatMessageCreatedWhen;
                info.TypeInfo.UpdateTimeStamp = false;
            }
            base.SetInfo(info);

            // Ensure auto UpdateTimeStamp after creating
            info.TypeInfo.UpdateTimeStamp = true;
        }

        #endregion
    }
}

