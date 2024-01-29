using System;
using System.Collections.Generic;

namespace WTE.Helpers
{
    /// <summary>
    /// Data manager helper
    /// </summary>
    public class WTEDataHelper 
    {
        #region get values

        /// <summary>
        /// Safe cast an object to string
        /// </summary>
        /// <param name="p_obj"></param>
        /// <returns></returns>
        public static string GetString(object p_obj)
        {
            return GetString(p_obj, null);
        }

        /// <summary>
        ///  Get String value
        /// </summary>
        /// <param name="p_obj"></param>
        /// <param name="p_default"></param>
        /// <returns></returns>
        public static string GetString(object p_obj, string p_default)
        {
            string ret = null;

            if (p_obj != null)
            {
                if (p_obj is Enum)
                {
                    ret = ((int)(p_obj)).ToString();
                }
                else
                {
                    ret = p_obj.ToString();
                }
            }

            // clean it up
            if (String.IsNullOrWhiteSpace(ret))
            {
                ret = p_default;
            }

            return ret;
        }

        /// <summary>
        /// Safe cast an object to nullable bool
        /// </summary>
        /// <param name="p_obj"></param>
        /// <returns></returns>
        public static bool? GetBool(object p_obj)
        {
            return GetBool(p_obj, null);
        }

        /// <summary>
        /// Get boolean value with default
        /// </summary>
        /// <param name="p_obj"></param>
        /// <param name="p_default"></param>
        /// <returns></returns>
        public static bool? GetBool(object p_obj, bool? p_default)
        {
            bool? ret = null;
            string val = GetString(p_obj);
            if (!String.IsNullOrWhiteSpace(val))
            {
                // the setting could be bool (true/false) or (0/1)
                bool b = false;
                if (bool.TryParse(val, out b))
                {
                    ret = b;
                }
                else
                {
                    // failed, try parsing it as int
                    int num = 0;
                    if (int.TryParse(val, out num))
                    {
                        ret = (num != 0);
                    }
                }
            }
            if (ret == null)
            {
                ret = p_default;
            }
            return ret;
        }

        /// <summary>
        /// Safe cast an object to nullable int
        /// </summary>
        /// <param name="p_obj"></param>
        /// <returns></returns>
        public static int? GetInt(object p_obj)
        {
            return GetInt(p_obj, null);
        }

        /// <summary>
        /// Safe cast an object to nullable int
        /// </summary>
        /// <param name="p_obj"></param>
        /// <param name="p_default"></param>
        /// <returns></returns>
        public static int? GetInt(object p_obj, int? p_default)
        {
            int? ret = null;
            string svalue = GetString(p_obj);
            if (!String.IsNullOrWhiteSpace(svalue))
            {
                int num = 0;
                if (int.TryParse(svalue, out num))
                {
                    ret = num;
                }
            }

            if (ret == null)
            {
                ret = p_default;
            }

            return ret;
        }

        /// <summary>
        /// Get decimal
        /// </summary>
        /// <param name="p_obj"></param>
        /// <returns></returns>
        public static decimal? GetDecimal(object p_obj)
        {
            return GetDecimal(p_obj, null);
        }

        /// <summary>
        /// Get decimal
        /// </summary>
        /// <param name="p_obj"></param>
        /// <param name="p_default"></param>
        /// <returns></returns>
        public static decimal? GetDecimal(object p_obj, decimal? p_default)
        {
            decimal? ret = null;
            string svalue = GetString(p_obj);
            if (!String.IsNullOrWhiteSpace(svalue))
            {
                decimal num = 0;
                if (decimal.TryParse(svalue, out num))
                {
                    ret = num;
                }
            }

            if (ret == null)
            {
                ret = p_default;
            }

            return ret;
        }

        /// <summary>
        /// Get DateTime? value
        /// </summary>
        /// <param name="p_obj"></param>
        /// <returns></returns>
        public static DateTime? GetDateTime(object p_obj)
        {
            return GetDateTime(p_obj, null);
        }

        /// <summary>
        /// Get date time value
        /// </summary>
        /// <param name="p_obj"></param>
        /// <param name="p_default"></param>
        /// <returns></returns>
        public static DateTime? GetDateTime(object p_obj, DateTime? p_default)
        {
            DateTime? ret = null;
            string val = GetString(p_obj);

            if (!String.IsNullOrWhiteSpace(val))
            {
                // default date is 01/01/1776

                // the minimum date for MS SQL server is January 1, 1753 for the DATETIME class
                // however DATETIME2 class does not have this limitation.

                DateTime d = DateTime.Now;
                if (DateTime.TryParse(val, out d))
                {
                    ret = d;
                }
                else
                {
                    ret = null;
                }
            }

            if (ret == null)
            {
                ret = p_default;
            }

            //return null if no date
            return ret;
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
            return GetSafeString(p_obj, String.Empty);
        }

