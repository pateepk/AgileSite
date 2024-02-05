using System;
using System.Diagnostics;
using System.Linq;
using System.Security;

using CMS.Base;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.IO;

namespace CMS.EventLog
{
    using EventLog = System.Diagnostics.EventLog;

    /// <summary>
    /// Helper class for registering event log source name to Windows event log.
    /// </summary>
    public static class EventLogSourceHelper
    {
        #region "Constants"

        private const string APPLICATION_LOG = "Application";

        // User-friendly name of event log source.
        private const string EVENT_LOG_SOURCE_NAME = "Kentico CMS Event log ({0})";

        #endregion


        #region "Properties"

        /// <summary>
        /// Indicates if default event log trace listener should be used.
        /// </summary>
        internal static bool UseDefaultEventLogTraceListener
        {
            get
            {
                return SettingsKeyInfoProvider.GetBoolValue("CMSUseEventLogListener");
            }
        }

        #endregion


        #region "Public methods"

        /// <summary>
        /// Registers instance to Windows event log.
        /// </summary>
        /// <param name="pathToWebConfig">Path to web.config</param>
        public static void RegisterInstance(string pathToWebConfig)
        {
            string sourceName = GetSourceName(true, pathToWebConfig);
            RegisterSourceName(sourceName);
        }


        /// <summary>
        /// Unregisters instance from Windows event log.
        /// </summary>
        /// <param name="pathToWebConfig">Path to web.config</param>
        public static void UnregisterInstance(string pathToWebConfig)
        {
            string sourceName = GetSourceName(true, pathToWebConfig);
            UnregisterSourceName(sourceName);
        }


        /// <summary>
        /// Checks if instance is registered in Windows event log.
        /// </summary>
        /// <param name="pathToWebConfig">Path to web.config</param>
        public static bool IsInstanceRegistered(string pathToWebConfig)
        {
            string sourceName = GetSourceName(true, pathToWebConfig);
            return IsSourceNameRegistered(sourceName);
        }

        #endregion


        #region "Internal methods"

        /// <summary>
        /// Creates event log registration of application in registry.
        /// </summary>
        /// <param name="sourceName">Event source name (backslash "\" in source name is not allowed).</param>
        internal static void RegisterSourceName(string sourceName)
        {
            if (!IsSourceNameRegistered(sourceName))
            {
                EventLog.CreateEventSource(sourceName, APPLICATION_LOG);
            }
        }
        

        /// <summary>
        /// Removes event log registration of application from registry.
        /// </summary>
        /// <param name="sourceName">Event source name (backslash "\" in source name is not allowed).</param>
        internal static void UnregisterSourceName(string sourceName)
        {
            if (IsSourceNameRegistered(sourceName))
            {
                EventLog.DeleteEventSource(sourceName);
            }
        }


        /// <summary>
        /// Checks if event log source name is registered.
        /// </summary>
        /// <param name="sourceName">Event source name (backslash "\" in source name is not allowed).</param>
        internal static bool IsSourceNameRegistered(string sourceName)
        {
            if (!String.IsNullOrEmpty(sourceName))
            {
                try
                {
                    return EventLog.SourceExists(sourceName);
                }
                catch (SecurityException)
                {
                    // Event source name was not found in Application so it doesn't exist (Exception is thrown if account has no permission to Security category - searched after Application)
                }
            }
            return false;
        }


        /// <summary>
        /// Registers default event log trace listener.
        /// </summary>
        /// <remarks>
        /// Registration is allowed only if <see cref="UseDefaultEventLogTraceListener"/> is true.
        /// </remarks>
        internal static void RegisterDefaultEventLogListener()
        {
            string sourceName = null;
            if (UseDefaultEventLogTraceListener && TryGetSourceName(ref sourceName))
            {
                // Check if application is registered in Windows event viewer
                if (!IsSourceNameRegistered(sourceName))
                {
                    EventLogProvider.LogEvent(EventType.WARNING, "Event log trace listener", "SETUP", ResHelper.GetString("eventlogtracelistener.sourcenotfound"));
                    return;
                }

                var defaultListener = GetListenerBySourceName(sourceName);
                if (defaultListener == null)
                {
                    Trace.Listeners.Add(new EventLogTraceListener(sourceName));
                }
            }

        }


        /// <summary>
        /// Unregisters default event log trace listener.
        /// </summary>
        internal static void UnregisterDefaultEventLogListener()
        {
            string sourceName = null;
            if (TryGetSourceName(ref sourceName))
            {
                var defaultListener = GetListenerBySourceName(sourceName);
                if (defaultListener != null)
                {
                    Trace.Listeners.Remove(defaultListener);
                }
            }
        }


        /// <summary>
        /// Loads application source name from web.config.
        /// </summary>
        /// <param name="pathToWebConfig">Path to web.config.</param>
        /// <param name="checkWebConfigPath">Indicates if web.config path should be checked</param>
        private static string GetSourceName(bool checkWebConfigPath = false, string pathToWebConfig = null)
        {
            string sourceName = null;
            if (!checkWebConfigPath || File.Exists(pathToWebConfig))
            {
                try
                {
                    string applicationName = SystemHelper.ApplicationName;

                    if (applicationName != null)
                    {
                        // Create source name (backslash in name is not allowed)
                        sourceName = String.Format(EVENT_LOG_SOURCE_NAME, applicationName).Replace("\\", "_").Replace("/", "_");
                    }
                }
                catch
                {
                    // Application name is not initialized or method is called from external application without these settings
                    return null;
                }
            }

            return sourceName;
        }
        

        /// <summary>
        /// Tries get source name for current application.
        /// </summary>
        /// <param name="sourceName"></param>
        /// <returns>Returns true if source name was obtained; otherwise false.</returns>
        internal static bool TryGetSourceName(ref string sourceName)
        {
            if (!SystemContext.IsFullTrustLevel)
            {
                return false;
            }

            sourceName = GetSourceName();

            return !String.IsNullOrEmpty(sourceName);
        }


        internal static TraceListener GetListenerBySourceName(string sourceName)
        {
            return Trace.Listeners.Cast<TraceListener>().FirstOrDefault(t => t.Name == sourceName);
        }

        #endregion
    }
}
