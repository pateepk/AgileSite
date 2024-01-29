using System;
using System.Collections;
using System.Text;

using CMS.Base;
using CMS.Helpers;
using CMS.LicenseProvider;
using CMS.SiteProvider;
using CMS.Membership;
using CMS.Protection;
using CMS.DataEngine;


namespace CMS.Chat
{
    /// <summary>
    /// Provides methods for chat protection.
    /// </summary>
    public class ChatProtectionHelper : AbstractHelper<ChatProtectionHelper>
    {
        /// <summary>
        /// Indicates whether support chat panel is enabled based on chat settings and user permissions.
        /// </summary>
        public static bool IsSupportChatPanelEnabled()
        {
            return HelperObject.IsSupportChatPanelEnabledInternal();
        }


        /// <summary>
        /// Throws <see cref="ChatBadWordsException"/> if there are some bad words in name.
        /// </summary>
        /// <param name="name">Name to be checked for bad words</param>
        /// <exception cref="ChatBadWordsException">Throws ChatBadWordsException when bad word is found.</exception>
        public static void CheckNameForBadWords(string name)
        {
            HelperObject.CheckNameForBadWordsInternal(name);
        }


        /// <summary>
        /// Checks if current user (CMSUser) has permission to perform specified chat-related action.
        /// </summary>
        /// <param name="permission">Permission to check</param>
        /// <returns>True if user has permission, otherwise false</returns>
        public static bool HasCurrentUserPermission(ChatPermissionEnum permission)
        {
            return HelperObject.HasCurrentUserPermissionInternal(permission);
        }


        /// <summary>
        /// Checks if specified user (CMSUser) has permission to perform specified chat-related action.
        /// </summary>
        /// <param name="user">UserInfo to check for permission</param>
        /// <param name="permission">Permission to check</param>
        /// <returns>True if user has permission, otherwise false</returns>
        public static bool HasUserPermission(UserInfo user, ChatPermissionEnum permission)
        {
            return HelperObject.HasUserPermissionInternal(user, permission);
        }


        /// <summary>
        /// Checks if specified operation happened too early after the previous call. If it happened too early (earlier than timespan specified in settings), 
        /// it is considered as flooding and operation should be stopped.
        /// </summary>
        /// <param name="operation">Type of operation</param>
        /// <returns>True if everything is ok. False if operation should be stopped.</returns>
        public static bool CheckOperationForFlooding(FloodOperationEnum operation)
        {
            return HelperObject.CheckOperationForFloodingInternal(operation);
        }

        
        /// <summary>
        /// Indicates whether support chat panel is enabled based on chat settings and user permissions.
        /// </summary>
        protected virtual bool IsSupportChatPanelEnabledInternal()
        {
            // Check the license
            string currentDomain = RequestContext.CurrentDomain;
            if (!String.IsNullOrEmpty(currentDomain))
            {
                // Chat needs BannerIP to work
                if (!LicenseHelper.CheckFeature(currentDomain, FeatureEnum.Chat))
                {
                    return false;
                }
            }

            // Display support chat UI only when chat module is present and user has support chat permission
            if (ChatSettingsProvider.IsSupportEnabledSetting && MembershipContext.AuthenticatedUser.IsAuthorizedPerResource("CMS.Chat", "EnterSupport"))
            {
                return true;
            }

            return false;
        }


        /// <summary>
        /// Throws <see cref="ChatBadWordsException"/> if there are some bad words in name.
        /// </summary>
        /// <param name="name">Name to be checked for bad words</param>
        /// <exception cref="ChatBadWordsException">Throws ChatBadWordsException when bad word is found.</exception>
        protected virtual void CheckNameForBadWordsInternal(string name)
        {
            if ((name != null) && BadWordsHelper.PerformBadWordsCheck(SiteContext.CurrentSiteName) && !BadWordInfoProvider.CanUseBadWords(MembershipContext.AuthenticatedUser, SiteContext.CurrentSiteName))
            {
                // Perform bad words check
                Hashtable foundWords = new Hashtable();
                BadWordActionEnum action = BadWordInfoProvider.CheckAllBadWords(null, SiteContext.CurrentSiteName, ref name, foundWords);

                if (action != BadWordActionEnum.None)
                {
                    bool found = false;
                    string words = string.Empty;
                    StringBuilder sb = null;

                    // If there are some found bad words
                    if (foundWords[action] != null)
                    {
                        // Get word hashtable depending on action
                        Hashtable actionHash = (Hashtable)foundWords[action];

                        // If this hashtable exists and is not empty
                        if ((actionHash != null) && (actionHash.Count > 0))
                        {
                            sb = new StringBuilder();

                            // For each expression in hashtable
                            foreach (string expression in actionHash.Keys)
                            {
                                // Add each occurrence to result string
                                ArrayList occurrences = ((ArrayList)actionHash[expression]);
                                foreach (string occurrence in occurrences)
                                {
                                    sb.Append(occurrence);
                                    sb.Append(", ");
                                    found = true;
                                }
                            }
                        }
                        if (found)
                        {
                            // Trim last comma
                            words = sb.ToString().Trim().TrimEnd(new char[] { ',' });
                        }
                    }

                    throw new ChatBadWordsException(ResHelper.GetAPIString("general.badwordsfound", "The text contains some expressions that are not allowed. Please remove these expressions: ") + " " + words);
                }
            }
        }


        /// <summary>
        /// Checks if current user (CMSUser) has permission to perform specified chat-related action.
        /// </summary>
        /// <param name="permission">Permission to check</param>
        /// <returns>True if user has permission, otherwise false</returns>
        protected virtual bool HasCurrentUserPermissionInternal(ChatPermissionEnum permission)
        {
            return HasUserPermission(MembershipContext.AuthenticatedUser, permission);
        }


        /// <summary>
        /// Checks if specified user (CMSUser) has permission to perform specified chat-related action.
        /// </summary>
        /// <param name="user">UserInfo to check for permission</param>
        /// <param name="permission">Permission to check</param>
        /// <returns>True if user has permission, otherwise false</returns>
        protected virtual bool HasUserPermissionInternal(UserInfo user, ChatPermissionEnum permission)
        {
            return UserInfoProvider.IsAuthorizedPerResource("CMS.Chat", permission.ToStringValue(), SiteContext.CurrentSiteName, user);
        }


        /// <summary>
        /// Checks if specified operation happened too early after the previous call. If it happened too early (earlier than timespan specified in settings), 
        /// it is considered as flooding and operation should be stopped.
        /// </summary>
        /// <param name="operation">Type of operation</param>
        /// <returns>True if everything is ok. False if operation should be stopped.</returns>
        protected virtual bool CheckOperationForFloodingInternal(FloodOperationEnum operation)
        {
            if (!ChatSettingsProvider.IsFloodingProtectionEnabledSetting)
            {
                return true;
            }

            ChatUserInfo chatUser = ChatOnlineUserHelper.GetLoggedInChatUser();

            if (chatUser == null)
            {
                return true;
            }

            return ChatGlobalData.Instance.FloodProtector.CheckOperation(chatUser.ChatUserID, operation);
        }
    }
}
