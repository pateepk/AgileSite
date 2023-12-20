using System;
using System.Data;
using System.Text;
using System.Web.UI;

using CMS.Base;
using CMS.Base.Web.UI;
using CMS.Helpers;
using CMS.PortalEngine;

namespace CMS.UIControls
{
    /// <summary>
    /// Logs control base class.
    /// </summary>
    public class LogControl : CMSUserControl
    {
        #region "Variables"

        /// <summary>
        /// Request log
        /// </summary>
        private RequestLog mLog;
        
        /// <summary>
        /// Index
        /// </summary>
        protected int index = 0;

        /// <summary>
        /// Last context information to check repetition
        /// </summary>
        private string lastContext;

        private RequestLogs mLogs;

        #endregion


        #region "Properties"

        /// <summary>
        /// Info message
        /// </summary>
        public string InfoMessage
        {
            get;
            set;
        }


        /// <summary>
        /// If true, the log initiates itself from the request guid query parameter
        /// </summary>
        public bool InitFromRequest
        {
            get;
            set;
        }


        /// <summary>
        /// Debug settings for this particular log
        /// </summary>
        public virtual DebugSettings Settings
        {
            get
            {
                throw new NotImplementedException();
            }
        }


        /// <summary>
        /// Request logs
        /// </summary>
        public virtual RequestLogs Logs
        {
            get
            {
                return mLogs ?? (mLogs = DebugContext.CurrentRequestLogs);
            }
            set
            {
                mLogs = value;
                mLog = (value != null) ? value[Settings] : null;
            }
        }


        /// <summary>
        /// Previous log
        /// </summary>
        public RequestLog PreviousLog
        {
            get;
            set;
        }


        /// <summary>
        /// Debug log.
        /// </summary>
        public virtual RequestLog Log
        {
            get
            {
                if ((mLog == null) && Settings.Enabled)
                {
                    mLog = GetCurrentLog();
                }

                return mLog;
            }
            set
            {
                mLog = value;
            }
        }


        /// <summary>
        /// If true, the control displays it's header
        /// </summary>
        public bool DisplayHeader
        {
            get;
            set;
        }


        /// <summary>
        /// Total duration.
        /// </summary>
        public double TotalDuration
        {
            get;
            set;
        }


        /// <summary>
        /// Maximum duration.
        /// </summary>
        public double MaxDuration
        {
            get;
            set;
        }


        /// <summary>
        /// Maximum size.
        /// </summary>
        public double MaxSize
        {
            get;
            set;
        }


        /// <summary>
        /// Total size.
        /// </summary>
        public long TotalSize
        {
            get;
            set;
        }


        /// <summary>
        /// If true, the context is always shown as complete.
        /// </summary>
        public bool ShowCompleteContext
        {
            get;
            set;
        }


        /// <summary>
        /// Header text
        /// </summary>
        public string HeaderText
        {
            get;
            set;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Constructor
        /// </summary>
        public LogControl()
        {
            DisplayHeader = true;

            Load += LogControl_Load;
        }


        /// <summary>
        /// Load event handler
        /// </summary>
        protected void LogControl_Load(object sender, EventArgs e)
        {
            EnableViewState = false;
            Visible = true;
        }


        /// <summary>
        /// Gets the current log from the context
        /// </summary>
        protected virtual RequestLog GetCurrentLog()
        {
            if (Settings.Live && PortalContext.ViewMode.IsLiveSite())
            {
                return Logs[Settings];
            }

            return null;
        }


        /// <summary>
        /// Binds the grid with the data
        /// </summary>
        /// <param name="grid">Grid control</param>
        /// <param name="dt">Data</param>
        protected void BindGrid(UIGridView grid, object dt)
        {
            lock (dt)
            {
                grid.DataSource = dt;
                grid.DataBind();
            }
        }


        /// <summary>
        /// Gets the log data for the current log
        /// </summary>
        protected DataTable GetLogData()
        {
            DataTable logData = null;

            var log = Log;

            // Load the security log
            if (InitFromRequest)
            {
                if (SystemContext.DevelopmentMode)
                {
                    Guid requestGuid = QueryHelper.GetGuid("requestguid", Guid.Empty);

                    var logs = DebugHelper.FindRequestLogs(requestGuid);
                    if (logs != null)
                    {
                        Log = log = logs[Settings];
                    }
                }

                if (log == null)
                {
                    return null;
                }
            }

            if (log != null)
            {
                // Get the log table
                var dt = log.LogTable;

                if (!DataHelper.DataSourceIsEmpty(dt))
                {
                    // Finalize the data
                    log.FinalizeData();

                    // Get max duration and size
                    var durationColumn = Settings.DurationColumn;

                    if (!String.IsNullOrEmpty(durationColumn))
                    {
                        MaxDuration = DataHelper.GetMaximumValue<double>(dt, durationColumn);
                    }

                    var sizeColumn = Settings.SizeColumn;

                    if (!String.IsNullOrEmpty(sizeColumn))
                    {
                        MaxSize = DataHelper.GetMaximumValue<int>(dt, sizeColumn);
                    }


                    logData = dt;
                }
            }

            return logData;
        }


        /// <summary>
        /// PreRender event handler
        /// </summary>
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            if (Visible)
            {
                ScriptHelper.RegisterClientScriptBlock(this, typeof(string), "LogControl", ScriptHelper.GetScript(
@"
function LC_Toggle(e) {
    if (e.show == null)
    {
        e.show = (e.nextSibling.style.display != 'none'); 
    }
    e.show = !e.show;
    e.nextSibling.style.display = (e.show ? 'block' : 'none'); 
    return false;
}
"
                ));

                CssRegistration.RegisterCssLink(Page, "Design", "Debug.css");
            }
        }


