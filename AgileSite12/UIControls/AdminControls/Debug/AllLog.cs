using System;
using System.Data;
using System.Web.UI;

using CMS.Base;
using CMS.Base.Web.UI;
using CMS.Helpers;

namespace CMS.UIControls
{
    /// <summary>
    /// Merging log control for debug purposes.
    /// </summary>
    public class AllLog : LogControl
    {
        #region "Properties"

        /// <summary>
        /// Debug settings
        /// </summary>
        public override DebugSettings Settings
        {
            get
            {
                return RequestDebug.Settings;
            }
        }


        /// <summary>
        /// Request logs
        /// </summary>
        public override RequestLogs Logs
        {
            get;
            set;
        }

        #endregion


        #region "Methods"


        /// <summary>
        /// OnPreRender event handler
        /// </summary>
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            ScriptHelper.RegisterDialogScript(Page);
        }


        /// <summary>
        /// Merges tables from all debugs to one table.
        /// </summary>
        /// <param name="logs">List of debug tables</param>
        /// <param name="page">Page object</param>
        /// <param name="showCompleteContext">If true, complete context is shown, otherwise only topmost context is shown</param>
        public DataTable MergeLogs(RequestLogs logs, Page page, bool showCompleteContext)
        {
            var result = CreateNewTable();

            // Go through all the logs and merge the rows
            foreach (var log in logs)
            {
                var dt = log.LogTable;
                if (dt != null)
                {
                    lock (dt)
                    {
                        // Only include debugs with global counter column to be able to sort the result in correct order
                        if (dt.Columns.Contains("Counter"))
                        {
                            log.FinalizeData();

                            bool hasIndentCol = dt.Columns.Contains("Indent");

                            // Get the max size
                            int maxSize = 0;

                            var settings = log.Settings;

                            var sizeColumn = settings.SizeColumn;
                            if (!String.IsNullOrEmpty(sizeColumn))
                            {
                                maxSize = DataHelper.GetMaximumValue<int>(dt, sizeColumn);
                            }

                            // Get the max size
                            double maxDuration = 0;
                            var durationColumn = settings.DurationColumn;
                            if (!String.IsNullOrEmpty(durationColumn))
                            {
                                maxDuration = DataHelper.GetMaximumValue<double>(dt, durationColumn);
                            }

                            foreach (DataRow row in dt.Rows)
                            {
                                int counter = ValidationHelper.GetInteger(row["Counter"], -1);
                                int indentation = 0;

                                if (hasIndentCol)
                                {
                                    indentation = ValidationHelper.GetInteger(row["Indent"], 0);
                                }

                                if ((counter >= 0) && ((dt.TableName.ToLowerCSafe() == "requestlog") || (indentation == 0)))
                                {
                                    DataRow dr = result.NewRow();

                                    dr["DebugType"] = settings.Name;
                                    dr["Information"] = GetInfo(dt, row, maxSize, page);
                                    dr["Context"] = GetContext((dt.Columns.Contains("Context") ? row["Context"] : ""), showCompleteContext);
                                    dr["Result"] = GetResult(dt, row);
                                    dr["Duration"] = GetDurationInfo(dt, row, maxDuration);
                                    dr["TotalDuration"] = GetTotalDurationInfo(dt, row, maxDuration);
                                    dr["Counter"] = row["Counter"];

                                    result.Rows.Add(dr);
                                }
                            }
                        }
                    }
                }
            }

            // Sort the rows according to the counter
            DataView dv = new DataView(result);
            dv.Sort = "Counter";

            // Prepare the index and interval columns 
            for (int i = 0; i < dv.Table.Rows.Count; i++)
            {
                DataRowView drv = dv[i];
                drv["Index"] = GetIndex(dv.Table.TableName, drv.Row, i);
            }

            return dv.ToTable();
        }


        /// <summary>
        /// Creates a new table for all log data
        /// </summary>
        private static DataTable CreateNewTable()
        {
            // Create new log table
            DataTable result = new DataTable();
            result.TableName = "AllLog";

            result.Columns.Add(new DataColumn("Index", typeof(string)));
            result.Columns.Add(new DataColumn("DebugType", typeof(string)));
            result.Columns.Add(new DataColumn("Information", typeof(string)));
            result.Columns.Add(new DataColumn("Context", typeof(string)));
            result.Columns.Add(new DataColumn("Result", typeof(string)));
            result.Columns.Add(new DataColumn("TotalDuration", typeof(string)));
            result.Columns.Add(new DataColumn("Duration", typeof(string)));
            result.Columns.Add(new DataColumn("Counter", typeof(int)));

            return result;
        }


        /// <summary>
        /// Gets the formatted Result column.
        /// </summary>
        /// <param name="table">Table with data (to get the debug type)</param>
        /// <param name="row">Current row</param>
        private static string GetResult(DataTable table, DataRow row)
        {
            string tableName = table.TableName.ToLowerCSafe();

            switch (tableName)
            {
                case "securitylog":
                    return SecurityLog.GetResult(row["Result"], row["Indent"], row["Important"]);

                case "macroslog":
                    return HTMLHelper.HTMLEncode(row["Result"].ToString());

                default:
                    return String.Empty;
            }
        }


