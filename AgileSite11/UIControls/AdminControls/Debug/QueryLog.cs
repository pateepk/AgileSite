using System;
using System.Text;
using System.Web;

using CMS.Base;
using CMS.Base.Web.UI;
using CMS.DataEngine;
using CMS.Helpers;

namespace CMS.UIControls
{
    /// <summary>
    /// Query log base class.
    /// </summary>
    public class QueryLog : LogControl
    {
        /// <summary>
        /// Size limit
        /// </summary>
        public static int SIZE_LIMIT = 512;


        #region "Properties"

        /// <summary>
        /// Debug settings for this particular log
        /// </summary>
        public override DebugSettings Settings
        {
            get
            {
                return SqlDebug.Settings;
            }
        }


        /// <summary>
        /// Total parameters size.
        /// </summary>
        public virtual long TotalParamSize
        {
            get;
            set;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Returns formatted information for given parameters.
        /// </summary>
        /// <param name="isInformation">True if the item is only an information</param>
        /// <param name="connectionString">Connection string</param>
        /// <param name="connectionOp">Connection operation</param>
        /// <param name="queryName">Query name</param>
        /// <param name="queryText">Query text</param>
        /// <param name="queryParameters">Query parameters</param>
        /// <param name="queryParametersSize">Query parameters size</param>
        /// <param name="queryResults">Query results</param>
        /// <param name="queryResultsSize">Query results size</param>
        /// <param name="maxSize">Maximum query data size</param>
        public static string GetInformation(object isInformation, object connectionString, object connectionOp, object queryName, object queryText, object queryParameters, object queryParametersSize, object queryResults, object queryResultsSize, double maxSize)
        {
            StringBuilder sb = new StringBuilder();

            // Transform the connection string to its nice name
            string cs = ValidationHelper.GetString(connectionString, "");
            cs = SettingsHelper.ConnectionStrings.GetConnectionStringName(cs);

            sb.Append("<div title=\"", cs, "\">");

            // Connection operation
            if (ValidationHelper.GetString(connectionOp, "") != "")
            {
                sb.Append("<strong>", HTMLHelper.HTMLEncodeLineBreaks(ValidationHelper.GetString(connectionOp, "")), "</strong>");
            }

            // Query name
            bool hasName = (ValidationHelper.GetString(queryName, "") != "");
            bool hasCs = false; // (!String.IsNullOrEmpty(cs) && (cs != SqlHelper.DEFAULT_CONNECTIONSTRING_NAME));

            if (hasName || hasCs)
            {
                sb.Append("<div class=\"QueryName\">");

                if (hasName)
                {
                    if (ValidationHelper.GetBoolean(isInformation, false))
                    {
                        // Title only
                        sb.Append("<strong>", HttpUtility.HtmlEncode(ValidationHelper.GetString(queryName, "")), "</strong>");
                    }
                    else
                    {
                        // Query name
                        sb.Append("<strong>(", HttpUtility.HtmlEncode(ValidationHelper.GetString(queryName, "").ToLowerCSafe()), ")</strong>");
                    }
                }

                // External connection
                if (hasCs)
                {
                    sb.Append(UIHelper.GetAccessibleIconTag("icon-database", connectionString.ToString()));
                }

                sb.Append("</div>");
            }
            sb.Append("<div class=\"debug-log-querytext\">", HTMLHelper.HTMLEncodeLineBreaks(ValidationHelper.GetString(queryText, "")), "</div>");

            // Parameters and query size
            long parametersSize = ValidationHelper.GetLong(queryParametersSize, 0);
            string parameters = ValidationHelper.GetString(queryParameters, "");
            if (!String.IsNullOrEmpty(parameters) || (parametersSize > SIZE_LIMIT))
            {
                sb.Append("<div class=\"QueryParams\">");

                if (!String.IsNullOrEmpty(parameters))
                {
                    sb.Append(HTMLHelper.HTMLEncodeLineBreaks(ValidationHelper.GetString(queryParameters, "")), HTMLHelper.HTML_BREAK);
                }
                sb.Append(DataHelper.GetSizeString(parametersSize), " ", GetChart(maxSize, queryParametersSize, SIZE_LIMIT, 0, 7));

                sb.Append("</div>");
            }

            if (ValidationHelper.GetString(queryResults, "") != "")
            {
                sb.Append("<div class=\"QueryResults\">",
                          HTMLHelper.HTMLEncodeLineBreaks(ValidationHelper.GetString(queryResults, "")) + HTMLHelper.HTML_BREAK,
                          GetChart(maxSize, queryResultsSize, SIZE_LIMIT, 0, 7), "</div>");
            }

            sb.Append("</div>");

            return sb.ToString();
        }


        /// <summary>
        /// Gets the duplicity icon.
        /// </summary>
        /// <param name="duplicit">If true, the duplicity sign is shown</param>
        /// <param name="queryText">Query text</param>
        public new static object GetDuplicity(object duplicit, object queryText)
        {
            if (queryText == DBNull.Value)
            {
                return null;
            }

            return LogControl.GetDuplicity(duplicit, ResHelper.GetString("QueryLog.Duplicit"));
        }


        /// <summary>
        /// Gets the external connection string icon.
        /// </summary>
        /// <param name="connectionString">Connection string used for the query</param>
        public static object GetConnectionString(object connectionString)
        {
            if (connectionString == DBNull.Value)
            {
                return null;
            }

            string cs = ValidationHelper.GetString(connectionString, "");
            cs = SettingsHelper.ConnectionStrings.GetConnectionStringName(cs);

            if (String.IsNullOrEmpty(cs) || (cs == ConnectionHelper.DEFAULT_CONNECTIONSTRING_NAME))
            {
                return null;
            }

            string result = UIHelper.GetAccessibleIconTag("icon-database", cs);
            if (!String.IsNullOrEmpty(result))
            {
                result = HTMLHelper.HTML_BREAK + result;
            }

            return result;
        }

        
        /// <summary>
        /// Gets the index of the record
        /// </summary>
        /// <param name="isInformation">Flag whether the item is information</param>
        /// <param name="resultsSize">Result size</param>
        /// <param name="parametersSize">Parameters size</param>
        /// <param name="queryName">Query name</param>
        /// <param name="queryText">Query text</param>
        protected object GetIndex(object isInformation, object resultsSize, object parametersSize, object queryName, object queryText)
        {
            if (ValidationHelper.GetBoolean(isInformation, false))
            {
                return UIHelper.GetAccessibleIconTag("icon-i-circle", queryName.ToString());
            }

            if (queryText == DBNull.Value)
            {
                return null;
            }

            TotalSize += ValidationHelper.GetInteger(resultsSize, 0);
            TotalParamSize += ValidationHelper.GetInteger(parametersSize, 0);

            return ++index;
        }

        #endregion
    }
}