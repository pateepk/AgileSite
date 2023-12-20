using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace CMS.Chat
{
    /// <summary>
    /// Info about one notification.
    /// </summary>
    [DataContract]
    public class ChatNotificationData
    {
        /// <summary>
        /// ID
        /// </summary>
        [DataMember]
        public int NotificationID { get; set; }


        /// <summary>
        /// Type.
        /// </summary>
        [DataMember]
        public ChatNotificationTypeEnum NotificationType { get; set; }


        /// <summary>
        /// Created date time.
        /// </summary>
        [DataMember]
        public DateTime NotificationDateTime { get; set; }


        /// <summary>
        /// Issuer of this notification (inviter for example).
        /// </summary>
        [DataMember]
        public string SenderNickname { get; set; }


        /// <summary>
        /// ID of a room (optional).
        /// </summary>
        [DataMember]
        public int? RoomID { get; set; }


        /// <summary>
        /// Name of a room.
        /// </summary>
        [DataMember]
        public string RoomName { get; set; }


        /// <summary>
        /// Read state of this notification
        /// </summary>
        [DataMember]
        public bool IsRead { get; set; }


        /// <summary>
        /// Indicates type of invitation - conversation is true, private room is false
        /// </summary>
        [DataMember]
        public bool? IsOneToOne { get; set; }


        /// <summary>
        /// When was this notification read
        /// </summary>
        public DateTime? ReadDateTime { get; set; }
    }
}
