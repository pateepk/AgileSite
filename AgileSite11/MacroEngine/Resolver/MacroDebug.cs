using System;
using System.Data;
using System.Text;

using CMS.Core;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Base;

namespace CMS.MacroEngine
{
    /// <summary>
    /// Class providing debug capabilities of macro engine.
    /// </summary>
    public static class MacroDebug
    {
        #region "Variables"

        /// <summary>
        /// If true macros are debugged in the detailed mode.
        /// </summary>
        private static bool? mDetailed;

        private static readonly CMSLazy<DebugSettings> mSettings = new CMSLazy<DebugSettings>(GetDebugSettings);

        #endregion


        #region "Debug properties"

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
        /// Determines whether the debug provides detailed output.
        /// </summary>
        public static bool Detailed
        {
            get
            {
                if (mDetailed == null)
                {
                    mDetailed = SettingsKeyInfoProvider.GetBoolValue("CMSDebugMacrosDetailed");
                }

                return mDetailed.Value;
            }
            set
            {
                mDetailed = value;
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
        /// Debug current request Macros operations.
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
        /// Current indentation of the Macros log.
        /// </summary>
        public static int CurrentLogIndent
        {
            get
            {
                return Settings.CurrentIndent;
            }
            set
            {
                Settings.CurrentIndent = value;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Initializes the MacroDebug
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
            return new DebugSettings("Macros")
            {
                DurationColumn = "Duration",
                LogControl = "~/CMSAdminControls/Debug/MacroLog.ascx"
            };
        }


        /// <summary>
        /// Creates a new table for the security log.
        /// </summary>
        private static DataTable NewLogTable()
        {
            // Create new log table
            DataTable result = new DataTable();
            result.TableName = "MacrosLog";

            var cols = result.Columns;

            cols.Add(new DataColumn("Expression", typeof(string)));
            cols.Add(new DataColumn("Indent", typeof(int)));
            cols.Add(new DataColumn("Result", typeof(string)));
            cols.Add(new DataColumn("Identity", typeof(string)));
            cols.Add(new DataColumn("User", typeof(string)));
            cols.Add(new DataColumn("Context", typeof(string)));
            cols.Add(new DataColumn("Counter", typeof(int)));
            cols.Add(new DataColumn("Time", typeof(DateTime)));
            cols.Add(new DataColumn("Duration", typeof(double)));
            cols.Add(new DataColumn("Error", typeof(bool)));

            return result;
        }


        /// <summary>
        /// Reserves new item for the macro operation.
        /// </summary>
        /// <returns>Returns the DataRow with new log item</returns>
        [HideFromDebugContext]
        public static DataRow ReserveMacroLogItem()
        {
            return LogMacroOperation(null, null, CurrentLogIndent);
        }


        /// <summary>
        /// Sets the log item result.
        /// </summary>
        /// <param name="dr">Macro log item</param>
        /// <param name="expression">Expression</param>
        /// <param name="result">Result</param>
        /// <param name="indent">Indent</param>
        ///  <param name="duration">Duration of the operation</param>
        public static void SetLogItemData(DataRow dr, string expression, object result, int indent, double duration)
        {
            MacroIdentityOption identityOption = null;
            if (expression != null)
            {
                string expr = MacroSecurityProcessor.RemoveSecurityParameters(expression.Trim(), false, null);
                if (expr.StartsWithCSafe("{%"))
                {
                    expr = MacroProcessor.RemoveDataMacroBrackets(expression);
                    dr["Expression"] = "{% " + MacroSecurityProcessor.RemoveMacroSecurityParams(expr, out identityOption) + " %}";
                }
                else
                {
                    dr["Expression"] = MacroSecurityProcessor.RemoveMacroSecurityParams(expr, out identityOption);
                }
            }

            var strResult = result as string;
            if (strResult != null)
            {
                dr["Result"] = MacroSecurityProcessor.RemoveSecurityParameters(strResult, false, null);
            }
            else
            {
                dr["Result"] = result;
            }

            dr["Indent"] = indent;
            dr["Identity"] = identityOption?.IdentityName;
            dr["User"] = identityOption?.UserName;
            dr["Duration"] = duration;
        }


        /// <summary>
        /// Logs the macro operation. Logs the security operation to the file and to current request log for debugging.
        /// </summary>
        /// <param name="expression">Expression</param>
        /// <param name="result">Result</param>
        /// <param name="indent">Indent</param>
        ///  <param name="duration">Duration of the operation</param>
        /// <returns>Returns the DataRow with new log item</returns>
        [HideFromDebugContext]
        public static DataRow LogMacroOperation(string expression, object result, int indent, double duration = 0)
        {
            if (DebugCurrentRequest)
            {
                // Log to the file
                LogToFile(expression, result, indent);

                if (Settings.Enabled)
                {
                    return CurrentRequestLog.LogNewItem(dr =>
                    {
                        if (expression != null)
                        {
                            SetLogItemData(dr, expression, result, indent, duration);
                        }

                        // Context
                        dr["Context"] = DebugHelper.GetContext(Settings.Stack);
                        dr["Counter"] = DebugHelper.GetDebugCounter(true);
                        dr["Time"] = DateTime.Now;

                        DebugEvents.MacroDebugItemLogged.StartEvent(dr);
                    });
                }
            }

            return null;
        }


        /// <summary>
        /// Logs the macro resolve operation to the log file.
        /// </summary>
        /// <param name="expression">Expression</param>
        /// <param name="result">Result of the macro</param>
        /// <param name="indent">Indentation</param>
        public static void LogToFile(string expression, object result, int indent)
        {
            if (Settings.LogToFile)
            {
                var log = Settings.RequestLog;

                if (!String.IsNullOrEmpty(expression))
                {
                    // Log to the request log
                    lock (log)
                    {
                        log.Append("\r\n");
                        log.Append(String.Empty.PadLeft(indent, ' '));
                        log.Append(expression);
                        log.Append(" = ");
                        log.Append(ValidationHelper.GetString(result, ""));
                    }
                }
            }
        }


        /// <summary>
        /// Logs information that <paramref name="expression"/> failed to evaluate for security reasons.
        /// </summary>
        /// <param name="expression">Expression the evaluation of which failed</param>
        /// <param name="userName">User name, or null.</param>
        /// <param name="identityName">Macro identity name, or null.</param>
        /// <param name="effectiveUserName">Effective user name when expression was evaluated under macro identity, or null.</param>
        public static void LogSecurityCheckFailure(string expression, string userName, string identityName, string effectiveUserName)
        {
            string errorMessage;
            if (!String.IsNullOrEmpty(identityName))
            {
                errorMessage = GetIdentityCheckFailureErrorMessage(expression, identityName, effectiveUserName);
            }
            else if (!String.IsNullOrEmpty(userName))
            {
                errorMessage = GetUserCheckFailureErrorMessage(expression, userName);
            }
            else
            {
                errorMessage = GetCheckFailureErrorMessage(expression);
            }
            LogMacroFailure(expression, errorMessage, "CHECKSECURITY");
        }


        private static string GetIdentityCheckFailureErrorMessage(string expression, string identityName, string effectiveUserName)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat("Security check of the expression '{0}' didn't pass. ", expression);
            sb.AppendFormat("The expression was signed by macro identity '{0}' ", identityName);
            if (String.IsNullOrEmpty(effectiveUserName))
            {
                sb.Append("which either does not exist or has no effective user assigned. " +
                          "Either create the macro identity and/or assign it an effective user with proper permissions for the expression evaluation, " +
                          "or remove the signature and re-save the expression by a user with proper permissions or proper macro identity assigned.");
            }
            else
            {
                sb.AppendFormat("and evaluated under effective user '{0}'. " +
                                "Make sure the effective user has proper permissions for the expression evaluation, " +
                                "or remove the signature and re-save the expression by a user with proper permissions or proper macro identity assigned.", effectiveUserName);
            }

            return sb.ToString();
        }


        private static string GetUserCheckFailureErrorMessage(string expression, string userName)
        {
            return String.Format("Security check of the expression '{0}' didn't pass. The expression was signed by user '{1}' which either does not exist or does not have permissions for the expression evaluation. " +
                                 "Create the user and/or assign them proper permissions for the expression evaluation " +
                                 "or remove the signature and re-save the expression by a user with proper permissions or proper macro identity assigned.", expression, userName);
        }


        private static string GetCheckFailureErrorMessage(string expression)
        {
            return String.Format("Security check of the expression '{0}' didn't pass. The signature is not valid.", expression);
        }


        /// <summary>
        /// Logs the failure information.
        /// </summary>
        /// <param name="expression">Expression the evaluation of which failed</param>
        /// <param name="errMessage">Error message to log</param>
        /// <param name="eventLogCode">Event log code</param>
        public static void LogMacroFailure(string expression, string errMessage, string eventLogCode)
        {
            CoreServices.EventLog.LogEvent("E", "MacroResolver", eventLogCode, errMessage);

            // Log the failure to macro debug
            var dr = LogMacroOperation(expression, errMessage, CurrentLogIndent);
            if (dr != null)
            {
                dr["Error"] = true;
            }
        }

        #endregion


        #region "Event handlers"

        /// <summary>
        /// Handles reset of debug settings.
        /// </summary>
        private static void SettingsReset_Execute(object sender, DebugEventArgs e)
        {
            mDetailed = null;
        }

        #endregion
    }
}