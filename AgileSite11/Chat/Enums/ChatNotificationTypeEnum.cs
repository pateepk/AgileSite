using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CMS.Chat
{
    /// <summary>
    /// Types of chat notifications.
    /// </summary>
    public enum ChatNotificationTypeEnum
    {
        /// <summary>
        /// Invitation
        /// </summary>
        Invitation = 0,

        /// <summary>
        /// Decline.
        /// </summary>
        InvitationDeclined = 1,

        /// <summary>
        /// Accept.
        /// </summary>
        InvitationAccepted = 2,

        /// <summary>
        /// Nickname was changed by system (when CMS user choose nickname, all anonyms with this nicknames are renamed).
        /// </summary>
        NicknameAutomaticallyChanged = 3,

        /// <summary>
        /// Receiver of this message was kicked by sender from room with RoomID of this notification.
        /// </summary>
        Kicked = 4,

        /// <summary>
        /// Receiver of this message was kicked by sender from room with RoomID of this notification. This kick is permanent.
        /// </summary>
        KickedPermanently = 5,

        /// <summary>
        /// Receiver of this message was added to the list of room admins
        /// </summary>
        AdminAdded = 6,

        /// <summary>
        /// Receiver of this message was deleted from the list of room admins
        /// </summary>
        AdminDeleted = 7
    }
}
