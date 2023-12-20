using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Web;

using CMS.Base;

namespace CMS.Helpers
{
    /// <summary>
    /// QueryString helper methods.
    /// </summary>
    public class QueryHelper : IDataContainer
    {
        #region "Variables"

        private static QueryHelper mInstance;
        private static bool? mHashEnabled;

        #endregion


        #region "Properties"

        /// <summary>
        /// Singleton Instance of the query helper.
        /// </summary>
        public static QueryHelper Instance
        {
            get
            {
                return mInstance ?? (mInstance = new QueryHelper());
            }
            set
            {
                mInstance = value;
            }
        }


        /// <summary>
        /// If true, the hash check is enabled.
        /// </summary>
        public static bool HashEnabled
        {
            get
            {
                if (mHashEnabled == null)
                {
                    mHashEnabled = ValidationHelper.GetBoolean(SettingsHelper.AppSettings["CMSEnableHash"], true);
                }

                return mHashEnabled.Value;
            }
            set
            {
                mHashEnabled = value;
            }
        }


        /// <summary>
        /// Query string
        /// </summary>
        protected static NameValueCollection QueryString
        {
            get
            {
                return CMSHttpContext.Current?.Request?.QueryString;
            }
        }


        /// <summary>
        /// Route data
        /// </summary>
        protected static IDictionary<string, object> RouteValues
        {
            get
            {
                return CMSHttpContext.Current?.Request?.RequestContext?.RouteData?.Values;
            }
        }


        /// <summary>
        /// Returns html encoded query string. 
        /// </summary>
        public static string EncodedQueryString
        {
            get
            {
                var qs = QueryString;
                if (qs == null)
                {
                    return null;
                }

                return HTMLHelper.HTMLEncode(qs.ToString());
            }
        }


        /// <summary>
        /// Returns string containing all parameters and their values from <see cref="RouteValues"/> and <see cref="QueryString"/>.
        /// Parameters are sorted by their names and merged together with <see cref="QueryString"/> having precedence over <see cref="RouteValues"/>.
        /// </summary>
        public static string GetParameterString()
        {
            var parameters = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);

            if (QueryString != null)
            {
                foreach (var key in QueryString.AllKeys.Where(k => k != null))
                {
                    parameters.Add(key, QueryString.Get(key));
                }
            }

            if (RouteValues != null)
            {
                foreach (var pair in RouteValues)
                {
                    if (!parameters.ContainsKey(pair.Key))
                    {
                        parameters.Add(pair.Key, pair.Value?.ToString());
                    }
                }
            }
            
            return parameters.OrderBy(p => p.Key).Select(p => p.Key + "=" + p.Value).Join("&");
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Returns the string representation of the query string
        /// </summary>
        public override string ToString()
        {
            var qs = QueryString;
            if (qs == null)
            {
                return "";
            }

            return qs.ToString();
        }


        /// <summary>
        /// Build the query from the given items and adds the hash to the query
        /// </summary>
        /// <param name="items">Items of the query, in format [param1name, param1value, param2name, param2value], skips the items with empty value</param>
        public static string BuildQueryWithHash(params string[] items)
        {
            string query = BuildQuery(items);

            string hash = GetHash(query, true);
            query += "&hash=" + HttpUtility.UrlEncode(hash);

            return query;
        }


        /// <summary>
        /// Build the query from the given items
        /// </summary>
        /// <param name="items">Items of the query, in format [param1name, param1value, param2name, param2value], skips the items with empty value</param>
        public static string BuildQuery(params string[] items)
        {
            StringBuilder sb = new StringBuilder();

            // Make count even
            int count = items.Length;
            if (count % 2 == 1)
            {
                count--;
            }

            // Process all items
            for (int i = 0; i < count; i++)
            {
                string name = items[i];
                i++;
                string value = items[i];

                if (!String.IsNullOrEmpty(value))
                {
                    // Add the parameter
                    sb.Append(sb.Length == 0 ? "?" : "&");
                    sb.Append(name, "=", HttpUtility.UrlEncode(value));
                }
            }

            return sb.ToString();
        }


        /// <summary>
        /// Returns the color representation of a query parameter or default value
        /// if parameter is not a valid color.
        /// </summary>
        /// <param name="name">Query parameter</param>
        /// <param name="defaultValue">Default value</param>
        public static Color GetColor(string name, Color defaultValue)
        {
            if (CMSHttpContext.Current == null)
            {
                return defaultValue;
            }

            string tmp = GetRequestValue(name);
            if (tmp != null)
            {
                tmp = tmp.Trim();
            }

            return ValidationHelper.GetColor(tmp, defaultValue);
        }


        /// <summary>
        /// Returns the integer representation of a query parameter or default value
        /// if parameter is not an integer number.
        /// </summary>
        /// <param name="name">Query parameter</param>
        /// <param name="defaultValue">Default value</param>
        public static int GetInteger(string name, int defaultValue)
        {
            if (CMSHttpContext.Current == null)
            {
                return defaultValue;
            }

            string tmp = GetRequestValue(name);
            if (tmp != null)
            {
                tmp = tmp.Trim();
            }

            return ValidationHelper.GetInteger(tmp, defaultValue);
        }


