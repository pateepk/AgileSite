using System;
using System.Collections.Generic;
using WTE.Helpers;

namespace WTE
{
    /// <summary>
    /// Base class with utilties
    /// </summary>
    public class WTEBase
    {
        #region methods

        #region get values

        /// <summary>
        /// Safe cast an object to string
        /// </summary>
        /// <param name="p_obj"></param>
        /// <returns></returns>
        public static string GetString(object p_obj)
        {
            return WTEDataHelper.GetString(p_obj);
        }

        /// <summary>
        ///  Get String value
        /// </summary>
        /// <param name="p_obj"></param>
        /// <param name="p_default"></param>
        /// <returns></returns>
        public static string GetString(object p_obj, string p_default)
        {
            return WTEDataHelper.GetString(p_obj, p_default);
        }

        /// <summary>
        /// Safe cast an object to nullable bool
        /// </summary>
        /// <param name="p_obj"></param>
        /// <returns></returns>
        public static bool? GetBool(object p_obj)
        {
            return WTEDataHelper.GetBool(p_obj);
        }

        /// <summary>
        /// Get bool value
        /// </summary>
        /// <param name="p_obj"></param>
        /// <param name="p_default"></param>
        /// <returns></returns>
        public static bool? GetBool(object p_obj, bool? p_default)
        {
            return WTEDataHelper.GetBool(p_obj, p_default);
        }

        /// <summary>
        /// Safe cast an object to nullable int
        /// </summary>
        /// <param name="p_obj"></param>
        /// <returns></returns>
        public static int? GetInt(object p_obj)
        {
            return WTEDataHelper.GetInt(p_obj);
        }

        /// <summary>
        /// Safe cast an object to nullable int
        /// </summary>
        /// <param name="p_obj"></param>
        /// <param name="p_default"></param>
        /// <returns></returns>
        public static int? GetInt(object p_obj, int? p_default)
        {
            return WTEDataHelper.GetInt(p_obj, p_default);
        }

        /// <summary>
        /// Get decimal
        /// </summary>
        /// <param name="p_obj"></param>
        /// <returns></returns>
        public static decimal? GetDecimal(object p_obj)
        {
            return WTEDataHelper.GetDecimal(p_obj);
        }

        /// <summary>
        /// Get decimal
        /// </summary>
        /// <param name="p_obj"></param>
        /// <param name="p_default"></param>
        /// <returns></returns>
        public static decimal? GetDecimal(object p_obj, decimal? p_default)
        {
            return WTEDataHelper.GetDecimal(p_obj, p_default);
        }

        /// <summary>
        /// Get DateTime? value
        /// </summary>
        /// <param name="p_obj"></param>
        /// <returns></returns>
        public static DateTime? GetDateTime(object p_obj)
        {
            return WTEDataHelper.GetDateTime(p_obj, null);
        }

        /// <summary>
        /// Get date time value
        /// </summary>
        /// <param name="p_obj"></param>
        /// <param name="p_default"></param>
        /// <returns></returns>
        public static DateTime? GetDateTime(object p_obj, DateTime? p_default)
        {
            return WTEDataHelper.GetDateTime(p_obj, p_default);
        }

        #endregion get values

        #region safe values

        /// <summary>
        /// Convert object to string
        /// </summary>
        /// <param name="p_obj"></param>
        /// <returns></returns>
        public static string GetSafeString(object p_obj)
        {
            return WTEDataHelper.GetSafeString(p_obj);
        }

        /// <summary>
        /// Convert object to string
        /// </summary>
        /// <param name="p_obj"></param>
        /// <param name="p_default"></param>
        /// <returns></returns>
        public static string GetSafeString(object p_obj, string p_default)
        {
            return WTEDataHelper.GetSafeString(p_obj, p_default);
        }

        /// <summary>
        /// Get string value or null
        /// </summary>
        /// <param name="p_obj"></param>
        /// <returns></returns>
        public static string GetStringValueOrNull(object p_obj)
        {
            return WTEDataHelper.GetStringValueOrNull(p_obj);
        }

        /// <summary>
        /// Convert object to boolean
        /// </summary>
        /// <param name="p_obj"></param>
        /// <returns></returns>
        public static bool GetSafeBool(object p_obj)
        {
            return WTEDataHelper.GetSafeBool(p_obj);
        }

        /// <summary>
        /// Get safe bool with default
        /// </summary>
        /// <param name="p_obj"></param>
        /// <param name="p_default"></param>
        /// <returns></returns>
        public static bool GetSafeBool(object p_obj, bool p_default)
        {
            return WTEDataHelper.GetSafeBool(p_obj, p_default);
        }

