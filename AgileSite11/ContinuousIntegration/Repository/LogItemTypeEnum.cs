using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CMS.ContinuousIntegration
{
    /// <summary>
    /// Log item type used for <see cref="LogItem"/> object.
    /// </summary>
    public enum LogItemTypeEnum
    {
        /// <summary>
        /// Informational message type.
        /// </summary>
        Info = 0,

        /// <summary>
        /// Error message type.
        /// </summary>
        Error = 1,

        /// <summary>
        /// Warning message type.
        /// </summary>
        Warning = 2
    }
}
