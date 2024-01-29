namespace CMS.EventLog
{
    /// <summary>
    /// Event log events
    /// </summary>
    public static class EventLogEvents
    {
        /// <summary>
        /// Fires when event is logged into system.
        /// </summary>
        public static EventLogHandler LogEvent = new EventLogHandler();
    }
}