        /// <summary>
        /// Get decimal or null (if value is false)
        /// </summary>
        /// <param name="p_obj"></param>
        /// <returns></returns>
        public static bool? GetBoolValueOrNull(object p_obj)
        {
            return WTEDataHelper.GetBoolValueOrNull(p_obj);
        }

        /// <summary>
        /// Convert object to int
        /// </summary>
        /// <param name="p_obj"></param>
        /// <returns></returns>
        public static int GetSafeInt(object p_obj)
        {
            return WTEDataHelper.GetSafeInt(p_obj);
        }

        /// <summary>
        /// Get safe int with default
        /// </summary>
        /// <param name="p_obj"></param>
        /// <param name="p_default"></param>
        /// <returns></returns>
        public static int GetSafeInt(object p_obj, int p_default)
        {
            return WTEDataHelper.GetSafeInt(p_obj, p_default);
        }

        /// <summary>
        /// Get int or null (if value is 0)
        /// </summary>
        /// <param name="p_obj"></param>
        /// <returns>null if failed to parse or if value is 0</returns>
        public static int? GetIntValueOrNull(object p_obj)
        {
            return WTEDataHelper.GetIntValueOrNull(p_obj);
        }

        /// <summary>
        /// Convert object to decimal
        /// </summary>
        /// <param name="p_obj"></param>
        /// <returns></returns>
        public static decimal GetSafeDecimal(object p_obj)
        {
            return WTEDataHelper.GetSafeDecimal(p_obj);
        }

        /// <summary>
        /// Get safe decimal with default
        /// </summary>
        /// <param name="p_obj"></param>
        /// <param name="p_default"></param>
        /// <returns></returns>
        public static decimal GetSafeDecimal(object p_obj, decimal p_default)
        {
            return WTEDataHelper.GetSafeDecimal(p_obj, p_default);
        }

        /// <summary>
        /// Get decimal or null (if value is 0)
        /// </summary>
        /// <param name="p_obj"></param>
        /// <returns></returns>
        public static decimal? GetDecimalValueOrNull(object p_obj)
        {
            return WTEDataHelper.GetDecimalValueOrNull(p_obj);
        }

        /// <summary>
        /// Convert object to date time
        /// </summary>
        /// <param name="p_obj"></param>
        /// <returns></returns>
        public static DateTime GetSafeDateTime(object p_obj)
        {
            return WTEDataHelper.GetSafeDateTime(p_obj, DateTime.Now);
        }

        /// <summary>
        /// Get safe date time with default
        /// </summary>
        /// <param name="p_obj"></param>
        /// <param name="p_default"></param>
        /// <returns></returns>
        public static DateTime GetSafeDateTime(object p_obj, DateTime p_default)
        {
            return WTEDataHelper.GetSafeDateTime(p_obj, p_default);
        }

        /// <summary>
        /// Get date time value or null!
        /// </summary>
        /// <param name="p_obj"></param>
        /// <returns></returns>
        public static DateTime? GetDateTimeValueOrNull(object p_obj)
        {
            return WTEDataHelper.GetDateTime(p_obj, null);
        }

        #endregion safe values

        #region html decode/encodde

        /// <summary>
        /// Get html decoded string
        /// </summary>
        /// <param name="p_text"></param>
        /// <returns></returns>
        public static string GetHtmlDecodedText(object p_text)
        {
            return WTEHttpHelper.GetHtmlDecodedText(p_text);
        }

        /// <summary>
        /// Get HtmlDecode Text with length limit (null safe)
        /// </summary>
        /// <param name="p_text"></param>
        /// <param name="p_lengthLimit">use 0 or negative number for no limit</param>
        /// <returns></returns>
        public static string GetHtmlDecodedText(object p_text, int? p_lengthLimit)
        {
            return WTEHttpHelper.GetHtmlDecodedText(p_text, p_lengthLimit);
        }

        /// <summary>
        /// Get html encoded string
        /// </summary>
        /// <param name="p_text"></param>
        /// <returns></returns>
        public static string GetHtmlEncodedText(object p_text)
        {
            return WTEHttpHelper.GetHtmlEncodedText(p_text);
        }

        #endregion html decode/encodde

        #region url encoding/decoding

        /// <summary>
        /// Encode string to the proper URL encoding (null safe)
        /// </summary>
        /// <param name="p_url"></param>
        /// <returns></returns>
        public static string EncodeUrl(string p_url)
        {
            return WTEHttpHelper.EncodeUrl(p_url);
        }

