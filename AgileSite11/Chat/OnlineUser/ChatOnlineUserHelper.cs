using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

using CMS.Helpers;
using CMS.EventLog;
using CMS.Membership;
using CMS.SiteProvider;

namespace CMS.Chat
{
    /// <summary>
    /// Helper class for chat online users.
    /// </summary>
    public static class ChatOnlineUserHelper
    {
        #region "Private properties"

        /// <summary>
        /// Chat user ID stored in session. Null if no chat user id is stored in session.
        /// </summary>
        private static int? ChatUserIDSession
        {
            get
            {
                return SessionHelper.GetValue("CurrentChatUserID") as int?;
            }
            set
            {
                if (value == null)
                {
                    SessionHelper.Remove("CurrentChatUserID");
                }
                else
                {
                    SessionHelper.SetValue("CurrentChatUserID", value);
                }
            }
        }

        #endregion


        #region "Public methods"

        /// <summary>
        /// Gets currently logged in chat user. It checks for various edge cases like CMSUser logout, change of CMSUser, etc.
        /// 
        /// Returns null if user is not logged in.
        /// </summary>
        /// <param name="isHidden">Is set to true if currently logged in user is hidden (should not be displayed in the list of online users). Is set to false if user is not hidden or user was not found.</param>
        public static ChatUserInfo GetLoggedInChatUser(out bool isHidden)
        {
            isHidden = false;
            ChatUserInfo chatUser;

            if (!ChatUserIDSession.HasValue)
            {
                // If TryLoginFromCookie returns true, ChatUserIDSession will be correctly set
                if (!TryLoginFromCookie())
                {
                    return null;
                }
            }

            // Get online user from cache
            OnlineUserData onlineUser = ChatGlobalData.Instance.Sites.Current.OnlineUsers.GetOnlineUser(ChatUserIDSession.Value);

            // User is not online
            if (onlineUser == null)
            {
                ClearChatUserSession();

                return null;
            }

            chatUser = onlineUser.ChatUser;

            // Check if logged in CMS user is correctly assigned to logged in chat user
            if (!CheckChatUserConsistency(chatUser))
            {
                ClearChatUserSession();

                return null;
            }

            // Set 'isHidden' output value
            isHidden = onlineUser.IsHidden;

            // Everything is fine:
            return chatUser;
        }


        /// <summary>
        /// Gets currently logged in chat user. It checks for various edge cases like CMSUser logout, change of CMSUser, etc.
        /// 
        /// Returns null if user is not logged in.
        /// </summary>
        public static ChatUserInfo GetLoggedInChatUser()
        {
            bool dummy;

            return GetLoggedInChatUser(out dummy);
        }


        /// <summary>
        /// Gets true if chat user is logged in or false if not.
        /// </summary>
        /// <param name="omitHidden">If true, hidden user won't be counted</param>
        public static bool IsChatUserLoggedIn(bool omitHidden)
        {
            bool isHidden;

            ChatUserInfo user = GetLoggedInChatUser(out isHidden);

            if ((user == null) || (omitHidden && isHidden))
            {
                return false;
            }

            return true;
        }


        /// <summary>
        /// Gets true if chat user is logged in or false if not.
        /// 
        /// Hidden user counts as logged in.
        /// </summary>
        public static bool IsChatUserLoggedIn()
        {
            return IsChatUserLoggedIn(false);
        }


        /// <summary>
        /// Logs in chat user. Insets him into session, memory and DB.
        /// </summary>
        /// <param name="chatUser">Chat user to log in</param>
        public static void LogInChatUser(ChatUserInfo chatUser)
        {
            LogInChatUser(chatUser, false);
        }


