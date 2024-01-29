using System;
using System.Data;
using System.Linq;
using System.Text;
using System.Web;

using CMS.Base;
using CMS.IO;

namespace CMS.Helpers
{
    /// <summary>
    /// Request debug methods
    /// </summary>
    public class RequestDebug
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
        /// Debug current request processing.
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
        /// Creates the settings for this debug
        /// </summary>
        private static DebugSettings GetDebugSettings()
        {
            return new DebugSettings("Requests")
            {
                DurationColumn = "Duration",
                FinalizeData = FinalizeData,
                LogControl = "~/CMSAdminControls/Debug/RequestLog.ascx"
            };
        }


        /// <summary>
        /// Finalizes the log data
        /// </summary>
        /// <param name="dt">Data</param>
        private static void FinalizeData(DataTable dt)
        {
            DebugHelper.EnsureDurationColumn(dt);
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
                result.TableName = "RequestLog";

                var cols = result.Columns;
                cols.Add(new DataColumn("Method", typeof(string)));
                cols.Add(new DataColumn("Parameter", typeof(string)));
                cols.Add(new DataColumn("Time", typeof(DateTime)));
                cols.Add(new DataColumn("Indent", typeof(int)));
                cols.Add(new DataColumn("Counter", typeof(int)));

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
        /// Logs the request operation. Logs the request operation to the file and to current request log for debugging.
        /// </summary>
        /// <param name="method">Method (Operation)</param>
        /// <param name="indent">Log indentation</param>
        /// <param name="parameter">Parameter</param>
        public static void LogRequestOperation(string method, string parameter, int indent)
        {
            if (DebugCurrentRequest)
            {
                // Log to the file
                LogToFile(method, parameter, indent);

                if (Settings.Enabled)
                {
                    CurrentRequestLog.LogNewItem(dr =>
                    {
                        // Prepare parameters string
                        dr["Method"] = method;
                        dr["Parameter"] = parameter;
                        dr["Time"] = DateTime.Now;
                        dr["Indent"] = indent;
                        dr["Counter"] = DebugHelper.GetDebugCounter(true);
                    });
                }
            }
        }


        /// <summary>
        /// Logs the request operation to the log file.
        /// </summary>
        /// <param name="method">Method (Operation)</param>
        /// <param name="parameter">Method parameter</param>
        /// <param name="indent">Indentation</param>
        public static void LogToFile(string method, string parameter, int indent)
        {
            if (Settings.LogToFile)
            {
                var log = Settings.RequestLog;

                // Log to the request log
                lock (log)
                {
                    log.Append("\r\n");
                    log.Append(String.Empty.PadLeft(indent, ' '));
                    log.Append(method);
                    log.Append((parameter != null ? ": " + parameter : null));
                    log.Append(" [");
                    log.Append(DateTime.Now);
                    log.Append("]");
                }
            }
        }


        /// <summary>
        /// Writes the request log to the log file.
        /// </summary>
        public static void WriteRequestLog()
        {
            if (!Settings.LogToFile)
            {
                return;
            }

            try
            {
                // Write the URL
                string url = DebugHelper.GetRequestUrl();

                // Write URLs to separate file
                string file = RequestHelper.LogURLsFile;
                if (file != null)
                {
                    url = DebugHelper.GetRequestUrl(true);
                    File.AppendAllText(file, "\r\n" + url, false);
                }
            }
            catch
            {
            }
        }


        /// <summary>
        /// Logs the current request values.
        /// </summary>
        /// <param name="responseCookies">Log response cookies</param>
        /// <param name="requestCookies">Log request cookies</param>
        /// <param name="requestInfo">Log request information</param>
        public static void LogRequestValues(bool responseCookies, bool requestCookies, bool requestInfo)
        {
            if (DebugCurrentRequest && Settings.Enabled)
            {
                // Get the log
                RequestLog log = CurrentRequestLog;
                if (log.ValueCollections == null)
                {
                    try
                    {
                        log.ValueCollections = new DataSet();

                        // Add response cookies
                        DataTable dt = CookieHelper.GetResponseCookieTable();
                        log.ValueCollections.Tables.Add(dt);

                        // Add request cookies
                        dt = CookieHelper.GetRequestCookieTable();
                        dt.Columns.Remove("Expires");
                        log.ValueCollections.Tables.Add(dt);

                        // Add other useful information
                        dt = new DataTable("Values");
                        dt.Columns.Add(new DataColumn("Name", typeof(string)));
                        dt.Columns.Add(new DataColumn("Value", typeof(string)));
                        log.ValueCollections.Tables.Add(dt);

                        // Add values
                        HttpRequest request = HttpContext.Current.Request;
                        dt.Rows.Add("HttpMethod", request.HttpMethod);
                        dt.Rows.Add("UrlReferrer", request.UrlReferrer);
                        dt.Rows.Add("UserAgent", request.UserAgent);
                        dt.Rows.Add("UserHostAddress", RequestContext.UserHostAddress);

                        if (request.UserLanguages != null)
                        {
                            dt.Rows.Add("UserLanguages", String.Join(";", request.UserLanguages));
                        }

                        // User content
                        if (HttpContext.Current.User != null)
                        {
                            dt.Rows.Add("UserName", RequestContext.UserName);
                        }
                        if (HttpContext.Current.Session != null)
                        {
                            dt.Rows.Add("SessionID", SessionHelper.GetSessionID());
                        }

                        // Server name / instance name
                        if (!String.IsNullOrEmpty(SystemContext.ServerName))
                        {
                            dt.Rows.Add("ServerName", SystemContext.ServerName);
                        }
                    }
                    catch //(Exception ex)
                    {
                    }
                }
            }
        }

        #endregion
    }
}
