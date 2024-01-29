using System;
using System.Data;
using System.Linq;

using CMS.DataEngine;
using CMS.Helpers;

namespace CMS.Chat
{
    /// <summary>
    /// Class providing ChatSupportTakenRoomsInfo management.
    /// </summary>
    public class ChatSupportTakenRoomInfoProvider : AbstractInfoProvider<ChatSupportTakenRoomInfo, ChatSupportTakenRoomInfoProvider>
    {
        #region "Public methods - Basic"

        /// <summary>
        /// Returns the query for all chat support taken rooms.
        /// </summary>
        public static ObjectQuery<ChatSupportTakenRoomInfo> GetChatSupportTakenRooms()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns room taken by support with specified ID.
        /// </summary>
        /// <param name="supportId">Room taken by support ID.</param>        
        public static ChatSupportTakenRoomInfo GetChatSupportTakenRoomsInfo(int supportId)
        {
            return ProviderObject.GetInfoById(supportId);
        }


        /// <summary>
        /// Sets (updates or inserts) specified room taken by support.
        /// </summary>
        /// <param name="supportObj">Room taken by support to be set.</param>
        public static void SetChatSupportTakenRoomsInfo(ChatSupportTakenRoomInfo supportObj)
        {
            ProviderObject.SetInfo(supportObj);
        }


        /// <summary>
        /// Deletes specified room taken by support.
        /// </summary>
        /// <param name="supportObj">Room taken by support to be deleted.</param>
        public static void DeleteChatSupportTakenRoomsInfo(ChatSupportTakenRoomInfo supportObj)
        {
            ProviderObject.DeleteInfo(supportObj);
        }


        /// <summary>
        /// Deletes room taken by support with specified ID.
        /// </summary>
        /// <param name="supportId">Room taken by support ID.</param>
        public static void DeleteChatSupportTakenRoomsInfo(int supportId)
        {
            ChatSupportTakenRoomInfo supportObj = GetChatSupportTakenRoomsInfo(supportId);
            DeleteChatSupportTakenRoomsInfo(supportObj);
        }

        #endregion


        #region "Public methods - Advanced"
        
        /// <summary>
        /// Deletes taken room. This room will no longer be taken. If room is taken by other user than <paramref name="chatUserID"/>, it does nothing.
        /// </summary>
        /// <param name="chatUserID">Chat user ID who has this room taken.</param>
        /// <param name="roomID">Room ID</param>
        public static void ResolveTakenRoom(int chatUserID, int roomID)
        {
            // Prepare the parameters
            QueryDataParameters parameters = new QueryDataParameters();
            parameters.Add("@ChatUserID", chatUserID);
            parameters.Add("@RoomID", roomID);

            ConnectionHelper.ExecuteQuery("Chat.SupportTakenRoom.resolvetakenroom", parameters);
        }

        
        /// <summary>
        /// Cleans taken rooms were support engineer is not pinging for more than <paramref name="inactiveForSeconds"/> seconds.
        /// 
        /// Last ping time is checked in table Chat_RoomUser.
        /// </summary>
        /// <param name="inactiveForSeconds">Seconds of inactivity needed for this room to be released</param>
        /// <returns>Number of cleaned rooms</returns>
        public static int CleanSupportTakenRooms(int inactiveForSeconds)
        {
            // Prepare the parameters
            QueryDataParameters parameters = new QueryDataParameters();
            parameters.Add("@InactiveForSeconds", inactiveForSeconds);

            // Get the data
            DataSet ds = ConnectionHelper.ExecuteQuery("Chat.SupportTakenRoom.cleantakenrooms", parameters);
            if (!DataHelper.DataSourceIsEmpty(ds))
            {
                return ds.Tables[0].Rows[0].Field<int>(0);
            }

            return 0;
        }


        /// <summary>
        /// Sets room as taken by chat user.
        /// </summary>
        /// <param name="chatUserID">Room will be taken by this chat user.</param>
        /// <param name="roomID">This room will be taken.</param>
        public static void TakeRoom(int chatUserID, int roomID)
        {
            // Prepare the parameters
            QueryDataParameters parameters = new QueryDataParameters();
            parameters.Add("@ChatUserID", chatUserID);
            parameters.Add("@RoomID", roomID);

            ConnectionHelper.ExecuteQuery("Chat.SupportTakenRoom.takeroom", parameters);
        }


        /// <summary>
        /// Checks if room is taken right now.
        /// </summary>
        /// <param name="roomID">ID of room to check</param>
        /// <param name="omitChatUserID">If room is taken by this user, it won't count</param>
        /// <returns>True if room is taken</returns>
        public static bool IsRoomTaken(int roomID, int omitChatUserID)
        {
            QueryDataParameters parameters = new QueryDataParameters();
            parameters.Add("@RoomID", roomID);
            parameters.Add("@OmitChatUserID", omitChatUserID);

            string where = "ChatSupportTakenRoomChatUserID IS NOT NULL AND ChatSupportTakenRoomRoomID = @RoomID AND ChatSupportTakenRoomChatUserID <> @OmitChatUserID";

            return !DataHelper.DataSourceIsEmpty(ConnectionHelper.ExecuteQuery("chat.SupportTakenRoom.selectall", parameters, where));
        }

        #endregion
    }
}
