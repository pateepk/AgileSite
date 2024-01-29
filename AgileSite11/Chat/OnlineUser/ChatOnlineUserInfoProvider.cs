using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;

using CMS.DataEngine;
using CMS.Helpers;

namespace CMS.Chat
{
    /// <summary>
    /// Class providing ChatOnlineUser management.
    /// </summary>
    public class ChatOnlineUserInfoProvider : AbstractInfoProvider<ChatOnlineUserInfo, ChatOnlineUserInfoProvider>
    {
        #region "Public methods - Basic"

        /// <summary>
        /// Returns the query for all chat online users.
        /// </summary>
        public static ObjectQuery<ChatOnlineUserInfo> GetChatOnlineUsers()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns chat online user with specified ID.
        /// </summary>
        /// <param name="userId">Chat online user ID.</param>        
        public static ChatOnlineUserInfo GetChatOnlineUser(int userId)
        {
            return ProviderObject.GetInfoById(userId);
        }


        /// <summary>
        /// Sets (updates or inserts) specified chat online user.
        /// </summary>
        /// <param name="userObj">Chat online user to be set.</param>
        public static void SetChatOnlineUser(ChatOnlineUserInfo userObj)
        {
            ProviderObject.SetInfo(userObj);
        }


        /// <summary>
        /// Deletes specified chat online user.
        /// </summary>
        /// <param name="userObj">Chat online user to be deleted.</param>
        public static void DeleteChatOnlineUser(ChatOnlineUserInfo userObj)
        {
            ProviderObject.DeleteInfo(userObj);
        }


        /// <summary>
        /// Deletes chat online user with specified ID.
        /// </summary>
        /// <param name="userId">Chat online user ID.</param>
        public static void DeleteChatOnlineUser(int userId)
        {
            ChatOnlineUserInfo userObj = GetChatOnlineUser(userId);
            DeleteChatOnlineUser(userObj);
        }

        #endregion


        #region "Public methods - Advanced"

        /// <summary>
        /// Gets online users for specified site.
        /// </summary>
        /// <param name="siteID">Site ID</param>
        /// <param name="changedSince">User changed since this time will be returned</param>
        /// <returns>Online users</returns>
        public static IEnumerable<OnlineUserData> GetChangedChatOnlineUsers(int siteID, DateTime changedSince)
        {
            // Prepare the parameters
            QueryDataParameters parameters = new QueryDataParameters();
            parameters.Add("@SiteID", siteID);
            parameters.Add("@ChangedSince", changedSince);

            // Get the data
            DataSet ds = ConnectionHelper.ExecuteQuery("Chat.OnlineUser.selectlatestonlineusers", parameters);
            if (!DataHelper.DataSourceIsEmpty(ds))
            {
                return ds.Tables[0].AsEnumerable().Select(row =>
                    new OnlineUserData(
                        new ChatUserInfo(row),
                        ValidationHelper.GetDateTime(row["LastChange"], DateTime.Now),
                        row.IsNull("LastChecking"),
                        ValidationHelper.GetBoolean(row["IsHidden"], false)
                        )
                    );
            }

            return Enumerable.Empty<OnlineUserData>();
        }


        /// <summary>
        /// Gets all users who are online right now on specified site.
        /// </summary>
        /// <param name="siteID">Site ID</param>
        /// <returns>Online users</returns>
        public static IEnumerable<OnlineUserData> GetAllChatOnlineUsers(int siteID)
        {
            // Prepare the parameters
            QueryDataParameters parameters = new QueryDataParameters();
            parameters.Add("@SiteID", siteID);

            // Get the data
            DataSet ds = ConnectionHelper.ExecuteQuery("Chat.OnlineUser.selectallonlineusers", parameters);
            if (!DataHelper.DataSourceIsEmpty(ds))
            {
                return ds.Tables[0].AsEnumerable().Select(row =>
                    new OnlineUserData(
                        new ChatUserInfo(row),
                        ValidationHelper.GetDateTime(row["LastChange"], DateTime.Now),
                        false,
                        ValidationHelper.GetBoolean(row["IsHidden"], false)
                        )
                    );
            }

            return Enumerable.Empty<OnlineUserData>();
        }


        /// <summary>
        /// Updates last checking to GETDATE() of the online chat user specified by siteID and chatUserID.
        /// 
        /// Does nothing if this online user does not exists.
        /// </summary>
        /// <param name="siteID">Site ID</param>
        /// <param name="chatUserID">Chat user ID</param>
        public static void UpdateLastChecking(int siteID, int chatUserID)
        {
            // Prepare the parameters
            QueryDataParameters parameters = new QueryDataParameters();
            parameters.Add("@SiteID", siteID);
            parameters.Add("@ChatUserID", chatUserID);

            // Execute query
            ConnectionHelper.ExecuteQuery("Chat.OnlineUser.updatelastchecking", parameters);
        }


