using System;
using System.Text;

namespace WTE.Helpers
{
    /// <summary>
    /// Text formatting helper
    /// </summary>
    [Serializable()]
    public class WTETextHelper
    {
        #region append text

        /// <summary>
        /// Concat 2 string wtih "new line"
        /// </summary>
        /// <param name="p_left"></param>
        /// <param name="p_right"></param>
        /// <param name="p_isHtml"></param>
        /// <returns></returns>
        public static string ConcatWithLineBreak(object p_left, object p_right, bool p_isHtml)
        {
            string ret = GetStringValue(p_left);
            if (!String.IsNullOrWhiteSpace(GetStringValue(p_right)))
            {
                if (!String.IsNullOrWhiteSpace(ret))
                {
                    ret += GetNewLine(p_isHtml);
                }
                ret += GetStringValue(p_right);
            }
            return ret;
        }

        /// <summary>
        /// Cocat string wth caption
        /// </summary>
        /// <param name="p_left"></param>
        /// <param name="p_right"></param>
        /// <param name="p_caption"></param>
        /// <param name="p_isHtml"></param>
        /// <returns></returns>
        public static string ConcatWithCaption(object p_left, object p_right, object p_caption, bool p_isHtml)
        {
            return ConcatWithCaption(p_left, p_right, p_caption, true, true, p_isHtml);
        }

        /// <summary>
        /// Concat with caption
        /// </summary>
        /// <param name="p_left"></param>
        /// <param name="p_right"></param>
        /// <param name="p_caption"></param>
        /// <param name="p_addSpace"></param>
        /// <param name="p_addColon"></param>
        /// <param name="p_isHtml"></param>
        /// <returns></returns>
        public static string ConcatWithCaption(object p_left, object p_right, object p_caption, bool p_addSpace, bool p_addColon, bool p_isHtml)
        {
            string ret = String.Empty;
            string right = String.Empty;
            string caption = GetStringValue(p_caption);

            string htmlNewline = GetNewLine(true);
            string envNewline = GetNewLine(false);
            string newlineChar = GetNewLine(p_isHtml);

            if (!String.IsNullOrWhiteSpace(GetStringValue(p_right)))
            {
                if (!String.IsNullOrWhiteSpace(caption))
                {
                    if (p_addColon)
                    {
                        caption += ":";
                    }

                    if (p_addSpace)
                    {
                        caption += " ";
                    }
                }
                right = caption + p_right;
            }

            ret = ConcatWithLineBreak(p_left, right, p_isHtml);

            if (!String.IsNullOrWhiteSpace(ret))
            {
                ret = ret.Replace(htmlNewline, newlineChar).Replace(envNewline, newlineChar);
            }

            return ret;
        }

        #endregion append text

        #region string concatenation

        /// <summary>
        /// Get padding
        /// </summary>
        /// <param name="p_chars"></param>
        /// <param name="p_num"></param>
        /// <returns></returns>
        public static string GetPadding(string p_chars, int p_num)
        {
            return GetPadding(p_num, p_chars, false);
        }

        /// <summary>
        /// Get Spaces
        /// </summary>
        /// <param name="p_numspaces"></param>
        /// <param name="p_isHtml"></param>
        /// <returns></returns>
        public static string GetSpaces(int p_numspaces, bool p_isHtml)
        {
            string _paddedString = String.Empty;

            if (p_numspaces > 0)
            {
                for (int i = 0; i < p_numspaces; i++)
                {
                    _paddedString += GetSpace(p_isHtml);
                }
            }

            return _paddedString;
        }

        /// <summary>
        /// Get padding characters
        /// </summary>
        /// <param name="p_num"></param>
        /// <param name="p_padChars"></param>
        /// <param name="p_isHtml"></param>
        /// <returns></returns>
        public static string GetPadding(int p_num, string p_padChars, bool p_isHtml)
        {
            string ret = String.Empty;
            string chars = WTEDataHelper.GetSafeString(p_padChars, GetSpace(p_isHtml));

            if (!String.IsNullOrWhiteSpace(chars))
            {
                if (p_num > 0)
                {
                    for (int i = 0; i < p_num; i++)
                    {
                        ret += chars;
                    }
                }
            }
            return ret;
        }

        /// <summary>
        /// Get the correct "space" character
        /// </summary>
        /// <param name="p_isHtml"></param>
        /// <returns></returns>
        public static string GetSpace(bool p_isHtml)
        {
            if (p_isHtml)
            {
                return "&nbsp;";
            }
            else
            {
                return " ";
            }
        }

        /// <summary>
        /// Get the correct new line character
        /// </summary>
        /// <param name="p_isHtml"></param>
        /// <returns></returns>
        public static string GetNewLine(bool p_isHtml)
        {
            if (p_isHtml)
            {
                return "<br />";
            }
            else
            {
                //return "\r\n";
                return System.Environment.NewLine;
            }
        }

        #endregion string concatenation

        #region indent text

        /// <summary>
        /// Get indentation string
        /// </summary>
        /// <param name="p_level"></param>
        /// <returns></returns>
        public static string GetIndent(int p_level)
        {
            return GetIndent(p_level, null);
        }

        /// <summary>
        /// Get indentation string
        /// </summary>
        /// <param name="p_level">Indent level</param>
        /// <param name="p_indentSize">Indent size</param>
        /// <returns></returns>
        public static string GetIndent(int p_level, int? p_indentSize)
        {
            return GetIndent(p_level, p_indentSize, " ");
        }

