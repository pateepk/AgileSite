using System;

using CMS.Base;

namespace CMS.EventLog
{
    /// <summary>
    /// Handler for <see cref="EventLogEvents.PrepareData"/> providing possibility to modify non-persisted <see cref="EventLogInfo"/> data when such object enters the system.
    /// </summary>
    /// <seealso cref="LogEventArgs"/>
    public sealed class PrepareEventLogDataHandler : SimpleHandler<PrepareEventLogDataHandler, LogEventArgs>
    {
        /// <summary>
        /// Start the event with given <paramref name="eventObject"/>.
        /// </summary>
        /// <param name="eventObject">Event object</param>
        /// <throws>
        /// <see cref="ArgumentNullException"/> when null event object is provided.
        /// </throws>
        public LogEventArgs StartEvent(EventLogInfo eventObject)
        {
            if (eventObject == null)
            {
                throw  new ArgumentNullException(nameof(eventObject));
            }

            var args = new LogEventArgs
            {
                Event = eventObject
            };

            return StartEvent(args);
        }
    }
}
