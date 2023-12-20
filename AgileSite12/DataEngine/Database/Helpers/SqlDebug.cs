using System;
using System.Data;
using System.Text;

using CMS.Base;
using CMS.Helpers;

namespace CMS.DataEngine
{
    /// <summary>
    /// SQL debug methods
    /// </summary>
    public class SqlDebug
    {
        #region "Variables"

        /// <summary>
        /// Debug SQL connections?
        /// </summary>
        public static CMSLazy<bool> DebugConnections = new CMSLazy<bool>(() => DebugHelper.GetDebugBoolSetting("CMSDebugSQLConnections"));

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
        /// Debug current request queries.
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

        #endregion


        #region "Methods"

        /// <summary>
        /// Initializes the SqlDebug
        /// </summary>
        internal static void Init()
        {
            DebugHelper.RegisterDebug(Settings);
            DebugEvents.SettingsReset.Execute += SettingsReset_Execute;
        }


        /// <summary>
        /// Gets the debug settings
        /// </summary>
        private static DebugSettings GetDebugSettings()
        {
            return new DebugSettings("SQLQueries")
            {
                SizeColumn = "QueryResultsSize",
                DurationColumn = "QueryDuration",
                FinalizeData = FinalizeData,
                LogControl = "~/CMSAdminControls/Debug/QueryLog.ascx"
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
                DataHelper.MarkDuplicitRows(dt, null, "Duplicit", "QueryText", "QueryName", "QueryParameters", "QueryResults");
            }
        }


        /// <summary>
        /// Creates a new SQL log table.
        /// </summary>
        private static DataTable NewLogTable()
        {
            return DebugHelper.CreateLogTable(
                "QueryLog",
                new DataColumn("IsInformation", typeof(bool)),
                new DataColumn("ConnectionOp", typeof(string)),
                new DataColumn("QueryName", typeof(string)),
                new DataColumn("QueryText", typeof(string)),
                new DataColumn("QueryParameters", typeof(string)),
                new DataColumn("QueryParametersSize", typeof(int)),
                new DataColumn("ConnectionString", typeof(string)),
                new DataColumn("QueryResults", typeof(string)),
                new DataColumn("QueryResultsSize", typeof(int)),
                new DataColumn("QueryStartTime", typeof(DateTime)),
                new DataColumn("QueryEndTime", typeof(DateTime)),
                new DataColumn("QueryDuration", typeof(double)),
                new DataColumn("Context", typeof(string)),
                new DataColumn("Counter", typeof(int))
                );
        }


        /// <summary>
        /// Logs Sql debug information
        /// </summary>
        /// <param name="title">Information title</param>
        /// <param name="text">Information text</param>
        /// <returns>Returns the new log item</returns>
        public static DataRow LogInformation(string title, string text)
        {
            if (DebugCurrentRequest && Settings.Enabled)
            {
                return CurrentRequestLog.LogNewItem(
                    dr =>
                    {
                        // Prepare parameters string
                        if (title != null)
                        {
                            dr["QueryName"] = title;
                        }
                        dr["QueryText"] = text;
                        dr["IsInformation"] = true;

                        // Context
                        dr["Counter"] = DebugHelper.GetDebugCounter(true);

                        DebugEvents.SQLDebugItemLogged.StartEvent(dr);
                    }, 
                    false, 
                    false
                );
            }

            return null;
        }


        /// <summary>
        /// Logs query start. Logs the query to the file and to current request log for debugging.
        /// </summary>
        /// <param name="queryName">Query name</param>
        /// <param name="queryText">Query text</param>
        /// <param name="parameters">Query parameters</param>
        /// <param name="conn">Connection for the query execution</param>
        /// <returns>Returns the new log item</returns>
        [HideFromDebugContext]
        public static void LogQueryStart(string queryName, string queryText, QueryDataParameters parameters, IDataConnection conn)
        {
            // Increment counter
            SqlHelper.RunningQueries.Increment(null);

            if (DebugCurrentRequest)
            {
                // Log query
                LogQueryToFile(queryText, parameters, conn);

                if (Settings.Enabled)
                {
                    CurrentRequestLog.LogNewItem(dr =>
                    {
                        // Prepare parameters string
                        int paramSize;
                        string paramString = SqlHelper.GetParamString(parameters, "\r\n", out paramSize);

                        if (queryName != null)
                        {
                            dr["QueryName"] = queryName;
                        }
                        dr["QueryText"] = queryText;
                        dr["QueryParameters"] = paramString;
                        dr["QueryParametersSize"] = paramSize + queryText.Length;

                        if (conn != null)
                        {
                            dr["ConnectionString"] = conn.ConnectionString;
                        }
                        dr["QueryStartTime"] = DateTime.Now;

                        // Context
                        dr["Context"] = DebugHelper.GetContext(Settings.Stack);
                        dr["Counter"] = DebugHelper.GetDebugCounter(true);

                        dr["IsInformation"] = false;
                    });
                }
            }
        }


