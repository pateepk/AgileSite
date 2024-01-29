using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CMS.Chat
{
    /// <summary>
    /// Enum of chat permissions.
    /// </summary>
    public enum ChatPermissionEnum
    {
        /// <summary>
        /// Enter support permission.
        /// </summary>
        [StringValue("EnterSupport")]
        EnterSupport,

        /// <summary>
        /// User with this permission is something like global admin for chat (can create rooms, is admin in every room, etc.).
        /// </summary>
        [StringValue("ManageRooms")]
        ManageRooms,

        /// <summary>
        /// User with this permission will be able to create rooms from the live site.
        /// </summary>
        [StringValue("CreateRoomsFromLiveSite")]
        CreateRoomsFromLiveSite,
    }
}
