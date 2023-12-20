using System;
using System.Text;

using CMS.Base;
using CMS.Helpers;

namespace CMS.OutputFilter
{
    /// <summary>
    /// Output debug methods
    /// </summary>
    public class OutputDebug
    {
        #region "Variables"

        private static readonly CMSLazy<DebugSettings> mSettings = new CMSLazy<DebugSettings>(GetDebugSettings);

        #endregion


        #region "Properties"

        /// <summary>
        /// Debug settings
        /// </summary>
        public static DebugSettings Settings
        {
            get
            {
                return mSettings.Value;
            }
        }


        /// <summary>
        /// Debug current request output.
        /// </summary>
        public static bool DebugCurrentRequest
        {
            get
            {
                if (Settings.LogOperations)
                {
                    return DebugContext.CurrentRequestSettings[Settings];
                }
                else
                {
                    return false;
                }
            }
            set
            {
                DebugContext.CurrentRequestSettings[Settings] = value;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Gets the debug settings
        /// </summary>
        private static DebugSettings GetDebugSettings()
        {
            return new DebugSettings("Output")
            {
                LogControl = "~/CMSAdminControls/Debug/OutputLog.ascx"
            };
        }


        /// <summary>
        /// Logs the output.
        /// </summary>
        /// <param name="output">Output data</param>
        /// <param name="toFile">Log the output to the file</param>
        /// <param name="toDebug">Log the output to the debug log</param>
        public static void LogOutput(OutputData output, bool toFile, bool toDebug)
        {
            if (DebugCurrentRequest)
            {
                // Log to file
                if (toFile)
                {
                    LogToFile(output.TrimmedHtml);
                }

                if (toDebug && Settings.Enabled)
                {
                    // Add new log
                    var logs = DebugContext.CurrentRequestLogs;

                    var newLog = logs.CreateLog(null, Settings);
                    newLog.Value = output.TrimmedHtml;

                    logs[Settings] = newLog;
                }
            }
        }


        /// <summary>
        /// Logs the query to file.
        /// </summary>
        /// <param name="outputHtml">Output HTML</param>
        public static void LogToFile(string outputHtml)
        {
            if (Settings.LogToFile)
            {
                // Build the log text
                var log = new StringBuilder(512);

                // URL
                log.Append("\r\n\r\n\r\n");

                // Write the response type
                string contentType = CMSHttpContext.Current.Response.ContentType.ToLowerInvariant();

                log.Append((contentType == "text/plain") ? "[AJAX Response: " : "[HTML Response: ");

                // Time and size
                log.Append(" (", DateTime.Now.ToString(), ", ", DataHelper.GetSizeString(outputHtml.Length), ")]\r\n");

                // Output
                log.Append(outputHtml);

                // Log directly to the file
                Settings.WriteToLogFile(DebugContext.CurrentLogFolder, log);
            }
        }

        #endregion
    }
}
