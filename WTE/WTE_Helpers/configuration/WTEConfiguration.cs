using System;
using System.Collections.Generic;
using System.Configuration;

namespace WTE.Configuration
{
    public class WTEConfiguration : WTEBase
    {
        #region specific settings

        /// <summary>
        /// Is this a console application?
        /// </summary>
        public static bool IsConsoleApplication
        {
            get
            {
                return GetBoolAppSetting("IsConsoleApplication", false);
            }
        }

        #endregion specific settings

        #region get local app settings

        #region boolean

        /// <summary>
        /// Get bool Local App setting
        /// </summary>
        /// <param name="p_key"></param>
        /// <param name="p_default"></param>
        /// <returns></returns>
        public static bool GetBoolAppSetting(string p_key, bool p_default)
        {
            return GetSafeBool(GetAppSettingBool(p_key, p_default));
        }

        /// <summary>
        /// Get local app settings as nullable bool
        /// </summary>
        /// <param name="p_key"></param>
        /// <returns></returns>
        public static bool? GetAppSettingBool(string p_key)
        {
            return GetBool(GetAppSettingString(p_key), null);
        }

        /// <summary>
        /// Get local app setting value as nullable int with default
        /// </summary>
        /// <param name="p_key"></param>
        /// <param name="p_default"></param>
        /// <returns></returns>
        public static bool? GetAppSettingBool(string p_key, bool? p_default)
        {
            return GetBool(GetAppSettingString(p_key), p_default);
        }

        #endregion boolean

        #region int

        /// <summary>
        /// Get int local app setting
        /// </summary>
        /// <param name="p_key"></param>
        /// <param name="p_default"></param>
        /// <returns></returns>
        public static int GetIntAppSetting(string p_key, int p_default)
        {
            return GetSafeInt(GetAppSettingInt(p_key, p_default));
        }

        /// <summary>
        /// Get local app setting value as nullable int
        /// </summary>
        /// <param name="p_key"></param>
        /// <returns></returns>
        public static int? GetAppSettingInt(string p_key)
        {
            return GetInt(GetAppSettingString(p_key), null);
        }

        /// <summary>
        /// Get local app setting value as nullable int with default
        /// </summary>
        /// <param name="p_key"></param>
        /// <param name="p_default"></param>
        /// <returns></returns>
        public static int? GetAppSettingInt(string p_key, int? p_default)
        {
            return GetInt(GetAppSettingString(p_key), p_default);
        }

        #endregion int

        #region datetime

        /// <summary>
        /// Get date time app setting
        /// </summary>
        /// <param name="p_key"></param>
        /// <param name="p_default"></param>
        /// <returns></returns>
        public static DateTime GetDateTimeAppSetting(string p_key, DateTime p_default)
        {
            return GetSafeDateTime(GetAppSettingDateTime(p_key), p_default);
        }

        /// <summary>
        /// Get Local app setting key as DateTime
        /// </summary>
        /// <param name="p_key"></param>
        /// <returns></returns>
        public static DateTime? GetAppSettingDateTime(string p_key)
        {
            return GetAppSettingDateTime(p_key, null);
        }

        /// <summary>
        /// Get Local app setting key as DateTime
        /// </summary>
        /// <param name="p_key"></param>
        /// <param name="p_default"></param>
        /// <returns></returns>
        public static DateTime? GetAppSettingDateTime(string p_key, DateTime? p_default)
        {
            return GetDateTime(GetAppSettingString(p_key), p_default);
        }

        #endregion datetime

        #region decimal

        /// <summary>
        /// Get decimal app settings
        /// </summary>
        /// <param name="p_key"></param>
        /// <param name="p_default"></param>
        /// <returns></returns>
        public static decimal GetDecimalAppSetting(string p_key, decimal p_default)
        {
            return GetSafeDecimal(GetAppSettingDecimal(p_key, p_default));
        }

        /// <summary>
        /// Get local appsetting as decimal
        /// </summary>
        /// <param name="p_key"></param>
        /// <returns></returns>
        public static decimal? GetAppSettingDecimal(string p_key)
        {
            return GetAppSettingDecimal(p_key, null);
        }

        /// <summary>
        /// Get local appsetting as decimal
        /// </summary>
        /// <param name="p_key"></param>
        /// <param name="p_default"></param>
        /// <returns></returns>
        public static decimal? GetAppSettingDecimal(string p_key, decimal? p_default)
        {
            return GetDecimal(GetAppSettingString(p_key), p_default);
        }

        #endregion decimal

        #region string

        /// <summary>
        /// Get string or null
        /// </summary>
        /// <param name="p_key"></param>
        /// <returns></returns>
        public static string GetAppSettingString(string p_key)
        {
            return GetAppSettingString(p_key, null);
        }

