using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Text;
using System.Text.RegularExpressions;

using CMS.Base;
using CMS.EventLog;
using CMS.Helpers;

using AngleSharp.Parser.Html;
using AngleSharp.Dom;
using AngleSharp.Extensions;

namespace CMS.UIControls.Internal
{
    /// <summary>
    /// Accessibility validator base class
    /// </summary>
    public abstract class AccessibilityValidator: DocumentValidator
    {
        private Regex mIntNumberRegEx = RegexHelper.GetRegex("\\b\\d+\\b");

        /// <summary>
        /// URL to which validator requests will be sent
        /// </summary>
        private static string ValidatorURL
        {
            get;
        } = DataHelper.GetNotEmpty(SettingsHelper.AppSettings["CMSValidationAccessibilityValidatorURL"], "https://achecker.ca/checker/index.php");


        /// <summary>
        /// Gets or sets validation standard
        /// </summary>
        protected AccessibilityStandardEnum Standard
        {
            get;
            set;
        }


        /// <summary>
        /// Send validation request to validator and obtain result 
        /// </summary>
        /// <param name="htmlData">HTML to validate</param>
        /// <param name="errorText">Error text</param>
        /// <returns>DataSet containing validator response</returns>
        public DataSet GetValidationResult(string htmlData, ref string errorText)
        {
            try
            {
                DataSet dsValResult = null;
                Random randGen = new Random();

                // Create web request
                HttpWebRequest req = WebRequest.CreateHttp(ValidatorURL);
                req.Method = WebRequestMethods.Http.Post;
                string boundary = "---------------------------" + randGen.Next(1000000, 9999999) + randGen.Next(1000000, 9999999);
                req.ContentType = "multipart/form-data; boundary=" + boundary;

                if (req.RequestUri.Scheme == Uri.UriSchemeHttps)
                {
                    req.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) =>
                    {
                        // Accept certificate if the certificate is valid and signed
                        return (sslPolicyErrors == SslPolicyErrors.None);
                    };
                }

                // Set data to web request for validation           
                byte[] data = Encoding.GetEncoding("UTF-8").GetBytes(GetRequestData(GetRequestDictionary(htmlData), boundary));
                req.ContentLength = data.Length;
                var writer = req.GetRequestStream();
                writer.Write(data, 0, data.Length);
                writer.Close();

                // Process server answer
                var answer = req.GetResponse().GetResponseStream();
                if (answer != null)
                {
                    // Load document with given HTML
                    var parser = new HtmlParser();
                    var document = parser.Parse(answer);

                    // Find the div node with validation results
                    var divErrors = document.All["AC_errors"];
                    DataTable dtErrors = CreateAccessibilityTable("errors", divErrors);

                    dsValResult = new DataSet();
                    dsValResult.Tables.Add(dtErrors);
                }

                return dsValResult;
            }
            catch (WebException ex) when (ex.Status == WebExceptionStatus.TrustFailure)
            {
                EventLogProvider.LogException("AccessibilityValidator", "GetValidationResult", ex);
                errorText = GetString("validation.servercertificateerror");
                return null;
            }
            catch
            {
                errorText = GetString("validation.servererror");
                return null;
            }
        }


        /// <summary>
        /// Get dictionary with request parameters
        /// </summary>
        /// <param name="data">HTML data to be checked</param>
        private Dictionary<string, string> GetRequestDictionary(string data)
        {
            Dictionary<string, string> reqData = new Dictionary<string, string>();
            reqData.Add("pastehtml", data);
            reqData.Add("validate_paste", "Check It");
            reqData.Add("checkbox_gid[]", AccessibilityStandardCode.FromEnum(Standard).ToString());
            reqData.Add("radio_gid[]", AccessibilityStandardCode.FromEnum(Standard).ToString());
            reqData.Add("rpt_format", "1");
            return reqData;
        }


