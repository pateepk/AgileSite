using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace CMS.Chat
{
    /// <summary>
    /// Data about online users.
    /// </summary>
    [DataContract]
    public class OnlineUsersData
    {
        /// <summary>
        /// Last change of online users.
        /// </summary>
        [DataMember]
        public long LastChange { get; set; }


        /// <summary>
        /// Online users.
        /// </summary>
        [DataMember]
        public IEnumerable<OnlineUserData> List { get; set; }
    }
}
