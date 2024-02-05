using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CMS.Scheduler;
using CMS.EventLog;
using CMS.Base;
using CMS.Helpers;
using CMS.SiteProvider;

namespace CMS.Chat
{
    /// <summary>
    /// Scheduled task which cleans inactive chat users and supporters.
    /// </summary>
    public class ChatOnlineUsersCleaner : ITask
    {
        /// <summary>
        /// Cleans inactive users and support engineer from the chat. Number of seconds needed to be inactive is taken from settings.
        /// 
        /// Inactive user is the one who is not pinging.
        /// </summary>
        /// <param name="task">Task</param>
        /// <returns>Number of cleaned records</returns>
        public string Execute(TaskInfo task)
        {
            int userTimeout = ChatSettingsProvider.UserLogoutTimeoutSetting;
            int supportTimeout = ChatSettingsProvider.SupportLogoutTimeoutSetting;
            int takenRoomsTimeout = ChatSettingsProvider.SupportRoomReleaseTimeoutSetting;

            string leaveMessageFormat = ChatMessageHelper.GetSystemMessageText(ChatMessageTypeEnum.LeaveRoom, "{nickname}");


            ChatOnlineUserInfoProvider.CleanOnlineUsers(userTimeout, leaveMessageFormat, ChatMessageTypeEnum.LeaveRoom);
            ChatRoomUserInfoProvider.CleanOnlineUsersInRooms(userTimeout, leaveMessageFormat, ChatMessageTypeEnum.LeaveRoom);

            ChatOnlineSupportInfoProvider.CleanOnlineSupport(supportTimeout);
            ChatSupportTakenRoomInfoProvider.CleanSupportTakenRooms(takenRoomsTimeout);

            return "";
        }
    }
}
