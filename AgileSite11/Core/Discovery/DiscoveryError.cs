using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace CMS.Core
{
    /// <summary>
    /// Represents an error that occurred during the assembly or module discovery process.
    /// </summary>
    public class DiscoveryError
    {
        #region "Constants and variables"

        /// <summary>
        /// The name of the event source for the event log.
        /// </summary>
        private const string EVENTSOURCE = "Discovery";


        /// <summary>
        /// The exception that was raised during the discovery process. 
        /// </summary>
        private readonly Exception mException;


        /// <summary>
        /// The full name of the assembly or a path to the assembly file.
        /// </summary>
        private readonly string mAssemblyNameOrFilePath;

        #endregion


        #region "Properties"

        /// <summary>
        /// Gets a value indicating whether the development mode is active.
        /// </summary>
        /// <remarks>
        /// The development mode setting is retrieved using the ConfigurationManager class as the core services might not be available during discovery.
        /// </remarks>
        private bool IsDevelopmentMode
        {
            get
            {
                bool result;
                return Boolean.TryParse(ConfigurationManager.AppSettings["CMSDevelopmentMode"], out result) && result;
            }
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Initializes a new instance of the <see cref="CMS.Core.DiscoveryError"/> class with the assembly name (or a file path) and the exception that was raised during the discovery process.
        /// </summary>
        /// <param name="exception">The exception that was raised during the discovery process.</param>
        /// <param name="assemblyNameOrFilePath">The full name of the assembly (or a file path).</param>
        public DiscoveryError(Exception exception, string assemblyNameOrFilePath)
        {
            mException = exception;
            mAssemblyNameOrFilePath = assemblyNameOrFilePath;
        }

        #endregion


        #region "Public methods"

        /// <summary>
        /// Appends this discovery error to the event log.
        /// </summary>
        /// <remarks>
        /// The contents of the event log entry vary depending on the error type.
        /// </remarks>
        public void LogEvent()
        {
            var description = new StringBuilder();
            description.AppendLine(mException.Message);

            var reflectionTypeLoadException = mException as ReflectionTypeLoadException;
            if (reflectionTypeLoadException != null)
            {
                LogReflectionTypeLoadException(reflectionTypeLoadException, description);
                return;
            }

            var badImageFormatException = mException as BadImageFormatException;
            if (badImageFormatException != null)
            {
                LogBadImageFormatException(description);
                return;
            }

            var fileLoadException = mException as FileLoadException;
            if (fileLoadException != null)
            {
                LogFileLoadException(description);
                return;
            }

            LogException(description);
        }

        #endregion


        #region "Private methods"

        /// <summary>
        /// Appends the specified <see cref="System.Reflection.ReflectionTypeLoadException"/> to the event log.
        /// </summary>
        /// <param name="exception">The <see cref="System.Reflection.ReflectionTypeLoadException"/> to append to the event log.</param>
        /// <param name="description">The description of the event log entry.</param>
        private void LogReflectionTypeLoadException(ReflectionTypeLoadException exception, StringBuilder description)
        {
            description.AppendFormat("Some types from the assembly {0} could not be loaded.", mAssemblyNameOrFilePath).AppendLine();
            description.AppendLine("There can be a referenced assembly missing or there can be a security problem.");
            description.AppendLine("The following problems were reported:").AppendLine();

            // There might me multiple instances of the same problem and there is no need to report them all.
            foreach (var innerException in exception.LoaderExceptions.GroupBy(x => x.Message).Select(x => x.First()))
            {
                description.AppendLine(innerException.Message);
            }
            
            LogEvent("W", description);
        }


        /// <summary>
        /// Appends the specified <see cref="System.BadImageFormatException"/> to the event log.
        /// </summary>
        /// <param name="description">The description of the event log entry.</param>
        private void LogBadImageFormatException(StringBuilder description)
        {
            description.AppendFormat("The file {0} is not an assembly or the assembly was compiled for a later version of the .NET runtime.", mAssemblyNameOrFilePath).AppendLine();
            
            LogEvent("E", description);
        }


        /// <summary>
        /// Appends the specified <see cref="System.IO.FileLoadException"/> to the event log.
        /// </summary>
        /// <param name="description">The description of the event log entry.</param>
        private void LogFileLoadException(StringBuilder description)
        {
            description.AppendFormat("The assembly {0} was found but could not be loaded.", mAssemblyNameOrFilePath).AppendLine();
            description.AppendLine("It might have the same name as the referenced one but different version, culture or public key token.");

            LogEvent("E", description);
        }


        /// <summary>
        /// Appends the specified <see cref="System.Exception"/> to the event log.
        /// </summary>
        /// <param name="description">The description of the event log entry.</param>
        private void LogException(StringBuilder description)
        {
            LogEvent("E", description);
        }


        /// <summary>
        /// Appends this error to the event log using the specified event code and description.
        /// </summary>
        /// <param name="eventCode">The code of the event log entry.</param>
        /// <param name="description">The description of the event log entry.</param>
        private void LogEvent(string eventCode, StringBuilder description)
        {
            if (IsDevelopmentMode)
            {
                description.AppendLine().Append(mException.StackTrace);
            }

            CoreServices.EventLog.LogEvent(eventCode, EVENTSOURCE, mAssemblyNameOrFilePath, description.ToString());
        }

        #endregion
    }

}