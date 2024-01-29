using CMS.Base;

namespace CMS.EventLog
{
    /// <summary>
    /// Event arguments for LogEventToCurrent event handler
    /// </summary>
    public class LogEventArgs : CMSEventArgs
    {
        /// <summary>
        /// Processed event log object
        /// </summary>
        public EventLogInfo Event
        {
            get;
            set;
        }
    }
}