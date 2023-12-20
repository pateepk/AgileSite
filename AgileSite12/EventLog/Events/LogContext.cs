using System;
using System.Collections.Generic;
using System.Text;

using CMS.Base;
using CMS.Core;
using CMS.Helpers;

namespace CMS.EventLog
{
    /// <summary>
    /// Provides the context for logging asynchronous events.
    /// </summary>
    public class LogContext : ILogContext, INotCopyThreadItem, IDisposable
    {
        #region "Constants"

        /// <summary>
        /// Suffix used during multiple event logging if MultipleOperationEventCode is not supplied.
        /// </summary>
        private const string MULTIPLE_OPERATION_SUFFIX = "MULTIPLE";


        /// <summary>
        /// Determines how many events should be logged at once.
        /// </summary>
        private const int LOG_AFTER_EVENTS = 100;

        #endregion


        #region "Variables"

        /// <summary>
        /// Table of existing logs [GUID] -> [LogContext]
        /// </summary>
        protected static SafeDictionary<Guid, LogContext> mLogs = new SafeDictionary<Guid, LogContext>();

        /// <summary>
        /// Current log.
        /// </summary>
        protected StringBuilder mLog = new StringBuilder();


        /// <summary>
        /// Explicit event log code used when LogSingleEvents is false (case of multiple logging).
        /// </summary>
        protected string mMultipleOperationEventCode = null;


        /// <summary>
        /// Logs GUID.
        /// </summary>
        protected Guid mLogGuid = Guid.Empty;


        /// <summary>
        /// Fires when the log has changed.
        /// </summary>
        public event EventHandler OnChanged;


        /// <summary>
        /// Indicates whether to log events one by one to event log.
        /// </summary>
        protected bool? mLogSingleEvents = null;


        /// <summary>
        /// Info object containing event.
        /// </summary>
        protected EventLogInfo mEvent = null;


        /// <summary>
        /// Counter used for logging multiple events at once.
        /// </summary>
        private int eventLogCounter;


        /// <summary>
        /// Object for locking the context within multiple threads
        /// </summary>
        private readonly object lockObject = new object();


        /// <summary>
        /// Set of context names from which the log context receives messages
        /// </summary>
        private HashSet<string> mAllowedContexts;


        /// <summary>
        /// Flag whether object was already disposed.
        /// </summary>
        private bool disposed;

        #endregion


        #region "Properties"

        /// <summary>
        /// Event which fires if some text is appended to the log
        /// </summary>
        public event Action<string, bool> TextAppended;


        /// <summary>
        /// Gets or sets current log context.
        /// </summary>
        public static LogContext Current
        {
            get
            {
                return (LogContext)RequestStockHelper.GetItem("LogContext", true);
            }
            set
            {
                RequestStockHelper.Add("LogContext", value, true);
            }
        }


        /// <summary>
        /// Maximum length of the log. If the length is exceeded, the log is trimmed to half, offset is set and the log continues.
        /// </summary>
        public int MaxLength
        {
            get;
            set;
        }


        /// <summary>
        /// Offset of a partial log in a complete log
        /// </summary>
        public int Offset
        {
            get;
            protected set;
        }


        /// <summary>
        /// Gets the current log.
        /// </summary>
        public string Log
        {
            get
            {
                lock (lockObject)
                {
                    return mLog.ToString();
                }
            }
            set
            {
                // Replace the log completely
                mLog.Clear();
                mLog.Append(value);

                Offset = 0;
            }
        }


        /// <summary>
        /// Explicit event log code used when LogSingleEvents is false (case of multiple logging).
        /// </summary>
        public string MultipleOperationEventCode
        {
            get
            {
                return mMultipleOperationEventCode;
            }
            set
            {
                mMultipleOperationEventCode = value;
            }
        }


        /// <summary>
        /// Logs guid.
        /// </summary>
        public Guid LogGuid
        {
            get
            {
                return mLogGuid;
            }
            set
            {
                mLogGuid = value;
            }
        }


        /// <summary>
        /// If true, the context always logs regardless of the action context settings
        /// </summary>
        public bool LogAlways
        {
            get;
            set;
        }

        #endregion


        #region "Event log properties"

        /// <summary>
        /// Indicates whether to log events one by one to event log.
        /// </summary>
        public bool LogSingleEvents
        {
            get
            {
                if (mLogSingleEvents == null)
                {
                    mLogSingleEvents = ValidationHelper.GetBoolean(SettingsHelper.AppSettings["CMSLogSingleEvents"], false);
                }
                return mLogSingleEvents.Value;
            }
            set
            {
                mLogSingleEvents = value;
            }
        }