        /// <summary>
        /// Renders the control
        /// </summary>
        /// <param name="writer">Writer for the output</param>
        protected override void Render(HtmlTextWriter writer)
        {
            var log = Log;

            writer.Write("<div class=\"debug-log\">");

            var header = DisplayHeader && !String.IsNullOrEmpty(HeaderText);
            if (header)
            {
                writer.Write("<a href=\"#\" class=\"debug-log-info\" onclick=\"return LC_Toggle(this);\">{0}</a><div>", HeaderText);
            }

            if ((log != null) && !DisplayHeader)
            {
                var requestGuid = log.RequestGUID;

                // Start anchor
                writer.Write(@"<a name=""{0}_start"" class=""header-actions-anchor""></a>", requestGuid);

                // Start envelope
                writer.Write("<div>&lrm;");

                RenderNavigation(writer);

                // Request info
                writer.Write(String.Concat("<strong>", GetRequestLink(log.RequestURL, requestGuid), "</strong> (", log.RequestTime.ToString("hh:mm:ss"), ")&lrm;<br /><br />"));

                RenderThreadStack(writer);

                base.Render(writer);

                RenderNotLogged(writer);

                // End envelope
                writer.Write("</div><br /><br />");

                // End anchor
                writer.Write(@"<a name=""{0}_end"" class=""header-actions-anchor""></a>", requestGuid);
            }
            else
            {
                base.Render(writer);

                RenderNotLogged(writer);
            }

            if (header)
            {
                writer.Write("</div>");
            }

            writer.Write("</div>");
        }


        private void RenderNavigation(HtmlTextWriter writer)
        {
            var requestGuid = Log.RequestGUID;

            // Next item link
            writer.Write(@"<a href=""#{0}_end"" title=""{1}"" class=""debug-log-nav""><i class=""icon-chevron-double-down"" aria-hidden=""true""></i><span class=""sr-only"">{1}</span></a>", requestGuid, GetString("Debug.NextEntry"));

            // Previous item link
            if (PreviousLog != null)
            {
                writer.Write(@"<a href=""#{0}_start"" title=""{1}"" class=""debug-log-nav""><i class=""icon-chevron-double-up"" aria-hidden=""true""></i><span class=""sr-only"">{1}</span></a>", PreviousLog.RequestGUID, GetString("Debug.PreviousEntry"));
            }
        }


        private void RenderThreadStack(HtmlTextWriter writer)
        {
            // Write thread stack
            var threadStack = Log.ThreadStack;

            if (!String.IsNullOrEmpty(threadStack))
            {
                var stack = HTMLHelper.EnsureHtmlLineEndings(threadStack.Trim());

                writer.Write("<div>{0}</div><br />", stack);
            }
        }


        private void RenderNotLogged(HtmlTextWriter writer)
        {
            if (Log.NotLoggedItems > 0)
            {
                writer.Write("<div>{0}</div>", ResHelper.GetStringFormat("Debug.MoreItems", Log.NotLoggedItems, DebugHelper.MaxLogSize));
            }
        }


        /// <summary>
        /// Gets the link to the request view.
        /// </summary>
        /// <param name="url">Request URL</param>
        /// <param name="guid">Request GUID</param>
        protected string GetRequestLink(string url, Guid guid)
        {
            var viewUrl = GetLogsUrl(guid);

            return String.Format("<a target=\"_blank\" href=\"{0}\" title=\"{1}\">{2}</a>", 
                viewUrl, HTMLHelper.EncodeForHtmlAttribute(url), HTMLHelper.HTMLEncode(TextHelper.LimitLength(url, 200)));
        }