        /// <summary>
        /// Decode the Url to normal text (null safe)
        /// </summary>
        /// <param name="p_url"></param>
        /// <returns></returns>
        public static string DecodeUrl(string p_url)
        {
            return WTEHttpHelper.DecodeUrl(p_url);
        }

        #endregion url encoding/decoding

        #region clean string

        /// <summary>
        /// Get safe string for XML or XSLT transform
        /// </summary>
        /// <param name="p_inString"></param>
        /// <param name="p_trim"></param>
        /// <returns></returns>
        public static string GetXMLSafeString(string p_inString, bool p_trim)
        {
            return WTEHttpHelper.GetXMLSafeString(p_inString, p_trim);
        }

        /// <summary>
        /// Replace all symbols with "-"
        /// </summary>
        /// <param name="p_inURL"></param>
        /// <returns></returns>
        public static string GetURLfriendlyString(string p_inURL)
        {
            return WTEHttpHelper.GetURLfriendlyString(p_inURL);
        }

        /// <summary>
        /// Get clean string (null safe)
        /// </summary>
        /// <param name="p_inString"></param>
        /// <returns></returns>
        public static string CleanString(string p_inString)
        {
            return WTEHttpHelper.CleanString(p_inString);
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
            return WTEHttpHelper.CleanString(p_inString, p_removeControlCharacters, p_trim);
        }

        /// <summary>
        /// Remove all html tags from a string
        /// </summary>
        /// <param name="p_inString"></param>
        /// <param name="p_replaceChar"></param>
        /// <returns></returns>
        public static string RemoveHtmlCode(string p_inString, string p_replaceChar)
        {
            return WTEHttpHelper.RemoveHtmlCode(p_inString, p_replaceChar);
        }

        /// <summary>
        /// Get filename friendly string
        /// </summary>
        /// <param name="p_instring"></param>
        /// <returns></returns>
        public static string GetFileNameFriendlyString(string p_instring)
        {
            return WTEHttpHelper.GetFileNameFriendlyString(p_instring);
        }

        #endregion clean string

        #region text helpers

        /// <summary>
        /// Get indentation string
        /// </summary>
        /// <param name="p_level"></param>
        /// <returns></returns>
        protected virtual string GetIndent(int p_level)
        {
            return WTETextHelper.GetIndent(p_level, null);
        }

        /// <summary>
        /// Get indentation string
        /// </summary>
        /// <param name="p_level">Indent level</param>
        /// <param name="p_indentSize">Indent size</param>
        /// <returns></returns>
        protected string GetIndent(int p_level, int? p_indentSize)
        {
            return WTETextHelper.GetIndent(p_level, p_indentSize, " ");
        }

        /// <summary>
        /// Get indentation string
        /// </summary>
        /// <param name="p_level"></param>
        /// <param name="p_indentSize"></param>
        /// <param name="p_indentString">string use as "indent"</param>
        /// <returns></returns>
        protected string GetIndent(int p_level, int? p_indentSize, string p_indentString)
        {
            return WTETextHelper.GetIndent(p_level, p_indentSize, p_indentString);
        }

        /// <summary>
        /// Get indented text wit a line feed
        /// </summary>
        /// <param name="p_indentLevel"></param>
        /// <param name="p_lineData"></param>
        /// <returns></returns>
        protected string GetIndentedLine(int p_indentLevel, object p_lineData)
        {
            return WTETextHelper.GetIndentedLine(p_indentLevel, p_lineData);
        }

        /// <summary>
        /// Get quoted string
        /// </summary>
        /// <param name="p_obj"></param>
        /// <returns></returns>
        protected string GetQuotedString(object p_obj)
        {
            return WTETextHelper.GetQuotedString(p_obj);
        }

        /// <summary>
        /// Get double quoted string
        /// </summary>
        /// <param name="p_obj"></param>
        /// <returns></returns>
        protected string GetDoubleQuotedString(object p_obj)
        {
            return WTETextHelper.GetDoubleQuotedString(p_obj);
        }

        /// <summary>
        /// Get wrapped string
        /// </summary>
        /// <param name="p_text"></param>
        /// <param name="p_startTag"></param>
        /// <param name="p_endTag"></param>
        /// <returns></returns>
        protected string GetWrappedString(string p_text, string p_startTag, string p_endTag)
        {
            return WTETextHelper.GetWrappedString(p_text, p_startTag, p_endTag);
        }

        /// <summary>
        /// Get the object as formatted string
        /// </summary>
        /// <param name="p_obj"></param>
        /// <returns></returns>
        protected string GetStringValue(object p_obj)
        {
            return WTETextHelper.GetStringValue(p_obj);
        }

        #endregion text helpers

        #region math

