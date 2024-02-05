using System;
using System.Collections.Generic;
using System.Data;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

using CMS.EventLog;
using CMS.Helpers;
using CMS.Membership;
using CMS.SiteProvider;

namespace CMS.PortalEngine.Web.UI
{
    /// <summary>
    /// Helper class for document validation
    /// </summary>
    public static class DocumentValidationHelper
    {
        #region "Variables"

        private static Regex mCapitalLetter;

        #endregion


        #region "Properties"

        private static Regex CapitalLetter
        {
            get
            {
                if (mCapitalLetter == null)
                {
                    mCapitalLetter = RegexHelper.GetRegex("([A-Z][a-z\\W])");
                }
                return mCapitalLetter;
            }
        }


        /// <summary>
        /// Returns source of inline CSS styles
        /// </summary>
        public static string InlineCSSSource
        {
            get
            {
                return ResHelper.GetAPIString("validation.css.inlinecss", "Page inline CSS styles");
            }
        }

        #endregion


        #region "Public methods"

        /// <summary>
        /// Process validation result and apply data transformations
        /// </summary>
        /// <param name="result">DataSet with validation results to process</param>
        /// <param name="validationTask">Type of document validation applied</param>
        /// <param name="additionalValues">Additional values used in data post-processing</param>
        public static DataTable ProcessValidationResult(DataSet result, DocumentValidationEnum validationTask, Dictionary<string, object> additionalValues)
        {
            DataTable dtResult = null;

            if (!DataHelper.DataSourceIsEmpty(result))
            {
                switch (validationTask)
                {
                    case DocumentValidationEnum.CSS:
                        {
                            // Get additional required values
                            UserInfo user = additionalValues["user"] as UserInfo;
                            string siteName = additionalValues["sitename"] as string;
                            string source = additionalValues["source"] as string;
                            string domainurl = additionalValues["domainurl"] as string;
                            string applicationurl = additionalValues["applicationurl"] as string;

                            // Check if user allowed to edit CSS
                            bool allowedToEdit = AllowedToEditCSS(user, siteName);

                            if (!DataHelper.DataSourceIsEmpty(result.Tables["error"]))
                            {
                                DataTable tbErrors = result.Tables["error"];
                                DataTable tbSource = new DataTable();

                                tbSource.Columns.Add("source");
                                tbSource.Rows.Add(source);

                                // Get result containing required columns
                                var results = from table1 in tbErrors.AsEnumerable()
                                              select new
                                              {
                                                  line = (string)table1["line"],
                                                  context = (string)table1["context"],
                                                  message = (string)table1["message"],
                                              };

                                // Convert result to DataTable
                                DataTable dtConverted = DataHelper.ConvertToDataTable(results);
                                DataTable dtSelected = dtConverted.Clone();
                                dtSelected.Columns["line"].DataType = typeof(int);

                                // Add source column and fill it with data
                                dtSelected.Columns.Add("source");
                                dtSelected.Columns.Add("url");

                                foreach (DataRow dr in dtConverted.Rows)
                                {
                                    dtSelected.ImportRow(dr);
                                    dtSelected.Rows[dtSelected.Rows.Count - 1]["source"] = source;
                                }

                                // Process rows data
                                foreach (DataRow dr in dtSelected.Rows)
                                {
                                    foreach (DataColumn dc in dtSelected.Columns)
                                    {
                                        switch (dc.ColumnName)
                                        {
                                            case "source":
                                                if (dr[dc].ToString() != InlineCSSSource)
                                                {
                                                    // Resolve URL and replace protocol if necessary
                                                    string url = URLHelper.GetAbsoluteUrl(dr[dc].ToString(), domainurl, applicationurl, null);
                                                    url = RequestContext.IsSSL ? "https://" : "http://" + URLHelper.RemoveProtocol(url);

                                                    DataHelper.SetDataRowValue(dr, "url", url);

                                                    // If allowed to edit, generate edit link
                                                    string stylesheetName = URLHelper.GetQueryValue(url, "stylesheetname");

                                                    if (allowedToEdit && !string.IsNullOrEmpty(stylesheetName))
                                                    {
                                                        CssStylesheetInfo cssInfo = CssStylesheetInfoProvider.GetCssStylesheetInfo(stylesheetName);

                                                        if ((cssInfo != null) && (CssStylesheetSiteInfoProvider.GetCssStylesheetSiteInfo(cssInfo.StylesheetID, SiteContext.CurrentSiteID) != null))
                                                        {
                                                            url = "~/CMSModules/CssStylesheets/Pages/CssStylesheet_General.aspx?editonlycode=true";
                                                            url = URLHelper.AddParameterToUrl(url, "hash", "##HASH##");
                                                            url = URLHelper.AddParameterToUrl(url, "objectid", cssInfo.StylesheetID.ToString());
                                                            url = URLHelper.AddParameterToUrl(url, "line", dr["line"].ToString());

                                                            DataHelper.SetDataRowValue(dr, dc.ColumnName, string.Format("<a href=\"#\" onclick=\"modalDialog('" + URLHelper.ResolveUrl(url) + "', 'ViewCSSCode', 1000, 700);return false;\" >{0}</a>", dr[dc] + "##EDITABLE##"));
                                                            break;
                                                        }
                                                    }

                                                    // Otherwise generate read only link
                                                    url = HttpUtility.UrlEncode(DisableMinificationOnUrl(url));

                                                    DataHelper.SetDataRowValue(dr, dc.ColumnName, String.Format("<a href=\"#\" onclick=\"modalDialog('" + URLHelper.ResolveUrl("~/CMSModules/Content/CMSDesk/Validation/ViewCode.aspx") + "?url={0}&format=css&hash=##HASH##', 'ViewCSSCode', 800, 600);return false;\" >{1}</a>", url, dr[dc]));
                                                }
                                                break;
                                        }
                                    }
                                }

                                dtResult = dtSelected;
                            }
                        }
                        break;

                    case DocumentValidationEnum.Link:
                        {
                            DataTable dtLinks = result.Tables[0];
                            string culture = additionalValues["culture"] as string;
                            // Process rows data
                            foreach (DataRow dr in dtLinks.Rows)
                            {
                                foreach (DataColumn dc in dtLinks.Columns)
                                {
                                    switch (dc.ColumnName)
                                    {
                                        case "url":
                                            DataHelper.SetDataRowValue(dr, dc.ColumnName, TextHelper.EnsureMaximumLineLength(dr[dc].ToString(), 50, BrowserHelper.IsIE() ? "<span></span>" : "<wbr>", false));
                                            break;

                                        case "message":
                                            DataHelper.SetDataRowValue(dr, dc.ColumnName, dr[dc].ToString().Replace("</b>", BrowserHelper.IsIE() ? "</b><span></span>" : "</b><wbr>"));
                                            break;

                                        case "type":
                                            {
                                                string evetType = ValidationHelper.GetString(dr[dc], "");
                                                DataHelper.SetDataRowValue(dr, dc.ColumnName, "<div style=\"width:100%;text-align:center;cursor:help;\" title=\"" + HTMLHelper.HTMLEncode(EventLogHelper.GetEventTypeText(evetType)) + "\">" + evetType + " </div>");
                                            }
                                            break;

                                        case "statuscode":
                                            string status = ValidationHelper.GetString(dr[dc], "");
                                            string[] codes = status.Split(new[] { "->" }, StringSplitOptions.RemoveEmptyEntries);

                                            StringBuilder sb = new StringBuilder();

                                            // Process response status codes
                                            foreach (string code in codes)
                                            {
                                                int codeNumber = ValidationHelper.GetInteger(code, 0);
                                                sb.Append(@" <span style=""cursor:help;"" title=""", codeNumber, ": ", HTMLHelper.HTMLEncode(HttpWorkerRequest.GetStatusDescription(codeNumber)), @""">", HTMLHelper.HTMLEncode(GetStatusCodeDescription(codeNumber, culture)), " </span>->");
                                            }

                                            // Remove ending arrow 
                                            if (sb.Length > 2)
                                            {
                                                sb.Remove(sb.Length - 2, 2);
                                            }

                                            DataHelper.SetDataRowValue(dr, dc.ColumnName, sb.ToString());
                                            break;

                                        case "time":
                                            string time = ValidationHelper.GetString(dr[dc], "");
                                            DataHelper.SetDataRowValue(dr, dc.ColumnName, time.Trim(new char[] { '(', ')' }));
                                            break;
                                    }
                                }
                                dtResult = dtLinks;
                            }
                        }
                        break;

                    case DocumentValidationEnum.Accessibility:
                        {
                            // Process accessibility results
                            result.Tables[0].DefaultView.Sort = "line ASC";
                            dtResult = result.Tables[0].DefaultView.ToTable();
                        }
                        break;
                }

                if (!DataHelper.DataSourceIsEmpty(dtResult))
                {
                    // Add DataColumn caption
                    foreach (DataColumn dc in dtResult.Columns)
                    {
                        dc.Caption = ResHelper.GetString("validation." + validationTask + "." + dc.ColumnName);
                    }
                }
            }

            return dtResult;
        }


        /// <summary>
        /// Post-Process validation data
        /// </summary>
        /// <param name="data">DataSet with validation data to process</param>
        /// <param name="validationTask">Type of document validation applied</param>
        /// <param name="additionalValues">Additional values used in data post-processing</param>
        public static DataSet PostProcessValidationData(DataSet data, DocumentValidationEnum validationTask, Dictionary<string, object> additionalValues)
        {
            if (!DataHelper.DataSourceIsEmpty(data))
            {
                switch (validationTask)
                {
                    case DocumentValidationEnum.CSS:
                        foreach (DataRow dr in data.Tables[0].Rows)
                        {
                            string sourceData = dr["source"].ToString();
                            string url = HTMLHelper.StripTags(sourceData);

                            // Set hash according to link type
                            string hash;
                            if (url.EndsWith("##EDITABLE##", StringComparison.InvariantCulture))
                            {
                                hash = QueryHelper.GetHash("?editonlycode=true");
                            }
                            else
                            {
                                hash = QueryHelper.GetHash("?url=" + HttpUtility.UrlEncode(DisableMinificationOnUrl(URLHelper.GetAbsoluteUrl(url))) + "&format=css");
                            }

                            // Update data in row
                            sourceData = sourceData.Replace("##HASH##", hash).Replace("##EDITABLE##", "");
                            DataHelper.SetDataRowValue(dr, "source", sourceData);
                        }
                        break;

                    case DocumentValidationEnum.Link:
                        DataTable dt = data.Tables[0];
                        if (!dt.Columns.Contains("statuscodevalue"))
                        {
                            dt.Columns.Add("statuscodevalue", typeof(string));
                        }

                        if (!dt.Columns.Contains("timeint"))
                        {
                            dt.Columns.Add("timeint", typeof(int));
                        }

                        foreach (DataRow dr in data.Tables[0].Rows)
                        {
                            // Set values of sorting colum for status code
                            string status = HTMLHelper.StripTags(ValidationHelper.GetString(dr["statuscode"], ""));
                            DataHelper.SetDataRowValue(dr, "statuscodevalue", status);

                            string time = ValidationHelper.GetString(dr["time"], "ms").Replace("ms", "");
                            DataHelper.SetDataRowValue(dr, "timeint", ValidationHelper.GetInteger(time, 0));
                        }
                        break;
                }
            }

            return data;
        }

        #endregion


        #region "Private helper methods"

        /// <summary>
        /// Update URL so it doesn't use minification
        /// </summary>
        /// <param name="url">URL to update</param>
        public static string DisableMinificationOnUrl(string url)
        {
            var urlToCheck = url.ToLowerInvariant();
            if (urlToCheck.Contains("getresource.ashx") || urlToCheck.Contains("getcss.aspx"))
            {
                url = URLHelper.AddParameterToUrl(url, "enableminification", false.ToString());
                url = URLHelper.AddParameterToUrl(url, "enablecompression", false.ToString());
            }
            return url;
        }


        /// <summary>
        /// Check is user is allowed to edit CSS
        /// </summary>
        /// <param name="user">User to check</param>
        /// <param name="siteName">Name of site under which CSS are checked to be editable</param>
        private static bool AllowedToEditCSS(UserInfo user, string siteName)
        {
            // Check if user can edit the stylesheet
            bool design = UserInfoProvider.IsAuthorizedPerResource("CMS.Design", "Design", siteName, user);
            bool uiElement = UserInfoProvider.IsAuthorizedPerUIElement("CMS.Content", new[] { "Properties", "Properties.General", "General.Design", "Design.EditCSSStylesheets" }, siteName, user);

            return (design && uiElement);
        }


        /// <summary>
        /// Get string representation of HTTP status code
        /// </summary>
        /// <param name="statusNumber">HTTP status code</param>
        /// <param name="culture">Culture of resulting text</param>
        public static string GetStatusCodeDescription(int statusNumber, string culture)
        {
            if (statusNumber > 0)
            {
                HttpStatusCode status = (HttpStatusCode)statusNumber;
                string statusCode;
                switch (status)
                {
                    // Moved permanently, follow redirection
                    case HttpStatusCode.MovedPermanently:
                        statusCode = HttpStatusCode.MovedPermanently.ToString();
                        break;

                    case HttpStatusCode.MultipleChoices:
                    case HttpStatusCode.Found:
                    case HttpStatusCode.RedirectMethod:
                    case HttpStatusCode.RedirectKeepVerb:
                        statusCode = HttpStatusCode.Found.ToString();
                        break;

                    default:
                        statusCode = status.ToString();
                        break;
                }

                return ResHelper.GetAPIString("validation.link.statuses." + statusCode, culture, CapitalLetter.Replace(statusCode, " $1"));
            }

            return null;
        }

        #endregion
    }
}