using System;

namespace CMS.Core
{
    /// <summary>
    /// Event log service interface
    /// </summary>
    public interface IEventLogService
    {
        /// <summary>
        /// Current log context
        /// </summary>
        ILogContext CurrentLogContext
        {
            get; 
        }


        /// <summary>
        /// Ensures the log for given GUID.
        /// </summary>
        /// <param name="logGuid">Log GUID</param>
        ILogContext EnsureLog(Guid logGuid);


        /// <summary>
        /// Closes given log context.
        /// </summary>
        /// <param name="logGuid">Log GUID</param>
        void CloseLog(Guid logGuid);


        /// <summary>
        /// Writes a new record to the event log.
        /// </summary>
        /// <param name="eventType">Type of the event. Please use predefined constants from EventLogProvider</param>
        /// <param name="source">Source of the event (Content, Administration, etc.)</param>
        /// <param name="eventCode">Event code (UPDATEDOC, SENDEMAIL, DELETE, etc.)</param>
        /// <param name="eventDescription">Detailed description of the event</param>
        void LogEvent(string eventType, string source, string eventCode, string eventDescription);


        /// <summary>
        /// Writes a new error to the event log.
        /// </summary>
        /// <param name="source">Source of the event (Content, Administration, etc.)</param>
        /// <param name="eventCode">Event code (UPDATEDOC, SENDEMAIL, DELETE, etc.)</param>
        /// <param name="ex">Exception to be logged</param>
        /// <param name="loggingPolicy">Logging policy.</param>
        void LogException(string source, string eventCode, Exception ex, LoggingPolicy loggingPolicy = null);
    }
}