        /// <summary>
        /// Create accessibility table with validation results
        /// </summary>
        /// <param name="tableName">Name of the result table</param>
        /// <param name="node">HTML node containing validation results</param>
        private DataTable CreateAccessibilityTable(string tableName, IElement node)
        {
            DataTable tb = null;

            if (node != null)
            {
                // Create table to store results
                tb = new DataTable(tableName);
                tb.Columns.AddRange(new DataColumn[] {  new DataColumn("line",typeof(int)),
                                                    new DataColumn("column",typeof(int)),
                                                    new DataColumn("accessibilityrule",typeof(string)),
                                                    new DataColumn("error",typeof(string)),
                                                    new DataColumn("fixsuggestion",typeof(string)),
                                                    new DataColumn("source",typeof(string))
                                                 });

                // Variables to store data
                string mainRule = null;
                string minorRule = null;
                string error = null;
                string fix = null;
                int line = 0;
                int col = 0;
                string source = null;

                // Process specified HTML nodes containing results
                var nodes =  node.QuerySelectorAll("h3 , h4, div.gd_one_check span.gd_msg a, div.gd_one_check div.gd_question_section, div.gd_one_check table tr");

                // Go through results
                if (nodes.Any())
                {
                    foreach (var child in nodes)
                    {
                        switch (child.LocalName.ToLowerInvariant())
                        {
                            case "h3":
                                mainRule = child.Text();
                                break;

                            case "h4":
                                minorRule = child.Text();
                                break;

                            case "a":
                                error = child.InnerHtml;
                                break;

                            case "div":
                                fix = child.InnerHtml;
                                break;

                                // Process error details
                                case "tr":

                                var errorChilds = child.QuerySelectorAll("td em, td code.input");

                                    foreach (var errorChild in errorChilds)
                                    {
                                        switch (errorChild.LocalName.ToLowerInvariant())
                                        {
                                            case "em":
                                                Match[] position = new Match[2];
                                                mIntNumberRegEx.Matches(errorChild.TextContent).CopyTo(position, 0);
                                                line = ValidationHelper.GetInteger(position[0].Value, 0);
                                                col = ValidationHelper.GetInteger(position[1].Value, 0);
                                                break;

                                            case "code":
                                                source = errorChild.InnerHtml;
                                                break;
                                        }
                                    }

                                    // Fill datarow content
                                    DataRow dr = tb.NewRow();
                                    dr["accessibilityrule"] = mainRule + "<br/>" + minorRule;
                                    dr["error"] = error;
                                    dr["fixsuggestion"] = fix;
                                    dr["line"] = line;
                                    dr["column"] = col;
                                    dr["source"] = source;

                                    // Add row to table
                                    tb.Rows.Add(dr);
                                    break;
                        }
                    }
                }
            }
            return tb;
        }


