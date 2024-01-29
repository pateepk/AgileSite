using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace CMS.Chat
{
    /// <summary>
    /// Chat notifications data.
    /// </summary>
    [DataContract]
    public class ChatNotificationsData
    {
        /// <summary>
        /// Last change of notifications
        /// </summary>
        [DataMember]
        public long LastChange { get; set; }


        /// <summary>
        /// Notifications.
        /// </summary>
        [DataMember]
        public IEnumerable<ChatNotificationData> List { get; set; }
    }
}
