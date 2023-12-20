using System;
using System.Data;
using System.Text;

using CMS.Base;
using CMS.Helpers;

namespace CMS.WebAnalytics
{
    /// <summary>
    /// Analytics debug methods
    /// </summary>
    public class AnalyticsDebug
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
        /// Debug current request Analytics access.
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
            return new DebugSettings("Analytics")
            {
                LogControl = "~/CMSAdminControls/Debug/AnalyticsLog.ascx"
            };
        }


        /// <summary>
        /// Creates a new table for the analytics log.
        /// </summary>
        private static DataTable NewLogTable()
        {
            // Create new log table
            DataTable result = new DataTable();
            result.TableName = "AnalyticsLog";

            var cols = result.Columns;
            cols.Add(new DataColumn("CodeName", typeof(string)));
            cols.Add(new DataColumn("SiteName", typeof(string)));
            cols.Add(new DataColumn("Culture", typeof(string)));
            cols.Add(new DataColumn("ObjectName", typeof(string)));
            cols.Add(new DataColumn("ObjectID", typeof(string)));
            cols.Add(new DataColumn("Count", typeof(int)));
            cols.Add(new DataColumn("Value", typeof(double)));

            cols.Add(new DataColumn("Context", typeof(string)));
            cols.Add(new DataColumn("Counter", typeof(int)));
            cols.Add(new DataColumn("Time", typeof(DateTime)));

            cols.Add(new DataColumn("IP", typeof(string)));
            cols.Add(new DataColumn("UserAgent", typeof(string)));

            return result;
        }


        /// <summary>
        /// Logs the analytics operation. Logs the analytics operation to the file and to current request log for debugging.
        /// </summary>
        /// <param name="codeName">Statistics codename</param>
        /// <param name="siteName">Site name</param>
        /// <param name="culture">Site culture code</param>
        /// <param name="objectName">Object name</param>
        /// <param name="objectId">Object ID</param>
        /// <param name="count">Hit count</param>
        /// <param name="value">Hit value</param>
        [HideFromDebugContext]
        public static void LogAnalyticsOperation(string codeName, string siteName, string culture, string objectName, int objectId, int count, double value)
        {
            if (DebugCurrentRequest)
            {
                // Log to the file
                LogToFile(codeName, siteName, culture, objectName, objectId, count, value);

                if (Settings.Enabled)
                {
                    CurrentRequestLog.LogNewItem(dr =>
                    {
                        // Values
                        dr["CodeName"] = codeName;
                        dr["SiteName"] = siteName;
                        dr["Culture"] = culture;
                        dr["ObjectName"] = objectName;
                        dr["ObjectID"] = objectId;
                        dr["Count"] = count;
                        dr["Value"] = value;

                        // Context
                        dr["Context"] = DebugHelper.GetContext(Settings.Stack);
                        dr["Counter"] = DebugHelper.GetDebugCounter(true);
                        dr["Time"] = DateTime.Now;

                        var cntx = CMSHttpContext.Current;
                        if (cntx != null)
                        {
                            dr["IP"] = RequestContext.UserHostAddress;
                            dr["UserAgent"] = cntx.Request.UserAgent;
                        }

                        DebugEvents.AnalyticsDebugItemLogged.StartEvent(dr);
                    });
                }
            }
        }


        /// <summary>
        /// Logs the analytics operation to the log file.
        /// </summary>
        /// <param name="codeName">Statistics codename</param>
        /// <param name="siteName">Site name</param>
        /// <param name="culture">Site culture code</param>
        /// <param name="objectName">Object name</param>
        /// <param name="objectId">Object ID</param>
        /// <param name="count">Hit count</param>
        /// <param name="value">Hit value</param>
        public static void LogToFile(string codeName, string siteName, string culture, string objectName, int objectId, int count, double value)
        {
            if (Settings.LogToFile)
            {
                // Log the new cache item
                try
                {
                    StringBuilder log = Settings.RequestLog;

                    // Build the log string
                    string ip = String.Empty;
                    string useragent = String.Empty;

                    var cntx = CMSHttpContext.Current;
                    if (cntx != null)
                    {
                        ip = RequestContext.UserHostAddress;
                        useragent = cntx.Request.UserAgent;
                    }

                    log.AppendFormat(
@"
STATISTICS: {0} ({1}, {2}), Count: {5}, Value: {6}
 Object: {3} (ID: {4})
 IP: {7}
 UserAgent:{8}
", 
                        codeName, 
                        siteName, 
                        culture,
                        objectName, 
                        objectId, 
                        count, 
                        value, 
                        ip, 
                        useragent
                    );
                }
                catch
                {
                }
            }
        }

        #endregion
    }
}
