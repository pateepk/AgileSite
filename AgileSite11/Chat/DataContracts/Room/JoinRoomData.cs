using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace CMS.Chat
{
    /// <summary>
    /// Class returned by JoinRoom operation.
    /// </summary>
    [DataContract]
    public class JoinRoomData
    {
        /// <summary>
        /// Chat room ID of joined room.
        /// </summary>
        [DataMember]
        public int ChatRoomID { get; set; }


        /// <summary>
        /// Display name of joined room.
        /// </summary>
        [DataMember]
        public string DisplayName { get; set; }


        /// <summary>
        /// True if joined room is private.
        /// </summary>
        [DataMember]
        public bool IsPrivate { get; set; }


        /// <summary>
        /// True if joined room has password.
        /// </summary>
        [DataMember]
        public bool HasPassword { get; set; }


        /// <summary>
        /// True if joined room allows anonym users.
        /// </summary>
        [DataMember]
        public bool AllowAnonym { get; set; }


        /// <summary>
        /// True if current user is admin in joined room (can invite users, kick them, etc.). Otherwise false.
        /// </summary>
        [DataMember]
        public bool IsCurrentUserAdmin { get; set; }


        /// <summary>
        /// True if this room is used to private conversation.
        /// </summary>
        [DataMember]
        public bool IsOneToOne { get; set; }


        /// <summary>
        /// True if this room is used to privide support.
        /// </summary>
        [DataMember]
        public bool IsSupport { get; set; }
    }
}
