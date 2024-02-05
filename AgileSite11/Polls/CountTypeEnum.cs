using System;

namespace CMS.Polls
{
    /// <summary>
    /// Count type enumeration.
    /// </summary>
    public enum CountTypeEnum : int
    {
        /// <summary>
        /// No count.
        /// </summary>
        None = 0,

        /// <summary>
        /// Absolute count.
        /// </summary>
        Absolute = 1,

        /// <summary>
        /// Percentage count.
        /// </summary>
        Percentage = 2
    }
}