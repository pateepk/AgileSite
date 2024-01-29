using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI.WebControls;
using System.Xml;
using System.Xml.Serialization;

namespace WTE.Helpers
{
    /// <summary>
    /// URL Helper class
    /// </summary>
    [CLSCompliant(true)]
    [Serializable]
    public class WTEHttpHelper
    {
        #region utilities

        /// <summary>
        /// return inURL with double "/" removed
        /// </summary>
        /// <param name="p_inURL"></param>
        /// <returns></returns>
        public static string GetCleanURL(string p_inURL)
        {
            string cleanURL = p_inURL;

            cleanURL = cleanURL.Replace("http://", "_HTTP_");
            cleanURL = cleanURL.Replace("https://", "_HTTPS_");
            cleanURL = cleanURL.Replace("//", "/");
            cleanURL = cleanURL.Replace("_HTTP_", "http://");
            cleanURL = cleanURL.Replace("_HTTPS_", "https://");

            return cleanURL;
        }

        /// <summary>
        /// Converts the provided app-relative path into an absolute Url containing the full host name
        /// </summary>
        /// <param name="p_relativeUrl">App-Relative path</param>
        /// <returns>Provided relativeUrl parameter as fully qualified Url</returns>
        /// <example>~/path/to/foo to http://www.web.com/path/to/foo</example>
        public static string GetAbsoluteURL(string p_relativeUrl)
        {
            return GetAbsoluteURL(p_relativeUrl, true);
        }

        /// <summary>
        /// Get absolute URL with option to unsecured the path
        /// </summary>
        /// <param name="p_relativeUrl"></param>
        /// <param name="p_unsecured"></param>
        /// <returns></returns>
        public static string GetAbsoluteURL(string p_relativeUrl, bool p_unsecured)
        {


            string theUrl = p_relativeUrl;

            if (!String.IsNullOrWhiteSpace(theUrl))
            {
                string host = String.Empty;
                string scheme = String.Empty;
                string port = String.Empty;

                if (theUrl.StartsWith("https://") || theUrl.StartsWith("http://"))
                {
                    #region the url is already an absoulute path - return url with fix for https and http

                    if (p_unsecured)
                    {
                        theUrl = theUrl.Replace("https://", "http://");
                    }
                    else
                    {
                        theUrl = theUrl.Replace("http://", "https://");
                    }

                    #endregion the url is already an absoulute path - return url with fix for https and http
                }
                else
                {
                    if (p_unsecured)
                    {
                        scheme = "http";
                    }
                    else
                    {
                        scheme = "https";
                    }

                    #region process the URL

                    /*
                    
                    if (WTEConfigurationHelper.GetBoolAppSetting("IsConsoleApplication", false) || HttpContext.Current == null)
                    {
                        Portal portal = p_defaultPortal;
                        if (portal != null)
                        {
                            Domain domain = portal.GetDomainConfig("localhost", true);
                            if (domain != null)
                            {
                                host = domain.DomainName;
                            }
                        }
                    }
                    else
                     * */
                    {
                        // get info from the current context
                        var url = HttpContext.Current.Request.Url;
                        if (url.Port != 80 && url.Port != 443)
                        {
                            port = (":" + url.Port);
                        }
                        host = url.Host;
                    }

                    // clean
                    if (theUrl.StartsWith("//"))
                    {
                        theUrl = theUrl.Insert(0, "~");
                    }

                    if (theUrl.StartsWith("/"))
                    {
                        theUrl = theUrl.Insert(0, "~");
                    }
                    theUrl = theUrl.Replace("~//", "/").Replace("~/", "/");

                    theUrl = String.Format("{0}://{1}{2}{3}", scheme, host, port, theUrl);

                    //theUrl = String.Format("{0}://{1}{2}{3}",
                    //    scheme, host, port, VirtualPathUtility.ToAbsolute(p_relativeUrl));

                    //if (p_unsecured)
                    //{
                    //    theUrl = theUrl.Replace("https", "http");
                    //}
                }

                    #endregion process the URL
            }

            return theUrl;
        }

