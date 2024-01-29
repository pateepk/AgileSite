using System;

namespace CMS.OnlineMarketing
{
    /// <summary>
    /// MVT test status enumeration.
    /// </summary>
    public enum MVTestStatusEnum
    {
        /// <summary>
        /// Disabled - task is disabled.
        /// </summary>
        Disabled = 0,

        /// <summary>
        /// Running status - task is running.
        /// </summary>
        Running = 1,

        /// <summary>
        /// Not running - task is enabled, but not running yet
        /// </summary>
        NotRunning = 2,

        /// <summary>
        /// Finished status - task has finished.
        /// </summary>
        Finished = 3
    }
}