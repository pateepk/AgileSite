using System;

using CMS.Core;

namespace CMS.Tests.Fakes
{
    /// <summary>
    /// Wrapper for <see cref="IEventLogService"/>
    /// Logs events via underlying service
    /// None event is logged to database via <see cref="FakeEventLogProvider"/>
    /// </summary>
    public class EventLogServiceWrapper : IEventLogService
    {
        /// <summary>
        /// Gets or sets the internal service
        /// </summary>
        public IEventLogService Service
        {
            get;
            set;
        }


        /// <summary>
        /// Current log context
        /// </summary>
        public ILogContext CurrentLogContext
        {

            get
            {
                return Service.CurrentLogContext;
            }
        }


        /// <summary>
        /// Ensures the log for given GUID.
        /// </summary>
        /// <param name="logGuid">Log GUID</param>
        public ILogContext EnsureLog(Guid logGuid)
        {
            return Service.EnsureLog(logGuid);
        }


        /// <summary>
        /// Closes given log context.
        /// </summary>
        /// <param name="logGuid">Log GUID</param>
        public void CloseLog(Guid logGuid)
        {
            Service.CloseLog(logGuid);
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
            Service.LogEvent(eventType, source, eventCode, eventDescription);
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
            Service.LogException(source, eventCode, ex, loggingPolicy);
        }
    }
}