        /// <summary>
        /// Get indentation string
        /// </summary>
        /// <param name="p_level"></param>
        /// <param name="p_indentSize"></param>
        /// <param name="p_indentString">string use as "indent"</param>
        /// <returns></returns>
        public static string GetIndent(int p_level, int? p_indentSize, string p_indentString)
        {
            StringBuilder sb = new StringBuilder();
            // indent 3 spaces as default
            int indentsize = p_indentSize.GetValueOrDefault(3);
            string indentString = !String.IsNullOrWhiteSpace(p_indentString) ? p_indentString : " ";
            int spaces = (int)(p_level * indentsize);
            for (int i = 0; i < spaces; i++)
            {
                sb.Append(indentString);
            }
            return sb.ToString();
        }

        /// <summary>
        /// Get indented text wit a line feed
        /// </summary>
        /// <param name="p_indentLevel"></param>
        /// <param name="p_lineData"></param>
        /// <returns></returns>
        public static string GetIndentedLine(int p_indentLevel, object p_lineData)
        {
            string ret = String.Empty;
            string indentedText = GetIndentedText(p_indentLevel, p_lineData);
            if (!String.IsNullOrWhiteSpace(indentedText))
            {
                ret = String.Format("{0}{1}", indentedText, Environment.NewLine);
            }
            return ret;
        }

        /// <summary>
        /// Get indented text without a line feed
        /// </summary>
        /// <param name="p_indentLevel"></param>
        /// <param name="p_data"></param>
        /// <returns></returns>
        public static string GetIndentedText(int p_indentLevel, object p_data)
        {
            return String.Format("{0}{1}", GetIndent(p_indentLevel), GetStringValue(p_data));
        }

        #endregion indent text

        #region string/text

        /// <summary>
        /// Get double quoted string
        /// </summary>
        /// <param name="p_obj"></param>
        /// <returns></returns>
        public static string GetDoubleQuotedString(object p_obj)
        {
            return GetWrappedString(GetStringValue(p_obj), "\"", "\"");
        }

        /// <summary>
        /// Get quoted string
        /// </summary>
        /// <param name="p_obj"></param>
        /// <returns></returns>
        public static string GetQuotedString(object p_obj)
        {
            return GetWrappedString(GetStringValue(p_obj), "'", "'");
        }

        /// <summary>
        /// Get wrapped string
        /// </summary>
        /// <param name="p_text"></param>
        /// <param name="p_startTag"></param>
        /// <param name="p_endTag"></param>
        /// <returns></returns>
        public static string GetWrappedString(string p_text, string p_startTag, string p_endTag)
        {
            return String.Format("{0}{1}{2}", p_startTag, p_text, p_endTag);
        }

        /// <summary>
        /// Get the object as formatted string
        /// </summary>
        /// <param name="p_obj"></param>
        /// <returns></returns>
        public static string GetStringValue(object p_obj)
        {
            string ret = String.Empty;

            if (p_obj != null)
            {
                if (p_obj.GetType() == typeof(decimal))
                {
                    ret = ((decimal)p_obj).ToString("#0.00");
                }
                else if (p_obj.GetType() == typeof(DateTime))
                {
                    ret = ((DateTime)p_obj).ToString("yyyy-MM-dd");
                }
                else
                {
                    ret = p_obj.ToString();
                }
            }

            return ret;
        }

        /// <summary>
        /// Get Html tag string
        /// </summary>
        /// <param name="p_text"></param>
        /// <param name="p_tag"></param>
        /// <returns></returns>
        public static string GetHTMLTagString(string p_text, string p_tag)
        {
            return GetHTMLTagString(p_text, p_tag, true);
        }

        /// <summary>
        /// Get Html tag string
        /// </summary>
        /// <param name="p_text"></param>
        /// <param name="p_tag"></param>
        /// <param name="p_isHtml"></param>
        /// <returns></returns>
        public static string GetHTMLTagString(string p_text, string p_tag, bool p_isHtml)
        {
            return GetHTMLTagString(p_text, p_tag, null, p_isHtml);
        }

        /// <summary>
        /// Get Html tag string
        /// </summary>
        /// <param name="p_text"></param>
        /// <param name="p_startTag"></param>
        /// <param name="p_endTag"></param>
        /// <returns></returns>
        public static string GetHTMLTagString(string p_text, string p_startTag, string p_endTag)
        {
            return GetHTMLTagString(p_text, p_startTag, p_endTag, true);
        }

        /// <summary>
        /// Get Html Tag wrapped string
        /// </summary>
        /// <param name="p_text"></param>
        /// <param name="p_startTag"></param>
        /// <param name="p_endTag"></param>
        /// <param name="p_isHtml"></param>
        /// <returns></returns>
        public static string GetHTMLTagString(string p_text, string p_startTag, string p_endTag, bool p_isHtml)
        {
            string ret = String.Empty;

            if (!String.IsNullOrWhiteSpace(p_text))
            {
                string start = String.Empty;
                string end = String.Empty;

                if (p_isHtml)
                {
                    if (!String.IsNullOrWhiteSpace(p_startTag))
                    {
                        start = String.Format("<{0}>", p_startTag);
                        if (String.IsNullOrWhiteSpace(p_endTag))
                        {
                            p_endTag = p_startTag;
                        }
                    }
                    if (!String.IsNullOrWhiteSpace(p_endTag))
                    {
                        end = String.Format("</{0}>", p_endTag);
                    }
                }

                ret = String.Format("{0}{1}{2}", start, p_text, end);
            }
            return ret;
        }

        #endregion string/text
    }
}