using System;
using System.Diagnostics;

using CMS.EventLog;

namespace CMS.Tests
{
    /// <summary>
    /// Fake event log provider for tests. By default reports logged errors as failed test, logs warnings to output, and ignores information.
    /// </summary>
    public class FakeEventLogProvider : EventLogProvider
    {
        /// <summary>
        /// Action performed when event is logged.
        /// </summary>
        public Action<EventLogInfo> OnSetEventLogInfoInternal;


        /// <summary>
        /// Function invoked when <see cref="LogEventInternal"/> is called. The base implementation is not called when function is set.
        /// </summary>
        public Func<EventLogInfo, EventLogInfo> OnLogEventInternal;


        /// <summary>
        /// If true, the events are logged to the database. Default false
        /// </summary>
        public bool LogEventsToDatabase
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates whether errors logged to the eventlog should be considered as test errors. 
        /// </summary>
        public bool FailForErrorEvent
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates whether events should be logged directly to the storage or worker queue should be used
        /// </summary>
        public bool LogEventsDirectly
        {
            get;
            set;
        }


        /// <summary>
        /// Creates new instance of <see cref="FakeEventLogProvider"/>.
        /// </summary>
        public FakeEventLogProvider()
        {
            FailForErrorEvent = true;
            LogEventsDirectly = true;
            OnSetEventLogInfoInternal = LogTestEvent;
        }


        /// <summary>
        /// Logs the event. Overrides the event by actions needed for tests
        /// </summary>
        /// <param name="eventObject">Event object</param>
        protected override void SetEventLogInfoInternal(EventLogInfo eventObject)
        {
            if (OnSetEventLogInfoInternal != null)
            {
                OnSetEventLogInfoInternal(eventObject);
            }

            if (LogEventsToDatabase)
            {
                base.SetEventLogInfoInternal(eventObject);
            }
        }


        /// <summary>
        /// Writes a new record to the event log.
        /// </summary>
        protected override EventLogInfo LogEventInternal(EventLogInfo eventObject, bool logDirectly, bool deleteOlder)
        {
            var onEventLogInternal = OnLogEventInternal;
            if (onEventLogInternal != null)
            {
                return onEventLogInternal(eventObject);
            }

            return base.LogEventInternal(eventObject, (logDirectly | LogEventsDirectly), deleteOlder);
        }


        /// <summary>
        /// Logs the event
        /// </summary>
        /// <param name="eventObject">Event object</param>
        private void LogTestEvent(EventLogInfo eventObject)
        {
            var text = GetEventLogText(eventObject);

            ReportError(eventObject.EventType, text);
        }


        internal void ReportError(string eventType, string text)
        {
            switch (eventType)
            {
                case EventType.ERROR:
                    // Fail upon error, tests should not log any unintentional errors to event log
                    if (FailForErrorEvent)
                    {
                        NUnit.Framework.Assert.Fail(GetFailMessage("ERROR", text));
                    }
                    break;

                case EventType.WARNING:
                    // For warnings, write them to test output
                    Trace.TraceInformation(GetFailMessage("WARNING", text));
                    break;

            // No special actions for other events
            }
        }


        private static string GetFailMessage(string type, string text)
        {
            return String.Format("[!] The test caused an {0} being written to the event log\r\n{1}\r\n=============================\r\n", type, text);
        }


        /// <summary>
        /// Executes when the event logging throws an error.
        /// </summary>
        /// <param name="ex">Exception thrown upon problem with logging</param>
        protected override void CannotLogEventInternal(Exception ex)
        {
            throw new InvalidOperationException("Event logging failed, the system was unable to log an event to event log. See the inner exception to determine the cause of logging failure.", ex);
        }
    }
}