        /// <summary>
        /// Returns the boolean representation of a query parameter or default value
        /// if parameter is not boolean.
        /// </summary>
        /// <param name="name">Query parameter</param>
        /// <param name="defaultValue">Default value</param>
        public static bool GetBoolean(string name, bool defaultValue)
        {
            if (CMSHttpContext.Current == null)
            {
                return defaultValue;
            }

            return ValidationHelper.GetBoolean(GetRequestValue(name), defaultValue);
        }


        /// <summary>
        /// Returns the double representation of a query parameter or default value if parameter is not a double number.
        /// </summary>
        /// <param name="name">Query parameter</param>
        /// <param name="defaultValue">Default value</param>
        public static double GetDouble(string name, double defaultValue)
        {
            if (CMSHttpContext.Current == null)
            {
                return defaultValue;
            }

            return ValidationHelper.GetDouble(GetRequestValue(name), defaultValue);
        }


        /// <summary>
        /// Returns the decimal representation of a query parameter or default value if parameter is not a decimal number.
        /// </summary>
        /// <param name="name">Query parameter</param>
        /// <param name="defaultValue">Default value</param>
        public static decimal GetDecimal(string name, decimal defaultValue)
        {
            if (CMSHttpContext.Current == null)
            {
                return defaultValue;
            }

            return ValidationHelper.GetDecimal(GetRequestValue(name), defaultValue);
        }


        /// <summary>
        /// Returns query string parameter or default value if query string is not defined.
        /// </summary>
        /// <param name="name">Query parameter</param>
        /// <param name="defaultValue">Default value</param>
        public static string GetString(string name, string defaultValue)
        {
            if (CMSHttpContext.Current == null || String.IsNullOrEmpty(name))
            {
                return defaultValue;
            }

            return ValidationHelper.GetString(GetRequestValue(name), defaultValue);
        }


        /// <summary>
        /// Returns HTML encoded query string parameter or default value if query string is not defined.
        /// </summary>
        /// <param name="name">Query parameter</param>
        /// <param name="defaultValue">Default value</param>
        public static string GetText(string name, string defaultValue)
        {
            return HTMLHelper.HTMLEncode(GetString(name, defaultValue));
        }


        /// <summary>
        /// Returns control client ID from a query parameter or default value
        /// if parameter is not a valid control client ID.
        /// </summary>
        /// <param name="name">Query parameter</param>
        /// <param name="defaultValue">Default value</param>
        public static string GetControlClientId(string name, string defaultValue)
        {
            if (CMSHttpContext.Current == null || String.IsNullOrEmpty(name))
            {
                return defaultValue;
            }

            return ValidationHelper.GetControlClientId(GetRequestValue(name), defaultValue);
        }


        /// <summary>
        /// Returns the GUID representation of a query parameter or default value
        /// if parameter is not GUID.
        /// </summary>
        /// <param name="name">Query parameter</param>
        /// <param name="defaultValue">Default value</param>
        public static Guid GetGuid(string name, Guid defaultValue)
        {
            if (CMSHttpContext.Current == null)
            {
                return defaultValue;
            }

            return ValidationHelper.GetGuid(GetRequestValue(name), defaultValue);
        }


        /// <summary>
        /// Returns true if query parameter is present in the query string.
        /// </summary>
        /// <param name="name">Query parameter</param>
        public static bool Contains(string name)
        {
            if (CMSHttpContext.Current == null)
            {
                return false;
            }

            return GetRequestValue(name) != null;
        }


        /// <summary>
        /// Generates hash for given QueryString.
        /// </summary>
        /// <param name="input">Query string starting with '?' or complete URL</param>
        /// <returns>SHA256 hash of query string</returns>
        public static string GetHash(string input)
        {
            return GetHash(input, input.StartsWith("?", StringComparison.Ordinal));
        }


        /// <summary>
        /// Generates hash for given QueryString.
        /// </summary>
        /// <param name="input">Query string starting with '?' or complete URL</param>
        /// <param name="isQueryString">Indicates if input is query string or URL</param>
        /// <returns>SHA256 hash of query string</returns>
        public static string GetHash(string input, bool isQueryString)
        {
            // Prepare the query
            string queryString = !isQueryString ? URLHelper.GetQuery(input) : input;

            // Get the hash
            return ValidationHelper.GetHashString(queryString, new HashSettings(""));
        }


