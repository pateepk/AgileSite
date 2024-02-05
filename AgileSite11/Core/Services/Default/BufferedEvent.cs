using System;

namespace CMS.Core
{
    /// <summary>
    /// Represents the data container for the buffered events.
    /// </summary>
    internal class BufferedEvent
    {
        #region "Properties"

        /// <summary>
        /// Event type
        /// </summary>
        public readonly string Type;

        /// <summary>
        /// Event source
        /// </summary>
        public readonly string Source;

        /// <summary>
        /// Event code
        /// </summary>
        public readonly string Code;

        /// <summary>
        /// Event description
        /// </summary>
        public readonly string Description;

        /// <summary>
        /// Exception to log
        /// </summary>
        public readonly Exception Exception;

        /// <summary>
        /// Logging policy.
        /// </summary>
        public LoggingPolicy LoggingPolicy
        {
            get;
            set;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="type">Event type</param>
        /// <param name="source">Event source</param>
        /// <param name="code">Event code</param>
        /// <param name="description">Event description</param>
        public BufferedEvent(string type, string source, string code, string description)
        {
            Type = type;
            Source = source;
            Code = code;
            Description = description;
        }


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="source">Event source</param>
        /// <param name="code">Event code</param>
        /// <param name="ex">Exception to log</param>
        public BufferedEvent(string source, string code, Exception ex)
        {
            Source = source;
            Code = code;
            Exception = ex;
        }


        /// <summary>
        /// Logs the buffered event to the event log
        /// </summary>
        public void LogEvent()
        {
            if (Exception == null)
            {
                CoreServices.EventLog.LogEvent(Type, Source, Code, Description);
            }
            else
            {
                CoreServices.EventLog.LogException(Source, Code, Exception, LoggingPolicy);
            }
        }

        #endregion
    }
}