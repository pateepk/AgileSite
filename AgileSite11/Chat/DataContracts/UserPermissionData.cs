using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace CMS.Chat
{
    /// <summary>
    /// Chat user permissions data.
    /// </summary>
    [DataContract]
    public class ChatUserPermissionData
    {
        /// <summary>
        /// Permission to manage rooms (create new room, kick users, etc).
        /// </summary>
        [DataMember]
        public bool ManageRooms { get; set; }


        /// <summary>
        /// Permission to create rooms from live site.
        /// </summary>
        [DataMember(Name="CreateRooms")]
        public bool CreateRoomsFromLiveSite { get; set; }
    }
}