        #endregion utilities

        #region clean up URL

        /// <summary>
        /// Strip out virtual roots, leading "/", leading "~/", or leading "//"
        /// </summary>
        /// <param name="p_inURL"></param>
        /// <returns></returns>
        public static string GetDisplay301RedirectionURL(string p_inURL)
        {
            string url = String.Empty;

            if (!String.IsNullOrWhiteSpace(p_inURL))
            {
                url = p_inURL.Replace("~/", String.Empty);
            }

            return url;
        }

        /// <summary>
        /// Add virtual root
        /// </summary>
        /// <param name="p_inURL"></param>
        /// <returns></returns>
        public static string GetFormatted301RedirectionURL(string p_inURL)
        {
            string url = String.Empty;

            if (!String.IsNullOrWhiteSpace(p_inURL))
            {
                url = "~/" + p_inURL;
            }

            return url;
        }

        /// <summary>
        /// Replace all symbols with "-"
        /// </summary>
        /// <param name="p_inURL"></param>
        /// <returns></returns>
        public static string GetURLfriendlyString(string p_inURL)
        {
            string charPattern = @"[^a-zA-Z0-9_]";
            return Regex.Replace(p_inURL.Replace(" ", "-"), charPattern, "-");
        }

        /// <summary>
        /// Get safe string for XML or XSLT transform
        /// </summary>
        /// <param name="p_inString"></param>
        /// <param name="p_trim"></param>
        /// <returns></returns>
        public static string GetXMLSafeString(string p_inString, bool p_trim)
        {
            return CleanString(p_inString, true, p_trim);
        }

        /// <summary>
        /// Get clean string (null safe)
        /// </summary>
        /// <param name="p_inString"></param>
        /// <returns></returns>
        public static string CleanString(string p_inString)
        {
            return CleanString(p_inString, false, true);
        }

        /// <summary>
        /// Get clean string with no control characters
        /// </summary>
        /// <param name="p_inString"></param>
        /// <param name="p_removeControlCharacters"></param>
        /// <param name="p_trim"></param>
        /// <returns></returns>
        public static string CleanString(string p_inString, bool p_removeControlCharacters, bool p_trim)
        {
            string charPattern = @"[\u0000-\u001F|\u007F]";
            //string charPattern = @"[^\u0009^\u000A^\u000D^\u0020-\u007E]";
            //^\u0009 means if not TAB
            //^\u000A means if not Line Feed
            //^\u000D means if not Carriage Return
            //^\u0020-\u007E means if not fall into space to ~
            //string charPattern = @"[\u0000-\u0008|\u000B-\u000C|\u000E-\u001F|\u007F]";

            string cleanString = p_inString;

            if (!String.IsNullOrWhiteSpace(cleanString))
            {
                cleanString = cleanString.Replace("\t", "_p_tab_p_").Replace("\r", "_p_cr_p_").Replace("\n", "_p_lf_p_");

                if (p_removeControlCharacters)
                {
                    cleanString = Regex.Replace(cleanString, charPattern, string.Empty);
                }

                if (!String.IsNullOrWhiteSpace(cleanString))
                {
                    cleanString = cleanString.Replace("_p_tab_p_", "\t").Replace("_p_cr_p_", "\r").Replace("_p_lf_p_", "\n");

                    if (p_trim)
                    {
                        cleanString = cleanString.Trim();
                    }
                }
            }
            else
            {
                cleanString = null;
            }

            return cleanString;
        }

        /// <summary>
        /// Remove all html tags from a string
        /// </summary>
        /// <param name="p_inString"></param>
        /// <param name="p_replaceChar"></param>
        /// <returns></returns>
        public static string RemoveHtmlCode(string p_inString, string p_replaceChar)
        {
            string pattern1 = @"(?></?\w+)(?>(?:[^>'""]+|'[^']*'|""[^""]*"")*)>";
            //string pattern2 = @"</?\w+((\s+\w+(\s*=\s*(?:"".*?""|'.*?'|[^'"">\s]+))?)+\s*|\s*)/?>";
            string cleanString = p_inString;
            if (!String.IsNullOrWhiteSpace(cleanString))
            {
                cleanString = Regex.Replace(cleanString, pattern1, p_replaceChar);
            }
            return cleanString;
        }

