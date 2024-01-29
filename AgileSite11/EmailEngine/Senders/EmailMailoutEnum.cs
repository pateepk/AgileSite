using System;

namespace CMS.EmailEngine
{
    /// <summary>
    /// Defines which e-mails should be sent when a mailout operation runs.
    /// </summary>
    [Flags]
    public enum EmailMailoutEnum : int
    {
        /// <summary>
        /// No messages should be sent.
        /// </summary>
        None = 0,


        /// <summary>
        /// Only newly created e-mail messages will be sent.
        /// </summary>
        New = 1,


        /// <summary>
        /// Only failed e-mail messages will be sent.
        /// </summary>
        Failed = 2,


        /// <summary>
        /// All messages will be sent.
        /// </summary>
        All = 3,


        /// <summary>
        /// Only messages from application queue will be sent.
        /// </summary>
        OnlyAppQueue = 4,


        /// <summary>
        /// Only archived e-mail messages will be sent.
        /// </summary>
        Archived = 8
    }
}