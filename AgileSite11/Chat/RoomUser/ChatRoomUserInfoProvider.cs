using System;
using System.Data;
using System.Linq;
using System.Collections.Generic;

using CMS.DataEngine;
using CMS.DataEngine.Query;
using CMS.Helpers;

namespace CMS.Chat
{
    /// <summary>
    /// Class providing ChatRoomUser management.
    /// </summary>
    public class ChatRoomUserInfoProvider : AbstractInfoProvider<ChatRoomUserInfo, ChatRoomUserInfoProvider>
    {
        #region "Public methods - Basic"

        /// <summary>
        /// Returns the query for all chat room users.
        /// </summary>
        public static ObjectQuery<ChatRoomUserInfo> GetChatRoomUsers()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns chat room user with specified ID.
        /// </summary>
        /// <param name="userId">Chat room user ID.</param>        
        public static ChatRoomUserInfo GetChatRoomUser(int userId)
        {
            return ProviderObject.GetInfoById(userId);
        }


        /// <summary>
        /// Sets (updates or inserts) specified chat room user.
        /// </summary>
        /// <param name="userObj">Chat room user to be set.</param>
        public static void SetChatRoomUser(ChatRoomUserInfo userObj)
        {
            ProviderObject.SetInfo(userObj);
        }


        /// <summary>
        /// Deletes specified chat room user.
        /// </summary>
        /// <param name="userObj">Chat room user to be deleted.</param>
        public static void DeleteChatRoomUser(ChatRoomUserInfo userObj)
        {
            ProviderObject.DeleteInfo(userObj);
        }


        /// <summary>
        /// Deletes chat room user with specified ID.
        /// </summary>
        /// <param name="userId">Chat room user ID.</param>
        public static void DeleteChatRoomUser(int userId)
        {
            ChatRoomUserInfo userObj = GetChatRoomUser(userId);
            DeleteChatRoomUser(userObj);
        }

        #endregion


        #region "Public methods - Advanced"

        /// <summary>
        /// Gets users in a room who are online right now.
        /// </summary>
        /// <param name="roomID">Chat room ID.</param>
        /// <returns>List of online users in a room.</returns>
        public static IEnumerable<RoomOnlineUserData> GetAllOnlineUsersInRoom(int roomID)
        {
            // Prepare the parameters
            QueryDataParameters parameters = new QueryDataParameters();
            parameters.Add("@RoomID", roomID);

            // Get the data
            DataSet ds = ConnectionHelper.ExecuteQuery("Chat.RoomUser.getallonlineusersinroom", parameters);
            if (!DataHelper.DataSourceIsEmpty(ds))
            {
                return ds.Tables[0].AsEnumerable().Select(row =>
                    new RoomOnlineUserData(
                        new ChatUserInfo(row),
                        ValidationHelper.GetBoolean(row["IsOnline"], false),
                        ChatHelper.GetEnum(ValidationHelper.GetInteger(row["ChatRoomUserAdminLevel"], 0), AdminLevelEnum.None),
                        ValidationHelper.GetDateTime(row["LastChange"], DateTime.Now),
                        false
                ));
            }

            return Enumerable.Empty<RoomOnlineUserData>();
        }


        /// <summary>
        /// Gets online users in a room.
        /// </summary>
        /// <param name="roomID">Chat room ID.</param>
        /// <param name="changedSince">Get room users changed since this time</param>
        /// <returns>List of online users in a room.</returns>
        public static IEnumerable<RoomOnlineUserData> GetRoomOnlineUsers(int roomID, DateTime changedSince)
        {
            // Prepare the parameters
            QueryDataParameters parameters = new QueryDataParameters();
            parameters.Add("@RoomID", roomID);
            parameters.Add("@ChangedSince", changedSince);

            // Get the data
            DataSet ds = ConnectionHelper.ExecuteQuery("Chat.RoomUser.selectlatestonlineusers", parameters);
            if (!DataHelper.DataSourceIsEmpty(ds))
            {
                return ds.Tables[0].AsEnumerable().Select(row => 
                    new RoomOnlineUserData(
                        new ChatUserInfo(row), 
                        ValidationHelper.GetBoolean(row["IsOnline"], false),
                        ChatHelper.GetEnum(ValidationHelper.GetInteger(row["ChatRoomUserAdminLevel"], 0), AdminLevelEnum.None),
                        ValidationHelper.GetDateTime(row["LastChange"], DateTime.Now),
                        ValidationHelper.GetBoolean(row["IsRemoved"], false)
                    )
                );
            }

            return Enumerable.Empty<RoomOnlineUserData>();
        }


