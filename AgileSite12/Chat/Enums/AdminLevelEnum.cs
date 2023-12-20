using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CMS.Chat
{
    /// <summary>
    /// Possible admin levels of chat user in room.
    /// </summary>
    public enum AdminLevelEnum
    {
        /// <summary>
        /// Noting (can't join in private rooms. Is the same as Join in public rooms).
        /// </summary>
        [StringValue("chat.adminlevel.none", IsResourceString = true)]
        None = 0,

        /// <summary>
        /// Can join to the private rooms.
        /// </summary>
        [StringValue("chat.adminlevel.join", IsResourceString = true)]
        Join = 1,

        /// <summary>
        /// Is admin in room.
        /// </summary>
        [StringValue("chat.adminlevel.admin", IsResourceString = true)]
        Admin = 2,

        /// <summary>
        /// Is creator of the room. This admin level can not be revoked.
        /// </summary>
        [StringValue("chat.creator", IsResourceString = true)]
        Creator = 3
    }
}
