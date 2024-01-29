using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CMS.Chat
{
    /// <summary>
    /// Possible results from JoinRoom operations.
    /// </summary>
    public enum JoinRoomResultEnum
    {
        /// <summary>
        /// User joined to the room in current operation.
        /// </summary>
        Joined,

        /// <summary>
        /// User was already joined.
        /// </summary>
        AlreadyIn,

        /// <summary>
        /// Wrong password.
        /// </summary>
        WrongPassword,

        /// <summary>
        /// Room was not found or is disabled.
        /// </summary>
        RoomDisabled,
    }
}
