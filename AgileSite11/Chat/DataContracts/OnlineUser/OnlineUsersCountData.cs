using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace CMS.Chat
{
    /// <summary>
    /// Info about counts of users in room.
    /// </summary>
    [DataContract]
    public class OnlineUsersCountData
    {
        /// <summary>
        /// RoomID
        /// </summary>
        [DataMember]
        public int RoomID { get; set; }

        
        /// <summary>
        /// Number of online users.
        /// </summary>
        [DataMember]
        public int UsersCount { get; set; }


        /// <summary>
        /// Last change of online users count.
        /// </summary>
        public DateTime LastChange { get; set; }
    }
}