        /// <summary>
        /// Gets the URL
        /// </summary>
        /// <param name="guid">Request GUID</param>
        public static string GetLogsUrl(Guid guid)
        {
            return UrlResolver.ResolveUrl("~/CMSModules/System/Debug/System_ViewRequest.aspx?guid=") + guid;
        }

        #endregion


        #region "Item rendering methods"

        /// <summary>
        /// Gets the item index
        /// </summary>
        protected int GetIndex()
        {
            return ++index;
        }


        /// <summary>
        /// Gets the item index for the items with the indent 0
        /// </summary>
        /// <param name="ind">indent</param>
        protected string GetIndex(object ind)
        {
            int indent = ValidationHelper.GetInteger(ind, 0);
            if (indent == 0)
            {
                return (++index).ToString();
            }
            else
            {
                return null;
            }
        }


        /// <summary>
        /// Gets the item index for the items with the important flag
        /// </summary>
        /// <param name="imp">Important flag</param>
        protected string GetIndexImportant(object imp)
        {
            var important = ValidationHelper.GetBoolean(imp, false);
            if (important)
            {
                return (++index).ToString();
            }
            else
            {
                return null;
            }
        }


        /// <summary>
        /// Gets the item text, makes the text bold if important
        /// </summary>
        /// <param name="text">Text to render</param>
        /// <param name="imp">If true, the item is important</param>
        protected object GetText(object text, object imp)
        {
            var important = ValidationHelper.GetBoolean(imp, false);
            if (important)
            {
                return String.Concat("<strong>", text, "</strong>");
            }
            else
            {
                return text;
            }
        }


        /// <summary>
        /// Gets the warning icon.
        /// </summary>
        /// <param name="condition">Condition, if met, the warning is displayed</param>
        /// <param name="title">Icon title</param>
        public static object GetWarning(object condition, object title)
        {
            if (ValidationHelper.GetBoolean(condition, false))
            {
                return UIHelper.GetAccessibleIconTag("icon-exclamation-triangle", title.ToString(), FontIconSizeEnum.NotDefined, "warning-icon");
            }

            return null;
        }


        /// <summary>
        /// Gets the warning icon.
        /// </summary>
        /// <param name="title">Icon title</param>
        public static object GetWarning(object title)
        {
            return GetWarning(true, title);
        }


        /// <summary>
        /// Gets the duplicity icon.
        /// </summary>
        /// <param name="duplicit">If true, the duplicity sign is shown</param>
        /// <param name="title">Icon title</param>
        public static object GetDuplicity(object duplicit, object title)
        {
            return GetWarning(duplicit, title);
        }

        
        /// <summary>
        /// Gets the rendered context.
        /// </summary>
        /// <param name="context">Context log</param>
        protected virtual string GetContext(object context)
        {
            return GetContext(context, ShowCompleteContext);
        }


        /// <summary>
        /// Gets the rendered context.
        /// </summary>
        /// <param name="context">Context log</param>
        /// <param name="imp">If true, the context is given only if the item is important</param>
        protected virtual string GetContext(object context, object imp)
        {
            var important = ValidationHelper.GetBoolean(imp, false);
            if (important)
            {
                return GetContext(context, ShowCompleteContext);
            }

            return null;
        }


        /// <summary>
        /// Gets the rendered context.
        /// </summary>
        /// <param name="context">Context log</param>
        /// <param name="showCompleteContext">If true, complete context is shown, otherwise only topmost context is shown</param>
        protected string GetContext(object context, bool showCompleteContext)
        {
            string stringContext = ValidationHelper.GetString(context, "");

            // Do not repeat context if the same as the previous context rendered
            if (stringContext == lastContext)
            {
                return "";
            }

            lastContext = stringContext;

            // Split the context into lines
            var cntx = stringContext.Split(new[]
            {
                '\r',
                '\n'
            }, StringSplitOptions.RemoveEmptyEntries);

            if (cntx.Length <= 1)
            {
                return stringContext;
            }

            // Build the context
            var sb = new StringBuilder(stringContext.Length + 200);

            bool first = true;

            for (int i = cntx.Length - 1; i >= 0; i--)
            {
                string item = HTMLHelper.HTMLEncode(cntx[i]);
                if (first)
                {
                    sb.Append("<a href=\"#\" onclick=\"return LC_Toggle(this);\">", item, "</a>");

                    sb.Append(showCompleteContext ? "<div class=\"debug-log-context\">" : "<div class=\"debug-log-context\" style=\"display: none;\">");
                }
                else
                {
                    sb.Append(item, "<br />");
                }
                first = false;
            }

            sb.Append("</div>");

            return sb.ToString();
        }


