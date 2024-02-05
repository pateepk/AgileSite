using System;
using System.Data;

using CMS.Base;

namespace CMS.Helpers
{
    /// <summary>
    /// Security debug methods
    /// </summary>
    public class SecurityDebug
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
                
                return logs[Settings] ?? (logs[Settings] = logs.CreateLog(NewLogTable(), Settings));
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
            return new DebugSettings("Security")
            {
                FinalizeData = FinalizeData,
                LogControl = "~/CMSAdminControls/Debug/SecurityLog.ascx"
            };
        }


        /// <summary>
        /// Finalizes the debug data
        /// </summary>
        /// <param name="dt">Data</param>
        private static void FinalizeData(DataTable dt)
        {
            if (dt == null)
            {
                return;
            }

            if (!dt.Columns.Contains("Duplicit"))
            {
                DataHelper.MarkDuplicitRows(dt, "Indent <= 0", "Duplicit", "UserName", "SecurityOperation", "Resource", "Name", "SiteName");
            }
        }


        /// <summary>
        /// Creates a new table for the security log.
        /// </summary>
        private static DataTable NewLogTable()
        {
            // Create new log table
            DataTable result = null;
            try
            {
                result = new DataTable();    
                result.TableName = "SecurityLog";

                var cols = result.Columns;
                cols.Add(new DataColumn("SecurityOperation", typeof(string)));
                cols.Add(new DataColumn("Indent", typeof(int)));
                cols.Add(new DataColumn("UserName", typeof(string)));
                cols.Add(new DataColumn("Resource", typeof(string)));
                cols.Add(new DataColumn("Name", typeof(string)));
                cols.Add(new DataColumn("Result", typeof(string)));
                cols.Add(new DataColumn("SiteName", typeof(string)));
                cols.Add(new DataColumn("Context", typeof(string)));
                cols.Add(new DataColumn("Counter", typeof(int)));
                cols.Add(new DataColumn("Time", typeof(DateTime)));
                cols.Add(new DataColumn("Important", typeof(bool)));

                return result;
            }
            catch
            {
                if (result != null)
                {
                    result.Dispose();
                }
                throw;
            }
        }


        /// <summary>
        /// Logs the security operation. Logs the security operation to the file and to current request log for debugging.
        /// </summary>
        /// <param name="operation">Operation</param>
        /// <returns>Returns the DataRow with new log item</returns>
        public static DataRow StartSecurityOperation(string operation)
        {
            if (DebugCurrentRequest)
            {
                return LogSecurityOperation(null, operation, null, null, null, null, Settings.CurrentIndent++);
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
        /// Sets the log item result.
        /// </summary>
        /// <param name="dr">Security log item</param>
        /// <param name="userName">User name</param>
        /// <param name="resource">Resource or class name</param>
        /// <param name="name">Permission or UI element name</param>
        /// <param name="result">Result of the check</param>
        /// <param name="siteName">Site name</param>
        public static void FinishSecurityOperation(DataRow dr, string userName, string resource, string name, object result, string siteName)
        {
            if (userName != null)
            {
                dr["UserName"] = userName;
            }
            if (result != null)
            {
                dr["Result"] = result;
            }
            if (resource != null)
            {
                dr["Resource"] = resource;
            }
            if (name != null)
            {
                dr["Name"] = name;
            }
            if (result != null)
            {
                dr["Result"] = result.ToString();
            }
            if (siteName != null)
            {
                dr["SiteName"] = siteName;
            }

            DebugEvents.SecurityDebugItemLogged.StartEvent(dr);

            Settings.CurrentIndent--;
        }


        /// <summary>
        /// Logs the security operation. Logs the security operation to the file and to current request log for debugging.
        /// </summary>
        /// <param name="userName">User name</param>
        /// <param name="operation">Operation</param>
        /// <param name="resource">Resource or class name</param>
        /// <param name="name">Permission or UI element name</param>
        /// <param name="result">Result of the check</param>
        /// <param name="siteName">Site name</param>
        /// <param name="indent">Indentation of the item</param>
        /// <returns>Returns the DataRow with new log item</returns>
        [HideFromDebugContext]
        public static DataRow LogSecurityOperation(string userName, string operation, string resource, string name, object result, string siteName, int indent = -1)
        {
            if (DebugCurrentRequest)
            {
                if (indent < 0)
                {
                    indent = Settings.CurrentIndent;
                }

                // Log to the file
                LogToFile(userName, operation, resource, name, result, siteName, indent);

                if (Settings.Enabled)
                {
                    return CurrentRequestLog.LogNewItem(dr =>
                    {
                        dr["UserName"] = userName;
                        dr["SecurityOperation"] = operation;

                        // Set indentation
                        dr["Indent"] = indent;

                        dr["Resource"] = resource;
                        dr["Name"] = name;
                        if (result != null)
                        {
                            dr["Result"] = result.ToString();
                        }
                        dr["SiteName"] = siteName;

                        // Context
                        dr["Context"] = DebugHelper.GetContext(Settings.Stack);
                        dr["Counter"] = DebugHelper.GetDebugCounter(true);
                        dr["Time"] = DateTime.Now;

                        if (result != null)
                        {
                            DebugEvents.SecurityDebugItemLogged.StartEvent(dr);
                        }
                    });
                }
            }

            return null;
        }


        /// <summary>
        /// Logs the security operation to the log file.
        /// </summary>
        /// <param name="userName">User name</param>
        /// <param name="operation">Operation</param>
        /// <param name="resource">Resource or class name</param>
        /// <param name="name">Permission or UI element name</param>
        /// <param name="result">Result of the check</param>
        /// <param name="siteName">Site name</param>
        /// <param name="indent">Indentation of the item</param>
        public static void LogToFile(string userName, string operation, string resource, string name, object result, string siteName, int indent)
        {
            if (Settings.LogToFile)
            {
                var log = Settings.RequestLog;

                // Log to the request log
                lock (log)
                {
                    log.Append("\r\n");
                    log.Append(String.Empty.PadLeft(indent, ' '));
                    log.Append(operation.ToUpperInvariant());
                    
                    if (!String.IsNullOrEmpty(resource))
                    {
                        log.AppendFormat("({0}, {1})", resource, name);
                    }

                    log.Append(" = "); 
                    log.Append(result);

                    if (!String.IsNullOrEmpty(userName))
                    {
                        log.Append(" [User: ", userName, "]");
                    }
                    if (!String.IsNullOrEmpty(siteName))
                    {
                        log.Append(" [Site: ", siteName, "]");
                    }
                }
            }
        }

        #endregion
    }
}