        /// <summary>
        /// Convert object to string
        /// </summary>
        /// <param name="p_obj"></param>
        /// <param name="p_default"></param>
        /// <returns></returns>
        public static string GetSafeString(object p_obj, string p_default)
        {
            string output = p_default;
            string value = GetString(p_obj, p_default);
            if (!String.IsNullOrWhiteSpace(value))
            {
                output = value.Trim();
            }
            return output;
        }

        /// <summary>
        /// Get string value or null
        /// </summary>
        /// <param name="p_obj"></param>
        /// <returns></returns>
        public static string GetStringValueOrNull(object p_obj)
        {
            string output = null;
            string value = GetString(p_obj);
            if (!String.IsNullOrWhiteSpace(value))
            {
                output = value;
            }
            return output;
        }

        /// <summary>
        /// Convert object to boolean
        /// </summary>
        /// <param name="p_obj"></param>
        /// <returns></returns>
        public static bool GetSafeBool(object p_obj)
        {
            return GetSafeBool(p_obj, false);
        }

        /// <summary>
        /// Get safe bool with default
        /// </summary>
        /// <param name="p_obj"></param>
        /// <param name="p_default"></param>
        /// <returns></returns>
        public static bool GetSafeBool(object p_obj, bool p_default)
        {
            bool ret = p_default;
            bool? value = GetBool(p_obj, p_default);
            if (value.HasValue)
            {
                ret = value.Value;
            }
            return ret;
        }

        /// <summary>
        /// Get decimal or null (if value is false)
        /// </summary>
        /// <param name="p_obj"></param>
        /// <returns></returns>
        public static bool? GetBoolValueOrNull(object p_obj)
        {
            bool? ret = GetBool(p_obj);
            return ret;
        }

        /// <summary>
        /// Convert object to int
        /// </summary>
        /// <param name="p_obj"></param>
        /// <returns></returns>
        public static int GetSafeInt(object p_obj)
        {
            return GetSafeInt(p_obj, 0);
        }

        /// <summary>
        /// Get safe int with default
        /// </summary>
        /// <param name="p_obj"></param>
        /// <param name="p_default"></param>
        /// <returns></returns>
        public static int GetSafeInt(object p_obj, int p_default)
        {
            int output = p_default;
            int? value = GetInt(p_obj, p_default);
            if (value.HasValue)
            {
                output = value.Value;
            }
            return output;
        }

        /// <summary>
        /// Get int or null (if value is 0)
        /// </summary>
        /// <param name="p_obj"></param>
        /// <returns>null if failed to parse or if value is 0</returns>
        public static int? GetIntValueOrNull(object p_obj)
        {
            int? output = null;
            int value = GetSafeInt(p_obj);
            if (value > 0)
            {
                output = value;
            }
            return output;
        }

        /// <summary>
        /// Convert object to decimal
        /// </summary>
        /// <param name="p_obj"></param>
        /// <returns></returns>
        public static decimal GetSafeDecimal(object p_obj)
        {
            return GetSafeDecimal(p_obj, 0);
        }

        /// <summary>
        /// Get safe decimal with default
        /// </summary>
        /// <param name="p_obj"></param>
        /// <param name="p_default"></param>
        /// <returns></returns>
        public static decimal GetSafeDecimal(object p_obj, decimal p_default)
        {
            decimal output = p_default;
            decimal? value = GetDecimal(p_obj, p_default);
            if (value.HasValue)
            {
                output = value.Value;
            }
            return output;
        }

        /// <summary>
        /// Get decimal or null (if value is 0)
        /// </summary>
        /// <param name="p_obj"></param>
        /// <returns></returns>
        public static decimal? GetDecimalValueOrNull(object p_obj)
        {
            decimal? output = null;
            decimal value = GetSafeDecimal(p_obj);
            if (value > 0)
            {
                output = value;
            }
            return output;
        }

        /// <summary>
        /// Convert object to date time
        /// </summary>
        /// <param name="p_obj"></param>
        /// <returns></returns>
        public static DateTime GetSafeDateTime(object p_obj)
        {
            return GetSafeDateTime(p_obj, DateTime.Now);
        }

