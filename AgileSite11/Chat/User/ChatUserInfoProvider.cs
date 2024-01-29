using System;
using System.Data;
using System.Linq;
using System.Collections.Generic;

using CMS.DataEngine;
using CMS.Helpers;

namespace CMS.Chat
{
    /// <summary>
    /// Class providing ChatUserInfo management.
    /// </summary>
    public class ChatUserInfoProvider : AbstractInfoProvider<ChatUserInfo, ChatUserInfoProvider>
    {
        #region "Public methods - Basic"

        /// <summary>
        /// Returns the query for all chat users.
        /// </summary>
        public static ObjectQuery<ChatUserInfo> GetChatUsers()
        {
            return ProviderObject.GetObjectQuery();
        }

        
        /// <summary>
        /// Returns chat user with specified ID.
        /// </summary>
        /// <param name="userId">Chat user ID.</param>        
        public static ChatUserInfo GetChatUserInfo(int userId)
        {
            return ProviderObject.GetInfoById(userId);
        }


        /// <summary>
        /// Returns chat user with specified name.
        /// </summary>
        /// <param name="userName">Chat user name.</param>                
        public static ChatUserInfo GetChatUserInfo(string userName)
        {
            return ProviderObject.GetInfoByCodeName(userName);
        }


        /// <summary>
        /// Sets (updates or inserts) specified chat user.
        /// </summary>
        /// <param name="userObj">Chat user to be set.</param>
        public static void SetChatUserInfo(ChatUserInfo userObj)
        {
            ProviderObject.SetInfo(userObj);
        }


        /// <summary>
        /// Deletes specified chat user.
        /// </summary>
        /// <param name="userObj">Chat user to be deleted.</param>
        public static void DeleteChatUserInfo(ChatUserInfo userObj)
        {
            ProviderObject.DeleteInfo(userObj);
        }


        /// <summary>
        /// Deletes chat user with specified ID.
        /// </summary>
        /// <param name="userId">Chat user ID.</param>
        public static void DeleteChatUserInfo(int userId)
        {
            ChatUserInfo userObj = GetChatUserInfo(userId);
            DeleteChatUserInfo(userObj);
        }

        #endregion


        #region "Public methods - Advanced"
        
        /// <summary>
        /// Gets chat user by CMS User ID. Returns NULL if user was not found.
        /// </summary>
        /// <param name="userID">CMS user ID</param>
        /// <returns>ChatUserInfo</returns>
        public static ChatUserInfo GetChatUserByUserID(int userID)
        {
            return GetChatUsers().WhereEquals("ChatUserUserID", userID).FirstOrDefault();
        }


        /// <summary>
        /// Checks number of nickname usages. This method compares incasesensitive.
        /// </summary>
        /// <param name="nickname">Nickname to find</param>
        /// <param name="includeAnonyms">If set true, count of non-anonymous and anonymous users will be returned. If false, only non-anonymous user nicknames will be counted</param>
        /// <param name="excludeChatUserID">User with this ID won't be counted</param>
        /// <returns>Count</returns>
        public static int GetCountOfChatUsers(string nickname, bool includeAnonyms, int? excludeChatUserID)
        {
            // Prepare the parameters
            QueryDataParameters parameters = new QueryDataParameters();
            parameters.Add("@Nickname", nickname);
            parameters.Add("@IncludeAnonyms", includeAnonyms);
            parameters.Add("@ExcludeChatUserID", excludeChatUserID);

            // Get the data
            DataSet ds = ConnectionHelper.ExecuteQuery("Chat.User.selectusersnicknamecount", parameters);
            if (!DataHelper.DataSourceIsEmpty(ds))
            {
                return ds.Tables[0].Rows[0].Field<int>(0);
            }

            return 0;
        }


        /// <summary>
        /// Returns anonymous chat users with the specified nickname.
        /// </summary>
        /// <param name="nickname">Nickname of the user</param>
        /// <rereturns>Anonymous users with specified nickname.</rereturns>
        public static IEnumerable<ChatUserInfo> GetAnonymousChatUsersByNickname(string nickname)
        {
            return GetChatUsers().WhereNull("ChatUserUserID").WhereEquals("ChatUserNickname", nickname);
        }

        #endregion


        #region "Internal methods - Basic"

        /// <summary>
        /// Inserts or Updates the object to the database.
        /// </summary>
        /// <param name="info">Object to insert / update</param>
        protected override void SetInfo(ChatUserInfo info)
        {
            // Throws ChatBadWordsException if there is any bad word in ChatUserNickname.
            ChatProtectionHelper.CheckNameForBadWords(info.ChatUserNickname);

            base.SetInfo(info);
        }

        #endregion
    }
}