        /// <summary>
        /// Round a decimal
        /// </summary>
        /// <param name="p_number"></param>
        /// <param name="p_decimalPoints"></param>
        /// <returns></returns>
        public static decimal Round(decimal p_number, int p_decimalPoints)
        {
            return WTEDataHelper.Round(p_number, p_decimalPoints);
        }

        /// <summary>
        /// Round a double
        /// </summary>
        /// <param name="p_number"></param>
        /// <param name="p_decimalPoints"></param>
        /// <returns></returns>
        public static double Round(double p_number, int p_decimalPoints)
        {
            return WTEDataHelper.Round(p_number, p_decimalPoints);
        }

        #endregion math

        #region utilities

        /// <summary>
        /// Get bitwise int from an object
        /// </summary>
        /// <param name="p_valueSet"></param>
        /// <returns></returns>
        public static int GetBitWiseInt(Object p_valueSet)
        {
            return WTEDataHelper.GetBitWiseInt(p_valueSet);
        }

        /// <summary>
        /// Get hashset from a comma delimited string
        /// </summary>
        /// <param name="p_valueSet"></param>
        /// <returns></returns>
        public static HashSet<int> GetIntHashSet(Object p_valueSet)
        {
            return WTEDataHelper.GetIntHashSet(p_valueSet);
        }

        /// <summary>
        /// Get int list
        /// </summary>
        /// <param name="p_valueSet"></param>
        /// <returns></returns>
        public static List<int> GetIntList(Object p_valueSet)
        {
            return WTEDataHelper.GetIntList(p_valueSet);
        }

        /// <summary>
        /// Check ID to see if it's an int.
        /// </summary>
        /// <param name="p_keywordList"></param>
        /// <returns></returns>
        public static bool ValidateIdString(Object p_keywordList)
        {
            return WTEDataHelper.ValidateIdString(p_keywordList);
        }

        /// <summary>
        /// Get display keyword string
        /// </summary>
        /// <param name="p_keywordList"></param>
        /// <returns></returns>
        public static string GetDisplayKeywordString(Object p_keywordList)
        {
            return WTEDataHelper.GetKeywordString(p_keywordList, ",", false, true);
        }

        /// <summary>
        /// Get keyword string
        /// </summary>
        /// <param name="p_keywordList"></param>
        /// <returns></returns>
        public static string GetKeywordString(Object p_keywordList)
        {
            return WTEDataHelper.GetKeywordString(p_keywordList, ",", false, false);
        }

        /// <summary>
        /// Get keyword string
        /// </summary>
        /// <param name="p_keywordList"></param>
        /// <param name="p_delimiter"></param>
        /// <param name="p_addQuotation"></param>
        /// <returns></returns>
        public static string GetKeywordString(Object p_keywordList, string p_delimiter, bool p_addQuotation)
        {
            return WTEDataHelper.GetKeywordString(p_keywordList, p_delimiter, p_addQuotation, false);
        }

        /// <summary>
        /// format keyword string to a proper string with delimiter
        /// </summary>
        /// <param name="p_keywordList"></param>
        /// <param name="p_delimiter"></param>
        /// <param name="p_addQuotation"></param>
        /// <param name="p_addSpace"></param>
        /// <returns></returns>
        public static string GetKeywordString(Object p_keywordList, string p_delimiter, bool p_addQuotation, bool p_addSpace)
        {
            return WTEDataHelper.GetKeywordString(p_keywordList, p_delimiter, p_addQuotation, p_addSpace);
        }

        /// <summary>
        /// Create keyword list from 2 objects
        /// </summary>
        /// <param name="p_list1"></param>
        /// <param name="p_list2"></param>
        /// <returns></returns>
        public static List<string> JoinKeyWordList(Object p_list1, Object p_list2)
        {
            return WTEDataHelper.JoinKeyWordList(p_list1, p_list2, false);
        }

        /// <summary>
        /// Create keyword list from 2 objects
        /// </summary>
        /// <param name="p_list1"></param>
        /// <param name="p_list2"></param>
        /// <param name="p_filterZero"></param>
        /// <returns></returns>
        public static List<string> JoinKeyWordList(Object p_list1, Object p_list2, bool p_filterZero)
        {
            return WTEDataHelper.JoinKeyWordList(p_list1, p_list2, p_filterZero);
        }

        /// <summary>
        /// Get a list of string from a string value.
        /// </summary>
        /// <param name="p_keywordList"></param>
        /// <returns></returns>
        public static List<string> GetKeywordList(Object p_keywordList)
        {
            return WTEDataHelper.GetKeywordList(p_keywordList);
        }

        #endregion utilities

        #endregion methods
    }
}