        /// <summary>
        /// User leaves room. Sets his JoinTime to null, LeaveTime to now, LastModification to now. Etc. 
        /// This method also inserts system message about leaving to this room.
        /// </summary>
        /// <param name="roomID">User will be left from this room</param>
        /// <param name="chatUserID">Chat user ID</param>
        /// <param name="leaveSystemMessageFormat">Format of system message. Placeholder {nickname} will be replaced by actual user's nickname. If null system message won't be inserted.</param>
        /// <param name="leaveSystemMessageType">Type of leave room system message (typically ChatMessageTypeEnum.LeaveRoom)</param>
        public static void LeaveRoom(int roomID, int chatUserID, string leaveSystemMessageFormat, ChatMessageTypeEnum? leaveSystemMessageType)
        {
            // Prepare the parameters
            QueryDataParameters parameters = new QueryDataParameters();
            parameters.Add("@ChatRoomID", roomID);
            parameters.Add("@ChatUserID", chatUserID);
            if ((leaveSystemMessageFormat != null) && leaveSystemMessageType.HasValue)
            {
                parameters.Add("@LeaveRoomSystemMessageFormat", leaveSystemMessageFormat);
                parameters.Add("@SystemMessageType", leaveSystemMessageType.Value);
            }
            else
            {
                parameters.Add("@LeaveRoomSystemMessageFormat", null);
                parameters.Add("@SystemMessageType", null);
            }

            // Execute update
            ConnectionHelper.ExecuteQuery("Chat.RoomUser.userleaveroom", parameters);

            RoomState room;
            if (ChatGlobalData.Instance.Sites.Current.Rooms.ForceTryGetRoom(roomID, out room) && room.RoomInfo.ChatRoomIsOneToOne)
            {
                room.OnlineUsers.Remove(chatUserID);
            }
        }


        /// <summary>
        /// Joins user to room. Sets his JoinTime to now, LeaveTime to null, etc.
        /// 
        /// This method checks automatically password. It takes into account actual 'join state' of this user -> if user is already joined, he does not have to provide correct password.
        /// 
        /// 'Enabled state' of this room is also checked.
        /// </summary>
        /// <param name="roomID">Room ID</param>
        /// <param name="chatUserID">Chat user ID</param>
        /// <param name="password">Room's password. If room has not password this parameter will be ignored.</param>
        /// <returns>Result of Joining</returns>
        public static JoinRoomResultEnum JoinRoom(int roomID, int chatUserID, string password)
        {
            // Prepare the parameters
            QueryDataParameters parameters = new QueryDataParameters();
            parameters.Add("@ChatRoomID", roomID);
            parameters.Add("@ChatUserID", chatUserID);
            parameters.Add("@RoomPassword", password);

            // Execute update
            DataSet ds = ConnectionHelper.ExecuteQuery("Chat.RoomUser.userjoinroom", parameters);

            if (DataHelper.DataSourceIsEmpty(ds))
            {
                return JoinRoomResultEnum.RoomDisabled;
            }

            switch (ds.Tables[0].Rows[0].Field<int>(0))
            {
                case 0:
                    return JoinRoomResultEnum.AlreadyIn;
                case 1:
                    return JoinRoomResultEnum.Joined;
                case -1:
                    return JoinRoomResultEnum.WrongPassword;
                case -2:
                default:
                    return JoinRoomResultEnum.RoomDisabled;
            }
        }


