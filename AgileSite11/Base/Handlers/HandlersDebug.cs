using System;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;

namespace CMS.Base
{
    /// <summary>
    /// Handlers debug methods
    /// </summary>
    public class HandlersDebug
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
        /// Debug current request security operations.
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
            return new DebugSettings("Handlers")
                {
                    LogControl = "~/CMSAdminControls/Debug/HandlersLog.ascx"
                };
        }


        /// <summary>
        /// Creates a new table for the security log.
        /// </summary>
        private static DataTable NewLogTable()
        {
            // Create new log table
            DataTable result = new DataTable();
            result.TableName = "HandlersLog";

            var cols = result.Columns;

            cols.Add(new DataColumn("Name", typeof(string)));
            cols.Add(new DataColumn("Important", typeof(bool)));
            cols.Add(new DataColumn("HandlersCalled", typeof(int)));
            cols.Add(new DataColumn("CallPrevented", typeof(bool)));
            cols.Add(new DataColumn("Indent", typeof(int)));
            cols.Add(new DataColumn("Context", typeof(string)));
            cols.Add(new DataColumn("Counter", typeof(int)));
            cols.Add(new DataColumn("Time", typeof(DateTime)));

            return result;
        }


        /// <summary>
        /// Logs the handler operation. Logs the handler operation to the file and to current request log for debugging.
        /// </summary>
        /// <param name="name">Name of the handler or method</param>
        /// <param name="partName">Name of the part of the handler that is called</param>
        /// <returns>Returns the DataRow with new log item</returns>
        public static DataRow StartHandlerOperation(string name, string partName)
        {
            if (DebugCurrentRequest)
            {
                return LogHandlerOperation(name, partName, Settings.CurrentIndent++);
            }

            return null;
        }


        /// <summary>
        /// Sets the log item as important one
        /// </summary>
        /// <param name="dr">Security log item</param>
        public static void SetLogItemImportant(DataRow dr)
        {
            dr["Important"] = true;
        }


        /// <summary>
        /// Logs the handler operation. Logs the handler operation to the file and to current request log for debugging.
        /// </summary>
        /// <param name="method">Called method</param>
        /// <returns>Returns the DataRow with new log item</returns>
        public static DataRow StartHandlerOperation(MethodInfo method)
        {
            if (DebugCurrentRequest)
            {
                return LogHandlerOperation(method, true, Settings.CurrentIndent++);
            }

            return null;
        }


        /// <summary>
        /// Finishes the log item
        /// </summary>
        /// <param name="dr">Log item</param>
        /// <param name="handlersCalled">Number of handlers that were called during the operation</param>
        public static void FinishHandlerOperation(DataRow dr, int handlersCalled = 0)
        {
            if (handlersCalled > 0)
            {
                dr["HandlersCalled"] = handlersCalled;
            }

            Settings.CurrentIndent--;
        }


        /// <summary>
        /// Logs the handler operation. Logs the handler operation to the file and to current request log for debugging.
        /// </summary>
        /// <param name="name">Name of the handler or method</param>
        /// <param name="partName">Name of the part of the handler that is called</param>
        /// <param name="indent">Indentation of the logged record</param>
        /// <returns>Returns the DataRow with new log item</returns>
        public static DataRow LogHandlerOperation(string name, string partName, int indent = -1)
        {
            if (DebugCurrentRequest)
            {
                return LogHandlerOperationInternal(name, partName, indent);
            }

            return null;
        }


        /// <summary>
        /// Logs the handler operation. Logs the handler operation to the file and to current request log for debugging.
        /// </summary>
        /// <param name="name">Name of the handler or method</param>
        /// <param name="partName">Name of the part of the handler that is called</param>
        /// <param name="indent">Indentation of the logged record</param>
        /// <returns>Returns the DataRow with new log item</returns>
        private static DataRow LogHandlerOperationInternal(string name, string partName, int indent)
        {
            if (indent < 0)
            {
                indent = Settings.CurrentIndent;
            }
            
            // Log to the file
            LogToFile(name, partName, indent);

            return CurrentRequestLog.LogNewItem(dr =>
            {
                if (!String.IsNullOrEmpty(partName))
                {
                    name = String.Concat(name, ".", partName);
                }
                dr["Name"] = name;

                // Set indentation
                dr["Indent"] = indent;

                // Context
                dr["Context"] = DebugHelper.GetContext(Settings.Stack);
                dr["Counter"] = DebugHelper.GetDebugCounter(true);
                dr["Time"] = DateTime.Now;
            });
        }


        /// <summary>
        /// Logs the handler operation. Logs the handler operation to the file and to current request log for debugging.
        /// </summary>
        /// <param name="method">Methods called by the handler</param>
        /// <param name="methodExecuted">If true, the method was actually executed</param>
        /// <param name="indent">Indentation of the logged record</param>
        /// <returns>Returns the DataRow with new log item</returns>
        public static DataRow LogHandlerOperation(MethodInfo method, bool methodExecuted, int indent = -1)
        {
            if (DebugCurrentRequest)
            {
                // Get method name
                var name = (method.DeclaringType != null) ? method.DeclaringType.Name : null;
                var partName = method.Name;

                var dr = LogHandlerOperationInternal(name, partName, indent);
                if (dr != null)
                {
                    dr["CallPrevented"] = !methodExecuted;
                }

                return dr;
            }

            return null;
        }


        /// <summary>
        /// Logs the handler operation to the log file.
        /// </summary>
        /// <param name="name">Name of the handler or method</param>
        /// <param name="partName">Name of the part of the handler that is called</param>
        /// <param name="indent">Indentation of the log</param>
        public static void LogToFile(string name, string partName, int indent)
        {
            if (Settings.LogToFile)
            {
                if (!String.IsNullOrEmpty(partName))
                {
                    name = String.Concat(name, ".", partName);
                }

                var log = Settings.RequestLog;

                // Log to the request log
                lock (log)
                {
                    log.Append("\r\n");
                    log.Append(String.Empty.PadLeft(indent, ' '));
                    log.Append(name);
                }
            }
        }

        #endregion
    }
}
