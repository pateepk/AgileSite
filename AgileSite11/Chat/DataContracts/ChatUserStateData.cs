using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace CMS.Chat
{
    /// <summary>
    /// State of chat user.
    /// </summary>
    [DataContract]
    public class ChatUserStateData
    {
        /// <summary>
        /// Is logged in.
        /// </summary>
        [DataMember]
        public bool IsLoggedIn { get; set; }


        /// <summary>
        /// Chat user ID.
        /// </summary>
        [DataMember]
        public int ChatUserID { get; set; }


        /// <summary>
        /// Nickname.
        /// </summary>
        [DataMember]
        public string Nickname { get; set; }


        /// <summary>
        /// Is anonymous - true if this chat user is assigned to CMS User (true if ChatUserUserID is null).
        /// </summary>
        [DataMember]
        public bool IsAnonymous { get; set; }
    }
}