        /// <summary>
        /// Logs in chat user. Insets him into session, memory and DB.
        /// </summary>
        /// <param name="chatUser">Chat user to log in</param>
        /// <param name="isHidden">If false, this user will be shown in online users on live site. If user was logged in as hidden before and now is logged in as 'not hidden' value will be overriden.</param>
        public static void LogInChatUser(ChatUserInfo chatUser, bool isHidden)
        {
            ChatUserIDSession = chatUser.ChatUserID;

            string loginToken = ChatOnlineUserInfoProvider.Login(SiteContext.CurrentSiteID, chatUser.ChatUserID, isHidden);

            StoreChatTokenInCookie(loginToken);

            ChatGlobalData.Instance.Sites.Current.OnlineUsers.Invalidate();
        }


        /// <summary>
        /// Logs into chat current CMS User.
        /// </summary>
        public static void LogInCurrentCMSUser()
        {
            LogInCurrentCMSUser(false);
        }


        /// <summary>
        /// Logs into chat current CMS User.
        /// </summary>
        public static void LogInCurrentCMSUser(bool isHidden)
        {
            var cmsUser = MembershipContext.AuthenticatedUser;

            if (cmsUser.IsPublic())
            {
                return;
            }
            else
            {
                ChatUserInfo loggedInChatUser = GetLoggedInChatUser();

                if ((loggedInChatUser == null) || loggedInChatUser.IsAnonymous || (cmsUser.UserID != loggedInChatUser.ChatUserUserID))
                {
                    LogOutOfChatCurrentUser();

                    LogInChatUser(ChatUserHelper.GetChatUserFromCMSUser(cmsUser), isHidden);
                }
            }
        }


        /// <summary>
        /// Logs current user out of chat.
        /// </summary>
        public static void LogOutOfChatCurrentUser()
        {
            ChatUserInfo chatUser = GetLoggedInChatUser();

            if (chatUser != null)
            {
                string leaveMessageFormat = ChatMessageHelper.GetSystemMessageText(ChatMessageTypeEnum.LeaveRoom, "{nickname}");

                ChatOnlineUserInfoProvider.Logout(SiteContext.CurrentSiteID, chatUser.ChatUserID, leaveMessageFormat, ChatMessageTypeEnum.LeaveRoom);

                ClearChatUserSession();

                RemoveChatTokenCookie();
            }
        }

        #endregion


        #region "Private methods"

        private static void ClearChatUserSession()
        {
            ChatUserIDSession = null;
        }


        private static void StoreChatTokenInCookie(string loginToken)
        {
            CookieHelper.SetValue(CookieName.ChatLoggedInToken, loginToken, DateTimeHelper.ZERO_TIME);
        }


        private static void RemoveChatTokenCookie()
        {
            CookieHelper.Remove(CookieName.ChatLoggedInToken);
        }


        private static bool TryLoginFromCookie()
        {
            string cookieToken = CookieHelper.GetValue(CookieName.ChatLoggedInToken);

            if (string.IsNullOrEmpty(cookieToken))
            {
                return false;
            }

            ChatOnlineUserInfo onlineUser = ChatOnlineUserInfoProvider.GetOnlineUserByToken(cookieToken, SiteContext.CurrentSiteID);

            // User with this token not found or is not online
            if (onlineUser == null)
            {
                // Remove token from cookies, so it won't query database on the next request
                RemoveChatTokenCookie();

                return false;
            }

            // Set chat user id to session
            ChatUserIDSession = onlineUser.ChatOnlineUserChatUserID;

            return true;
        }


        private static bool CheckChatUserConsistency(ChatUserInfo chatUser)
        {
            var currentUser = MembershipContext.AuthenticatedUser;

            // Nothing to check - return false
            if (chatUser == null)
            {
                return false;
            }

            // CMSUser has changed
            if ((chatUser.ChatUserUserID != currentUser.UserID) && !currentUser.IsPublic())
            {
                return false;
            }
            // CMSUser has been logged out
            else if (!chatUser.IsAnonymous && currentUser.IsPublic())
            {
                return false;
            }

            // Everything is fine
            return true;
        }

        #endregion
    }
}
