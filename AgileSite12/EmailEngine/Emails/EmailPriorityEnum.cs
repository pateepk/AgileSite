using System;

namespace CMS.EmailEngine
{
    /// <summary>
    /// E-mail priority levels used by EmailMessage object.
    /// </summary>
    public enum EmailPriorityEnum
    {
        /// <summary>
        /// Low priority.
        /// </summary>
        Low = 0,

        /// <summary>
        /// Normal priority.
        /// </summary>
        Normal = 1,

        /// <summary>
        /// High priority.
        /// </summary>
        High = 2
    }
}