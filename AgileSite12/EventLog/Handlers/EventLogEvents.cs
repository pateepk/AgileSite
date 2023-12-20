namespace CMS.EventLog
{
    /// <summary>
    /// Event log events
    /// </summary>
    public static class EventLogEvents
    {
        /// <summary>
        /// Event is triggered when event is being persisted.
        /// </summary>
        public static readonly EventLogHandler LogEvent = new EventLogHandler();


        /// <summary>
        /// Event is triggered when <see cref="EventLogInfo"/> object enters to the system using <see cref="EventLogProvider"/> at the moment when <see cref="System.Web.HttpContext"/> or any other application contexts are available.
        /// </summary>
        /// <remarks>
        /// Event can fill any additional data to not persisted <see cref="EventLogInfo"/> object.
        /// </remarks>
        /// <seealso cref="PrepareEventLogDataHandler"/>
        /// <seealso cref="LogEventArgs"/>
        public static readonly PrepareEventLogDataHandler PrepareData = new PrepareEventLogDataHandler();
    }
}