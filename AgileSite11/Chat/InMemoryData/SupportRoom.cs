using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CMS.Chat
{
    /// <summary>
    /// Class storing information about room which has some pending messages needed support. This class is intended to be retrieved from DB and stored in chache.
    /// </summary>
    public class SupportRoom : IChatIncrementalCacheable
    {
        /// <summary>
        /// Room id.
        /// </summary>
        public int ChatRoomID { get; set; }


        /// <summary>
        /// Room display name.
        /// </summary>
        public string DisplayName { get; set; }


        /// <summary>
        /// Count of new messages.
        /// </summary>
        public int UnreadMessagesCount { get; set; }


        /// <summary>
        /// Id of chat user who took this room (so this user is now providing support in this room). If null, room is not taken and should be displayed to everybody.
        /// </summary>
        public int? TakenByChatUserID { get; set; }


        /// <summary>
        /// Time of last change of taken state (taken -> released, taken -> resolved, released -> taken).
        /// </summary>
        public DateTime? TakenStateLastChange { get; set; }

        
        /// <summary>
        /// Last change of this record (it is either TakenStateLastChange or time of last message added - the greater of both).
        /// </summary>
        public DateTime ChangeTime { get; set; }
    }
}
