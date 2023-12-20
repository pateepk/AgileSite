using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;

using CMS.DataEngine;
using CMS.Helpers;

namespace CMS.Chat
{
    /// <summary>
    /// Class providing ChatSupportCannedResponseInfo management.
    /// </summary>
    public class ChatSupportCannedResponseInfoProvider : AbstractInfoProvider<ChatSupportCannedResponseInfo, ChatSupportCannedResponseInfoProvider>
    {
        #region "Public methods - Basic"

        /// <summary>
        /// Returns the query for all chat support canned responses.
        /// </summary>
        public static ObjectQuery<ChatSupportCannedResponseInfo> GetChatSupportCannedResponses()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns chat support canned response with specified ID.
        /// </summary>
        /// <param name="responseId">Chat support canned response ID.</param>        
        public static ChatSupportCannedResponseInfo GetChatSupportCannedResponseInfo(int responseId)
        {
            return ProviderObject.GetInfoById(responseId);
        }


        /// <summary>
        /// Sets (updates or inserts) specified chat support canned response.
        /// </summary>
        /// <param name="responseObj">Chat support canned response to be set.</param>
        public static void SetChatSupportCannedResponseInfo(ChatSupportCannedResponseInfo responseObj)
        {
            ProviderObject.SetInfo(responseObj);
        }


        /// <summary>
        /// Deletes specified chat support canned response.
        /// </summary>
        /// <param name="responseObj">Chat support canned response to be deleted.</param>
        public static void DeleteChatSupportCannedResponseInfo(ChatSupportCannedResponseInfo responseObj)
        {
            ProviderObject.DeleteInfo(responseObj);
        }


        /// <summary>
        /// Deletes chat support canned response with specified ID.
        /// </summary>
        /// <param name="responseId">Chat support canned response ID.</param>
        public static void DeleteChatSupportCannedResponseInfo(int responseId)
        {
            ChatSupportCannedResponseInfo responseObj = GetChatSupportCannedResponseInfo(responseId);
            DeleteChatSupportCannedResponseInfo(responseObj);
        }

        #endregion


        #region "Public methods - Advanced"

        /// <summary>
        /// Gets canned responses for specified user and site.
        /// 
        /// Gets:
        /// - canned responses assigned to user
        /// - canned responses assigned to site
        /// - global canned responses (all sites)
        /// </summary>
        /// <param name="chatUserID">Owner of canned responses</param>
        /// <param name="siteID">Site of canned responses</param>
        /// <returns>Canned responses</returns>
        public static IEnumerable<ChatSupportCannedResponseInfo> GetCannedResponses(int chatUserID, int siteID)
        {
            // Connect to DB and get all canned responses for current user
            QueryDataParameters parameters = new QueryDataParameters();
            parameters.Add("@ChatUserID", chatUserID);
            parameters.Add("@SiteID", siteID);

            DataSet ds = ConnectionHelper.ExecuteQuery("Chat.SupportCannedResponse.selectbychatuserid", parameters);
            if (!DataHelper.DataSourceIsEmpty(ds))
            {
                return ds.Tables[0].AsEnumerable().Select(row => new ChatSupportCannedResponseInfo(row));
            }

            return Enumerable.Empty<ChatSupportCannedResponseInfo>();
        }

        #endregion
    }
}
