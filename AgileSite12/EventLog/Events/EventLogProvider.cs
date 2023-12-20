using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;

using CMS.Base;
using CMS.Core;
using CMS.DataEngine;
using CMS.DataEngine.Query;
using CMS.Helpers;

namespace CMS.EventLog
{
    /// <summary>
    /// Provides basic operations with the event log.
    /// </summary>
    public class EventLogProvider : AbstractInfoProvider<EventLogInfo, EventLogProvider>
    {
        #region "Constants"

        private const string MODE_DATABASE = "database";
        private const string MODE_FILESYSTEM = "filesystem";
        private const string MODE_TRACE = "trace";
        private const string MessageId = "0x0f9";
        private const string MESSAGE_PREFIX = "Message: ";
        private const string EXCEPTION_TYPE_PREFIX = "Exception type: ";
        private const string STACK_TRACE_PREFIX = "Stack trace: ";
        private const string TOGGLE_DEFAULT_EVENT_LOG_LISTENER_ACTION_NAME = "ToggleDefaultEventLogListener";

        /// <summary>
        /// Constant used to determine maximum amount of items that can be deleted at once (value is 50.000).
        /// </summary>
        internal const int MAXTODELETE = 50000;

        #endregion


        #region "Variables"

        // Table lock for loading.
        private static readonly object tableLock = new object();

        // Indicates if the logging is enabled.
        private static bool? mLoggingEnabled;

        // Indicates if the log should contain the changes for particular fields.
        private static bool? mLogFieldChanges;

        // Indicates if the log should contain the changes for particular document fields.
        private static bool? mLogDocumentFieldChanges;

        // Coefficient for log deletion, keeps the specified number of log items percent alive and deletes the log by batch when the percents are exceeded.
        private static double? mLogDeleteCoefficient;

        // Cached log items count [siteId] -> [count]
        private static Hashtable mLogItems;

        // Cached log items count [siteId] -> [logSize]
        private static Hashtable mLogSizes;

        private static readonly Lazy<IPerformanceCounter> mWarnings = new Lazy<IPerformanceCounter>(Service.Resolve<IPerformanceCounter>);
        private static readonly Lazy<IPerformanceCounter> mErrors = new Lazy<IPerformanceCounter>(Service.Resolve<IPerformanceCounter>);

        private static bool? mUseEventLogPopUp;
        private static bool mClearEventsOfNonexistingSites = true;

        #endregion


        #region "Properties"

        /// <summary>
        /// Indicates whether pop-up message module is allowed (web.config key CMSUseEventLogPopUp)
        /// </summary>
        private static bool UseEventLogPopUp
        {
            get
            {
                if (mUseEventLogPopUp == null)
                {
                    mUseEventLogPopUp = ValidationHelper.GetBoolean(SettingsHelper.AppSettings["CMSUseEventLogPopUp"], false);
                }
                return mUseEventLogPopUp.Value;
            }
        }


        /// <summary>
        /// Coefficient for log deletion, keeps the specified number of log items percent alive and deletes the log by batch when the percents are exceeded.
        /// </summary>
        private static double LogDeleteCoefficient
        {
            get
            {
                if (mLogDeleteCoefficient == null)
                {
                    mLogDeleteCoefficient = (100 + ValidationHelper.GetInteger(SettingsHelper.AppSettings["CMSLogKeepPercent"], 10)) / 100.0;
                }

                return mLogDeleteCoefficient.Value;
            }
        }


        /// <summary>
        /// Indicates if logging is enabled.
        /// </summary>
        public static bool LoggingEnabled
        {
            get
            {
                if (mLoggingEnabled == null)
                {
                    mLoggingEnabled = ValidationHelper.GetBoolean(SettingsHelper.AppSettings["CMSLogEvents"], true);
                }

                return mLoggingEnabled.Value && CMSActionContext.CurrentLogEvents;
            }
            set
            {
                mLoggingEnabled = value;
            }
        }


        /// <summary>
        /// Indicates if log should contain the changes to particular fields.
        /// </summary>
        public static bool LogFieldChanges
        {
            get
            {
                if (mLogFieldChanges == null)
                {
                    mLogFieldChanges = ValidationHelper.GetBoolean(SettingsHelper.AppSettings["CMSLogFieldChanges"], false);
                }

                return mLogFieldChanges.Value;
            }
            set
            {
                mLogFieldChanges = value;
            }
        }


        /// <summary>
        /// Indicates if log should contain the changes to particular document document fields.
        /// </summary>
        public static bool LogDocumentFieldChanges
        {
            get
            {
                if (mLogDocumentFieldChanges == null)
                {
                    mLogDocumentFieldChanges = ValidationHelper.GetBoolean(SettingsHelper.AppSettings["CMSLogDocumentFieldChanges"], false);
                }

                return mLogDocumentFieldChanges.Value;
            }
            set
            {
                mLogDocumentFieldChanges = value;
            }
        }


        /// <summary>
        /// Cached log items count [siteId] -> [count]
        /// </summary>
        private static Hashtable LogItems
        {
            get
            {
                return mLogItems ?? (mLogItems = new Hashtable());
            }
        }


        /// <summary>
        /// Cached log items count [siteId] -> [logSize]
        /// </summary>
        private static Hashtable LogSizes
        {
            get
            {
                return mLogSizes ?? (mLogSizes = new Hashtable());
            }
        }


