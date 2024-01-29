using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace CMS.Chat
{
    /// <summary>
    /// State of a room (response to PingRoom).
    /// </summary>
    [DataContract]
    public class PingRoomResponseData
    {
        /// <summary>
        /// Room id.
        /// </summary>
        [DataMember]
        public int RoomID { get; set; }


        /// <summary>
        /// True if current user is admin
        /// </summary>
        [DataMember]
        public bool IsCurrentUserAdmin { get; set; }


        /// <summary>
        /// New messages
        /// </summary>
        [DataMember]
        public MessagesData Messages { get; set; }


        /// <summary>
        /// Changed online users
        /// </summary>
        [DataMember]
        public RoomOnlineUsersData Users { get; set; }
    }
}
