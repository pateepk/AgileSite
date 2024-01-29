using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace CMS.Chat
{
    /// <summary>
    /// Data about support rooms.
    /// </summary>
    [DataContract]
    public class SupportRoomsData
    {
        /// <summary>
        /// Datetime of the newest message added to support rooms.
        /// 
        /// Client should send this datetime in next SupportPing().
        /// </summary>
        [DataMember]
        public long LastChange { get; set; }


        /// <summary>
        /// Rooms with new messages.
        /// </summary>
        [DataMember]
        public IEnumerable<SupportRoomData> List { get; set; }
    }
}