        /// <summary>
        /// Info object of type EventLogInfo.
        /// </summary>
        public EventLogInfo Event
        {
            get
            {
                return mEvent;
            }
            set
            {
                mEvent = value;
            }
        }


        /// <summary>
        /// Name of machine.
        /// </summary>
        public string MachineName
        {
            get;
            set;
        }


        /// <summary>
        /// Referrer url.
        /// </summary>
        public string UrlReferrer
        {
            get;
            set;
        }


        /// <summary>
        /// Browser identification.
        /// </summary>
        public string UserAgent
        {
            get;
            set;
        }


        /// <summary>
        /// IP address of client.
        /// </summary>
        public string IPAddress
        {
            get;
            set;
        }


        /// <summary>
        /// URL of event.
        /// </summary>
        public string EventURL
        {
            get;
            set;
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Registers the context within request.
        /// </summary>
        public LogContext()
        {
            Current = this;

            mLogGuid = Guid.NewGuid();
            mLogs[mLogGuid] = this;
        }


        /// <summary>
        /// Constructor - Registers the context within request.
        /// </summary>
        public LogContext(Guid logGuid)
        {
            Current = this;

            mLogGuid = logGuid;
            mLogs[mLogGuid] = this;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Sets the list of the context names that the log context accepts. Use empty string for context which logs messages that do not provide context.
        /// </summary>
        /// <param name="contextNames">Context names</param>
        public void SetAllowedContexts(params string[] contextNames)
        {
            mAllowedContexts = new HashSet<string>(contextNames, StringComparer.InvariantCultureIgnoreCase);
        }


        /// <summary>
        /// Closes the log.
        /// </summary>
        public void Close()
        {
            // Save event log object
            if (Event != null)
            {
                EventLogProvider.SetEventLogInfo(Event);
            }

            // Clear current context
            if (this == Current)
            {
                Current = null;
            }
            mLogs[mLogGuid] = null;
        }


        /// <summary>
        /// Appends text to the log.
        /// </summary>
        /// <param name="text">Text to append</param>
        /// <param name="contextName">Context name</param>
        public static void AppendLine(string text, string contextName = null)
        {
            var context = Current;
            if ((context != null) && context.AllowMessageFromContext(contextName))
            {
                context.AppendText(text);
            }
        }


        /// <summary>
        /// Appends text to the log.
        /// </summary>
        /// <param name="text">Text to append</param>
        /// <param name="contextName">Context name</param>
        public static void Append(string text, string contextName = null)
        {
            if (!String.IsNullOrEmpty(text))
            {
                var context = Current;
                if ((context != null) && context.AllowMessageFromContext(contextName))
                {
                    context.AppendText(text, false);
                }
            }
        }


        /// <summary>
        /// Drops the current log context.
        /// </summary>
        public static void CloseCurrent()
        {
            var context = Current;
            if (context != null)
            {
                context.Close();
            }
        }


        /// <summary>
        /// Closes given log context.
        /// </summary>
        /// <param name="logGuid">Log GUID</param>
        public static void CloseLog(Guid logGuid)
        {
            var log = mLogs[logGuid];
            if (log != null)
            {
                log.Close();
            }
        }


        /// <summary>
        /// Ensures the log for given GUID.
        /// </summary>
        /// <param name="logGuid">Log GUID</param>
        public static LogContext EnsureLog(Guid logGuid)
        {
            var log = mLogs[logGuid];
            if (log == null)
            {
                log = new LogContext(logGuid);

                // Initialize properties
                log.IPAddress = RequestContext.UserHostAddress;
                log.EventURL = RequestContext.RawURL;
                log.UrlReferrer = RequestContext.URLReferrer;
                log.MachineName = SystemContext.MachineName;
                log.UserAgent = RequestContext.UserAgent;
            }

            Current = log;

            return log;
        }


        /// <summary>
        /// Returns true if the given log exists.
        /// </summary>
        /// <param name="logGuid">Log GUID</param>
        public static bool LogExists(Guid logGuid)
        {
            return (mLogs[logGuid] != null);
        }


        /// <summary>
        /// Clears the log
        /// </summary>
        public void Clear()
        {
            mLog.Clear();
            Offset = 0;
        }


        /// <summary>
        /// Disposes the object.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }


        /// <summary>
        /// Disposes object's log.
        /// </summary>
        /// <param name="disposing">Determines whether method was called from <see cref="Dispose()"/> or from the destructor</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                Close();
            }

            disposed = true;
        }


