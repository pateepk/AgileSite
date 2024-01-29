using System;

using CMS.Helpers;

namespace CMS.EmailEngine
{
    /// <summary>
    /// Enumeration of possible SMTP server priorities.
    /// </summary>
    public enum SMTPServerPriorityEnum
    {
        /// <summary>
        /// Low priority.
        /// </summary>
        [EnumOrder(0)]
        Low = -1,


        /// <summary>
        /// Normal priority.
        /// </summary>
        [EnumDefaultValue]
        [EnumOrder(1)]
        Normal = 0,


        /// <summary>
        /// High priority.
        /// </summary>
        [EnumOrder(2)]
        High = 1
    }
}