        /// <summary>
        /// Gets all chat room users by roomID.
        /// </summary>
        /// <param name="roomID">Room ID</param>
        public static IEnumerable<ChatRoomUserInfo> GetChatRoomUsersByRoomID(int roomID)
        {
            return GetChatRoomUsers()
                .WhereEquals("ChatRoomUserRoomID", roomID);
        }


        /// <summary>
        /// Gets anonymous chat room users in room.
        /// </summary>
        /// <param name="roomID">Room ID</param>
        public static IEnumerable<ChatRoomUserInfo> GetAnonymousChatRoomUsersByRoomID(int roomID)
        {
            return GetChatRoomUsers()
                .WhereEquals("ChatRoomUserRoomID", roomID)
                .WhereNull(new ObjectQuery<ChatUserInfo>().Column("ChatUserUserID").WhereEquals("ChatUserID", "ChatRoomUserChatUserID".AsColumn()));
        }


        /// <summary>
        /// Gets chat room user.
        /// </summary>
        /// <param name="chatUserID">Chat user ID</param>
        /// <param name="roomID">Room ID</param>
        /// <returns>ChatRoomUserInfo or null</returns>
        public static ChatRoomUserInfo GetChatRoomUser(int chatUserID, int roomID)
        {
            return GetChatRoomUsers()
                .WhereEquals("ChatRoomUserChatUserID", chatUserID)
                .WhereEquals("ChatRoomUserRoomID", roomID)
                .TopN(1).FirstOrDefault();
        }


        /// <summary>
        /// Updates LastChecking (LastPing) in table ChatRoomUser of chat user with <paramref name="chatUserID"/>
        /// in room with <paramref name="chatRoomID"/> to GETDATE() (sql server time).
        /// </summary>
        /// <param name="chatUserID">ID of a chat user</param>
        /// <param name="chatRoomID">Room ID</param>
        /// <returns>True if everything is ok. False if user is not online in this room.</returns>
        public static bool UpdateLastChecking(int chatUserID, int chatRoomID)
        {
            // Prepare the parameters
            QueryDataParameters parameters = new QueryDataParameters();
            parameters.Add("@RoomID", chatRoomID);
            parameters.Add("@ChatUserID", chatUserID);

            // Execute update
            DataSet ds = ConnectionHelper.ExecuteQuery("Chat.RoomUser.updatelastchecking", parameters);
            if (!DataHelper.DataSourceIsEmpty(ds))
            {
                return ds.Tables[0].Rows[0].Field<int>(0) == 1;
            }
            return false;
        }


        /// <summary>
        /// Gets counts of users in rooms for all rooms in this site.
        /// </summary>
        /// <param name="siteID">Site ID</param>
        public static IEnumerable<OnlineUsersCountData> GetOnlineUsersCounts(int siteID)
        {
            // Prepare the parameters
            QueryDataParameters parameters = new QueryDataParameters();
            parameters.Add("@SiteID", siteID);

            // Get the data
            DataSet ds = ConnectionHelper.ExecuteQuery("Chat.RoomUser.getonlineuserscounts", parameters);
            if (!DataHelper.DataSourceIsEmpty(ds))
            {
                return ds.Tables[0].AsEnumerable().Select(
                    dr => new OnlineUsersCountData()
                    {
                        RoomID = ValidationHelper.GetInteger(dr["RoomID"], 0),
                        UsersCount = ValidationHelper.GetInteger(dr["OnlineUsersCount"], 0),
                        LastChange = ValidationHelper.GetDateTime(dr["LastChange"], DateTime.MinValue),
                    }
                );
            }

            return Enumerable.Empty<OnlineUsersCountData>();
        }