        /// <summary>
        /// Returns true if the given context allows messages from the given context name
        /// </summary>
        /// <param name="contextName">Context name to check</param>
        private bool AllowMessageFromContext(string contextName)
        {
            // If contexts are not 
            if (mAllowedContexts == null)
            {
                return true;
            }

            // When context name is not defined, check if empty context is allowed
            if (contextName == null)
            {
                contextName = String.Empty;
            }

            return mAllowedContexts.Contains(contextName);
        }


        /// <summary>
        /// Appends the text.
        /// </summary>
        /// <param name="text">Text to append</param>
        /// <param name="newLine">Append as new line</param>
        public void AppendText(string text, bool newLine = true)
        {
            if (LogAlways || CMSActionContext.CurrentEnableLogContext)
            {
                lock (lockObject)
                {
                    if (newLine && (mLog.Length != 0))
                    {
                        mLog.Append(Environment.NewLine);
                    }

                    mLog.Append(text);

                    // Trim the log if the max length is exceeded
                    if ((MaxLength > 0) && (mLog.Length > MaxLength))
                    {
                        // Remove the first half of the log trimmed to whole lines
                        int removeToIndex = mLog.Length / 2;

                        // Adjust the index to the whole line to trim the log meaningfully
                        while ((removeToIndex > 0) && (mLog[removeToIndex] != '\n'))
                        {
                            removeToIndex--;
                        }

                        // Remove the first part of the log
                        if (removeToIndex > 0)
                        {
                            if (mLog[removeToIndex] == '\n')
                            {
                                removeToIndex++;
                            }

                            mLog.Remove(0, removeToIndex);

                            // Adjust the offset
                            Offset += removeToIndex;
                        }
                    }

                    if (TextAppended != null)
                    {
                        TextAppended(text, newLine);
                    }
                }

                RaiseOnChanged();
            }
        }


        /// <summary>
        /// Fires the onchanged event.
        /// </summary>
        protected void RaiseOnChanged()
        {
            if (OnChanged != null)
            {
                OnChanged(this, null);
            }
        }

        #endregion


        #region "Event logging"

        /// <summary>
        /// Writes a new record to the event log using <see cref="Current"/> context.
        /// </summary>
        /// <param name="eventType">Type of the event. Please use predefined constants from <see cref="EventType"/> class.</param>
        /// <param name="source">Source of the event (Content, Administration, etc.)</param>
        /// <param name="eventCode">Event code (Security, Update, Delete, etc.)</param>
        /// <param name="eventDescription">Detailed description of the event</param>
        /// <param name="eventUrl">Event URL address</param>
        /// <param name="userId">ID of the user, who caused logged event</param>
        /// <param name="userName">Name of the user, who caused logged event</param>
        /// <param name="nodeId">ID value of the document</param>
        /// <param name="documentName">NamePath value of the document</param>
        /// <param name="ipAddress">IP Address of the user, who caused logged event</param>
        /// <param name="siteId">ID of the site</param>
        /// <param name="machineName">Name of machine</param>
        /// <param name="urlReferrer">Referrer URL</param>
        /// <param name="userAgent">User agent</param>
        /// <param name="eventTime">Date and time when the event occurs</param>
        public static void LogEventToCurrent(string eventType, string source, string eventCode, string eventDescription, string eventUrl, int userId, string userName, int nodeId, string documentName, string ipAddress, int siteId, string machineName, string urlReferrer, string userAgent, DateTime eventTime)
        {
            var current = Current;
            if (current != null)
            {
                current.LogEvent(eventType, source, eventCode, eventDescription, eventUrl, userId, userName, nodeId, documentName, ipAddress, siteId, machineName, urlReferrer, userAgent, eventTime);
            }
            else
            {
                // Log event
                EventLogProvider.LogEvent(eventType, source, eventCode, eventDescription, eventUrl, userId, userName, nodeId, documentName, ipAddress, siteId, machineName, urlReferrer, userAgent, eventTime);
            }
        }


