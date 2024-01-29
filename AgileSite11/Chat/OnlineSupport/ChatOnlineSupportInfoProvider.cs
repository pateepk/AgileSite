using System;
using System.Data;
using System.Linq;
using System.Collections.Generic;

using CMS.DataEngine;
using CMS.Helpers;

namespace CMS.Chat
{
    /// <summary>
    /// Class providing ChatOnlineSupportInfo management.
    /// </summary>
    public class ChatOnlineSupportInfoProvider : AbstractInfoProvider<ChatOnlineSupportInfo, ChatOnlineSupportInfoProvider>
    {
        #region "Public methods - Basic"

        /// <summary>
        /// Returns the query for all chat online support engineers.
        /// </summary>
        public static ObjectQuery<ChatOnlineSupportInfo> GetChatOnlineSupportEngineers()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns online support engineer with specified ID.
        /// </summary>
        /// <param name="engineerId">Online support engineer ID.</param>        
        public static ChatOnlineSupportInfo GetChatOnlineSupportInfo(int engineerId)
        {
            return ProviderObject.GetInfoById(engineerId);
        }


        /// <summary>
        /// Returns online support engineer with specified GUID.
        /// </summary>
        /// <param name="engineerGuid">Online support engineer GUID.</param>                
        public static ChatOnlineSupportInfo GetChatOnlineSupportInfo(Guid engineerGuid)
        {
            return ProviderObject.GetInfoByGuid(engineerGuid);
        }


        /// <summary>
        /// Sets (updates or inserts) specified online support engineer.
        /// </summary>
        /// <param name="engineerObj">Online support engineer to be set.</param>
        public static void SetChatOnlineSupportInfo(ChatOnlineSupportInfo engineerObj)
        {
            ProviderObject.SetInfo(engineerObj);
        }


        /// <summary>
        /// Deletes specified online support engineer.
        /// </summary>
        /// <param name="engineerObj">Online support engineer to be deleted.</param>
        public static void DeleteChatOnlineSupportInfo(ChatOnlineSupportInfo engineerObj)
        {
            ProviderObject.DeleteInfo(engineerObj);
        }


        /// <summary>
        /// Deletes online support engineer with specified ID.
        /// </summary>
        /// <param name="engineerId">Online support engineer ID.</param>
        public static void DeleteChatOnlineSupportInfo(int engineerId)
        {
            ChatOnlineSupportInfo engineerObj = GetChatOnlineSupportInfo(engineerId);
            DeleteChatOnlineSupportInfo(engineerObj);
        }

        #endregion


        #region "Public methods - Advanced"

        /// <summary>
        /// Updates lastChecking info to GETDATE() for selected online support engineer.
        /// </summary>
        /// <param name="siteID">Last checking for user will be updated on this site</param>
        /// <param name="chatUserID">ID of chat user who is online on support</param>
        public static void UpdateLastChecking(int siteID, int chatUserID)
        {
            // Prepare the parameters
            QueryDataParameters parameters = new QueryDataParameters();
            parameters.Add("@ChatUserID", chatUserID);
            parameters.Add("@SiteID", siteID);

            ConnectionHelper.ExecuteQuery("Chat.OnlineSupport.updatelastchecking", parameters);
        }


        /// <summary>
        /// Gets dictionary (ChatUserID => ChatOnlineSupportInfo) with all online supporters from DB.
        /// </summary>
        /// <param name="siteID">Supporters online on this site will be returned</param>
        public static Dictionary<int, ChatOnlineSupportInfo> GetAllOnlineSupporters(int siteID)
        {
             return GetChatOnlineSupportEngineers().OnSite(siteID).ToDictionary(os => os.ChatOnlineSupportChatUserID);
        }

        
        /// <summary>
        /// Inserts user into table Chat_OnlineSupport. If user is already logged into support, it returns existing token. Otherwise it generates new token and returns it.
        /// </summary>
        /// <param name="siteID">User will be logged into support on this site</param>
        /// <param name="chatUserID">User to enter support</param>
        /// <returns>Login token which should be stored in cookies</returns>
        public static string EnterSupport(int siteID, int chatUserID)
        {
            // Prepare the parameters
            QueryDataParameters parameters = new QueryDataParameters();
            parameters.Add("@ChatUserID", chatUserID);
            parameters.Add("@SiteID", siteID);

            // Execute query
            return ConnectionHelper.ExecuteQuery("Chat.OnlineSupport.entersupport", parameters).Tables[0].Rows[0].Field<string>(0);
        }


        /// <summary>
        /// Leaves user from support.
        /// </summary>
        /// <param name="siteID">User will be logged out of support on this site</param>
        /// <param name="chatUserID">User to leave.</param>
        public static void LeaveSupport(int siteID, int chatUserID)
        {
            // Prepare the parameters
            QueryDataParameters parameters = new QueryDataParameters();
            parameters.Add("@ChatUserID", chatUserID);
            parameters.Add("@SiteID", siteID);
            
            ConnectionHelper.ExecuteQuery("Chat.OnlineSupport.leavesupport", parameters);
        }


        /// <summary>
        /// Cleans inactive (not pinging) online supporters.
        /// </summary>
        /// <param name="inactiveForSeconds">Number of seconds of inactivity needed to clean user</param>
        /// <returns>Number of cleaned users</returns>
        public static int CleanOnlineSupport(int inactiveForSeconds)
        {
            // Prepare the parameters
            QueryDataParameters parameters = new QueryDataParameters();
            parameters.Add("@InactiveForSeconds", inactiveForSeconds);

            // Get the data
            DataSet ds = ConnectionHelper.ExecuteQuery("Chat.OnlineSupport.cleanonlinesupport", parameters);
            if (!DataHelper.DataSourceIsEmpty(ds))
            {
                return ds.Tables[0].Rows[0].Field<int>(0);
            }

            return 0;
        }


        /// <summary>
        /// Gets online supporter by token. Returns null if user with this token is not found or is not online.
        /// </summary>
        /// <param name="token">Login token</param>
        /// <param name="siteID">Users on this site will be searched</param>
        public static ChatOnlineSupportInfo GetOnlineSupportByToken(string token, int siteID)
        {
            // Prepare the parameters
            QueryDataParameters parameters = new QueryDataParameters();
            parameters.Add("@ChatOnlineSupportToken", token);
            parameters.Add("@SiteID", siteID);

            DataSet ds = ConnectionHelper.ExecuteQuery("Chat.OnlineSupport.selectall", parameters, "[ChatOnlineSupportToken] = @ChatOnlineSupportToken AND [ChatOnlineSupportSiteID] = @SiteID");

            if (!DataHelper.DataSourceIsEmpty(ds))
            {
                return new ChatOnlineSupportInfo(ds.Tables[0].Rows[0]);
            }

            return null;
        }

        #endregion
    }
}
