using System;
using System.Collections.Generic;
using System.Data;

namespace CMS.Base
{
    /// <summary>
    /// Contains the log information for the request.
    /// </summary>
    public class RequestLog
    {
        #region "Properties"

        /// <summary>
        /// Debug settings
        /// </summary>
        public DebugSettings Settings = null;

        /// <summary>
        /// Request GUID.
        /// </summary>
        public Guid RequestGUID = Guid.Empty;

        /// <summary>
        /// Parent logs.
        /// </summary>
        public RequestLogs ParentLogs = null;

        /// <summary>
        /// Logs table.
        /// </summary>
        public DataTable LogTable = null;

        /// <summary>
        /// Request URL.
        /// </summary>
        public string RequestURL = null;

        /// <summary>
        /// Request time.
        /// </summary>
        public DateTime RequestTime = DateTime.MinValue;

        /// <summary>
        /// Currently active log item.
        /// </summary>
        public DataRow CurrentLogItem = null;

        /// <summary>
        /// Currently active context item.
        /// </summary>
        public DataRow CurrentContextItem = null;

        /// <summary>
        /// Value for general single value purposes.
        /// </summary>
        public object Value = null;

        /// <summary>
        /// Value collections for multiple values purposes.
        /// </summary>
        public DataSet ValueCollections = null;

        /// <summary>
        /// Number of items that weren't logged due to exceeding of max log size
        /// </summary>
        public int NotLoggedItems;

        /// <summary>
        /// Current thread stack
        /// </summary>
        public string ThreadStack;

        #endregion


        #region "Methods"

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="parentLogs">Parent logs container</param>
        /// <param name="logTable">DataTable for the log</param>
        /// <param name="settings">Debug settings</param>
        public RequestLog(RequestLogs parentLogs, DataTable logTable, DebugSettings settings)
        {
            Settings = settings;

            ParentLogs = parentLogs;
            RequestGUID = ParentLogs.RequestGUID;
            LogTable = logTable;

            RequestTime = DateTime.Now;
            RequestURL = DebugHelper.GetRequestUrl();
            ThreadStack = DebugHelper.GetThreadStack();
        }


        /// <summary>
        /// Log a new item to the debug. Returns the item Data row if the item was logged
        /// </summary>
        /// <param name="itemSetup">Item setup</param>
        /// <param name="addBeforeLast">If true, the new item is added before the last item</param>
        /// <param name="rememberItem">If true, the item is remembered in context for further editing</param>
        [HideFromDebugContext]
        public DataRow LogNewItem(Action<DataRow> itemSetup, bool addBeforeLast = false, bool rememberItem = true)
        {
            var dt = LogTable;

            lock (dt)
            {
                // Single log cannot exceed the configured limit
                if (dt.Rows.Count >= DebugHelper.MaxLogSize)
                {
                    // Increase not logged items that exceed log size
                    NotLoggedItems++;
                    return null;
                }

                // Create new item
                var dr = dt.NewRow();

                itemSetup(dr);

                // Register the complete item within log
                if (addBeforeLast)
                {
                    dt.Rows.InsertAt(dr, dt.Rows.Count - 1);
                }
                else
                {
                    dt.Rows.Add(dr);
                }

                if (rememberItem)
                {
                    CurrentLogItem = dr;
                }

                return dr;
            }
        }


        /// <summary>
        /// Finalizes the debug data
        /// </summary>
        public void FinalizeData()
        {
            var finalize = Settings.FinalizeData;
            if (finalize != null)
            {
                var dt = LogTable;
                lock (dt)
                {
                    finalize(dt);
                }
            }
        }


        /// <summary>
        /// Registers the log within the log list.
        /// </summary>
        public void Register()
        {
            var lastLogs = Settings.LastLogs;
            var logLength = Settings.LogLength;

            if ((lastLogs != null) && (logLength > 0))
            {
                lock (lastLogs)
                {
                    // Add the log
                    lastLogs.Add(this);

                    // Trim the logs to the maximum length
                    while (lastLogs.Count > logLength)
                    {
                        lastLogs.RemoveAt(0);
                    }
                }
            }
        }


        /// <summary>
        /// Attempts to find the log by the GUID.
        /// </summary>
        /// <param name="logs">Logs to search</param>
        /// <param name="guid">Guid to search</param>
        public static RequestLog FindByGUID(IEnumerable<RequestLog> logs, Guid guid)
        {
            lock (logs)
            {
                // Search all logs
                foreach (var log in logs)
                {
                    if ((log != null) && (log.RequestGUID == guid))
                    {
                        return log;
                    }
                }
            }

            return null;
        }

        #endregion
    }
}