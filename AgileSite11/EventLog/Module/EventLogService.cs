using System;

using CMS.Core;

namespace CMS.EventLog
{
    /// <summary>
    /// Service to provide logging events
    /// </summary>
    internal class EventLogService : IEventLogService
    {
        /// <summary>
        /// Current log context
        /// </summary>
        public ILogContext CurrentLogContext
        {
            get
            {
                return LogContext.Current;
            }
        }


        /// <summary>
        /// Ensures the log for given GUID.
        /// </summary>
        /// <param name="logGuid">Log GUID</param>
        public ILogContext EnsureLog(Guid logGuid)
        {
            LogContext log = LogContext.EnsureLog(logGuid);

            return log;
        }


        /// <summary>
        /// Closes given log context.
        /// </summary>
        /// <param name="logGuid">Log GUID</param>
        public void CloseLog(Guid logGuid)
        {
            LogContext.CloseLog(logGuid);
        }


        /// <summary>
        /// Writes a new record to the event log.
        /// </summary>
        /// <param name="eventType">Type of the event. Please use predefined constants from EventLogProvider</param>
        /// <param name="source">Source of the event (Content, Administration, etc.)</param>
        /// <param name="eventCode">Event code (UPDATEDOC, SENDEMAIL, DELETE, etc.)</param>
        /// <param name="eventDescription">Detailed description of the event</param>
        public void LogEvent(string eventType, string source, string eventCode, string eventDescription)
        {
            EventLogProvider.LogEvent(eventType, source, eventCode, eventDescription);
        }


        /// <summary>
        /// Writes a new error to the event log.
        /// </summary>
        /// <param name="source">Source of the event (Content, Administration, etc.)</param>
        /// <param name="eventCode">Event code (UPDATEDOC, SENDEMAIL, DELETE, etc.)</param>
        /// <param name="ex">Exception to be logged</param>
        /// <param name="loggingPolicy">Logging policy.</param>
        public void LogException(string source, string eventCode, Exception ex, LoggingPolicy loggingPolicy = null)
        {
            EventLogProvider.LogException(source, eventCode, ex, loggingPolicy: loggingPolicy);
        }
    }
}
