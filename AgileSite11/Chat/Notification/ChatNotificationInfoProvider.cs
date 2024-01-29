using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;

using CMS.DataEngine;
using CMS.Helpers;

namespace CMS.Chat
{
    /// <summary>
    /// Class providing ChatNotificationInfo management.
    /// </summary>
    public class ChatNotificationInfoProvider : AbstractInfoProvider<ChatNotificationInfo, ChatNotificationInfoProvider>
    {
        #region "Public methods - Basic"
        
        /// <summary>
        /// Returns the query for all chat notifications.
        /// </summary>
        public static ObjectQuery<ChatNotificationInfo> GetChatNotifications()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns chat notification with specified ID.
        /// </summary>
        /// <param name="notificationId">Chat notification ID.</param>        
        public static ChatNotificationInfo GetChatNotificationInfo(int notificationId)
        {
            return ProviderObject.GetInfoById(notificationId);
        }


        /// <summary>
        /// Sets (updates or inserts) specified chat notification.
        /// </summary>
        /// <param name="notificationObj">Chat notification to be set.</param>
        public static void SetChatNotificationInfo(ChatNotificationInfo notificationObj)
        {
            ProviderObject.SetInfo(notificationObj);
        }


        /// <summary>
        /// Deletes specified chat notification.
        /// </summary>
        /// <param name="notificationObj">Chat notification to be deleted.</param>
        public static void DeleteChatNotificationInfo(ChatNotificationInfo notificationObj)
        {
            ProviderObject.DeleteInfo(notificationObj);
        }


        /// <summary>
        /// Deletes chat notification with specified ID.
        /// </summary>
        /// <param name="notificationId">Chat notification ID.</param>
        public static void DeleteChatNotificationInfo(int notificationId)
        {
            ChatNotificationInfo notificationObj = GetChatNotificationInfo(notificationId);
            DeleteChatNotificationInfo(notificationObj);
        }

        #endregion


        #region "Public methods - Advanced"

        /// <summary>
        /// Gets chat notifications.
        /// </summary>
        /// <param name="receiverID">Chat user ID</param>
        /// <param name="sinceWhen">Sends since when (all if null).</param>
        /// <param name="isRead">Is read (or all if null).</param>
        /// <param name="siteID">Only notification assigned to this site will be returned</param>
        /// <returns>List of notifications</returns>
        public static IEnumerable<ChatNotificationData> GetChatNotifications(int receiverID, DateTime? sinceWhen, bool? isRead, int siteID)
        {
            // Prepare the parameters
            QueryDataParameters parameters = new QueryDataParameters();
            parameters.Add("@ReceiverID", receiverID);
            parameters.Add("@SinceWhen", sinceWhen);
            parameters.Add("@IsRead", isRead);
            parameters.Add("@SiteID", siteID);

            // Get the data
            DataSet ds = ConnectionHelper.ExecuteQuery("Chat.Notification.selectchatnotifications", parameters);
            if (!DataHelper.DataSourceIsEmpty(ds))
            {
                return ds.Tables[0].AsEnumerable().Select(dr => new ChatNotificationData
                {
                    NotificationID = ValidationHelper.GetInteger(dr["ChatNotificationID"], 0),
                    NotificationDateTime = ValidationHelper.GetDateTime(dr["ChatNotificationSendDateTime"], DateTime.MinValue),
                    NotificationType = (ChatNotificationTypeEnum)ValidationHelper.GetInteger(dr["ChatNotificationType"], 0),
                    RoomID = dr.IsNull("ChatNotificationRoomID") ? (int?)null : ValidationHelper.GetInteger(dr["ChatNotificationRoomID"], 0),
                    RoomName = ValidationHelper.GetString(dr["RoomName"], ""),
                    SenderNickname = ValidationHelper.GetString(dr["SenderNickname"], ""),
                    IsRead = ValidationHelper.GetBoolean(dr["ChatNotificationIsRead"], false),
                    IsOneToOne = dr.IsNull("IsOneToOne") ? (bool?)null : ValidationHelper.GetBoolean(dr["IsOneToOne"], false),
                    ReadDateTime = dr.IsNull("ChatNotificationReadDateTime") ? (DateTime?)null : ValidationHelper.GetDateTime(dr["ChatNotificationReadDateTime"], DateTime.MinValue),
                });
            }

            return Enumerable.Empty<ChatNotificationData>();
        }


