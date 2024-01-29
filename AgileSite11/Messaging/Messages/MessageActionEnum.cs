using System;

namespace CMS.Messaging
{
    /// <summary>
    /// Enumeration for send message mode.
    /// </summary>
    public enum MessageActionEnum
    {
        /// <summary>
        /// None.
        /// </summary>
        None = 0,

        /// <summary>
        /// New.
        /// </summary>
        New = 1,

        /// <summary>
        /// Reply.
        /// </summary>
        Reply = 2,

        /// <summary>
        /// Forward.
        /// </summary>
        Forward = 3
    }
}