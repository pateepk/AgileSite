using System;

using CMS.Core;
using CMS.EventLog;

namespace CMS.Tests
{
    /// <summary>
    /// Fake event log service for testing purposes using <see cref="NUnit.Framework.AssertionException"/> instead of logging errors into database
    /// </summary>
    public class FakeEventLogService : ITestEventLogService
    {
        private readonly ILogContext mCurrentLogContext = new DefaultLogContext();


        /// <summary>
        /// Returns current log context
        /// </summary>
        public ILogContext CurrentLogContext
        {
            get
            {
                return mCurrentLogContext;
            }
        }

        /// <summary>
        /// Instance of FakeEventLogProvider to provide logging options
        /// </summary>
        public FakeEventLogProvider TestsEventLogProvider
        {
            get;
            set;
        }


        /// <summary>
        /// Ensures the log for given GUID.
        /// </summary>
        /// <param name="logGuid">Log GUID</param>
        public ILogContext EnsureLog(Guid logGuid)
        {
            return new DefaultLogContext { LogGuid = logGuid };
        }


        /// <summary>
        /// Closes given log context.
        /// </summary>
        /// <param name="logGuid">Log GUID</param>
        public void CloseLog(Guid logGuid)
        {
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
            var message = String.Format("{0}\r\n{1}\r\n{2}", source, eventCode, eventDescription);

            TestsEventLogProvider.ReportError(eventType, message);
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
            LogEvent(EventType.ERROR, source, eventCode, ex.ToString());
        }
    }
}
