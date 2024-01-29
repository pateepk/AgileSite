using System;
using System.Diagnostics;
using System.IO;

namespace CMS.WinServiceEngine
{
    /// <summary>
    /// Help class for service.
    /// </summary>
    public static class ServiceHelper
    {
        #region "Constants"

        /// <summary>
        /// Prefix of web path service parameter
        /// </summary>
        public const string WEB_PATH_PREFIX = "webpath=";


        /// <summary>
        /// Event Log name
        /// </summary>
        public const string EVENT_LOG_NAME = "Application";

        #endregion


        #region "Methods"

        /// <summary>
        /// Check if directory of web application path exists.
        /// </summary>
        /// <param name="webAppPath">Web application path</param>
        public static void CheckWebApplicationPath(string webAppPath)
        {
            if (!Directory.Exists(webAppPath))
            {
                throw new Exception(string.Format("The directory '{0}' doesn't exist.", webAppPath));
            }
        }


        /// <summary>
        /// Restarts windows service
        /// </summary>
        /// <param name="serviceName">Service name</param>
        public static void RestartService(string serviceName)
        {
            Process process = new Process();
            process.StartInfo.FileName = "cmd";
            process.StartInfo.Arguments = string.Format("/c net stop \"{0}\" & net start \"{1}\"", serviceName, serviceName);
            process.Start();
        }

        #endregion


        #region "Event Log methods"

        /// <summary>
        /// Logs message to the console and windows event log.
        /// </summary>
        /// <param name="source">Event log source</param>
        /// <param name="message">Message</param>
        /// <param name="logToConsole">Indicates if message should be written to the console</param>
        public static void LogMessage(string source, string message, bool logToConsole)
        {
            if (logToConsole)
            {
                // Write to the console
                Console.WriteLine(message);
            }

            EventLog.WriteEntry(source, message, EventLogEntryType.Information);
        }


        /// <summary>
        /// Logs message to the windows event log.
        /// </summary>
        /// <param name="source">Event log source</param>
        /// <param name="message">Message to log</param>
        public static void LogMessage(string source, string message)
        {
            LogMessage(source, message, false);
        }


        /// <summary>
        /// Logs exceptions to the windows event log.
        /// </summary>
        /// <param name="source">Event log source</param>
        /// <param name="ex">Exception to log</param>
        public static void LogException(string source, Exception ex)
        {
            LogException(source, ex, null, false);
        }


        /// <summary>
        /// Logs exception to the windows event log.
        /// </summary>
        /// <param name="source">Event log source</param>
        /// <param name="ex">Exception to log</param>
        /// <param name="generalMessageInConsole">Indicates if general message for exception should be displayed in the console.
        /// More details about occurred exception will be stored in event log.</param>
        public static void LogException(string source, Exception ex, bool generalMessageInConsole)
        {
            if (generalMessageInConsole)
            {
                Console.WriteLine("An error occurred. Please see windows event log for more details.");
            }

            LogException(source, ex, null, false);
        }


        /// <summary>
        /// Logs exceptions to the windows event log.
        /// </summary>
        /// <param name="source">Event log source</param>
        /// <param name="ex">Exception to log</param>
        /// <param name="message">Custom additional message</param>
        /// <param name="logToConsole">Indicates if message should be written to the console</param>
        public static void LogException(string source, Exception ex, string message, bool logToConsole)
        {
            string text = ex.ToString();
            if (!string.IsNullOrEmpty(message))
            {
                text = message + " " + text;
            }

            if (logToConsole)
            {
                // Write to the console
                Console.WriteLine(text);
            }

            EventLog.WriteEntry(source, text, EventLogEntryType.Error);
        }

        #endregion
    }
}