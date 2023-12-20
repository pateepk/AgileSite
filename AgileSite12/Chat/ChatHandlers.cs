using System;
using System.Linq;
using System.Text;

using CMS.Membership;

namespace CMS.Chat
{
    /// <summary>
    /// Provides handlers for chat.
    /// </summary>
    internal static class ChatHandlers
    {

        #region "Public methods"

        /// <summary>
        /// Initializes the chat handlers.
        /// </summary>
        public static void Init()
        {
            SecurityEvents.SignOut.Before += User_SignOut_Before;
        }

        #endregion


        #region "Private methods"

        /// <summary>
        /// Event handler logs out user from chat
        /// </summary>
        private static void User_SignOut_Before(object sender, SignOutEventArgs e)
        {
            ChatOnlineSupportHelper.LeaveSupport();
            ChatOnlineUserHelper.LogOutOfChatCurrentUser();
        }

        #endregion

    }
}
