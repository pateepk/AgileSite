using System;
using System.Collections.Generic;

namespace CMS.Core
{
    /// <summary>
    /// Represents the default event log service implementation.
    /// Stores the events in the buffer to be used later by the proper event log implementation.
    /// </summary>
    internal class DefaultEventLogService : IEventLogService
    {
        #region "Buffered events"

        private static readonly List<BufferedEvent> mBufferedEvents = new List<BufferedEvent>();
        
        #endregion


        #region "IEventLogService members"

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
        /// Ensures the log for given GUID.
        /// </summary>
        /// <param name="logGuid">Log GUID</param>
        public ILogContext EnsureLog(Guid logGuid)
        {
            return new DefaultLogContext() { LogGuid = logGuid };
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
            // Store event in the buffer
            var ev = new BufferedEvent(eventType, source, eventCode, eventDescription);
            AddBufferedEvent(ev);
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
            // Store event in the buffer
            var ev = new BufferedEvent(source, eventCode, ex)
                {
                    LoggingPolicy = loggingPolicy
                };

            AddBufferedEvent(ev);
        }


        /// <summary>
        /// Adds the event to the buffered list.
        /// </summary>
        /// <param name="ev">Event to add</param>
        private static void AddBufferedEvent(BufferedEvent ev)
        {
            if (mBufferedEvents.Count == 0)
            {
                // Add manually Init event when the first event is logged using the default event log provider
                mBufferedEvents.Add(new BufferedEvent("I", "Application_Init", "INITAPP", "The init discovery process of the application started."));
            }
            mBufferedEvents.Add(ev);
        }


        /// <summary>
        /// Logs the buffered events to the event log
        /// </summary>
        internal static void LogBufferedEvents()
        {
            // If event log service implementation was replaced
            if (!(CoreServices.EventLog is DefaultEventLogService))
            {
                try
                {
                    // Log events buffered by the default event log service implementation
                    foreach (var ev in mBufferedEvents)
                    {
                        ev.LogEvent();
                    }
                }
                catch (Exception ex)
                {
                    // Log the attempt to write buffered events to buffer
                    var ev = new BufferedEvent("E", "LOGBUFFEREDEVENTS", ex);

                    AddBufferedEvent(ev);
                }
            }
        }
        
        #endregion
    }
}