        /// <summary>
        /// Get request data which will be sent using HTTP request to validator
        /// </summary>
        /// <param name="data">Data to create </param>
        /// <param name="boundary">HTTP boundary string</param>
        private string GetRequestData(Dictionary<string, string> data, string boundary)
        {
            string separator = TextHelper.NewLine;
            boundary = "--" + boundary;

            // Prepare beginning of the request data
            StringBuilder sbRequest = new StringBuilder();
            sbRequest.Append(boundary);
            sbRequest.Append(separator);

            // Process request form data
            foreach (string key in data.Keys)
            {
                // Note: Do not encode key name. The server side requires utf-8 defined name
                sbRequest.Append(String.Format("Content-Disposition: form-data; name=\"{0}\"", key));
                sbRequest.Append(separator);
                sbRequest.Append(separator);
                sbRequest.Append(data[key]);
                sbRequest.Append(separator);
                sbRequest.Append(boundary);
                sbRequest.Append(separator);
            }
            string request = sbRequest.ToString();

            // Add final boundary dashes
            request = request.Insert(request.Length - 2, "--");
            return request;
        }
    }


    /// <summary>
    /// Accessibility standard enum
    /// </summary>
    public enum AccessibilityStandardEnum
    {
        /// <summary>
        /// BIK BITV 1
        /// </summary>
        BITV1_0 = 1,

        /// <summary>
        /// Section 508
        /// </summary>
        Section508 = 2,

        /// <summary>
        /// Stanca Act
        /// </summary>
        StancaAct = 3,

        /// <summary>
        /// WCAG 1 A
        /// </summary>
        WCAG1_0A = 4,

        /// <summary>
        /// WCAG 1 AA
        /// </summary>
        WCAG1_0AA = 5,

        /// <summary>
        /// WCAG 1 AAA
        /// </summary>
        WCAG1_0AAA = 6,

        /// <summary>
        /// WCAG 2 A
        /// </summary>
        WCAG2_0A = 7,

        /// <summary>
        /// WCAG 2 AA
        /// </summary>
        WCAG2_0AA = 8,

        /// <summary>
        /// WCAG 2 AAA
        /// </summary>
        WCAG2_0AAA = 9
    }


    /// <summary>
    /// Accessibility standard code
    /// </summary>
    public static class AccessibilityStandardCode
    {
        #region "Constants"

        /// <summary>
        /// BIK BITV 1
        /// </summary>
        public const int BITV1_0 = 1;

        /// <summary>
        /// Section 508
        /// </summary>
        public const int Section508 = 2;

        /// <summary>
        /// Stanca Act
        /// </summary>
        public const int StancaAct = 3;

        /// <summary>
        /// WCAG 1 A
        /// </summary>
        public const int WCAG1_0A = 4;

        /// <summary>
        /// WCAG 1 AA
        /// </summary>
        public const int WCAG1_0AA = 5;

        /// <summary>
        /// WCAG 1 AAA
        /// </summary>
        public const int WCAG1_0AAA = 6;

        /// <summary>
        /// WCAG 2 A
        /// </summary>
        public const int WCAG2_0A = 7;

        /// <summary>
        /// WCAG 2 AA
        /// </summary>
        public const int WCAG2_0AA = 8;

        /// <summary>
        /// WCAG 2 AAA
        /// </summary>
        public const int WCAG2_0AAA = 9;

        #endregion


        #region "Methods"

        /// <summary>
        /// Returns the enumeration representation of the Accessibility standard code.
        /// </summary>
        /// <param name="code">Accessibility standard code</param>
        public static AccessibilityStandardEnum ToEnum(int code)
        {
            switch (code)
            {
                case BITV1_0:
                    return AccessibilityStandardEnum.BITV1_0;

                case Section508:
                    return AccessibilityStandardEnum.Section508;

                case StancaAct:
                    return AccessibilityStandardEnum.StancaAct;

                case WCAG1_0A:
                    return AccessibilityStandardEnum.WCAG1_0A;

                case WCAG1_0AA:
                    return AccessibilityStandardEnum.WCAG1_0AA;

                case WCAG1_0AAA:
                    return AccessibilityStandardEnum.WCAG1_0AAA;

                case WCAG2_0A:
                    return AccessibilityStandardEnum.WCAG2_0A;

                case WCAG2_0AA:
                    return AccessibilityStandardEnum.WCAG2_0AA;

                case WCAG2_0AAA:
                    return AccessibilityStandardEnum.WCAG2_0AAA;

                default:
                    return AccessibilityStandardEnum.WCAG2_0AA;
            }
        }


        /// <summary>
        /// Returns the accessibility standard code from the enumeration value.
        /// </summary>
        /// <param name="value">Value to convert</param>
        public static int FromEnum(AccessibilityStandardEnum value)
        {
            switch (value)
            {
                case AccessibilityStandardEnum.BITV1_0:
                    return BITV1_0;

                case AccessibilityStandardEnum.Section508:
                    return Section508;

                case AccessibilityStandardEnum.StancaAct:
                    return StancaAct;

                case AccessibilityStandardEnum.WCAG1_0A:
                    return WCAG1_0A;

                case AccessibilityStandardEnum.WCAG1_0AA:
                    return WCAG1_0AA;

                case AccessibilityStandardEnum.WCAG1_0AAA:
                    return WCAG1_0AAA;

                case AccessibilityStandardEnum.WCAG2_0A:
                    return WCAG2_0A;

                case AccessibilityStandardEnum.WCAG2_0AA:
                    return WCAG2_0AA;

                case AccessibilityStandardEnum.WCAG2_0AAA:
                    return WCAG2_0AAA;

                default:
                    return WCAG2_0AA;
            }
        }

        #endregion
    }
}