        /// <summary>
        /// Get safe date time with default
        /// </summary>
        /// <param name="p_obj"></param>
        /// <param name="p_default"></param>
        /// <returns></returns>
        public static DateTime GetSafeDateTime(object p_obj, DateTime p_default)
        {
            DateTime output = p_default;
            DateTime? value = GetDateTimeValueOrNull(p_obj);
            if (value.HasValue)
            {
                output = value.Value;
            }

            // the minimum date for MS SQL server is January 1, 1753 for the DATETIME class
            // however DATETIME2 class does not have this limitation.

            // clean up the value so that it is ms sql safe.
            if (output.Year < 1900)
            {
                output.AddYears(1900 - output.Year);
            }

            return output;
        }

        /// <summary>
        /// Get date time value or null!
        /// </summary>
        /// <param name="p_obj"></param>
        /// <returns></returns>
        public static DateTime? GetDateTimeValueOrNull(object p_obj)
        {
            DateTime? output = GetDateTime(p_obj, null);
            return output;
        }

        #endregion safe values

        #region math

        /// <summary>
        /// Round a decimal
        /// </summary>
        /// <param name="p_number"></param>
        /// <param name="p_decimalPoints"></param>
        /// <returns></returns>
        public static decimal Round(decimal p_number, int p_decimalPoints)
        {
            return Convert.ToDecimal(Round(Convert.ToDouble(p_number), p_decimalPoints));
        }

        /// <summary>
        /// Round a double
        /// </summary>
        /// <param name="p_number"></param>
        /// <param name="p_decimalPoints"></param>
        /// <returns></returns>
        public static double Round(double p_number, int p_decimalPoints)
        {
            double decimalPowerOfTen = Math.Pow(10, p_decimalPoints);
            return Math.Floor(p_number * decimalPowerOfTen + 0.5) / decimalPowerOfTen;
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
            int val = 0;
            HashSet<int> valueSet = GetIntHashSet(p_valueSet);
            foreach (int v in valueSet)
            {
                val += v;
            }
            return val;
        }

        /// <summary>
        /// Get hashset from a comma delimited string
        /// </summary>
        /// <param name="p_valueSet"></param>
        /// <returns></returns>
        public static HashSet<int> GetIntHashSet(Object p_valueSet)
        {
            HashSet<int> ret = new HashSet<int>();
            if (p_valueSet.GetType() == typeof(HashSet<int>))
            {
                ret = (HashSet<int>)p_valueSet;
            }
            else
            {
                List<string> ids = GetKeywordList(p_valueSet);
                foreach (string id in ids)
                {
                    ret.Add(GetSafeInt(ids));
                }
            }
            return ret;
        }

        /// <summary>
        /// Get int list
        /// </summary>
        /// <param name="p_valueSet"></param>
        /// <returns></returns>
        public static List<int> GetIntList(Object p_valueSet)
        {
            List<int> ret = new List<int>();

            if (p_valueSet != null)
            {
                if (p_valueSet.GetType() == typeof(List<int>))
                {
                    ret = (List<int>)p_valueSet;
                }
                else
                {
                    List<string> ids = GetKeywordList(p_valueSet);
                    foreach (string id in ids)
                    {
                        ret.Add(GetSafeInt(id));
                    }
                }
            }

            return ret;
        }

        /// <summary>
        /// Check ID to see if it's an int.
        /// </summary>
        /// <param name="p_keywordList"></param>
        /// <returns></returns>
        public static bool ValidateIdString(Object p_keywordList)
        {
            bool valid = true;
            List<string> ids = GetKeywordList(p_keywordList);
            foreach (string id in ids)
            {
                int val = 0;
                if (!int.TryParse(id, out val))
                {
                    valid = false;
                    break;
                }
            }
            return valid;
        }

        /// <summary>
        /// Get display keyword string
        /// </summary>
        /// <param name="p_keywordList"></param>
        /// <returns></returns>
        public static string GetDisplayKeywordString(Object p_keywordList)
        {
            return GetKeywordString(p_keywordList, ",", false, true);
        }

