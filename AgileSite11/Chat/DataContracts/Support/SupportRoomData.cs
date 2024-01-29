using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace CMS.Chat
{
    /// <summary>
    /// Data about support room.
    /// </summary>
    [DataContract]
    public class SupportRoomData
    {
        /// <summary>
        /// Room id.
        /// </summary>
        [DataMember]
        public int ChatRoomID { get; set; }


        /// <summary>
        /// Room display name.
        /// </summary>
        [DataMember]
        public string DisplayName { get; set; }


        /// <summary>
        /// Count of new messages.
        /// </summary>
        [DataMember]
        public int UnreadMessagesCount { get; set; }


        /// <summary>
        /// If true, this room was taken by somebody else and should be removed from the list on client.
        /// 
        /// There won't be any further updates about this room.
        /// </summary>
        [DataMember]
        public bool IsRemoved { get; set; }


        /// <summary>
        /// If true, this room is taken by current user, and can be kept in the list on client.
        /// 
        /// New messages in this room will be send to client.
        /// </summary>
        [DataMember]
        public bool IsTaken { get; set; }
    }
}
