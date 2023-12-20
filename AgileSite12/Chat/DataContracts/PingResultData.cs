using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace CMS.Chat
{
    /// <summary>
    /// Response of Ping.
    /// </summary>
    [DataContract]
    public class PingResultData
    {
        /// <summary>
        /// ChatUserState of current user.
        /// </summary>
        [DataMember]
        public ChatUserStateData CurrentChatUserState { get; set; }
        

        /// <summary>
        /// Notifications.
        /// </summary>
        [DataMember]
        public ChatNotificationsData Notifications { get; set; }


        /// <summary>
        /// Online users.
        /// </summary>
        [DataMember]
        public OnlineUsersData OnlineUsers { get; set; }


        /// <summary>
        /// Rooms.
        /// </summary>
        [DataMember]
        public ChatRoomsData Rooms { get; set; }


        /// <summary>
        /// Counts of users in rooms.
        /// </summary>
        [DataMember]
        public UsersInRoomsCountsData UsersInRooms { get; set; }
    }
}