        /// <summary>
        /// Get keyword string
        /// </summary>
        /// <param name="p_keywordList"></param>
        /// <returns></returns>
        public static string GetKeywordString(Object p_keywordList)
        {
            return GetKeywordString(p_keywordList, ",", false, false);
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
            return GetKeywordString(p_keywordList, p_delimiter, p_addQuotation, false);
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
            string ret = String.Empty;
            string delimiter = ",";
            if (p_keywordList != null)
            {
                if (!String.IsNullOrWhiteSpace(p_delimiter))
                {
                    delimiter = p_delimiter.Trim();
                }

                List<string> keywords = GetKeywordList(p_keywordList);
                foreach (string keyword in keywords)
                {
                    if (!String.IsNullOrWhiteSpace(keyword))
                    {
                        if (!String.IsNullOrWhiteSpace(ret))
                        {
                            ret += delimiter;
                            if (p_addSpace)
                            {
                                ret += " ";
                            }
                        }
                        if (p_addQuotation)
                        {
                            ret += String.Format("'{0}'", keyword.Trim());
                        }
                        else
                        {
                            ret += keyword.Trim();
                        }
                    }
                }
            }
            return ret;
        }

        /// <summary>
        /// Create keyword list from 2 objects
        /// </summary>
        /// <param name="p_list1"></param>
        /// <param name="p_list2"></param>
        /// <returns></returns>
        public static List<string> JoinKeyWordList(Object p_list1, Object p_list2)
        {
            return JoinKeyWordList(p_list1, p_list2, false);
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
            List<string> ret = new List<string>();
            List<string> list1 = GetKeywordList(p_list1);
            List<string> list2 = GetKeywordList(p_list2);

            if (list1.Count > 0)
            {
                foreach (string item in list1)
                {
                    if (!p_filterZero || item != "0")
                    {
                        if (!ret.Contains(item))
                        {
                            ret.Add(item);
                        }
                    }
                }
            }

            if (list2.Count > 0)
            {
                foreach (string item in list2)
                {
                    if (!p_filterZero || item != "0")
                    {
                        if (!ret.Contains(item))
                        {
                            ret.Add(item);
                        }
                    }
                }
            }
            return ret;
        }

        /// <summary>
        /// Get a list of string from a string value.
        /// </summary>
        /// <param name="p_keywordList"></param>
        /// <returns></returns>
        public static List<string> GetKeywordList(Object p_keywordList)
        {
            List<string> keywordList = new List<string>();
            if (p_keywordList != null)
            {
                if (p_keywordList.GetType() == typeof(List<string>))
                {
                    // return the keyword list
                    keywordList = (List<string>)p_keywordList;
                }
                else if (p_keywordList.GetType() == typeof(HashSet<int>))
                {
                    // add each item to the collection
                    foreach (int keyword in (HashSet<int>)p_keywordList)
                    {
                        string valueString = GetSafeString(keyword);
                        if (!keywordList.Contains(valueString))
                        {
                            keywordList.Add(valueString);
                        }
                    }
                }
                else
                {
                    #region any other type, convert to string parse and add each item to the list

                    string tempValue = GetSafeString(p_keywordList);
                    if (!String.IsNullOrWhiteSpace(tempValue))
                    {
                        tempValue = tempValue.Replace("\\", "|").Replace("/", "|").Replace(" ", "|").Replace(",", "|").Trim();
                        string[] tempKeywords = tempValue.Split('|');
                        foreach (string keyword in tempKeywords)
                        {
                            if (!String.IsNullOrWhiteSpace(keyword))
                            {
                                if (!keywordList.Contains(keyword.Trim()))
                                {
                                    keywordList.Add(keyword.Trim());
                                }
                            }
                        }
                    }

                    #endregion any other type, convert to string parse and add each item to the list
                }
            }
            return keywordList;
        }

        /// <summary>
        /// Check to see if the an object is nullable
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="p_object"></param>
        /// <returns></returns>
        public static bool IsNullable<T>(T p_object)
        {
            if (!typeof(T).IsGenericType)
                return false;
            return typeof(T).GetGenericTypeDefinition() == typeof(Nullable<>);
        }

        /// <summary>
        /// Get value with default
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="p_value"></param>
        /// <param name="p_default"></param>
        /// <returns></returns>
        public static T GetDefaultValue<T>(T p_value, T p_default)
        {
            T ret = p_value;

            if (p_value == null)
            {
                // null just set to default
                ret = p_default;
            }
            else
            {
                Type t = p_value.GetType();
                if ((t == typeof(String)) || (t == typeof(string)))
                {
                    if (String.IsNullOrWhiteSpace(p_value.ToString()))
                    {
                        ret = p_default;
                    }
                    else
                    {
                        ret = p_value;
                    }
                }
                else
                {
                    ret = p_value;
                }
            }

            return ret;
        }

        #endregion utilities
    }
}