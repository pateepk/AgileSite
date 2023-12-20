using System;
using System.Data;

using CMS;
using CMS.Base;
using CMS.Core;
using CMS.DataEngine;
using CMS.EventLog;
using CMS.Helpers;

[assembly: RegisterObjectType(typeof(EventLogInfo), EventLogInfo.OBJECT_TYPE)]

namespace CMS.EventLog
{
    /// <summary>
    /// EventLogInfo data container class.
    /// </summary>
    [Serializable]
    public class EventLogInfo : AbstractInfo<EventLogInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "cms.eventlog";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(EventLogProvider), OBJECT_TYPE, "CMS.EventLog", "EventID", "EventTime", null, null, null, null, "SiteID", null, null)
        {
            SynchronizationSettings =
            {
                IncludeToSynchronizationParentDataSet = IncludeToParentEnum.None,
                LogSynchronization = SynchronizationTypeEnum.None,
            },
            AllowRestore = false,
            ModuleName = "cms.eventlog",
            SupportsCloning = false,
            ContainsMacros = false,
            SupportsSearch = false,
            SupportsGlobalObjects = true,
            LogEvents = false,
            UpdateTimeStamp = false, // Event time should be the time when the event occurred, not when it is saved to DB

            ImportExportSettings =
            {
                IncludeToExportParentDataSet = IncludeToParentEnum.None,
            }
        };

        #endregion


        #region "Variables"

        private LoggingPolicy mLoggingPolicy;
        private bool mDataEnsured;

        #endregion


        #region "Properties"

        /// <summary>
        /// Name of machine on which event is being logged.
        /// </summary>
        public virtual string EventMachineName
        {
            get
            {
                return GetStringValue("EventMachineName", string.Empty);
            }
            set
            {
                if (String.IsNullOrEmpty(value))
                {
                    ExecuteSilently(() => value = SystemContext.MachineName);
                }
                SetValue("EventMachineName", value);
            }
        }


        /// <summary>
        /// Type of event, use predefined constants from <see cref="EventType" /> (or "I" for Info, "W" for Warning, "E" for Error).
        /// </summary>
        public virtual string EventType
        {
            get
            {
                return GetStringValue("EventType", string.Empty);
            }
            set
            {
                SetValue("EventType", value);
            }
        }


        /// <summary>
        /// IP address.
        /// </summary>
        public virtual string IPAddress
        {
            get
            {
                return GetStringValue("IPAddress", string.Empty);
            }
            set
            {
                if (String.IsNullOrEmpty(value))
                {
                    ExecuteSilently(() => value = RequestContext.UserHostAddress);
                }
                SetValue("IPAddress", value);
            }
        }


        /// <summary>
        /// Time of event.
        /// </summary>
        public virtual DateTime EventTime
        {
            get
            {
                return GetDateTimeValue("EventTime", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("EventTime", value);
            }
        }


        /// <summary>
        /// Site identifier.
        /// </summary>
        public virtual int SiteID
        {
            get
            {
                return GetIntegerValue("SiteID", 0);
            }
            set
            {
                if (value <= 0)
                {
                    SetValue("SiteID", null);
                }
                else
                {
                    SetValue("SiteID", value);
                }
            }
        }


        /// <summary>
        /// URL of event.
        /// </summary>
        public virtual string EventUrl
        {
            get
            {
                return GetStringValue("EventUrl", string.Empty);
            }
            set
            {
                if (String.IsNullOrEmpty(value))
                {
                   ExecuteSilently(() => value = RequestContext.RawURL);
                }
                SetValue("EventUrl", value);
            }
        }


        /// <summary>
        /// Description of event.
        /// </summary>
        public virtual string EventDescription
        {
            get
            {
                return GetStringValue("EventDescription", string.Empty);
            }
            set
            {
                SetValue("EventDescription", value);
            }
        }


        /// <summary>
        /// User name.
        /// </summary>
        public virtual string UserName
        {
            get
            {
                return GetStringValue("UserName", string.Empty);
            }
            set
            {
                SetValue("UserName", value);
            }
        }


        /// <summary>
        /// Name of document.
        /// </summary>
        public virtual string DocumentName
        {
            get
            {
                return GetStringValue("DocumentName", string.Empty);
            }
            set
            {
                SetValue("DocumentName", value);
            }
        }


        /// <summary>
        /// Referrer URL.
        /// </summary>
        public virtual string EventUrlReferrer
        {
            get
            {
                return GetStringValue("EventUrlReferrer", string.Empty);
            }
            set
            {
                if (String.IsNullOrEmpty(value))
                {
                    ExecuteSilently(() => value = RequestContext.URLReferrer);
                }
                SetValue("EventUrlReferrer", value);
            }
        }


        /// <summary>
        /// ID of node.
        /// </summary>
        public virtual int NodeID
        {
            get
            {
                return GetIntegerValue("NodeID", 0);
            }
            set
            {
                if (value <= 0)
                {
                    SetValue("NodeID", null);
                }
                else
                {
                    SetValue("NodeID", value);
                }
            }
        }


        /// <summary>
        /// Code of event.
        /// </summary>
        public virtual string EventCode
        {
            get
            {
                return GetStringValue("EventCode", string.Empty);
            }
            set
            {
                SetValue("EventCode", value);
            }
        }


        /// <summary>
        /// Browser identification.
        /// </summary>
        public virtual string EventUserAgent
        {
            get
            {
                return GetStringValue("EventUserAgent", string.Empty);
            }
            set
            {
                if (String.IsNullOrEmpty(value))
                {
                    ExecuteSilently(() => value = RequestContext.UserAgent);
                }
                SetValue("EventUserAgent", value);
            }
        }


        /// <summary>
        /// Source of event.
        /// </summary>
        public virtual string Source
        {
            get
            {
                return GetStringValue("Source", string.Empty);
            }
            set
            {
                SetValue("Source", value);
            }
        }


        /// <summary>
        /// User identifier.
        /// </summary>
        public virtual int UserID
        {
            get
            {
                return GetIntegerValue("UserID", 0);
            }
            set
            {
                if (value <= 0)
                {
                    SetValue("UserID", null);
                }
                else
                {
                    SetValue("UserID", value);
                }
            }
        }


        /// <summary>
        /// Event identifier.
        /// </summary>
        public virtual int EventID
        {
            get
            {
                return GetIntegerValue("EventID", 0);
            }
            set
            {
                SetValue("EventID", value);
            }
        }


        /// <summary>
        /// Logging policy for this event.
        /// </summary>
        /// <remarks>
        /// <see cref="CMS.Core.LoggingPolicy.DEFAULT"/> is used as default.
        /// </remarks>
        internal LoggingPolicy LoggingPolicy
        {
            get
            {
                return mLoggingPolicy ?? LoggingPolicy.DEFAULT;
            }
            set
            {
                mLoggingPolicy = value ?? LoggingPolicy.DEFAULT;
            }
        }


        /// <summary>
        /// Store Exception caused by Event.
        /// </summary>
        public Exception Exception
        {
            get;
            set;
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty EventLogInfo object.
        /// </summary>
        public EventLogInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new EventLogInfo object from the given DataRow.
        /// </summary>
        public EventLogInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }


        /// <summary>
        /// Constructor - Creates a new EventLogInfo.
        /// </summary>
        /// <param name="eventType">Type of the event. Please use predefined constants from <see cref="EventType"/> class.</param>
        /// <param name="source">Source of the event (Content, Administration, etc.)</param>
        /// <param name="eventCode">Event code (UPDATEDOC, SENDEMAIL, DELETE, etc.)</param>
        public EventLogInfo(string eventType, string source, string eventCode)
            : base(TYPEINFO)
        {
            ValidateParameters(eventType, source, eventCode);

            //Set context properties to default value for this event.
            SetContextProperties();

            EventType = eventType;
            Source = source;
            EventCode = eventCode;
        }


        /// <summary>
        /// Loads the object default data
        /// </summary>
        protected override void LoadDefaultData()
        {
            base.LoadDefaultData();

            EventTime = DateTime.Now;
        }

        #endregion


        #region "GeneralizedInfo properties and methods"

        /// <summary>
        /// Converts permissions enum to permission code name when <see cref="BaseInfo.CheckPermissions"/> is called.
        /// </summary>
        /// <param name="permission">Permissions enum</param>
        protected override string GetPermissionName(PermissionsEnum permission)
        {
            switch (permission)
            {
                case PermissionsEnum.Modify:
                case PermissionsEnum.Create:
                case PermissionsEnum.Delete:
                    return "ClearLog";

                default:
                    return base.GetPermissionName(permission);
            }
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            EventLogProvider.SetEventLogInfo(this);
        }

        #endregion


        #region"Private methods"

        /// <summary>
        /// Validate required parameters.
        /// </summary>
        /// <param name="eventType">Type of the event. Please use predefined constants from EventLogProvider</param>
        /// <param name="source">Source of the event (Content, Administration, etc.)</param>
        /// <param name="eventCode">Event code (UPDATEDOC, SENDEMAIL, DELETE, etc.)</param>
        private void ValidateParameters(string eventType, string source, string eventCode)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (eventCode == null)
            {
                throw new ArgumentNullException(nameof(eventCode));
            }

            ValidateEventType(eventType);
        }


        /// <summary>
        /// Check if eventType is defined in EventType enum.
        /// </summary>
        /// <param name="eventType"></param>
        private void ValidateEventType(string eventType)
        {
            eventType = eventType.ToUpperInvariant();

            if ((eventType != EventLog.EventType.INFORMATION) && (eventType != EventLog.EventType.WARNING) && (eventType != EventLog.EventType.ERROR))
            {
                throw new ArgumentException("Value of eventType parameter is invalid. Only these values are allowed: I = information, E = error, W = warning");
            }
        }


        /// <summary>
        /// Set properties to default value.
        /// </summary>
        private void SetContextProperties()
        {
            // Reading any of the following values can throw exception even if HttpContext is available
            // so that's why silent try/catch has been used.
            ExecuteSilently(() => EventUrl = RequestContext.RawURL);
            ExecuteSilently(() => IPAddress = RequestContext.UserHostAddress);
            ExecuteSilently(() => EventMachineName = SystemContext.MachineName);
            ExecuteSilently(() => EventUserAgent = RequestContext.UserAgent);
            ExecuteSilently(() => EventUrlReferrer = RequestContext.URLReferrer);
        }


        /// <summary>
        /// Executes given action and suppresses any exception (intended for reading properties from HttpContext).
        /// </summary>
        /// <param name="action">Action</param>
        private static void ExecuteSilently(Action action)
        {
            try
            {
                action();
            }
            catch
            {
            }
        }

        #endregion


        #region"Internal methods"

        /// <summary>
        /// Ensures missing data.
        /// </summary>
        /// <remarks>
        /// Triggers <see cref="EventLogEvents.PrepareData"/> event.
        /// </remarks>
        internal void EnsureEventData()
        {
            if (mDataEnsured)
            {
                return;
            }

            // Ensure event time as the time the event occurred
            if (EventTime == DateTimeHelper.ZERO_TIME)
            {
                EventTime = DateTime.Now;
            }

            // Get exception log
            if (Exception != null)
            {
                string exceptionMessage = EventLogProvider.GetExceptionLogMessage(Exception);

                // Add non empty messages only and avoid message duplication
                if (!String.IsNullOrEmpty(exceptionMessage)
                    && (String.IsNullOrEmpty(EventDescription) || !EventDescription.EndsWith(exceptionMessage, StringComparison.InvariantCultureIgnoreCase)))
                {
                    if (!String.IsNullOrEmpty(EventDescription))
                    {
                        EventDescription += Environment.NewLine;
                    }
                    EventDescription += exceptionMessage;
                }
            }

            EventLogEvents.PrepareData.StartEvent(this);

            mDataEnsured = true;
        }

        #endregion
    }
}