        /// <summary>
        /// Get filename friendly string
        /// </summary>
        /// <param name="p_instring"></param>
        /// <returns></returns>
        public static string GetFileNameFriendlyString(string p_instring)
        {
            if (!String.IsNullOrWhiteSpace(p_instring))
            {
                p_instring = GetURLfriendlyString(p_instring).Replace("-", "_");
            }
            return p_instring;
        }

        #endregion clean up URL

        #region request parsing

        /// <summary>
        /// Check string for IP address
        /// </summary>
        /// <param name="p_requestUrl"></param>
        /// <returns></returns>
        public static bool IsIPAddress(string p_requestUrl)
        {
            bool containsIp = false;
            string IpPattern = "^(?:[0-9]{1,3}\\.){3}[0-9]{1,3}$";
            Match match;
            containsIp = GetRegexMatch(p_requestUrl, IpPattern, out match);
            return containsIp;
        }

        /// <summary>
        /// return a set of items matched.
        /// </summary>
        /// <param name="p_url">The string</param>
        /// <param name="p_pattern">pattern to match</param>
        /// <param name="p_match">output variable</param>
        /// <returns>Match success</returns>
        public static bool GetRegexMatch(string p_url, string p_pattern, out Match p_match)
        {
            p_match = Regex.Match(p_url, p_pattern, RegexOptions.IgnoreCase);
            return p_match.Success;
        }

        #endregion request parsing


        #region url encoding/decoding

        /// <summary>
        /// Encode string to the proper URL encoding (null safe)
        /// </summary>
        /// <param name="p_url"></param>
        /// <returns></returns>
        public static string EncodeUrl(string p_url)
        {
            if (!String.IsNullOrWhiteSpace(p_url))
            {
                return HttpUtility.UrlEncode(p_url);
            }
            return String.Empty;
        }

        /// <summary>
        /// Decode the Url to normal text (null safe)
        /// </summary>
        /// <param name="p_url"></param>
        /// <returns></returns>
        public static string DecodeUrl(string p_url)
        {
            if (!String.IsNullOrWhiteSpace(p_url))
            {
                return HttpUtility.UrlDecode(p_url);
            }
            return String.Empty;
        }

        #endregion url encoding/decoding

        #region text encoding/decoding

        /// <summary>
        /// Get HtmlDecode Text with no length limit (null safe)
        /// </summary>
        /// <param name="p_text"></param>
        /// <returns></returns>
        public static string GetHtmlDecodedText(object p_text)
        {
            return GetHtmlDecodedText(p_text, null);
        }

        /// <summary>
        /// Get HtmlDecode Text with length limit (null safe)
        /// </summary>
        /// <param name="p_text"></param>
        /// <param name="p_lengthLimit">use 0 or negative number for no limit</param>
        /// <returns></returns>
        public static string GetHtmlDecodedText(object p_text, int? p_lengthLimit)
        {
            string text = WTEDataHelper.GetSafeString(p_text);
            string txt = String.Empty;
            if (!String.IsNullOrWhiteSpace(text))
            {
                txt = HttpUtility.HtmlDecode(text);
                int lengthLimit = p_lengthLimit.GetValueOrDefault(0);
                if (lengthLimit > 0 && txt.Length > lengthLimit)
                {
                    txt = txt.Substring(0, lengthLimit - 3) + "...";
                }
            }
            return txt;
        }

        /// <summary>
        /// Get Html Encoded text (null safe)
        /// </summary>
        /// <param name="p_text"></param>
        /// <returns></returns>
        public static string GetHtmlEncodedText(object p_text)
        {
            string text = WTEDataHelper.GetSafeString(p_text);
            string txt = String.Empty;
            if (!String.IsNullOrWhiteSpace(text))
            {
                txt = HttpUtility.HtmlEncode(text);
            }
            return txt;
        }

        #endregion text encoding/decoding
    }
}
