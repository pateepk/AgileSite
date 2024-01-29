using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace CMS.Chat
{
    /// <summary>
    /// Response of support Ping.
    /// </summary>
    [DataContract]
    public class SupportPingResponseData
    {
        /// <summary>
        /// ID of current chat user logged in to support. Null if support is offline.
        /// </summary>
        [DataMember]
        public int? OnlineSupportChatUserID { get; set; }


        /// <summary>
        /// Changed rooms - is null if nothing has changed.
        /// </summary>
        [DataMember]
        public SupportRoomsData Rooms { get; set; }
    }
}