        /// <summary>
        /// Counter of warnings.
        /// </summary>
        public static IPerformanceCounter Warnings
        {
            get
            {
                return mWarnings.Value;
            }
        }


        /// <summary>
        /// Counter of errors.
        /// </summary>
        public static IPerformanceCounter Errors
        {
            get
            {
                return mErrors.Value;
            }
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Initializes a new instance of the <see cref="EventLogProvider"/> class.
        /// </summary>
        public EventLogProvider()
            : base(EventLogInfo.TYPEINFO)
        {
        }

        #endregion


        #region "Public methods - other"

        /// <summary>
        /// Clears entire event log and writes new event record with information about this action and the user who performed it.
        /// </summary>
        /// <param name="userId">User ID for logging purposes</param>
        /// <param name="userName">User name for logging purposes</param>
        /// <param name="ipAddress">IP address for logging purposes</param>
        /// <param name="siteId">Site ID for logging purposes</param>
        public static void ClearEventLog(int userId, string userName, string ipAddress, int siteId)
        {
            ProviderObject.ClearEventLogInternal(userId, userName, ipAddress, siteId);
        }


        /// <summary>
        /// Returns specified event info object from the event log.
        /// </summary>
        /// <param name="eventId">Event ID</param>
        public static EventLogInfo GetEventLogInfo(int eventId)
        {
            return ProviderObject.GetEventInfoInternal(eventId);
        }


        /// <summary>
        /// Returns the query for all events.
        /// </summary>
        public static ObjectQuery<EventLogInfo> GetEvents()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns the previous and next events of the given event in the order specified by ORDER BY parameter matching the WHERE criteria.
        /// </summary>
        /// <param name="eventId">ID of the event relative to which the previous and next events are returned</param>
        /// <param name="where">Where condition</param>
        /// <param name="orderBy">Order by columns</param>
        public static int[] GetPreviousNext(int eventId, string where, string orderBy)
        {
            return ProviderObject.GetPreviousNextInternal(eventId, where, orderBy);
        }


        /// <summary>
        /// Sets event log.
        /// </summary>
        /// <param name="eventLog">Event log object</param>
        public static void SetEventLogInfo(EventLogInfo eventLog)
        {
            ProviderObject.SetEventLogInfoInternal(eventLog);
        }


        /// <summary>
        /// Gets complete log for exception with message and deep stack trace.
        /// </summary>
        /// <param name="ex">Exception to log</param>
        public static string GetExceptionLogMessage(Exception ex)
        {
            return ProviderObject.GetExceptionLogMessageInternal(ex);
        }


        /// <summary>
        /// Gets the log items count for specified site.
        /// </summary>
        /// <param name="siteId">Site ID</param>
        public static int GetLogItems(int siteId)
        {
            return ProviderObject.GetLogItemsInternal(siteId);
        }


        /// <summary>
        /// Gets the log size for specified site.
        /// </summary>
        /// <param name="siteId">Site ID</param>
        public static int GetLogSize(int siteId)
        {
            return ProviderObject.GetLogSizeInternal(siteId);
        }


        /// <summary>
        /// Returns true if the metadata should be logged.
        /// </summary>
        /// <param name="siteName">Site name</param>
        public static bool LogMetadata(string siteName)
        {
            return LoggingEnabled && SettingsKeyInfoProvider.GetBoolValue(siteName + ".CMSLogMetadata");
        }


        /// <summary>
        /// Creates string representation of event log object.
        /// </summary>
        /// <param name="eventLogInfo">EventLogInfo object</param>
        public static string GetEventLogText(EventLogInfo eventLogInfo)
        {
            return ProviderObject.GetEventLogTextInternal(eventLogInfo);
        }


        /// <summary>
        /// Logs the application start event to the event log
        /// </summary>
        public static void LogApplicationStart()
        {
            // Do not log application start unless in context of a web site/application
            if (!SystemContext.IsWebSite)
            {
                return;
            }

            try
            {
                // Write "Application start" event to the event log
                var message = String.Format("Web application has started with following modules loaded:{0}{0}", Environment.NewLine);

                var description = ModuleEntryManager.Modules.Aggregate(message, (s, m) => s + Environment.NewLine + m.Name);

                LogEvent(EventType.INFORMATION, "Application_Start", "STARTAPP", description);
            }
            catch (Exception ex)
            {
                CannotLogEvent(ex);
            }
        }


        /// <summary>
        /// Logs the application end.
        /// </summary>
        public static void LogApplicationEnd()
        {
            // Do not log application start unless in context of a web site/application
            if (!SystemContext.IsWebSite)
            {
                return;
            }

            try
            {
                var logMessage = "Web application has ended";

                // Get the shutdown reason
                var runtime = (HttpRuntime)typeof(HttpRuntime).InvokeMember("_theRuntime", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.GetField, null, null, null);
                if (runtime != null)
                {
                    // Get the shutdown reason
                    string shutDownMessage = Convert.ToString(GetFieldValue(runtime, "_shutDownMessage"));
                    string shutDownStack = Convert.ToString(GetFieldValue(runtime, "_shutDownMessage"));

                    StackTrace stack = new StackTrace();

                    logMessage += String.Format("{0}{0}Message: {1}{0}{0}Shutdown stack: {2}{0}{0}Call stack:{3}", Environment.NewLine, shutDownMessage, shutDownStack, stack);
                }

                LogEvent(EventType.WARNING, "Application_End", "ENDAPP", logMessage);
            }
            catch (Exception ex)
            {
                CannotLogEvent(ex);
            }
        }


        /// <summary>
        /// Gets a field value from the given object
        /// </summary>
        /// <param name="obj">Object</param>
        /// <param name="fieldName">Field name</param>
        private static object GetFieldValue(object obj, string fieldName)
        {
            return obj.GetType().InvokeMember(fieldName, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField, null, obj, null);
        }

        #endregion


        #region "Public logging methods"

        /// <summary>
        /// Writes a new record to the event log.
        /// </summary>
        /// <param name="eventType">Type of the event. Please use predefined constants from <see cref="EventType"/></param>
        /// <param name="source">Source of the event (Content, Administration, etc.)</param>
        /// <param name="eventCode">Event code (UPDATEDOC, SENDEMAIL, DELETE, etc.)</param>
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
        /// <param name="loggingPolicy">Logging policy.</param>
        public static EventLogInfo LogEvent(string eventType, string source, string eventCode, string eventDescription = null, string eventUrl = null, int userId = 0, string userName = null, int nodeId = 0, string documentName = null, string ipAddress = null, int siteId = 0, string machineName = null, string urlReferrer = null, string userAgent = null, DateTime? eventTime = null, LoggingPolicy loggingPolicy = null)
        {
            return ProviderObject.LogEventInternal(eventType, source, eventCode, eventDescription, eventUrl, userId, userName, nodeId, documentName, ipAddress, siteId, machineName, urlReferrer, userAgent, eventTime, loggingPolicy);
        }


        /// <summary>
        /// Writes a new record to the event log.
        /// </summary>
        /// <param name="eventObject">Event object.</param>
        public static EventLogInfo LogEvent(EventLogInfo eventObject)
        {
            return LogEventCore(eventObject, false);
        }


        /// <summary>
        /// Writes a new record to the event log.
        /// </summary>
        /// <param name="eventObject">Event object.</param>
        /// <param name="logDirectly">If true, the event is logged directly to the database. Otherwise, the event is logged to the queue processed by background worker to optimize performance.</param>
        internal static EventLogInfo LogEventCore(EventLogInfo eventObject, bool logDirectly)
        {
            try
            {
                using (new EventLoggingContext { EventLoggingInProgress = true })
                {
                    return ProviderObject.LogEventInternal(eventObject, logDirectly);
                }
            }
            catch (Exception ex)
            {
                if (EventLoggingContext.CurrentEventLoggingInProgress)
                {
                    // Unless at the top level of logging, allow exception propagation
                    throw;
                }

                CannotLogEvent(ex);
            }

            return eventObject;
        }


        /// <summary>
        /// Writes a new record to the event log.
        /// </summary>
        /// <param name="eventObject">Contains event</param>
        /// <param name="logDirectly">If true, the event is logged directly to the database. Otherwise, the event is logged to the queue processed by background worker to optimize performance.</param>
        /// <param name="deleteOlder">If true, older items are deleted if the log length exceeds maximum</param>
        [Obsolete("Use member EventLogInfo LogEvent(EventLogInfo) instead.")]
        public static EventLogInfo LogEvent(EventLogInfo eventObject, bool logDirectly = false, bool deleteOlder = false)
        {
            return LogEventCore(eventObject, logDirectly);
        }


        /// <summary>
        /// Writes a new error to the event log.
        /// </summary>
        /// <param name="source">Source of the event (Content, Administration, etc.)</param>
        /// <param name="eventCode">Event code (UPDATEDOC, SENDEMAIL, DELETE, etc.)</param>
        /// <param name="ex">Exception to be logged</param>
        /// <param name="siteId">Current site ID</param>
        /// <param name="additionalMessage">Additional information to the exception message</param>
        /// <param name="loggingPolicy">Logging policy.</param>
        public static void LogException(string source, string eventCode, Exception ex, int siteId = 0, string additionalMessage = null, LoggingPolicy loggingPolicy = null)
        {
            ProviderObject.LogExceptionInternal(source, eventCode, ex, siteId, additionalMessage, loggingPolicy);
        }


        /// <summary>
        /// Writes a new warning to the event log.
        /// </summary>
        /// <param name="source">Source of the event (Content, Administration, etc.)</param>
        /// <param name="eventCode">Event code (UPDATEDOC, SENDEMAIL, DELETE, etc.)</param>
        /// <param name="ex">Exception to be logged</param>
        /// <param name="siteId">Current site ID</param>
        /// <param name="additionalMessage">Additional information to the exception message</param>
        public static void LogWarning(string source, string eventCode, Exception ex, int siteId, string additionalMessage)
        {
            SafelyExecuteLogging(() =>
            {
                var eventObject = new EventLogInfo(EventType.WARNING, source, eventCode)
                {
                    EventDescription = additionalMessage,
                    SiteID = siteId,
                    Exception = ex
                };

                LogEvent(eventObject);
            });
        }


        /// <summary>
        /// Writes a new information to the event log.
        /// </summary>
        /// <param name="source">Source of the event (Content, Administration, etc.)</param>
        /// <param name="eventCode">Event code (Security, Update, Delete, etc.)</param>
        /// <param name="eventDescription">Additional event information</param>
        public static void LogInformation(string source, string eventCode, string eventDescription = "")
        {
            SafelyExecuteLogging(() =>
            {
                var eventObject = new EventLogInfo(EventType.INFORMATION, source, eventCode)
                {
                    EventDescription = eventDescription,
                };

                LogEvent(eventObject);
            });
        }


        /// <summary>
        /// Executes when the event logging throws an error.
        /// </summary>
        /// <param name="ex">Exception thrown upon problem with logging</param>
        private static void CannotLogEvent(Exception ex)
        {
            ProviderObject.CannotLogEventInternal(ex);
        }

        #endregion


        #region "Internal methods"

        /// <summary>
        /// Writes a new record to the event log.
        /// </summary>
        /// <param name="eventObject">Contains event</param>
        /// <param name="logDirectly">If true, the event is logged directly to the database. Otherwise, the event is logged to the queue processed by background worker to optimize performance.</param>
        protected virtual EventLogInfo LogEventInternal(EventLogInfo eventObject, bool logDirectly)
        {
            if (!ProviderObject.CanLogEvent(eventObject, logDirectly))
            {
                return null;
            }

            var eventKey = $"EventLogInfo{{EventType:{eventObject.EventType},Source:{eventObject.Source},EventCode:{eventObject.EventCode}}}";
            using (RecursionControl rc = new RecursionControl($"LogEventInternal({eventKey},{logDirectly},bool)"))
            {
                if (rc.Continue)
                {
                    try
                    {
                        eventObject.EnsureEventData();
                    }
                    catch (EventLoggingRecursionException ex)
                    {
                        AddRecursionInformation(eventObject, ex);
                    }

                    if (!logDirectly)
                    {
                        // Add event to worker queue
                        EventLoggingContext.CurrentLogWorker.Enqueue(eventObject, DatabaseHelper.IsDatabaseAvailable);
                    }
                    else
                    {
                        IncrementEventCounters(eventObject);

                        // Log event directly
                        eventObject = ProviderObject.LogEventInternal(eventObject);

                        IncrementLoggedEventsCount(eventObject.SiteID);
                    }

                    return eventObject;
                }

                throw new EventLoggingRecursionException("A recursive call to EventLogProvider.LogEventInternal has been detected.");
            }
        }


        /// <summary>
        /// Writes a new record to the event log.
        /// </summary>
        /// <param name="eventObject">Contains event</param>
        /// <param name="logDirectly">If true, the event is logged directly to the database. Otherwise, the event is logged to the queue processed by background worker to optimize performance.</param>
        /// <param name="deleteOlder">If true, older items are deleted if the log length exceeds maximum</param>
        [Obsolete("Use member EventLogInfo LogEventInternal(EventLogInfo, bool) instead.")]
        protected virtual EventLogInfo LogEventInternal(EventLogInfo eventObject, bool logDirectly, bool deleteOlder)
        {
            return LogEventInternal(eventObject, logDirectly);
        }


        /// <summary>
        /// Writes a new record to the event log.
        /// </summary>
        /// <param name="eventType">Type of the event. Please use predefined constants from EventLogProvider</param>
        /// <param name="source">Source of the event (Content, Administration, etc.)</param>
        /// <param name="eventCode">Event code (UPDATEDOC, SENDEMAIL, DELETE, etc.)</param>
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
        /// <param name="loggingPolicy">Logging policy.</param>
        protected virtual EventLogInfo LogEventInternal(string eventType, string source, string eventCode, string eventDescription, string eventUrl, int userId, string userName, int nodeId, string documentName, string ipAddress, int siteId, string machineName, string urlReferrer, string userAgent, DateTime? eventTime, LoggingPolicy loggingPolicy)
        {
            try
            {
                var eventObject = new EventLogInfo(eventType, source, eventCode)
                {
                    EventDescription = eventDescription,
                    EventUrl = eventUrl,
                    SiteID = siteId,
                    UserName = userName,
                    NodeID = nodeId,
                    DocumentName = documentName,
                    IPAddress = ipAddress,
                    EventMachineName = machineName,
                    EventUrlReferrer = urlReferrer,
                    EventUserAgent = userAgent,
                    LoggingPolicy = loggingPolicy
                };

                if (eventTime != null)
                {
                    eventObject.EventTime = eventTime.Value;
                }

                return LogEvent(eventObject);
            }
            catch (Exception ex)
            {
                if (EventLoggingContext.CurrentEventLoggingInProgress)
                {
                    // Unless at the top level of logging, allow exception propagation
                    throw;
                }

                CannotLogEvent(ex);
            }

            return null;
        }


        /// <summary>
        /// Writes a new error to the event log.
        /// </summary>
        /// <param name="source">Source of the event (Content, Administration, etc.)</param>
        /// <param name="eventCode">Event code (UPDATEDOC, SENDEMAIL, DELETE, etc.)</param>
        /// <param name="ex">Exception to be logged</param>
        /// <param name="siteId">Current site ID</param>
        /// <param name="additionalMessage">Additional information to the exception message</param>
        /// <param name="loggingPolicy">Logging policy.</param>
        protected virtual void LogExceptionInternal(string source, string eventCode, Exception ex, int siteId, string additionalMessage, LoggingPolicy loggingPolicy)
        {
            SafelyExecuteLogging(() =>
            {
                var eventObject = new EventLogInfo(EventType.ERROR, source, eventCode)
                {
                    EventDescription = additionalMessage,
                    SiteID = siteId,
                    Exception = ex,
                    LoggingPolicy = loggingPolicy
                };

                LogEvent(eventObject);
            });
        }


        /// <summary>
        /// Executes <paramref name="loggingAction"/> while making sure no exception leaks.
        /// </summary>
        private static void SafelyExecuteLogging(Action loggingAction)
        {
            try
            {
                loggingAction();
            }
            catch (Exception ex)
            {
                if (EventLoggingContext.CurrentEventLoggingInProgress)
                {
                    // Unless at the top level of logging, allow exception propagation
                    throw;
                }

                CannotLogEvent(ex);
            }
        }


        /// <summary>
        /// Adds information about recursive logging to <paramref name="eventObject"/>.
        /// </summary>
        private void AddRecursionInformation(EventLogInfo eventObject, EventLoggingRecursionException exception)
        {
            string optionalTypeChangedMessage = null;
            if (eventObject.EventType != EventType.ERROR)
            {
                optionalTypeChangedMessage = String.Format("The type of the event being logged has been changed to '{0}' due to the recursion issue. The original type of the event was '{1}'.{2}{2}",
                    EventType.ERROR, eventObject.EventType, Environment.NewLine);
                eventObject.EventType = EventType.ERROR;
            }

            string newEventDescription = String.Format("A recursive call to event logging procedure has occurred during event logging. See the stack trace of related exception to identify the recursion source:{0}{1}" +
                                    "{0}{0}{2}The original event description follows:{0}{3}", Environment.NewLine, exception, optionalTypeChangedMessage, eventObject.EventDescription);

            eventObject.EventDescription = newEventDescription;
        }


        /// <summary>
        /// Writes a new record to the event log.
        /// </summary>
        /// <param name="eventObject">Contains event</param>
        protected virtual EventLogInfo LogEventInternal(EventLogInfo eventObject)
        {
            using (var h = EventLogEvents.LogEvent.StartEvent(eventObject))
            {
                if (h.CanContinue())
                {
                    // Log event to database
                    if (CheckLogAvailability(MODE_DATABASE))
                    {
                        try
                        {
                            SetEventLogInfo(eventObject);
                        }
                        catch (Exception ex)
                        {
                            if (EventLoggingContext.CurrentEventLoggingInProgress)
                            {
                                // Unless at the top level of logging, allow exception propagation
                                throw;
                            }

                            CannotLogEvent(ex);
                        }
                    }

                    // Log event to file
                    if (CheckLogAvailability(MODE_FILESYSTEM))
                    {
                        EventLogHelper.LogEventToFile(eventObject);
                    }

                    // Log event to trace
                    if (CheckLogAvailability(MODE_TRACE))
                    {
                        LogToTrace(eventObject);
                    }

                    if (UseEventLogPopUp)
                    {
                        LogForEventLogPopUp(eventObject);
                    }

                    SendEmailIfRequired(eventObject);
                }

                h.FinishEvent();
            }

            return eventObject;
        }


        /// <summary>
        /// Returns true if the event can be logged
        /// </summary>
        /// <param name="eventObject">Event object</param>
        /// <param name="logDirectly">Whether the event is being logged directly (meaning not using the thread worker).</param>
        protected virtual bool CanLogEvent(EventLogInfo eventObject, bool logDirectly)
        {
            return
                // Global settings (AppSetting/CMSActionContext/Property setter)
                LoggingEnabled &&
                // Log size is not set to any usable value
                (GetLogSize(eventObject.SiteID) > 0) &&
                // Check EventLogPolicy, if true, event was not registered yet and current event was marked as logged
                // Must be last in the list because the method is marking current event as already logged
                (logDirectly ? eventObject.TryMarkEventAsLogged() : !eventObject.IsLogged());
        }


        /// <summary>
        /// Increments the event counters
        /// </summary>
        /// <param name="eventObject">Event object</param>
        private static void IncrementEventCounters(EventLogInfo eventObject)
        {
            // Increment counter if event type is warning or error
            if (eventObject.EventType == EventType.WARNING)
            {
                Warnings.Increment(null);
            }
            else if (eventObject.EventType == EventType.ERROR)
            {
                Errors.Increment(null);
            }
        }


        /// <summary>
        /// Send email notification about logged event.
        /// </summary>
        /// <param name="ev">Logged event</param>
        private static void SendEmailIfRequired(EventLogInfo ev)
        {
            // Send notification e-mail it error occurs
            if ((ev.EventType == EventType.ERROR) && (!String.Equals(ev.EventCode, "sendemail", StringComparison.OrdinalIgnoreCase)))
            {
                // Get name of site
                string siteName = null;

                // Check whether siteId is set
                if (ev.SiteID > 0)
                {
                    siteName = ProviderHelper.GetCodeName(PredefinedObjectType.SITE, ev.SiteID);
                }

                // Send error notification
                EventLogHelper.SendEmailNotification(ev, siteName);
            }
        }


        /// <summary>
        /// Executes stored procedure for deleting old events of given <paramref name="siteId"/> from event log.
        /// </summary>
        /// <param name="siteId">Site ID.</param>
        /// <param name="logSize">Log size for the given site.</param>
        /// <returns>Number of remaining events of given <paramref name="siteId"/> in event log.</returns>
        private static int DeleteOlderItems(int siteId, int logSize)
        {
            var parameters = new QueryDataParameters();

            parameters.Add("@LogMaxSize", logSize);
            parameters.Add("@SiteId", siteId);
            parameters.Add("@MaxToDelete", MAXTODELETE);
            parameters.Add("@DeleteEventsOfNonexistingSites", mClearEventsOfNonexistingSites);

            // Events that belongs to non-existing site is cleaned only once per lifetime
            mClearEventsOfNonexistingSites = false;

            var remainingEventsCount = ConnectionHelper.ExecuteScalar("Proc_CMS_EventLog_DeleteOlderLogs", parameters, QueryTypeEnum.StoredProcedure);

            return ValidationHelper.GetInteger(remainingEventsCount, 0);
        }


        /// <summary>
        /// Deletes the older items for the given list of <paramref name="siteIds"/>.
        /// </summary>
        /// <param name="siteIds">Site IDs</param>
        /// <returns>True if any item has been deleted, otherwise false.</returns>
        internal static bool DeleteOlderItems(IEnumerable<int> siteIds)
        {
            bool someDeleted = false;

            // Process all given sites
            foreach (var siteId in siteIds)
            {
                var logSize = GetLogSize(siteId);

                // Skip if size of log is not limited
                if (logSize <= 0)
                {
                    continue;
                }

                // Delete older items if size of log is exceeded
                if (GetLogItemsCount(siteId) > (logSize * LogDeleteCoefficient))
                {
                    LogItems[siteId] = DeleteOlderItems(siteId, logSize);
                    someDeleted = true;
                }
            }

            return someDeleted;
        }


        /// <summary>
        /// Returns specified event info object from the event log.
        /// </summary>
        /// <param name="eventId">Event identifier</param>
        protected virtual EventLogInfo GetEventInfoInternal(int eventId)
        {
            return GetObjectQuery().WhereEquals("EventID", eventId).TopN(1).FirstOrDefault();
        }


        /// <summary>
        /// Creates string representation of event log object.
        /// </summary>
        /// <param name="eventLogInfo">EventLogInfo object</param>
        protected virtual string GetEventLogTextInternal(EventLogInfo eventLogInfo)
        {
            // Build the log text
            var sb = new StringBuilder(512);

            if (!String.IsNullOrEmpty(eventLogInfo.EventType))
            {
                sb.AppendLine("Event type: " + eventLogInfo.EventType);
            }
            sb.AppendLine("Event time: " + eventLogInfo.EventTime);

            if (!String.IsNullOrEmpty(eventLogInfo.Source))
            {
                sb.AppendLine("Source: " + eventLogInfo.Source);
            }
            if (!String.IsNullOrEmpty(eventLogInfo.EventCode))
            {
                sb.AppendLine("Event code: " + eventLogInfo.EventCode);
            }
            if (!String.IsNullOrEmpty(eventLogInfo.IPAddress))
            {
                sb.AppendLine("IP address: " + eventLogInfo.IPAddress);
            }
            if (!String.IsNullOrEmpty(eventLogInfo.EventMachineName))
            {
                sb.AppendLine("Machine name: " + eventLogInfo.EventMachineName);
            }
            if (!String.IsNullOrEmpty(eventLogInfo.EventUrl))
            {
                sb.AppendLine("Event URL: " + eventLogInfo.EventUrl);
            }
            if (!String.IsNullOrEmpty(eventLogInfo.EventUrlReferrer))
            {
                sb.AppendLine("URL referrer: " + eventLogInfo.EventUrlReferrer);
            }
            if (!String.IsNullOrEmpty(eventLogInfo.EventUserAgent))
            {
                sb.AppendLine("User agent: " + eventLogInfo.EventUserAgent);
            }
            // Add username and id to one line
            int userID = eventLogInfo.UserID;
            if ((!String.IsNullOrEmpty(eventLogInfo.UserName)) || (userID != 0))
            {
                sb.Append("User: " + eventLogInfo.UserName);
                if ((userID != 0))
                {
                    sb.Append(" (ID: " + userID + ")");
                }
                sb.AppendLine();
            }
            if (eventLogInfo.SiteID != 0)
            {
                sb.AppendLine("Site ID: " + eventLogInfo.SiteID);
            }
            if (eventLogInfo.NodeID != 0)
            {
                sb.AppendLine("Node ID: " + eventLogInfo.NodeID);
            }
            if (!String.IsNullOrEmpty(eventLogInfo.DocumentName))
            {
                sb.AppendLine("Page name: " + eventLogInfo.DocumentName);
            }
            if (!String.IsNullOrEmpty(eventLogInfo.EventDescription))
            {
                sb.AppendLine("Description: " + eventLogInfo.EventDescription);
            }

            return sb.ToString();
        }


        /// <summary>
        /// Returns the previous and next events of the given event in the order specified by ORDER BY parameter matching the WHERE criteria.
        /// </summary>
        /// <param name="eventId">ID of the event relative to which the previous and next events are returned</param>
        /// <param name="where">Where condition</param>
        /// <param name="orderBy">Order by columns</param>
        protected virtual int[] GetPreviousNextInternal(int eventId, string where, string orderBy)
        {
            // Prepare the parameters
            QueryDataParameters parameters = new QueryDataParameters();
            parameters.Add("@EventID", eventId);

            DataSet ds = ConnectionHelper.ExecuteQuery("cms.eventlog.selectpreviousnext", parameters, where, orderBy);
            if (!DataHelper.DataSourceIsEmpty(ds))
            {
                int[] result = new int[2];
                if (ds.Tables[0].Rows.Count == 2)
                {
                    result[0] = ValidationHelper.GetInteger(ds.Tables[0].Rows[0]["EventID"], 0);
                    result[1] = ValidationHelper.GetInteger(ds.Tables[0].Rows[1]["EventID"], 0);
                }
                else
                {
                    int base_rn = ValidationHelper.GetInteger(ds.Tables[0].Rows[0]["BASE_RN"], 0);
                    int rn = ValidationHelper.GetInteger(ds.Tables[0].Rows[0]["RN"], 0);
                    if (rn > base_rn)
                    {
                        result[0] = 0;
                        result[1] = ValidationHelper.GetInteger(ds.Tables[0].Rows[0]["EventID"], 0);
                    }
                    else
                    {
                        result[0] = ValidationHelper.GetInteger(ds.Tables[0].Rows[0]["EventID"], 0);
                        result[1] = 0;
                    }
                }

                return result;
            }

            return null;
        }


        /// <summary>
        /// Clears entire event log and writes new event record with information about this action and the user who performed it.
        /// </summary>
        /// <param name="userId">User ID for logging purposes</param>
        /// <param name="userName">User name for logging purposes</param>
        /// <param name="ipAddress">IP address for logging purposes</param>
        /// <param name="siteId">Site ID for logging purposes</param>
        protected virtual void ClearEventLogInternal(int userId, string userName, string ipAddress, int siteId)
        {
            // Clear counters
            Warnings.Clear();
            Errors.Clear();

            var siteWhere = (siteId >= 0) ? new WhereCondition().WhereID("SiteID", siteId) : null;
            BulkDelete(siteWhere);

            // Write new event information about the log deletion.
            LogEvent(EventType.INFORMATION, "Event log", "CLEARLOG", null, null, userId, userName, 0, null, ipAddress, siteId);
        }


        /// <summary>
        /// Returns the log items count for specified site.
        /// </summary>
        /// <param name="siteId">Site ID.</param>
        /// <remarks>This method get item count straight from database without any caching.</remarks>
        protected virtual int GetLogItemsInternal(int siteId)
        {
            return GetLogItemsCountFromDatabase(siteId);
        }


        /// <summary>
        /// Returns number of logged items in event log for given <paramref name="siteId"/>.
        /// </summary>
        /// <param name="siteId">Site ID.</param>
        /// <remarks>This method uses internal caching mechanism that is not intended for public use.</remarks>
        private static int GetLogItemsCount(int siteId)
        {
            var itemsCount = ValidationHelper.GetInteger(LogItems[siteId], -1);
            if (itemsCount < 0)
            {
                itemsCount = GetLogItemsCountFromDatabase(siteId);
                LogItems[siteId] = itemsCount;
            }

            return itemsCount;
        }


        /// <summary>
        /// Returns the log items count for specified site straight from database.
        /// </summary>
        /// <param name="siteId">Site ID.</param>
        private static int GetLogItemsCountFromDatabase(int siteId)
        {
            return GetEvents().OnSite(siteId).GetCount();
        }


        /// <summary>
        /// Sets event log.
        /// </summary>
        /// <param name="eventLog">Event log object</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="eventLog"/> is null.</exception>
        protected virtual void SetEventLogInfoInternal(EventLogInfo eventLog)
        {
            if (eventLog == null)
            {
                throw new ArgumentNullException("eventLog");
            }

            if (eventLog.EventID > 0)
            {
                eventLog.Generalized.UpdateData();
            }
            else
            {
                eventLog.Generalized.InsertData();
            }

            // Connect object explicitly (needs to be here, because Info is not SynchonizedInfo)
            eventLog.Generalized.Reconnect();
        }


        /// <summary>
        /// Gets the log size for specified site.
        /// </summary>
        /// <param name="siteId">Site ID</param>
        protected virtual int GetLogSizeInternal(int siteId)
        {
            int logSize = ValidationHelper.GetInteger(LogSizes[siteId], -1);
            if (logSize < 0)
            {
                if (siteId > 0)
                {
                    // Get site name
                    string siteName = ProviderHelper.GetCodeName(PredefinedObjectType.SITE, siteId);
                    logSize = SettingsKeyInfoProvider.GetIntValue(siteName + ".CMSLogSize");
                }
                else
                {
                    logSize = SettingsKeyInfoProvider.GetIntValue("CMSLogSize");
                }

                LogSizes[siteId] = logSize;
            }

            return logSize;
        }


        /// <summary>
        /// Gets complete log for exception with message and deep stack trace.
        /// </summary>
        /// <param name="ex">Exception to log</param>
        protected virtual string GetExceptionLogMessageInternal(Exception ex)
        {
            StringBuilder message = new StringBuilder();
            AppendException(message, ex);

            // Add inner exception stack trace
            Exception inner;
            while ((inner = ex.InnerException) != null)
            {
                AppendException(message, inner);

                ex = inner;
            }

            return message.ToString();
        }


        /// <summary>
        /// Appends the exception to the message
        /// </summary>
        /// <param name="message">Message</param>
        /// <param name="ex">Exception to append</param>
        private static void AppendException(StringBuilder message, Exception ex)
        {
            if (message.Length > 0)
            {
                message.AppendLine();
            }

            message.Append(MESSAGE_PREFIX, ex.Message, Environment.NewLine, Environment.NewLine);
            message.Append(EXCEPTION_TYPE_PREFIX, ex.GetType().FullName, Environment.NewLine);

            // Add stack trace
            var stack = ex.StackTrace;

            AppendStackTrace(message, stack);
        }


        /// <summary>
        /// Appends the given stack trace to the result
        /// </summary>
        /// <param name="message">Message</param>
        /// <param name="stack">Stack to append</param>
        private static void AppendStackTrace(StringBuilder message, string stack)
        {
            if (!String.IsNullOrEmpty(stack))
            {
                message.AppendLine(STACK_TRACE_PREFIX);
                message.AppendLine(stack);
            }
        }


        /// <summary>
        /// Clears up hashtables of EventLogProvider.
        /// </summary>
        /// <param name="logTasks">If true, web farm tasks are logged</param>
        protected override void ClearHashtables(bool logTasks)
        {
            base.ClearHashtables(logTasks);

            lock (tableLock)
            {
                LogItems?.Clear();
                LogSizes?.Clear();
            }
        }


        /// <summary>
        /// Registers or unregisters the default event log listener based on <paramref name="isRegistered"/> flag
        /// and logs the corresponding web farm task.
        /// </summary>
        /// <param name="isRegistered">Pass true to register the default event log listener, false to unregister it.</param>
        internal static void ToggleDefaultEventLogListenerRegistration(bool isRegistered)
        {
            ToggleDefaultEventLogListenerRegistrationCore(isRegistered);

            ProviderObject.CreateWebFarmTask(TOGGLE_DEFAULT_EVENT_LOG_LISTENER_ACTION_NAME, isRegistered.ToString());
        }


        /// <summary>
        /// Registers or unregisters the default event log listener based on <paramref name="isRegistered"/> flag.
        /// </summary>
        /// <param name="isRegistered">Pass true to register the default event log listener, false to unregister it.</param>
        private static void ToggleDefaultEventLogListenerRegistrationCore(bool isRegistered)
        {
            if (isRegistered)
            {
                EventLogSourceHelper.RegisterDefaultEventLogListener();
            }
            else
            {
                EventLogSourceHelper.UnregisterDefaultEventLogListener();
            }
        }


        /// <summary>
        /// Runs the processing of specific web farm task for current provider
        /// </summary>
        /// <param name="actionName">Action name</param>
        /// <param name="data">Custom task data</param>
        /// <param name="binary">Binary data</param>
        protected override void ProcessWebFarmTaskInternal(string actionName, string data, byte[] binary)
        {
            if (String.Equals(actionName, TOGGLE_DEFAULT_EVENT_LOG_LISTENER_ACTION_NAME, StringComparison.OrdinalIgnoreCase))
            {
                ToggleDefaultEventLogListenerRegistrationCore(ValidationHelper.GetBoolean(data, false));
            }
            else
            {
                base.ProcessWebFarmTaskInternal(actionName, data, binary);
            }
        }

        #endregion


        #region "Private methods"

        /// <summary>
        /// Checks if logging mode is enabled.
        /// </summary>
        /// <param name="mode">Logging mode</param>
        private bool CheckLogAvailability(string mode)
        {
            switch (mode)
            {
                case MODE_DATABASE:
                    return SettingsKeyInfoProvider.GetBoolValue("CMSLogToDatabase");

                case MODE_FILESYSTEM:
                    return SettingsKeyInfoProvider.GetBoolValue("CMSLogToFileSystem");

                case MODE_TRACE:
                    return SettingsKeyInfoProvider.GetBoolValue("CMSLogToTrace");
            }

            return false;
        }


        /// <summary>
        /// Logs event for eventlog popup
        /// </summary>
        /// <param name="eventLogInfo">EventLogInfo object</param>
        private void LogForEventLogPopUp(EventLogInfo eventLogInfo)
        {
            try
            {
                Trace.Write(MessageId + " " + eventLogInfo.EventType + " " + eventLogInfo.EventCode);
            }
            catch
            {
                // Logging failed, consume exception and carry on
            }
        }


        /// <summary>
        /// Logs event message to Trace.
        /// </summary>
        /// <param name="eventLogInfo">EventLogInfo object</param>
        private void LogToTrace(EventLogInfo eventLogInfo)
        {
            string message = GetEventLogText(eventLogInfo);

            try
            {
                switch (eventLogInfo.EventType)
                {
                    case EventType.ERROR:
                        Trace.TraceError(message);
                        break;

                    case EventType.INFORMATION:
                        Trace.TraceInformation(message);
                        break;

                    case EventType.WARNING:
                        Trace.TraceWarning(message);
                        break;
                }

                // Flush data and close listeners
                Trace.Close();
            }
            catch
            {
                // Logging failed, consume exception and carry on
            }
        }


        /// <summary>
        /// Executes when the event logging throws an error.
        /// </summary>
        /// <param name="ex">Exception thrown upon problem with logging</param>
        protected virtual void CannotLogEventInternal(Exception ex)
        {
            try
            {
                string message = GetExceptionLogMessage(ex);

                Trace.TraceError(message);
            }
            catch
            {
                // All attempts for logging failed, do not propagate this error higher to let the system continue
            }
        }


        /// <summary>
        /// Increment cached number of logged events for given <paramref name="siteId"/>.
        /// </summary>
        private static void IncrementLoggedEventsCount(int siteId)
        {
            LogItems[siteId] = GetLogItemsCount(siteId) + 1;
        }

        #endregion
    }
}