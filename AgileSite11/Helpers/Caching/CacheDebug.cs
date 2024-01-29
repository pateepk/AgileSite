using System;
using System.Data;
using System.Web.Caching;

using CMS.Base;

namespace CMS.Helpers
{
    /// <summary>
    /// Cache debug methods
    /// </summary>
    public class CacheDebug
    {
        #region "Variables"

        private static CMSLazy<DebugSettings> mSettings = new CMSLazy<DebugSettings>(GetDebugSettings);

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
        /// Debug current request cache access.
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
            return new DebugSettings("Cache")
            {
                LogControl = "~/CMSAdminControls/Debug/CacheLog.ascx"
            };
        }


        /// <summary>
        /// Creates a new table for the cache log.
        /// </summary>
        private static DataTable NewLogTable()
        {
            // Create new log table
            DataTable result = null;
            try
            {
                result = new DataTable();
                result.TableName = "CacheLog";

                var cols = result.Columns;
                cols.Add(new DataColumn("CacheOperation", typeof(string)));
                cols.Add(new DataColumn("CacheKey", typeof(string)));
                cols.Add(new DataColumn("CacheValue", typeof(string)));
                cols.Add(new DataColumn("AbsoluteExpiration", typeof(DateTime)));
                cols.Add(new DataColumn("SlidingExpiration", typeof(TimeSpan)));
                cols.Add(new DataColumn("CachePriority", typeof(CacheItemPriority)));
                cols.Add(new DataColumn("Context", typeof(string)));
                cols.Add(new DataColumn("Dependencies", typeof(string)));
                cols.Add(new DataColumn("Counter", typeof(int)));
                cols.Add(new DataColumn("Time", typeof(DateTime)));

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
        /// Logs the cache operation. Logs the cache operation to the file and to current request log for debugging.
        /// </summary>
        /// <param name="operation">Operation</param>
        /// <param name="key">Cache key</param>
        /// <param name="value">Object to cache</param>
        /// <param name="dependencies">Cache dependencies</param>
        /// <param name="absoluteExpiration">Absolute expiration</param>
        /// <param name="slidingExpiration">Sliding expiration</param>
        /// <param name="priority">Priority of the cache item</param>
        /// <param name="useFullKey">If true, full cache key is used</param>
        [HideFromDebugContext]
        public static void LogCacheOperation(string operation, string key, object value, CMSCacheDependency dependencies, DateTime absoluteExpiration, TimeSpan slidingExpiration, CacheItemPriority priority, bool useFullKey)
        {
            if (DebugCurrentRequest)
            {
                // Log to the file
                LogToFile(operation, key, value, absoluteExpiration, slidingExpiration, priority);

                if (Settings.Enabled)
                {
                    CurrentRequestLog.LogNewItem(dr =>
                    {
                        // Prepare parameters string
                        string objectString = DataHelper.GetObjectString(value);

                        dr["CacheOperation"] = operation;

                        if (useFullKey)
                        {
                            key = CacheHelper.GetFullKey(key);
                        }
                        dr["CacheKey"] = key;

                        dr["CacheValue"] = objectString;
                        dr["AbsoluteExpiration"] = absoluteExpiration;
                        dr["SlidingExpiration"] = slidingExpiration;
                        dr["CachePriority"] = priority;

                        // Add dependencies
                        if (dependencies != null)
                        {
                            if (dependencies.CacheKeys != null)
                            {
                                // Build the list of dependencies
                                string dep = null;
                                foreach (string ck in dependencies.CacheKeys)
                                {
                                    if (!String.IsNullOrEmpty(ck))
                                    {
                                        if (dep != null)
                                        {
                                            dep += "\r\n";
                                        }

                                        dep += ck;
                                    }
                                }
                                dr["Dependencies"] = dep;
                            }
                        }

                        // Context
                        dr["Context"] = DebugHelper.GetContext(Settings.Stack);
                        dr["Counter"] = DebugHelper.GetDebugCounter(true);
                        dr["Time"] = DateTime.Now;

                        DebugEvents.CacheDebugItemLogged.StartEvent(dr);
                    });
                }
            }
        }


        /// <summary>
        /// Logs the cache operation to the log file.
        /// </summary>
        /// <param name="operation">Operation</param>
        /// <param name="key">Cache key</param>
        /// <param name="value">Object to cache</param>
        /// <param name="absoluteExpiration">Absolute expiration</param>
        /// <param name="slidingExpiration">Sliding expiration</param>
        /// <param name="priority">Priority of the cache item</param>
        public static void LogToFile(string operation, string key, object value, DateTime absoluteExpiration, TimeSpan slidingExpiration, CacheItemPriority priority)
        {
            if (Settings.LogToFile)
            {
                var log = Settings.RequestLog;

                bool isAdd = (operation == CacheOperation.ADD) || (operation == CacheOperation.ADD_PERSISTENT);

                log.Append("\r\n", operation.ToUpperInvariant(), " ", key);

                if (isAdd)
                {
                    // Expiration
                    if (absoluteExpiration != Cache.NoAbsoluteExpiration)
                    {
                        log.Append("\r\n Expires: ", absoluteExpiration.ToString());
                    }
                    else if (slidingExpiration != Cache.NoSlidingExpiration)
                    {
                        log.Append("\r\n Expires after: ", slidingExpiration.ToString());
                    }

                    log.Append(" (", priority.ToString(), ")\r\n ", DataHelper.GetObjectString(value));
                }
            }
        }

        #endregion
    }
}
