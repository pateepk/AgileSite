using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace CMS.Chat
{
    /// <summary>
    /// Counts of users in all accesible or changed rooms.
    /// </summary>
    [DataContract]
    public class UsersInRoomsCountsData
    {
        /// <summary>
        /// Last change of users counts.
        /// </summary>
        [DataMember]
        public long LastChange { get; set; }


        /// <summary>
        /// Counts
        /// </summary>
        [DataMember]
        public IEnumerable<OnlineUsersCountData> List { get; set; }
    }
}