        /// <summary>
        /// Gets the formatted TotalDuration column.
        /// </summary>
        /// <param name="table">Table with data (to get the debug type)</param>
        /// <param name="row">Current row</param>
        /// <param name="maxDuration">Max durations for the debug</param>
        private static string GetTotalDurationInfo(DataTable table, DataRow row, double maxDuration)
        {
            string tableName = table.TableName.ToLowerCSafe();

            switch (tableName)
            {
                case "requestlog":
                    return GetDurationStr(row["Duration"]) + "<br />" + GetChart(ValidationHelper.GetDouble(maxDuration, 0), row["Duration"], 0.005, 0, 0);

                default:
                    return "N/A";
            }
        }


        /// <summary>
        /// Gets the formatted Duration column.
        /// </summary>
        /// <param name="table">Table with data (to get the debug type)</param>
        /// <param name="row">Current row</param>
        /// <param name="maxDuration">Max durations for the debug</param>
        private static string GetDurationInfo(DataTable table, DataRow row, double maxDuration)
        {
            string tableName = table.TableName.ToLowerCSafe();

            switch (tableName)
            {
                case "querylog":
                    return GetDurationStr(row["QueryDuration"]) + "<br />" + GetChart(ValidationHelper.GetDouble(maxDuration, 0), row["QueryDuration"], 0.005, 0, 0);

                default:
                    return "N/A";
            }
        }


        /// <summary>
        /// Returns duration string.
        /// </summary>
        /// <param name="duration">Duration</param>
        private static string GetDurationStr(object duration)
        {
            double dur = ValidationHelper.GetDouble(duration, -1);
            if (dur < 0)
            {
                return "N/A";
            }

            return dur.ToString("F3");
        }


        /// <summary>
        /// Gets the formatted Info column.
        /// </summary>
        /// <param name="sourceTable">Table with data (to get the debug type)</param>
        /// <param name="row">Current row</param>
        /// <param name="maxSize">Max size for the debug</param>
        /// <param name="page">Page object</param>
        private static string GetInfo(DataTable sourceTable, DataRow row, int maxSize, Page page)
        {
            string info = "";
            string tableName = sourceTable.TableName.ToLowerCSafe();

            switch (tableName)
            {
                case "fileslog":
                    info += FilesLog.GetFileOperation(row["FileOperation"], row["FileParameters"], true) + "&nbsp;" + FilesLog.GetPath(row["FilePath"]) + FilesLog.GetText(row["FileText"]) +
                            FilesLog.GetSizeAndAccesses(maxSize, row["FileSize"], row["FileAccesses"], true);
                    break;

                case "querylog":
                    info += QueryLog.GetInformation(row["IsInformation"], row["ConnectionString"], row["ConnectionOp"], row["QueryName"], row["QueryText"], row["QueryParameters"], row["QueryParametersSize"], row["QueryResults"], row["QueryResultsSize"], ValidationHelper.GetDouble(maxSize, 0));
                    break;

                case "cachelog":
                    info += "<strong>" + row["CacheOperation"] + "</strong><br />" + CacheLog.GetInformation(page, row["CacheKey"], row["Dependencies"], row["CacheValue"], row["CacheOPERATION"]);
                    break;

                case "macroslog":
                    info += HTMLHelper.HTMLEncode(ValidationHelper.GetString(row["Expression"], "")) + "";
                    break;

                case "requestlog":
                    info += "<strong>" + row["Method"] + "</strong>";
                    break;

                case "securitylog":
                    info += "<strong>" + row["SecurityOperation"] + "</strong>&nbsp;(" + row["UserName"] + ")";
                    if (ValidationHelper.GetString(row["Resource"], "") != "")
                    {
                        info += "<br />" + ResHelper.GetString("SecurityLog.Resource") + ": " + row["Resource"];
                    }
                    if (ValidationHelper.GetString(row["Name"], "") != "")
                    {
                        info += "<br />" + ResHelper.GetString("SecurityLog.Name") + ": " + HTMLHelper.HTMLEncode(ValidationHelper.GetString(row["Name"], ""));
                    }
                    break;

                default:
                    // Output all columns except the system ones for other debugs
                    foreach (DataColumn column in sourceTable.Columns)
                    {
                        if ((column.ColumnName != "Indent") && (column.ColumnName != "Context") && (column.ColumnName != "Counter") && (column.ColumnName != "Duplicit") && (column.ColumnName != "Time"))
                        {
                            string item = ValidationHelper.GetString(row[column.ColumnName], "");
                            if (!string.IsNullOrEmpty(item))
                            {
                                info += "<strong>" + column.ColumnName + "</strong>: " + row[column.ColumnName] + "<br />";
                            }
                        }
                    }
                    break;
            }

            return info;
        }


        /// <summary>
        /// Gets the formatted Index column.
        /// </summary>
        /// <param name="tableName">Table with data (to get the debug type)</param>
        /// <param name="row">Current row</param>
        /// <param name="index">Index of the item</param>
        private static string GetIndex(string tableName, DataRow row, int index)
        {
            switch (tableName.ToLowerCSafe())
            {
                case "fileslog":
                    return index + ((string)FilesLog.GetWarning(row["FileNotClosed"], row["FileOperation"]));

                case "querylog":
                    return index + ((string)QueryLog.GetDuplicity(row["Duplicit"], row["QueryText"]));

                case "securitylog":
                    return index + ((string)SecurityLog.GetDuplicity(row["Duplicit"], row["Indent"]));

                default:
                    return index + "";
            }
        }

        #endregion
    }
}