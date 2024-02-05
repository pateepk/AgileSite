using System;

namespace CMS.ContinuousIntegration.Internal
{
    /// <summary>
    /// Event arguments used for <see cref="AbstractFileSystemProgressLoggingJob.LogProgress"/> event.
    /// </summary>
    internal class LogProgressEventArgs : EventArgs
    {
        /// <summary>
        /// Creates new <see cref="LogProgressEventArgs"/>.
        /// </summary>
        /// <param name="logItem">Log item object.</param>
        public LogProgressEventArgs(LogItem logItem)
        {
            LogItem = logItem;
        }


        /// <summary>
        /// Log item object.
        /// </summary>
        public LogItem LogItem
        {
            get;
            private set;
        }
    }
}