        /// <summary>
        /// Updates KickExpiration in table ChatRoomUser of chat user with <paramref name="chatUserID"/>
        /// in room with <paramref name="chatRoomID"/>.
        /// </summary>
        /// <param name="chatUserID">ID of a chat user</param>
        /// <param name="chatRoomID">Room ID</param>
        /// <param name="kickExpiration">KickExpiration to set</param>
        public static void KickUser(int chatUserID, int chatRoomID, DateTime kickExpiration)
        {
            // Prepare the parameters
            QueryDataParameters parameters = new QueryDataParameters();
            parameters.Add("@RoomID", chatRoomID);
            parameters.Add("@ChatUserID", chatUserID);
            parameters.Add("@KickExpiration", kickExpiration);

            // Execute update
            ConnectionHelper.ExecuteQuery("Chat.RoomUser.kickuser", parameters);
        }


        /// <summary>
        /// Kicks permanently from room (removes access rights)
        /// </summary>
        /// <param name="roomID">Room ID</param>
        /// <param name="chatUserID">Chat user ID</param>
        public static void KickPermanentlyFromRoom(int roomID, int chatUserID)
        {
            // Prepare the parameters
            QueryDataParameters parameters = new QueryDataParameters();
            parameters.Add("@ChatRoomID", roomID);
            parameters.Add("@ChatUserID", chatUserID);

            // Execute update
            ConnectionHelper.ExecuteQuery("Chat.RoomUser.KickPermanentlyFromRoom", parameters);
        }


        /// <summary>
        /// Gets chat user who is present in room with <paramref name="roomID"/>.
        /// 
        /// User with ID in <paramref name="currentChatUserID"/> is omited.
        /// 
        /// This method should be used only for one to one rooms.
        /// </summary>
        /// <param name="currentChatUserID">ID of a current chat user</param>
        /// <param name="roomID">Room ID</param>
        public static ChatRoomUserInfo GetSecondChatUserIDInOneToOneRoom(int currentChatUserID, int roomID)
        {
            // Prepare the parameters
            QueryDataParameters parameters = new QueryDataParameters();
            parameters.Add("@CurrentChatUserID", currentChatUserID);
            parameters.Add("@RoomID", roomID);

            // Get the data
            DataSet ds = ConnectionHelper.ExecuteQuery("Chat.RoomUser.getsecondchatuseridinonetooneroom", parameters);
            if (!DataHelper.DataSourceIsEmpty(ds) && ds.Tables[0].Rows.Count == 1)
            {
                return new ChatRoomUserInfo(ds.Tables[0].Rows[0]);
            }

            return null;
        }


        /// <summary>
        /// Removes inactive users from all rooms.
        /// </summary>
        /// <param name="inactiveForSeconds">Number of seconds needed for user to be considered as inactive.</param>
        /// <param name="leaveSystemMessageFormat">Format of leave room system message. {nickname} will be replaced with actual user's nickname</param>
        /// <param name="leaveSystemMessageType">Type of leave room system message.</param>
        /// <returns>Number of cleaned users</returns>
        public static int CleanOnlineUsersInRooms(int inactiveForSeconds, string leaveSystemMessageFormat, ChatMessageTypeEnum leaveSystemMessageType)
        {
            // Prepare the parameters
            QueryDataParameters parameters = new QueryDataParameters();
            parameters.Add("@InactiveForSeconds", inactiveForSeconds);
            parameters.Add("@LeaveRoomSystemMessageFormat", leaveSystemMessageFormat);
            parameters.Add("@SystemMessageType", leaveSystemMessageType);

            // Execute update
            DataSet ds = ConnectionHelper.ExecuteQuery("Chat.RoomUser.CleanOnlineUsersInRoom", parameters);

            if (!DataHelper.DataSourceIsEmpty(ds))
            {
                return ds.Tables[0].Rows[0].Field<int>(0);
            }

            return 0;
        }


