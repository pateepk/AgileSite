using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace CMS.Chat
{
    /// <summary>
    /// Info about chat room.
    /// </summary>
    [DataContract]
    public class ChatRoomData
    {
        /// <summary>
        /// Chat room ID.
        /// </summary>
        [DataMember]
        public int ChatRoomID { get; set; }


        /// <summary>
        /// Display name of a room.
        /// </summary>
        [DataMember]
        public string DisplayName { get; set; }


        /// <summary>
        /// Is private.
        /// </summary>
        [DataMember]
        public bool IsPrivate { get; set; }


        /// <summary>
        /// Has password.
        /// </summary>
        [DataMember]
        public bool HasPassword { get; set; }


        /// <summary>
        /// Description of the chat room.
        /// </summary>
        [DataMember]
        public string Description { get; set; }


        /// <summary>
        /// Allow anonym.
        /// </summary>
        [DataMember]
        public bool AllowAnonym { get; set; }


        /// <summary>
        /// If user can manage this room (edit, delete).
        /// </summary>
        [DataMember]
        public bool CanManage { get; set; }


        /// <summary>
        /// True if this room should be removed from list on client.
        /// </summary>
        [DataMember]
        public bool IsRemoved { get; set; }
    }
}
