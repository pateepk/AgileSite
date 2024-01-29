using System.Data;
using System.Collections.Generic;
using System;
using System.Linq;

using CMS.DataEngine;
using CMS.Helpers;
using CMS.SiteProvider;

namespace CMS.Chat
{
    /// <summary>
    /// Class providing ChatRoomInfo management.
    /// </summary>
    public class ChatRoomInfoProvider : AbstractInfoProvider<ChatRoomInfo, ChatRoomInfoProvider>
    {
        #region "Public methods - Basic"

        /// <summary>
        /// Returns the query for all chat rooms.
        /// </summary>
        public static ObjectQuery<ChatRoomInfo> GetChatRooms()
        {
            return ProviderObject.GetObjectQuery();
        }
        

        /// <summary>
        /// Returns chat room with specified ID.
        /// </summary>
        /// <param name="roomId">Chat room ID.</param>        
        public static ChatRoomInfo GetChatRoomInfo(int roomId)
        {
            return ProviderObject.GetInfoById(roomId);
        }


        /// <summary>
        /// Returns chat room with specified name.
        /// </summary>
        /// <param name="roomName">Chat room name.</param>                
        /// <param name="siteName">Site name.</param>                
        public static ChatRoomInfo GetChatRoomInfo(string roomName, string siteName)
        {
            return ProviderObject.GetInfoByCodeName(roomName, SiteInfoProvider.GetSiteID(siteName));
        }


        /// <summary>
        /// Returns chat room with specified name.
        /// </summary>
        /// <param name="roomCodeName">Chat room code name.</param>           
        public static ChatRoomInfo GetChatRoomInfo(string roomCodeName)
        {
            return ProviderObject.GetInfoByCodeName(roomCodeName);
        }




        /// <summary>
        /// Sets (updates or inserts) specified chat room.
        /// </summary>
        /// <param name="roomObj">Chat room to be set.</param>
        public static void SetChatRoomInfo(ChatRoomInfo roomObj)
        {
            ProviderObject.SetInfo(roomObj);
        }


        /// <summary>
        /// Deletes specified chat room.
        /// </summary>
        /// <param name="roomObj">Chat room to be deleted.</param>
        public static void DeleteChatRoomInfo(ChatRoomInfo roomObj)
        {
            ProviderObject.DeleteInfo(roomObj);
        }


        /// <summary>
        /// Deletes chat room with specified ID.
        /// </summary>
        /// <param name="roomId">Chat room ID.</param>
        public static void DeleteChatRoomInfo(int roomId)
        {
            ChatRoomInfo roomObj = GetChatRoomInfo(roomId);
            DeleteChatRoomInfo(roomObj);
        }

        #endregion


        #region "Public methods - Advanced"

        /// <summary>
        /// Gets chat rooms changed since specified time. Does not select one to one rooms!
        /// </summary>
        /// <param name="siteID">Site ID</param>
        /// <param name="changedSince">Rooms changed since this time will be returned. If null all rooms will be returned</param>
        public static IEnumerable<ChatRoomInfo> GetChangedChatRooms(int siteID, DateTime? changedSince)
        {
            // Prepare the parameters
            QueryDataParameters parameters = new QueryDataParameters();
            parameters.Add("@SiteID", siteID);
            parameters.Add("@ChangedSince", changedSince);

            // Get the data
            DataSet ds = ConnectionHelper.ExecuteQuery("Chat.Room.selectchangedrooms", parameters);

            return ds.Tables[0].AsEnumerable().Select(dr => new ChatRoomInfo(dr));
        }


        /// <summary>
        /// Disables room.
        /// </summary>
        /// <param name="roomID">Room ID</param>
        public static void DisableRoom(int roomID)
        {
            // Prepare the parameters
            QueryDataParameters parameters = new QueryDataParameters();
            parameters.Add("@ChatRoomID", roomID);

            ConnectionHelper.ExecuteQuery("Chat.Room.disableroom", parameters);
            UpdateCachedRooms();
        }


        /// <summary>
        /// Enables room.
        /// </summary>
        /// <param name="roomID">Room ID</param>
        public static void EnableRoom(int roomID)
        {
            // Prepare the parameters
            QueryDataParameters parameters = new QueryDataParameters();
            parameters.Add("@ChatRoomID", roomID);

            ConnectionHelper.ExecuteQuery("Chat.Room.enableroom", parameters);
        }


        /// <summary>
        /// Marks room to be deleted. Sets ChatRoomScheduledToDelete.
        /// </summary>
        /// <param name="roomID">RoomID to delete</param>
        public static void SafeDelete(int roomID)
        {
            // Prepare the parameters
            QueryDataParameters parameters = new QueryDataParameters();
            parameters.Add("@ChatRoomID", roomID);
            parameters.Add("@AfterHours", 4); // After 4 hours room will be deleted by scheduled task

            ConnectionHelper.ExecuteQuery("Chat.Room.safedelete", parameters);
            UpdateCachedRooms();
        }


        /// <summary>
        /// Deletes rooms scheduled to be deleted (by SafeDelete()).
        /// </summary>
        public static void DeleteScheduledRooms()
        {
            var oldChatRooms = GetChatRooms().Where("ChatRoomScheduledToDelete < GETDATE()");

            foreach (ChatRoomInfo chatRoom in oldChatRooms)
            {
                chatRoom.Delete();
            }
        }


