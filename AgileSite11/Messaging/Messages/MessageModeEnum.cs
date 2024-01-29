using System;

namespace CMS.Messaging
{
    /// <summary>
    /// Message mode enumeration.
    /// </summary>
    public enum MessageModeEnum
    {
        /// <summary>
        /// Inbox.
        /// </summary>
        Inbox = 0,

        /// <summary>
        /// Outbox.
        /// </summary>
        Outbox = 1
    }
}