        /// <summary>
        /// Gets kicked users in one room.
        /// </summary>
        /// <param name="chatRoomID">Room ID</param>
        /// <returns>Kicked users</returns>
        public static KickedUsers GetKickedUsers(int chatRoomID)
        {
            // Prepare the parameters
            QueryDataParameters parameters = new QueryDataParameters();
            parameters.Add("@ChatRoomID", chatRoomID);
            
            // Execute update
            DataSet ds = ConnectionHelper.ExecuteQuery("Chat.RoomUser.getkickedusers", parameters);

            if (!DataHelper.DataSourceIsEmpty(ds))
            {
                return new KickedUsers(ds.Tables[0].AsEnumerable().ToDictionary(
                                row => ValidationHelper.GetInteger(row["ChatUserID"], 0), 
                                row => ValidationHelper.GetDateTime(row["KickExpiration"], DateTime.MinValue)));
            }

            return new KickedUsers(null);
        }


        /// <summary>
        /// Gets rooms where specified chat user is online now.
        /// </summary>
        /// <param name="chatUserID">Chat user ID</param>
        /// <returns>IDs of rooms</returns>
        public static IEnumerable<int> GetRoomsWhereChatUserIsOnline(int chatUserID)
        {
            // Prepare the parameters
            QueryDataParameters parameters = new QueryDataParameters();
            parameters.Add("@ChatUserID", chatUserID);

            // Execute update
            DataSet ds = ConnectionHelper.ExecuteQuery("Chat.RoomUser.GetRoomsWhereChatUserIsOnline", parameters);

            if (!DataHelper.DataSourceIsEmpty(ds))
            {
                return ds.Tables[0].AsEnumerable().Select(row => row.Field<int>(0));
            }

            return Enumerable.Empty<int>();
        }

        #region "Admin stuff"

        /// <summary>
        /// Sets admin level of specified user in specified room to specified level.
        /// </summary>
        /// <param name="chatRoomID">Room ID</param>
        /// <param name="chatUserID">Chat user ID</param>
        /// <param name="adminLevel">New admin level</param>
        public static void SetChatAdminLevel(int chatRoomID, int chatUserID, AdminLevelEnum adminLevel)
        {
            QueryDataParameters parameters = new QueryDataParameters();
            parameters.Add("@ChatUserID", chatUserID);
            parameters.Add("@ChatRoomID", chatRoomID);
            parameters.Add("@AdminLevel", (int)adminLevel);

            ConnectionHelper.ExecuteQuery("Chat.RoomUser.SetChatAdminLevel", parameters);
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
            QueryDataParameters parameters = new QueryDataParameters();
            parameters.Add("@ChatUserID", chatUserID);
            parameters.Add("@ChatRoomID", chatRoomID);
            parameters.Add("@AdminLevel", (int)adminLevel);

            ConnectionHelper.ExecuteQuery("Chat.RoomUser.IncreaseChatAdminLevel", parameters);
        }


        /// <summary>
        /// Gets admin states in rooms for one user. All rooms where this user was ever present are returned.
        /// </summary>
        /// <param name="chatUserID">Chat user ID</param>
        /// <returns>Admin states</returns>
        public static UserRoomAdminState GetAdminStates(int chatUserID)
        {
            QueryDataParameters parameters = new QueryDataParameters();
            parameters.Add("@ChatUserID", chatUserID);

            DataSet ds = ConnectionHelper.ExecuteQuery("Chat.RoomUser.selectadminstates", parameters);
            if (!DataHelper.DataSourceIsEmpty(ds))
            {
                return new UserRoomAdminState(
                    ds.Tables[0].AsEnumerable().Select(row => new RoomAdminState(
                        ValidationHelper.GetInteger(row["RoomID"], 0),
                        ValidationHelper.GetDateTime(row["LastChange"], DateTime.MinValue),
                        (AdminLevelEnum)ValidationHelper.GetInteger(row["AdminLevel"], 0),
                        ValidationHelper.GetBoolean(row["ChatRoomIsOneToOne"], false)
                    )
                ));
            }

            return null;
        }

        #endregion

        #endregion
    }
}