        /// <summary>
        /// Gets the query duration.
        /// </summary>
        protected virtual string GetDuration(object duration)
        {
            double dur = ValidationHelper.GetDouble(duration, -1);
            if (dur < 0)
            {
                return "N/A";
            }

            TotalDuration += dur;

            return dur.ToString("F3");
        }


        /// <summary>
        /// Gets the effectiveness chart.
        /// </summary>
        /// <param name="maxValue">Maximum value</param>
        /// <param name="value">Chart value</param>
        /// <param name="valueOk">Value which is still considered OK even if it has high percentage</param>
        /// <param name="height">Height of the chart</param>
        /// <param name="width">Width of the chart</param>
        public static string GetChart(object maxValue, object value, double valueOk, int width, int height)
        {
            double maxVal = ValidationHelper.GetDouble(maxValue, -1);
            double val = ValidationHelper.GetDouble(value, -1);

            if ((val < 0) || (maxVal <= 0))
            {
                return "";
            }

            int percent = (int)((val * 100) / maxVal);

            // Always leave some space (5%)
            if (percent < 5)
            {
                percent = 5;
            }
            else if (percent > 95)
            {
                percent = 95;
            }

            string cls;

            if ((percent < 25) || (val <= valueOk))
            {
                // Green - OK
                cls = "good";
            }
            else if (percent > 75)
            {
                // Red - Bad
                cls = "bad";
            }
            else
            {
                // Orange - Warning
                cls = "warn";
            }

            // Prepare styles
            string style = null;
            string innerStyle = null;

            if (width > 0)
            {
                style += String.Format("width: {0}px;", width);
            }
            if (height > 0)
            {
                innerStyle = String.Format("height: {0}px;", height);
                style += innerStyle;
            }
            if (!String.IsNullOrEmpty(style))
            {
                style = String.Format(" style=\"{0}\"", style);
            }

            string code = String.Format(
                "<div class=\"debug-log-chart debug-log-chart-{0}\"{1}><div class=\"debug-log-chart-inner\" style=\"width: {2}%; {3}\">&nbsp;</div></div>",
                cls,
                style,
                percent,
                innerStyle
            );

            return code;
        }


        /// <summary>
        /// Gets the duration chart.
        /// </summary>
        /// <param name="duration">Duration value</param>
        /// <param name="valueOk">Value which is still considered OK even if it has high percentage</param>
        /// <param name="height">Height of the chart</param>
        /// <param name="width">Width of the chart</param>
        protected virtual string GetDurationChart(object duration, double valueOk, int width, int height)
        {
            return GetChart(MaxDuration, duration, valueOk, width, height);
        }


        /// <summary>
        /// Gets the size chart.
        /// </summary>
        /// <param name="size">Size value</param>
        /// <param name="valueOk">Value which is still considered OK even if it has high percentage</param>
        /// <param name="height">Height of the chart</param>
        /// <param name="width">Width of the chart</param>
        protected virtual string GetSizeChart(object size, double valueOk, int width, int height)
        {
            return GetChart(MaxSize, size, valueOk, width, height);
        }

        
        /// <summary>
        /// Gets colored yes/no
        /// </summary>
        /// <param name="value">Value</param>
        protected string ColourYesNo(object value)
        {
            string str = ValidationHelper.GetString(value, String.Empty);
            string[] values = str.Split(new [] { '\n' }, StringSplitOptions.RemoveEmptyEntries);

            str = String.Empty;

            foreach (string val in values)
            {
                str += UniGridFunctions.ColoredSpanYesNoReversed(val) + "<br />";
            }

            return str;
        }

        
        /// <summary>
        /// Gets the begin of the indentation string
        /// </summary>
        /// <param name="ind">Indentation</param>
        protected string GetBeginIndent(object ind)
        {
            int indent = ValidationHelper.GetInteger(ind, 0);
            string result = "";
            for (int i = 0; i < indent; i++)
            {
                result += "&gt;"; //"<div style=\"padding-left: 10px;\">";
            }

            if (indent > 0)
            {
                result += "&nbsp;";
            }

            return result;
        }


        /// <summary>
        /// Gets the end of the indentation string
        /// </summary>
        /// <param name="ind">Indentation</param>
        protected string GetEndIndent(object ind)
        {
            int indent = ValidationHelper.GetInteger(ind, 0);
            string result = "";
            /*
            for (int i = 0; i < indent; i++)
            {
                result += "</div>";
            }*/

            return result;
        }

        #endregion
    }
}
