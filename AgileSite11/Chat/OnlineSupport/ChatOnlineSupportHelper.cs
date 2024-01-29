using System;
using System.Data;
using System.Collections.Generic;

using CMS.Helpers;
using CMS.SiteProvider;

namespace CMS.Chat
{
    /// <summary>
    /// Helper class for online support.
    /// </summary>
    public static class ChatOnlineSupportHelper
    {
        #region "Private properties"

        /// <summary>
        /// Shortcut to online supporters cache for the current site.
        /// </summary>
        private static OnlineSupport OnlineSupport
        {
            get
            {
                return ChatGlobalData.Instance.Sites.Current.OnlineSupport;
            }
        }


        /// <summary>
        /// ID of chat user who is logged in support "at this computer". Value is taken from and saved into session.
        /// </summary>
        private static int? SupportChatUserIDSession
        {
            get
            {
                return SessionHelper.GetValue("CurrentSupportChatUserID") as int?;
            }
            set
            {
                if (value == null)
                {
                    SessionHelper.Remove("CurrentSupportChatUserID");
                }
                else
                {
                    SessionHelper.SetValue("CurrentSupportChatUserID", value);
                }
            }
        }

        #endregion


        #region "Public properties"

        /// <summary>
        /// Gets currently logged in supporter or null if supporter is not online.
        /// </summary>
        public static ChatUserInfo SupportChatUser
        {
            get
            {
                // Get chat user id from session
                int? chatUserID = SupportChatUserIDSession;

                if (chatUserID == null)
                {
                    // If TryLoginFromCookie returns true, SupportChatUserIDSession will be correctly set
                    if (!TryLoginFromCookie())
                    {
                        return null;
                    }

                    chatUserID = SupportChatUserIDSession;
                }

                ChatUserInfo loggedInChatUser = ChatOnlineUserHelper.GetLoggedInChatUser();

                if ((loggedInChatUser == null) || (loggedInChatUser.ChatUserID != chatUserID.Value) || !OnlineSupport.OnlineSupporters.ContainsKey(chatUserID.Value))
                {
                    CleanSupportSession();

                    return null;
                }

                return loggedInChatUser;
            }
        }


        /// <summary>
        /// True if supporter is online at this computer.
        /// </summary>
        public static bool IsSupportOnline
        {
            get
            {
                return SupportChatUser != null;
            }
        }

        #endregion


        #region "Public methods"

        /// <summary>
        /// Logs currently logged in CMS User into support chat.
        /// </summary>
        public static void EnterSupport()
        {
            // Get chat user for currently logged in CMS User
            ChatUserInfo chatUser = ChatUserHelper.GetChatUserFromCMSUser();

            // Store user ID in support session
            SupportChatUserIDSession = chatUser.ChatUserID;

            // Login chat user into classic chat as Hidden
            ChatOnlineUserHelper.LogInChatUser(chatUser, true);

            // Store information about user being online on support to database
            string loginToken = ChatOnlineSupportInfoProvider.EnterSupport(SiteContext.CurrentSiteID, chatUser.ChatUserID);

            StoreSupportChatTokenInCookie(loginToken);

            OnlineSupport.InvalidateOnlineSupportCache();
        }


        /// <summary>
        /// Loggs current support user out of support chat.
        /// </summary>
        public static void LeaveSupport()
        {
            ChatUserInfo chatUser = SupportChatUser;

            if (chatUser != null)
            {
                CleanSupportSession();

                ChatOnlineSupportInfoProvider.LeaveSupport(SiteContext.CurrentSiteID, chatUser.ChatUserID);

                RemoveSupportChatTokenCookie();

                OnlineSupport.InvalidateOnlineSupportCache();
            }
        }

        #endregion


        #region "Private methods"

        private static void CleanSupportSession()
        {
            SupportChatUserIDSession = null;
        }


        private static bool TryLoginFromCookie()
        {
            string cookieToken = CookieHelper.GetValue(CookieName.ChatSupportLoggedInToken);

            // There is no token in cookies
            if (string.IsNullOrEmpty(cookieToken))
            {
                return false;
            }

            ChatUserInfo onlineChatUser = ChatOnlineUserHelper.GetLoggedInChatUser();

            // If user is not online on classic chat, he can't be online on support
            if (onlineChatUser == null)
            {
                return false;
            }

            ChatOnlineSupportInfo onlineSupport = ChatOnlineSupportInfoProvider.GetOnlineSupportByToken(cookieToken, SiteContext.CurrentSiteID);

            // Support with this token not found
            if (onlineSupport == null)
            {
                // Remove token from cookies if user was not found, so DB won't be queried on the next request
                RemoveSupportChatTokenCookie();

                return false;
            }

            // Supporter is different with currently logged in chat user
            if (onlineSupport.ChatOnlineSupportChatUserID != onlineChatUser.ChatUserID)
            {
                return false;
            }

            // Store user ID in support session
            SupportChatUserIDSession = onlineSupport.ChatOnlineSupportChatUserID;

            OnlineSupport.InvalidateOnlineSupportCache();

            return true;
        }


        private static void StoreSupportChatTokenInCookie(string loginToken)
        {
            CookieHelper.SetValue(CookieName.ChatSupportLoggedInToken, loginToken, DateTimeHelper.ZERO_TIME);
        }


        private static void RemoveSupportChatTokenCookie()
        {
            CookieHelper.Remove(CookieName.ChatSupportLoggedInToken);
        }


        #endregion
    }
}
