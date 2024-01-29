using System;

namespace CMS.Base
{
    /// <summary>
    /// Process status enumeration.
    /// </summary>
    public enum ProcessStatus
    {
        /// <summary>
        /// Stopped.
        /// </summary>
        Stopped = 0,

        /// <summary>
        /// Running.
        /// </summary>
        Running = 1,

        /// <summary>
        /// Finished.
        /// </summary>
        Finished = 2,

        /// <summary>
        /// Error occurred.
        /// </summary>
        Error = 3,

        /// <summary>
        /// Application restarted.
        /// </summary>
        Restarted = 4
    }
}