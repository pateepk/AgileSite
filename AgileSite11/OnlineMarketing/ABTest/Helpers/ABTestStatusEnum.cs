using System;

namespace CMS.OnlineMarketing
{
    /// <summary>
    /// AB test status enumeration.
    /// </summary>
    public enum ABTestStatusEnum
    {
        /// <summary>
        /// Running status - test is running.
        /// </summary>
        Running = 0,


        /// <summary>
        /// Scheduled status - test is scheduled to be started.
        /// </summary>
        Scheduled = 1,


        /// <summary>
        /// NotStarted status - test hasn't been started yet.
        /// </summary>
        NotStarted = 2,


        /// <summary>
        /// Finished status - test has finished.
        /// </summary>
        Finished = 3,
    }
}