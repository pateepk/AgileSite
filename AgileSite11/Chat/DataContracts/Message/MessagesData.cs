using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace CMS.Chat
{
    /// <summary>
    /// Data about messages in a room.
    /// </summary>
    [DataContract]
    public class MessagesData
    {
        /// <summary>
        /// Last change of messages.
        /// </summary>
        [DataMember]
        public long LastChange { get; set; }


        /// <summary>
        /// Messages.
        /// </summary>
        [DataMember]
        public IEnumerable<MessageData> List { get; set; }
    }
}
