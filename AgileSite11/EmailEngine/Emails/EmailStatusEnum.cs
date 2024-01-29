using System;

namespace CMS.EmailEngine
{
    /// <summary>
    /// Statuses used by EmailMessage object.
    /// </summary>
    public enum EmailStatusEnum
    {
        /// <summary>
        /// E-mail is being created.
        /// </summary>
        Created = 0,

        /// <summary>
        /// New or failed e-mail waiting for sending.
        /// </summary>
        Waiting = 1,

        /// <summary>
        /// E-mail is being sent.
        /// </summary>
        Sending = 2,

        /// <summary>
        /// Archived e-mail.
        /// </summary>
        Archived = 3
    }
}