using System;
using System.Web;

using CMS.Base;
using CMS.Helpers;
using CMS.MacroEngine;

namespace CMS.UIControls
{
    /// <summary>
    /// Query log base class.
    /// </summary>
    public class MacroLog : LogControl
    {
        #region "Properties"

        /// <summary>
        /// Debug settings for this particular log
        /// </summary>
        public override DebugSettings Settings
        {
            get
            {
                return MacroDebug.Settings;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Gets the macro expression
        /// </summary>
        /// <param name="indent">Indent</param>
        /// <param name="expression">Expression</param>
        protected string GetExpression(object indent, object expression)
        {
            string result = null;

            // Main expression
            bool main = (ValidationHelper.GetInteger(indent, 0) <= 0);
            if (main)
            {
                result += "<strong>";
            }
            else
            {
                result += "<span class=\"debug-log-lessimportant\">";
            }

            string expr = ValidationHelper.GetString(expression, "").Replace("%\\}", "%}").Replace("\\n", "\n").Replace("\\r", "\r");
            bool isReturnStatement = expr.StartsWithCSafe("return", true);

            result += GetBeginIndent(indent);
            result += (isReturnStatement ? "<strong>" : "");
            result += HTMLHelper.HTMLEncode(MacroSecurityProcessor.RemoveSecurityParameters(expr, false, null));
            result += (isReturnStatement ? "</strong>" : "");

            if (main)
            {
                result += "</strong>";
            }
            else
            {
                result += "</span>";
            }

            return result;
        }


        /// <summary>
        /// Gets the context for items with indent 0
        /// </summary>
        /// <param name="indent">Indent</param>
        /// <param name="context">Context</param>
        protected new string GetContext(object indent, object context)
        {
            if (ValidationHelper.GetInteger(indent, 0) <= 0)
            {
                return GetContext(context);
            }

            return "";
        }


        /// <summary>
        /// Gets the result
        /// </summary>
        /// <param name="result">Result</param>
        /// <param name="ind">Indent</param>
        /// <param name="expression">Expression</param>
        /// <param name="error">Error</param>
        protected string GetResult(object result, object ind, object expression, object error)
        {
            if ((result == null) || (result == DBNull.Value))
            {
                return null;
            }

            string stringResult = "";

            // Main expression
            bool main = ValidationHelper.GetInteger(ind, 0) <= 0;
            if (main)
            {
                stringResult += "<strong>";
            }
            else
            {
                stringResult += "<span class=\"debug-log-lessimportant\">";
            }

            // Error icon
            bool err = ValidationHelper.GetBoolean(error, false);
            if (err)
            {
                stringResult += "<span class=\"debug-log-error\">";
            }

            // Content
            string original = ValidationHelper.GetString(result, "");
            bool isReturnStatement = ValidationHelper.GetString(expression, "").StartsWithCSafe("return", true);

            if (original.Length > 100)
            {
                stringResult += (isReturnStatement ? "<strong>" : "");
                stringResult += String.Format("<div title=\"{0}\" \">", HTMLHelper.HTMLEncode(original));
                stringResult += HTMLHelper.HTMLEncode(TextHelper.LimitLength(original, 100)) + "</div>" + (isReturnStatement ? "</strong>" : "");
            }
            else
            {
                stringResult += (isReturnStatement ? "<strong>" : "") + HttpUtility.HtmlEncode(original) + (isReturnStatement ? "</strong>" : "");
            }

            if (err)
            {
                stringResult += "</span>";
            }

            if (main)
            {
                stringResult += "</strong>";
            }
            else
            {
                stringResult += "</span>";
            }

            return stringResult;
        }

        #endregion
    }
}