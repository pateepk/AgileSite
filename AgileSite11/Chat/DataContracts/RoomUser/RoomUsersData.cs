using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace CMS.Chat
{
    /// <summary>
    /// Info about online users in a room.
    /// </summary>
    [DataContract]
    public class RoomOnlineUsersData
    {
        /// <summary>
        /// Last change of room online users.
        /// </summary>
        [DataMember]
        public long LastChange { get; set; }


        /// <summary>
        /// Online users in a room.
        /// </summary>
        [DataMember]
        public IEnumerable<RoomOnlineUserData> List { get; set; }
    }
}