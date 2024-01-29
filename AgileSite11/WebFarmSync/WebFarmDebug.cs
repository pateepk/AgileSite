using System;
using System.Data;
using System.Linq;
using System.Text;

using CMS.Base;
using CMS.Core;
using CMS.Helpers;

namespace CMS.WebFarmSync
{
    /// <summary>
    /// Web farm debug methods
    /// </summary>
    public class WebFarmDebug
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
        /// Current request log.
        /// </summary>
        public static RequestLog CurrentRequestLog
        {
            get
            {
                // Create new log if not present
                var logs = DebugContext.CurrentRequestLogs;
                if (logs[Settings] == null)
                {
                    logs[Settings] = logs.CreateLog(NewLogTable(), Settings);
                }

                return logs[Settings];
            }
            set
            {
                DebugContext.CurrentRequestLogs[Settings] = value;
            }
        }


        /// <summary>
        /// Debug current request WebFarm operations.
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
            return new DebugSettings("WebFarm")
            {
                LogControl = "~/CMSAdminControls/Debug/WebFarmLog.ascx"
            };
        }


        /// <summary>
        /// Creates a new table for the web farm log.
        /// </summary>
        private static DataTable NewLogTable()
        {
            // Create new log table
            DataTable result = new DataTable();
            result.TableName = "WebFarmLog";

            var cols = result.Columns;

            cols.Add(new DataColumn("TaskType", typeof(string)));
            cols.Add(new DataColumn("TextData", typeof(string)));
            cols.Add(new DataColumn("Target", typeof(string)));
            cols.Add(new DataColumn("BinaryData", typeof(string)));
            cols.Add(new DataColumn("Context", typeof(string)));

            return result;
        }


        /// <summary>
        /// Logs the web farm operation. Logs the web farm operation to the file and to current request log for debugging.
        /// </summary>
        /// <param name="taskType">Task type</param>
        /// <param name="textData">Task text data</param>
        /// <param name="binaryData">Task binary data</param>
        /// <param name="target">Task target</param>
        [HideFromDebugContext]
        public static DataRow LogWebFarmOperation(string taskType, string textData, BinaryData binaryData, string target)
        {
            if (DebugCurrentRequest)
            {
                // Log to the file
                LogTaskToFile(taskType, textData, DateTime.Now, target);

                if (Settings.Enabled)
                {
                    return CurrentRequestLog.LogNewItem(dr =>
                    {
                        // Prepare parameters string
                        dr["TaskType"] = taskType;
                        dr["TextData"] = textData;
                        if (binaryData != null)
                        {
                            dr["BinaryData"] = DataHelper.GetSizeString(binaryData.Length);
                        }
                        dr["Target"] = target;

                        // Context
                        dr["Context"] = DebugHelper.GetContext(Settings.Stack);
                    });
                }
            }

            return null;
        }


        /// <summary>
        /// Logs the task to the log file.
        /// </summary>
        public static void LogTaskToFile(string taskTypeString, string taskTextData, DateTime taskTime, string taskTarget)
        {
            if (Settings.LogToFile)
            {
                var log = Settings.RequestLog;

                log.Append("\r\n\r\n[", taskTime, "] ", taskTypeString, " : ", taskTarget, "\r\n", taskTextData);
            }
        }

        #endregion
    }
}