        /// <summary>
        /// Logouts user from chat.
        /// 
        /// User is also removed from all rooms where he is online now. Leave room system message is inserted to this room.
        /// 
        /// Does nothing if this online user does not exists.
        /// </summary>
        /// <param name="siteID">Site ID</param>
        /// <param name="chatUserID">Chat user ID</param>
        /// <param name="leaveSystemMessageFormat">Format of system message. Placeholder {nickname} will be replaced by actual user's nickname. If null system message won't be inserted.</param>
        /// <param name="leaveSystemMessageType">Type of leave room system message (typically ChatMessageTypeEnum.LeaveRoom)</param>
        public static void Logout(int siteID, int chatUserID, string leaveSystemMessageFormat, ChatMessageTypeEnum? leaveSystemMessageType)
        {
            // Prepare the parameters
            QueryDataParameters parameters = new QueryDataParameters();
            parameters.Add("@SiteID", siteID);
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

            // Execute query
            ConnectionHelper.ExecuteQuery("Chat.OnlineUser.logout", parameters);
        }


        /// <summary>
        /// Logs in user to chat.
        /// It returns login token which can be used later to re-login user to chat if server looses session.
        /// If user is already logged in, old token is returned. Otherwise it is generated.
        /// </summary>
        /// <param name="siteID">Site ID</param>
        /// <param name="chatUserID">Chat user ID</param>
        /// <param name="isHidden">If false, this user will be shown in online users on live site. False has higher priority than true.</param>
        /// <returns>Login token</returns>
        public static string Login(int siteID, int chatUserID, bool isHidden)
        {
            // Prepare the parameters
            QueryDataParameters parameters = new QueryDataParameters();
            parameters.Add("@SiteID", siteID);
            parameters.Add("@ChatUserID", chatUserID);
            parameters.Add("@Hidden", isHidden);

            // Execute query
            return ConnectionHelper.ExecuteQuery("Chat.OnlineUser.login", parameters).Tables[0].Rows[0].Field<string>(0);
        }


        /// <summary>
        /// Cleans inactive (not pinging) users.
        /// </summary>
        /// <param name="inactiveForSeconds">Seconds of inactivity needed to clean user.</param>
        /// <param name="leaveSystemMessageFormat">Format of system message. Placeholder {nickname} will be replaced by actual user's nickname. If null system message won't be inserted.</param>
        /// <param name="leaveSystemMessageType">Type of leave room system message (typically ChatMessageTypeEnum.LeaveRoom)</param>
        /// <returns>Number of cleaned users</returns>
        public static int CleanOnlineUsers(int inactiveForSeconds, string leaveSystemMessageFormat, ChatMessageTypeEnum leaveSystemMessageType)
        {
            // Prepare the parameters
            QueryDataParameters parameters = new QueryDataParameters();
            parameters.Add("@InactiveForSeconds", inactiveForSeconds);
            parameters.Add("@LeaveRoomSystemMessageFormat", leaveSystemMessageFormat);
            parameters.Add("@SystemMessageType", leaveSystemMessageType);

            // Execute update
            DataSet ds = ConnectionHelper.ExecuteQuery("Chat.OnlineUser.CleanOnlineUsers", parameters);

            if (!DataHelper.DataSourceIsEmpty(ds))
            {
                return ds.Tables[0].Rows[0].Field<int>(0);
            }

            return 0;
        }


        /// <summary>
        /// Gets online user by token set in Login. User has to be logged in (Join time not null) and be logged in on the current site.
        /// </summary>
        /// <param name="token">Token</param>
        /// <param name="siteID">Only users online on this site will be searhed</param>
        public static ChatOnlineUserInfo GetOnlineUserByToken(string token, int siteID)
        {
            // Prepare the parameters
            QueryDataParameters parameters = new QueryDataParameters();
            parameters.Add("@ChatOnlineUserToken", token);
            parameters.Add("@SiteID", siteID);

            DataSet ds = ConnectionHelper.ExecuteQuery("Chat.OnlineUser.selectall", parameters, "[ChatOnlineUserToken] = @ChatOnlineUserToken AND [ChatOnlineUserSiteID] = @SiteID AND [ChatOnlineUserJoinTime] IS NOT NULL");

            if (!DataHelper.DataSourceIsEmpty(ds))
            {
                return new ChatOnlineUserInfo(ds.Tables[0].Rows[0]);
            }

            return null;
        }

        #endregion
    }
}