        /// <summary>
        /// Get Local app setting key as string or default
        /// </summary>
        /// <param name="p_key"></param>
        /// <param name="p_default"></param>
        /// <returns></returns>
        public static string GetAppSettingString(string p_key, string p_default)
        {
            return GetString(ConfigurationManager.AppSettings[p_key], p_default);
        }

        #endregion string

        #region lists

        /// <summary>
        /// Get app settings as integer list
        /// </summary>
        /// <param name="p_key"></param>
        /// <returns></returns>
        public static List<int> GetAppSettingIntList(string p_key)
        {
            return GetIntList(GetAppSettingString(p_key));
        }

        #endregion lists

        #region utilities

        /// <summary>
        /// Check to see if the site has a local app setting
        /// </summary>
        /// <param name="p_key"></param>
        /// <returns></returns>
        public static bool HasAppSetting(string p_key)
        {
            bool hasSettings = false;
            if (ConfigurationManager.AppSettings[p_key] != null
                && !String.IsNullOrWhiteSpace(ConfigurationManager.AppSettings[p_key]))
            {
                hasSettings = true;
            }
            return hasSettings;
        }

        /// <summary>
        /// Check to see if we have a setting entry with empty value
        /// </summary>
        /// <param name="p_key"></param>
        /// <returns></returns>
        public static bool HasEmptyAppSetting(string p_key)
        {
            bool hasEmptySetting = false;
            if (ConfigurationManager.AppSettings[p_key] != null)
            {
                if (String.IsNullOrWhiteSpace(ConfigurationManager.AppSettings[p_key]))
                {
                    hasEmptySetting = true;
                }
            }
            return hasEmptySetting;
        }

        #endregion utilities

        #region objects

        /// <summary>
        /// Get app setting or null
        /// </summary>
        /// <param name="p_key"></param>
        /// <returns></returns>
        public static Object GetAppSettingValue(object p_key)
        {
            return GetAppSettingValue(p_key, null);
        }

        /// <summary>
        /// Get app setting as an object with default
        /// </summary>
        /// <param name="p_key"></param>
        /// <param name="p_default"></param>
        /// <returns></returns>
        public static Object GetAppSettingValue(object p_key, object p_default)
        {
            Object obj = p_default;

            string key = GetSafeString(p_key);
            if (!String.IsNullOrWhiteSpace(key))
            {
                if (ConfigurationManager.AppSettings[key] != null)
                {
                    return ConfigurationManager.AppSettings[key];
                }
            }

            return obj;
        }

        /// <summary>
        /// Get the machine name
        /// </summary>
        /// <returns></returns>
        public static string GetMachineName()
        {
            return System.Environment.MachineName;
        }

        #endregion objects

        #region formatting

        /// <summary>
        /// List all properties
        /// </summary>
        /// <returns></returns>
        protected virtual string GetSettingsString()
        {
            string ret = String.Empty;
            System.Reflection.PropertyInfo[] test = this.GetType().GetProperties();
            foreach (System.Reflection.PropertyInfo info in test)
            {
                object obj = GetPropValue(this, info.Name);
                string value = GetString(obj, "N/A");
                ret = ConcatLine(ret, GetSettingString(info.Name, value));
            }

            if (!String.IsNullOrWhiteSpace(ret))
            {
                ret = "Settings :" + System.Environment.NewLine + ret;
            }
            else
            {
                ret = "No Settings";
            }

            return ret;
        }

        /// <summary>
        /// Get setting string
        /// </summary>
        /// <param name="p_key"></param>
        /// <returns></returns>
        protected string GetSettingString(string p_key)
        {
            return GetSettingString(p_key, GetPropValue(this, p_key));
        }

        /// <summary>
        /// Get Key and value pair as string
        /// </summary>
        /// <param name="p_key"></param>
        /// <param name="p_value"></param>
        /// <returns></returns>
        protected string GetSettingString(string p_key, object p_value)
        {
            return String.Format("{0} : {1}", p_key, GetString(p_value, "N/A"));
        }
       
        /// <summary>
        /// Get property value
        /// </summary>
        /// <param name="src"></param>
        /// <param name="propName"></param>
        /// <returns></returns>
        public static object GetPropValue(object src, string propName)
        {
            Object value = null;
            System.Reflection.PropertyInfo property = src.GetType().GetProperty(propName);
            if (property != null)
            {
                value = property.GetValue(src, null);
            }
            return value;
        }


        /// <summary>
        /// Get concatenated line
        /// </summary>
        /// <param name="src"></param>
        /// <param name="input"></param>
        /// <returns></returns>
        protected string ConcatLine(string src, string input)
        {
            string ret = src;
            if (!String.IsNullOrWhiteSpace(ret))
            {
                ret += System.Environment.NewLine;
            }
            ret += input;
            return ret;
        }

        /// <summary>
        /// To string
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            //return base.ToString();
            return GetSettingsString();
        }

        #endregion

        #endregion get local app settings
    }
}