        /// <summary>
        /// Logs the end of the query processing.
        /// </summary>
        /// <param name="result">Result</param>
        public static void LogQueryEnd(object result)
        {
            // Decrement counter
            SqlHelper.RunningQueries.Decrement(null);

            string resultsString = null;
            int totalSize = -1;

            if (DebugCurrentRequest)
            {
                // Log query
                LogQueryEndToFile(result, ref resultsString, ref totalSize);

                if (Settings.Enabled)
                {
                    RequestLog log = CurrentRequestLog;
                    DataRow dr = log.CurrentLogItem;
                    if (dr != null)
                    {
                        // End time is now
                        DateTime now = DateTime.Now;
                        dr["QueryEndTime"] = now;

                        // Evaluate the duration
                        DateTime startTime = ValidationHelper.GetDateTime(dr["QueryStartTime"], DateTime.MinValue);
                        if (startTime != DateTime.MinValue)
                        {
                            dr["QueryDuration"] = now.Subtract(startTime).TotalSeconds;
                        }

                        // Add results
                        if (result != null)
                        {
                            if (resultsString == null)
                            {
                                resultsString = SqlHelper.GetResultsString(result, out totalSize);
                            }

                            dr["QueryResults"] = resultsString;
                            dr["QueryResultsSize"] = totalSize;
                        }
                    }

                    log.CurrentLogItem = null;
                    log.CurrentContextItem = null;

                    DebugEvents.SQLDebugItemLogged.StartEvent(dr);
                }
            }
        }


        /// <summary>
        /// Logs the query.
        /// </summary>
        /// <param name="queryText">Query text to log</param>
        /// <param name="parameters">Query parameters</param>
        /// <param name="conn">Connection used for this query</param>
        public static void LogQueryToFile(string queryText, QueryDataParameters parameters, IDataConnection conn)
        {
            if (Settings.LogToFile)
            {
                var log = Settings.RequestLog;

                log.Append("\r\n\r\n[", DateTime.Now.ToString(), "]\r\n", queryText);

                // Add parameters
                int size;

                string paramString = SqlHelper.GetParamString(parameters, "\r\n", out size);
                if (paramString != "")
                {
                    log.Append("\r\n", paramString);
                }
            }
        }


        /// <summary>
        /// Logs the end of the query processing.
        /// </summary>
        /// <param name="result">Result object</param>
        /// <param name="resultsString">Returning the results string</param>
        /// <param name="totalSize">Returning the total size of results</param>
        public static void LogQueryEndToFile(object result, ref string resultsString, ref int totalSize)
        {
            if (Settings.LogToFile)
            {
                // Log the query
                try
                {
                    resultsString = SqlHelper.GetResultsString(result, out totalSize);

                    // Write results to the request log
                    if (resultsString != "")
                    {
                        StringBuilder log = Settings.RequestLog;

                        log.Append("\r\n", resultsString);
                    }
                }
                catch
                {
                    // Do not log the error, this is too low level and could cause stack overflow if underlying logging would also fail
                }
            }
        }


        /// <summary>
        /// Logs the connection operation to the query log.
        /// </summary>
        /// <param name="operation">Connection operation</param>
        /// <param name="allowBeforeQuery">If true, the operation is allowed before the query when the query is open</param>
        /// <param name="conn">Connection around the operation</param>
        [HideFromDebugContext]
        public static void LogConnectionOperation(string operation, bool allowBeforeQuery, IDataConnection conn)
        {
            if (DebugConnections && DebugCurrentRequest)
            {
                // Log query
                LogQueryToFile(operation, null, conn);

                if (Settings.Enabled)
                {
                    var log = CurrentRequestLog;

                    DataRow current = log.CurrentLogItem;
                    var addBeforeLast = ((current != null) && allowBeforeQuery);

                    log.LogNewItem(
                        dr =>
                        {
                            dr["ConnectionOp"] = operation;

                            dr["QueryStartTime"] = DateTime.Now;
                            dr["ConnectionString"] = conn.ConnectionString;

                            // Context
                            dr["Context"] = DebugHelper.GetContext(Settings.Stack);
                            dr["Counter"] = -1;

                            DebugEvents.SQLDebugItemLogged.StartEvent(dr);
                        }, 
                        addBeforeLast, 
                        false
                    );
                }
            }
        }

        #endregion


        #region "Event handlers"

        /// <summary>
        /// Handles reset of debug settings.
        /// </summary>
        private static void SettingsReset_Execute(object sender, DebugEventArgs e)
        {
            DebugConnections.Reset();
        }

        #endregion
    }
}