        /// <summary>
        /// Validates given QueryString against hash.
        /// </summary>
        /// <param name="name">Name of hash parameter in query string</param>
        /// <param name="excludedParameters">Parameters to exclude from hash validation. Multiple names separated by ';'.</param>
        /// <param name="validateWithoutExcludedParameters">Indicates if the query string is validated without the excluded parameters if validation with excluded parameters fails</param>
        /// <param name="settings">Hash settings</param>
        /// <returns>True if query string is valid.</returns>
        public static bool ValidateHash(string name, string excludedParameters = null, HashSettings settings = null, bool validateWithoutExcludedParameters = false)
        {
            settings = settings ?? new HashSettings("");

            string hashValue = GetString(name, null);
            if (hashValue == null)
            {
                // Redirect to information
                if (settings.Redirect)
                {
                    URLHelper.Redirect(AdministrationUrlHelper.GetAccessDeniedUrl("dialogs.missinghashtext"));
                }

                return false;
            }

            // Get query string without the hash parameter
            string queryString = URLHelper.UrlEncodeQueryString("?" + CMSHttpContext.Current.Request.QueryString);
            queryString = URLHelper.RemoveUrlParameter(queryString, name);

            if (string.IsNullOrEmpty(excludedParameters))
            {
                // Validate
                return ValidateHashString(queryString, hashValue, settings);
            }

            // Exclude parameters from the query string
            string queryStringExcluded = RemoveParameters(queryString, excludedParameters.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries));

            if (validateWithoutExcludedParameters)
            {
                var noRedirect = settings.Clone();
                noRedirect.Redirect = false;

                // Validate with excluded parameters, without redirect
                bool isValidExcluded = ValidateHashString(queryStringExcluded, hashValue, noRedirect);
                if (!isValidExcluded)
                {
                    // Validate without excluded parameters
                    return ValidateHashString(queryString, hashValue, settings);
                }

                return true;
            }

            // Validate with excluded parameters
            return ValidateHashString(queryStringExcluded, hashValue, settings);
        }


        /// <summary>
        /// Removes the specified parameters from the specified query string.
        /// </summary>
        /// <param name="queryString">Query string starting with '?'</param>
        /// <param name="paramsToRemove">Parameters to be removed from the query string</param>
        /// <returns>Specified query string without the specified parameters</returns>
        private static string RemoveParameters(string queryString, params string[] paramsToRemove)
        {
            if (paramsToRemove.Length > 0)
            {
                foreach (string param in paramsToRemove)
                {
                    if (param != "")
                    {
                        queryString = URLHelper.RemoveUrlParameter(queryString, param);
                    }
                }
            }

            return queryString;
        }


        /// <summary>
        /// Validates given value against hash.
        /// </summary>
        /// <param name="value">Value to validate</param>
        /// <param name="hash">Hash value</param>
        /// <param name="settings">Hash settings</param>
        /// <returns>True if hash is valid.</returns>
        public static bool ValidateHashString(string value, string hash, HashSettings settings = null)
        {
            settings = settings ?? new HashSettings("");

            bool result = true;

            if (HashEnabled)
            {
                // Compare hash from query string with just generated hash
                result = ValidationHelper.ValidateHash(value, hash, settings);
            }

            return result;
        }

        #endregion


        #region "ISimpleDataContainer Members"

        /// <summary>
        /// Gets or sets the value of the column.
        /// </summary>
        /// <param name="columnName">Column name</param>
        public object this[string columnName]
        {
            get
            {
                return GetValue(columnName);
            }
            set
            {
                SetValue(columnName, value);
            }
        }


        /// <summary>
        /// Gets the value from QueryString.
        /// </summary>
        /// <param name="key">QueryString key</param>
        public object GetValue(string key)
        {
            object retval;
            TryGetValue(key, out retval);
            return retval;
        }


        /// <summary>
        /// Not implemented.
        /// </summary>
        /// <param name="key">QueryString key</param>
        /// <param name="value">New value</param>
        public bool SetValue(string key, object value)
        {
            throw new NotImplementedException();
        }

        #endregion


        #region IDataContainer Members

        /// <summary>
        /// Column names.
        /// </summary>
        public List<string> ColumnNames
        {
            get
            {
                var queryColumns = QueryString?.AllKeys.Where(k => k != null) ?? Enumerable.Empty<string>();

                return queryColumns.ToList();
            }
        }


        /// <summary>
        /// Returns value of column.
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="value">Returns the value</param>
        /// <returns>Returns true if the operation was successful (the value was present)</returns>
        public bool TryGetValue(string columnName, out object value)
        {
            value = GetString(columnName, null);
            return value != null;
        }


        /// <summary>
        /// Returns true if the object contains specified column.
        /// </summary>
        /// <param name="columnName">Column name</param>
        public bool ContainsColumn(string columnName)
        {
            return GetRequestValue(columnName) == null;
        }


        private static string GetRequestValue(string columnName)
        {
            return GetQueryStringValue(columnName) ?? GetRouteValue(columnName);
        }


        private static string GetQueryStringValue(string columnName)
        {
            return QueryString?[columnName];
        }


        private static string GetRouteValue(string columnName)
        {
            return ValidationHelper.GetString(RouteValues?[columnName], null);
        }

        #endregion
    }
}