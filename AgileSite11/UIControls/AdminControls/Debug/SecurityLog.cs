using System;

using CMS.Base;
using CMS.Helpers;

namespace CMS.UIControls
{
    /// <summary>
    /// Query log base class.
    /// </summary>
    public class SecurityLog : LogControl
    {
        #region "Properties"

        /// <summary>
        /// Debug settings for this particular log
        /// </summary>
        public override DebugSettings Settings
        {
            get
            {
                return SecurityDebug.Settings;
            }
        }
        
        #endregion


        #region "Methods"

        /// <summary>
        /// Gets the duplicity icon.
        /// </summary>
        /// <param name="duplicit">If true, the duplicity sign is shown</param>
        /// <param name="indent">Current item indentation</param>
        public new static object GetDuplicity(object duplicit, object indent)
        {
            if (ValidationHelper.GetInteger(indent, 0) > 0)
            {
                return null;
            }

            return LogControl.GetDuplicity(duplicit, ResHelper.GetString("SecurityLog.Duplicit"));
        }


        /// <summary>
        /// Returns colored security operation result string.
        /// </summary>
        /// <param name="result">Result of the security operation</param>
        /// <param name="ind">Indentation</param>
        /// <param name="imp">Important item</param>
        public static string GetResult(object result, object ind, object imp)
        {
            if ((result == null) || (result == DBNull.Value))
            {
                return null;
            }

            string stringResult = "";
            int indent = ValidationHelper.GetInteger(ind, 0);
            bool important = ValidationHelper.GetBoolean(imp, false);

            if (important || (indent == 0))
            {
                stringResult = "<strong>";

                switch (result.ToString().ToLowerCSafe())
                {
                    case "true":
                    case "allowed":
                        stringResult += "<span class=\"debug-log-yes\">" + result + "</span>";
                        break;

                    case "false":
                    case "denied":
                        stringResult += "<span class=\"debug-log-no\">" + result + "</span>";
                        break;

                    default:
                        stringResult += result.ToString();
                        break;
                }
            }
            else
            {
                stringResult += result.ToString();
            }

            if (indent == 0)
            {
                stringResult += "</strong>";
            }

            return stringResult;
        }


        /// <summary>
        /// Gets the user name for the items with indent 0
        /// </summary>
        /// <param name="userName">User name</param>
        /// <param name="indent">Indent</param>
        protected object GetUserName(object userName, object indent)
        {
            if (ValidationHelper.GetInteger(indent, 0) > 0)
            {
                return null;
            }

            return userName;
        }

        #endregion
    }
}