        /// <summary>
        /// Gets times of last changed notifications for online users.
        /// 
        /// Keys in dictionary are user's IDs and Values are times of last change.
        /// </summary>
        /// <param name="siteID">Site ID</param>
        public static Dictionary<int, DateTime> GetLastNotificationChanges(int siteID)
        {
            // Prepare the parameters
            QueryDataParameters parameters = new QueryDataParameters();
            parameters.Add("@SiteID", siteID);

            // Get the data
            DataSet ds = ConnectionHelper.ExecuteQuery("Chat.Notification.selectlastnotificationtimes", parameters);
            if (!DataHelper.DataSourceIsEmpty(ds))
            {
                return ds.Tables[0].AsEnumerable().ToDictionary(
                    dr => ValidationHelper.GetInteger(dr["ReceiverID"], 0), 
                    dr => ValidationHelper.GetDateTime(dr["LastNotificationChange"], DateTime.MinValue));
            }

            return new Dictionary<int, DateTime>();
        }


        /// <summary>
        /// Inserts new notification to the database. If there is already an unread notification with the same sender, receiver and roomID, the new one is not inserted!
        /// </summary>
        /// <param name="type">Type of notification</param>
        /// <param name="senderID">Sender (chat user ID)</param>
        /// <param name="receiverID">Receiver (chat user ID)</param>
        /// <param name="roomID">Room ID (in case of join room, leave room, etc. notifications). Null, if notification is not room related.</param>
        /// <param name="siteID">Notification will be assigned to this site (null is global)</param>
        public static void InsertNotification(ChatNotificationTypeEnum type, int senderID, int receiverID, int? roomID, int? siteID)
        {
            // Prepare the parameters
            QueryDataParameters parameters = new QueryDataParameters();
            parameters.Add("@SenderID", senderID);
            parameters.Add("@ReceiverID", receiverID);
            parameters.Add("@Type", type);
            parameters.Add("@SiteID", siteID);
            parameters.Add("@RoomID", roomID);

            ConnectionHelper.ExecuteQuery("Chat.Notification.InsertNotification", parameters);
        }


        /// <summary>
        /// Marks as read chat notification.
        /// </summary>
        /// <param name="chatNotificationID">Notification ID</param>
        public static void MarkAsReadChatNotification(int chatNotificationID)
        {
            // Prepare the parameters
            QueryDataParameters parameters = new QueryDataParameters();
            parameters.Add("@NotificationID", chatNotificationID);

            ConnectionHelper.ExecuteQuery("Chat.Notification.CloseNotification", parameters);
        }


        /// <summary>
        /// Gets all unred notifications which arrived before passed time.
        /// </summary>
        /// <param name="receiverID">Receiver of notifications</param>
        /// <param name="untilWhen">Notifications arrived before this time</param>
        public static IEnumerable<ChatNotificationInfo> GetUnreadNotificationsUntil(int receiverID, DateTime untilWhen)
        {
            // Prepare the parameters
            QueryDataParameters parameters = new QueryDataParameters();
            parameters.Add("@ReceiverID", receiverID);
            parameters.Add("@UntilWhen", untilWhen);

            // Get the data
            DataSet ds = ConnectionHelper.ExecuteQuery("Chat.Notification.GetUnreadNotificationsUntil", parameters);
            if (!DataHelper.DataSourceIsEmpty(ds))
            {
                return ds.Tables[0].AsEnumerable().Select(dr => new ChatNotificationInfo(dr));
            }

            return Enumerable.Empty<ChatNotificationInfo>();
        }


        #endregion
    }
}
