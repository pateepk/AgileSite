using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace CMS.Chat
{
    /// <summary>
    /// Data about chat rooms.
    /// </summary>
    [DataContract]
    public class ChatRoomsData
    {
        /// <summary>
        /// Last change.
        /// </summary>
        [DataMember]
        public long LastChange { get; set; }


        /// <summary>
        /// Rooms.
        /// </summary>
        [DataMember]
        public IEnumerable<ChatRoomData> List { get; set; }
    }
}
