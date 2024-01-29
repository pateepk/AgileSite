using System;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;

using CMS.Base;
using CMS.Helpers;
using CMS.UIControls;

namespace CMS.WebAnalytics.Web.UI
{
    /// <summary>
    /// Cache log base class.
    /// </summary>
    public class AnalyticsLog : LogControl
    {
        #region "Variables"

        private string mIp = String.Empty;
        private string mAgent = String.Empty;

        #endregion


        #region "Properties"

        /// <summary>
        /// Debug settings for this particular log
        /// </summary>
        public override DebugSettings Settings
        {
            get
            {
                return AnalyticsDebug.Settings;
            }
        }
        
        #endregion


        #region "Methods"

        /// <summary>
        /// Update footer values
        /// </summary>
        protected void GetIPAndAgent(object sender, GridViewRowEventArgs e)
        {
            // Keep current user agent and ip
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                mIp = Convert.ToString(DataBinder.Eval(e.Row.DataItem, "IP"));
                mAgent = Convert.ToString(DataBinder.Eval(e.Row.DataItem, "UserAgent"));
            }
            // Add IP and UserAgent to the footer
            else if (e.Row.RowType == DataControlRowType.Footer)
            {
                // IP
                var ltlIp = e.Row.FindControl("ltlIp") as Literal;
                if (ltlIp != null)
                {
                    ltlIp.Text = HTMLHelper.HTMLEncode(mIp);
                }

                // User agent
                var ltlAgent = e.Row.FindControl("ltlAgent") as Literal;
                if (ltlAgent != null)
                {
                    ltlAgent.Text = HTMLHelper.HTMLEncode(mAgent);
                }
            }
        }


        /// <summary>
        /// Returns information writable to analytics debug.
        /// </summary>
        /// <paramref name="objectName">Object name</paramref>
        /// <paramref name="culture">Culture</paramref>
        /// <paramref name="objectId">Object ID</paramref>
        protected string GetInformation(object objectName, object culture, object objectId)
        {
            var sb = new StringBuilder();

            // Standard web analytics action
            sb.Append(HTMLHelper.HTMLEncode(ValidationHelper.GetString(objectName, string.Empty)));

            int id = ValidationHelper.GetInteger(objectId, 0);
            if (id > 0)
            {
                sb.Append(objectId);
            }

            string stringCulture = ValidationHelper.GetString(culture, null);
            if (!String.IsNullOrEmpty(stringCulture))
            {
                sb.Append(" (", stringCulture, ")");
            }

            return sb.ToString();
        }


        /// <summary>
        /// Gets the count/value information
        /// </summary>
        /// <param name="count">Count</param>
        /// <param name="value">Value</param>
        protected string GetCount(object count, object value)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(count);

            double dValue = ValidationHelper.GetDouble(value, 0);
            if (dValue > 0)
            {
                sb.Append(" (", dValue, ")");
            }

            return sb.ToString();
        }

        #endregion
    }
}