        /// <summary>
        /// Gets one to one chat room with specified code name. Room must be global, because all one one one rooms are global.
        /// </summary>
        /// <param name="roomCodeName">Code name of a room.</param>
        /// <returns>ChatRoomInfo or null</returns>
        public static ChatRoomInfo GetOneToOneChatRoomInfo(string roomCodeName)
        {
            return GetChatRooms()
                .WhereTrue("ChatRoomIsOneToOne")
                .WhereEquals("ChatRoomName", roomCodeName)
                .WhereNull("ChatRoomSiteID")
                .TopN(1).FirstOrDefault();
        }


        /// <summary>
        /// Gets one to one chat room with specified id.
        /// </summary>
        /// <param name="chatRoomID">ID of a room.</param>
        /// <returns>ChatRoomInfo or null</returns>
        public static ChatRoomInfo GetOneToOneChatRoomInfo(int chatRoomID)
        {
            return GetChatRooms()
                .WhereTrue("ChatRoomIsOneToOne")
                .WhereEquals("ChatRoomID", chatRoomID)
                .TopN(1).FirstOrDefault();
        }


        /// <summary>
        /// Gets support chat room with specified code name.
        /// </summary>
        /// <param name="roomCodeName">Code name of a room</param>
        /// <returns>ChatRoomInfo or null</returns>
        public static ChatRoomInfo GetSupportChatRoomInfo(string roomCodeName)
        {
            return GetChatRooms()
                .WhereTrue("ChatRoomIsSupport")
                .WhereEquals("ChatRoomName", roomCodeName)
                .TopN(1).FirstOrDefault();
        }


        /// <summary>
        /// Gets support rooms where new messages was added since <paramref name="lastChange"/> or taken room state was changed.
        /// </summary>
        /// <param name="lastChange">If null, all messages which were added later than resolveddatetime are considered as new.</param>
        /// <param name="siteID">Only global rooms and rooms on this site will be returned</param>
        public static IEnumerable<SupportRoom> GetSupportRoomsWithNewMessages(DateTime? lastChange, int siteID)
        {
            // Prepare the parameters
            QueryDataParameters parameters = new QueryDataParameters();
            parameters.Add("@LastChange", lastChange);
            parameters.Add("@SiteID", siteID);

            // Get the data
            DataSet ds = ConnectionHelper.ExecuteQuery("Chat.Room.selectchangedsupportrooms", parameters);
            if (!DataHelper.DataSourceIsEmpty(ds))
            {
                return ds.Tables[0].AsEnumerable().Select(dr =>
                    new SupportRoom
                    {
                        ChatRoomID = ValidationHelper.GetInteger(dr["ChatRoomID"], 0),
                        DisplayName = ValidationHelper.GetString(dr["ChatRoomDisplayName"], ""),
                        UnreadMessagesCount = ValidationHelper.GetInteger(dr["UnreadMessagesCount"], 0),
                        TakenByChatUserID = dr.IsNull("TakenByChatUserID") ? null : (int?)ValidationHelper.GetInteger(dr["TakenByChatUserID"], 0),
                        TakenStateLastChange = dr.IsNull("TakenStateLastChange") ? null : (DateTime?)ValidationHelper.GetDateTime(dr["TakenStateLastChange"], DateTimeHelper.ZERO_TIME),
                        ChangeTime = ValidationHelper.GetDateTime(dr["LastChange"], DateTimeHelper.ZERO_TIME),
                    }
                );
            }

            return Enumerable.Empty<SupportRoom>();
        }


        /// <summary>
        /// Cleans unused records from Chat. Affects Users, Messages and Rooms.
        /// 
        /// This query affects all chat tables.
        /// </summary>
        /// <returns>Number of deleted records</returns>
        public static DeletedRecords CleanOldChatRecords(int daysOld)
        {
            // Prepare the parameters
            QueryDataParameters parameters = new QueryDataParameters();
            parameters.Add("@DaysOld", daysOld);

            // Get the data
            DataSet ds = ConnectionHelper.ExecuteQuery("Chat.Room.CleanOldChatRecords", parameters);
            if (!DataHelper.DataSourceIsEmpty(ds))
            {
                DataRow row = ds.Tables[0].Rows[0];
                return new DeletedRecords
                {
                    MessagesDeleted = ValidationHelper.GetInteger(row["MessagesDeleted"], 0),
                    RoomsDeleted = ValidationHelper.GetInteger(row["RoomsDeleted"], 0),
                    UsersDeleted = ValidationHelper.GetInteger(row["UsersDeleted"], 0),
                };
            }

            return null;
        }


        /// <summary>
        /// Gets current time on SQL Server. 
        /// </summary>
        internal static DateTime GetCurrentDateTime()
        {
            DataSet ds = ConnectionHelper.ExecuteQuery("SELECT GETDATE() AS CurrentDateTime", null, QueryTypeEnum.SQLQuery);

            return ds.Tables[0].Rows[0].Field<DateTime>(0);
        }


        /// <summary>
        /// Updates data of all cached rooms.
        /// </summary>
        internal static void UpdateCachedRooms()
        {
            SiteState siteState = ChatGlobalData.Instance.Sites.Current;

            if (siteState != null)
            {
                SiteRooms roomsOnCurrentSite = siteState.Rooms;

                roomsOnCurrentSite.ForceUpdate();
            }
        }

        #endregion


        #region "Internal methods - Basic"

        /// <summary>
        /// Inserts or Updates the object to the database.
        /// </summary>
        /// <param name="info">Object to insert / update</param>
        protected override void SetInfo(ChatRoomInfo info)
        {
            ChatProtectionHelper.CheckNameForBadWords(info.ChatRoomDisplayName);
            ChatProtectionHelper.CheckNameForBadWords(info.ChatRoomDescription);

            // Special cases (such as calling special stored procedure on update) are handled in ChatRoomInfo's UpdateData method.
            base.SetInfo(info);
        }

        #endregion
    }
}
