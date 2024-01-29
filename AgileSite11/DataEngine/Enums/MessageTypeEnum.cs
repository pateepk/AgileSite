using System;
using System.Linq;
using System.Text;

namespace CMS.DataEngine
{
    /// <summary>
    /// Message type returned to log panel.
    /// </summary>
    public enum MessageTypeEnum
    {
        /// <summary>
        /// Informational message type.
        /// </summary>
        Info = 0,

        /// <summary>
        /// Warning message type.
        /// </summary>
        Warning = 1,

        /// <summary>
        /// Error message type.
        /// </summary>
        Error = 2,

        /// <summary>
        /// Logging finished.
        /// </summary>
        Finished = 3
    }
}
