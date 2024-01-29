using System;

using CMS.Helpers;

namespace CMS.TranslationServices
{
    /// <summary>
    /// Translation priority levels.
    /// </summary>
    public enum TranslationPriorityEnum
    {
        /// <summary>
        /// Low priority.
        /// </summary>
        [EnumStringRepresentation("Low")]
        Low = 0,

        /// <summary>
        /// Normal priority.
        /// </summary>
        [EnumStringRepresentation("Normal")]
        [EnumDefaultValue]
        Normal = 1,

        /// <summary>
        /// High priority.
        /// </summary>
        [EnumStringRepresentation("High")]
        High = 2
    }
}