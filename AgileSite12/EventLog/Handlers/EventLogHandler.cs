using CMS.Base;

namespace CMS.EventLog
{
    /// <summary>
    /// Defines handlers for events related to records in the system's Event log.
    /// </summary>
    public sealed class EventLogHandler : AdvancedHandler<EventLogHandler, LogEventArgs>
    {
        /// <summary>
        /// Initiates the event handling
        /// </summary>
        /// <param name="eventObject">Event object</param>
        public EventLogHandler StartEvent(EventLogInfo eventObject)
        {
            var e = new LogEventArgs
            {
                Event = eventObject
            };

            return StartEvent(e);
        }
    }
}
