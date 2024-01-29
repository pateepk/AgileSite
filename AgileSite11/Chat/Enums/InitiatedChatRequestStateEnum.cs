using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CMS.Chat
{
    /// <summary>
    /// State of initiated chat request.
    /// </summary>
    public enum InitiatedChatRequestStateEnum
    {
        /// <summary>
        /// Default state. User will be notified about requests in this state.
        /// </summary>
        New = 1,

        /// <summary>
        /// User has accepted this request and is (or was) chatting with supporter.
        /// </summary>
        Accepted = 2,

        /// <summary>
        /// Request was declined and shall not be shown to user ever again.
        /// </summary>
        Declined = 3,

        /// <summary>
        /// Request is marked for deletion - should be removed from cache and will be deleted from database in the next cleaning.
        /// </summary>
        Deleted = 4,
    }
}