        /// <summary>
        /// Writes a new record to the event log.
        /// </summary>
        /// <param name="eventType">Type of the event. Please use predefined constants from <see cref="EventType"/> class.</param>
        /// <param name="source">Source of the event (Content, Administration, etc.)</param>
        /// <param name="eventCode">Event code (Security, Update, Delete, etc.)</param>
        /// <param name="eventDescription">Detailed description of the event</param>
        /// <param name="eventUrl">Event URL address</param>
        /// <param name="userId">ID of the user, who caused logged event</param>
        /// <param name="userName">Name of the user, who caused logged event</param>
        /// <param name="nodeId">ID value of the document</param>
        /// <param name="documentName">NamePath value of the document</param>
        /// <param name="ipAddress">IP Address of the user, who caused logged event</param>
        /// <param name="siteId">ID of the site</param>
        /// <param name="machineName">Name of machine</param>
        /// <param name="urlReferrer">Referrer URL</param>
        /// <param name="userAgent">User agent</param>
        /// <param name="eventTime">Date and time when the event occurs</param>
        public void LogEvent(string eventType, string source, string eventCode, string eventDescription, string eventUrl, int userId, string userName, int nodeId, string documentName, string ipAddress, int siteId, string machineName, string urlReferrer, string userAgent, DateTime eventTime)
        {
            bool newEvent = false;
            var eventLogInfo = Event;

            // Get new event object
            if (eventLogInfo == null)
            {
                newEvent = true;
                eventLogInfo = new EventLogInfo();
            }

            string currentEventType = eventLogInfo.EventType;
            string currentEventDescription = eventLogInfo.EventDescription;

            // Set proper event type
            if (currentEventType != "E")
            {
                if (currentEventType == "W")
                {
                    if (eventType == "E")
                    {
                        eventLogInfo.EventType = eventType;
                    }
                }
                else
                {
                    eventLogInfo.EventType = eventType;
                }
            }

            if (String.IsNullOrEmpty(currentEventDescription))
            {
                currentEventDescription = eventDescription;
            }
            else
            {
                currentEventDescription += Environment.NewLine + eventDescription;
            }

            // Append description
            eventLogInfo.EventDescription = currentEventDescription;

            if (LogSingleEvents)
            {
                eventLogInfo.EventCode = eventCode;
            }
            else
            {
                // Append multiple suffix in case this is not first event to be logged
                if (!newEvent)
                {
                    eventCode += MultipleOperationEventCode ?? MULTIPLE_OPERATION_SUFFIX;
                    // Clear document information -> multiple action
                    eventLogInfo.NodeID = 0;
                    eventLogInfo.DocumentName = null;
                }

                // Set event code
                eventLogInfo.EventCode = eventCode;
            }

            if (newEvent)
            {
                ipAddress = DataHelper.GetNotEmpty(ipAddress, IPAddress);
                machineName = DataHelper.GetNotEmpty(machineName, MachineName);
                urlReferrer = DataHelper.GetNotEmpty(urlReferrer, UrlReferrer);
                userAgent = DataHelper.GetNotEmpty(userAgent, UserAgent);
                eventUrl = DataHelper.GetNotEmpty(eventUrl, EventURL);

                if (eventTime == DateTimeHelper.ZERO_TIME)
                {
                    eventTime = DateTime.Now;
                }

                eventLogInfo.EventTime = eventTime;
                eventLogInfo.UserID = userId;
                eventLogInfo.UserName = userName;
                eventLogInfo.IPAddress = ipAddress;
                eventLogInfo.SiteID = siteId;
                eventLogInfo.EventUrl = eventUrl;
                eventLogInfo.Source = source;
                eventLogInfo.EventMachineName = machineName;
                eventLogInfo.EventUrlReferrer = urlReferrer;
                eventLogInfo.EventUserAgent = userAgent;
                eventLogInfo.NodeID = nodeId;
                eventLogInfo.DocumentName = documentName;
            }

            if (LogSingleEvents)
            {
                // Log event and clear Event property
                EventLogProvider.LogEvent(eventLogInfo);
                Event = null;
            }
            else
            {
                // Increment counter
                eventLogCounter++;
                if (newEvent)
                {
                    // Log event
                    Event = EventLogProvider.LogEvent(eventLogInfo);
                }
                else
                {
                    Event = eventLogInfo;
                    if (eventLogCounter >= LOG_AFTER_EVENTS)
                    {
                        // Update existing
                        if (EventLogProvider.LoggingEnabled)
                        {
                            EventLogProvider.SetEventLogInfo(Event);
                        }

                        // Reset counter
                        eventLogCounter = 0;
                    }
                }
            }
        }

        #endregion
    }
}