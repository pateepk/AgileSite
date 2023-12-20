using System;
using System.Collections.Generic;
using System.Data;

using CMS.Base;
using CMS.Core;

namespace CMS.Helpers
{
    /// <summary>
    /// Provides the cookie management methods.
    /// </summary>
    public class CookieHelper : AbstractHelper<CookieHelper>, IDataContainer
    {
        #region "Variables"

        private static bool? mAllowCookies;

        private static readonly object lockObject = new object();

        // Dictionary of the cookie levels [CookieName -> Level]
        private static StringSafeDictionary<CookieSettings> mCookieSettings;

        #endregion


        #region "Properties"

        /// <summary>
        /// Dictionary of the cookie levels [CookieName -> Level]
        /// </summary>
        private static StringSafeDictionary<CookieSettings> CookieSettings
        {
            get
            {
                if (mCookieSettings == null)
                {
                    lock (lockObject)
                    {
                        mCookieSettings = new StringSafeDictionary<CookieSettings>();

                        // Register the default cookies
                        RegisterDefaultCookies();
                    }
                }

                return mCookieSettings;
            }
        }


        /// <summary>
        /// Singleton Instance of the cookie helper.
        /// </summary>
        public static CookieHelper Instance => HelperObject;


        /// <summary>
        /// If true, the cookies are allowed to be used.
        /// </summary>
        public static bool AllowCookies
        {
            get
            {
                if (mAllowCookies == null)
                {
                    mAllowCookies = ValidationHelper.GetBoolean(SettingsHelper.AppSettings["CMSAllowCookies"], true);
                }

                return mAllowCookies.Value;
            }
            set
            {
                mAllowCookies = value;
            }
        }


        /// <summary>
        /// Request cookies collection
        /// </summary>
        private static IHttpCookieCollection RequestCookiesCollection => CMSHttpContext.CurrentStandard.Request.Cookies;


        /// <summary>
        /// Response cookies collection
        /// </summary>
        private static IHttpCookieCollection ResponseCookiesCollection => CMSHttpContext.CurrentStandard.Response.Cookies;


        /// <summary>
        /// Request cookies status [CookieName -> true/false]
        /// </summary>
        private static StringSafeDictionary<bool> RequestCookies
        {
            get
            {
                if (!AllowCookies)
                {
                    return null;
                }

                // Get from the request
                var cookies = RequestStockHelper.GetItem("RequestCookies", true) as StringSafeDictionary<bool>;
                if ((cookies == null) && (CMSHttpContext.CurrentStandard != null) && (RequestCookiesCollection != null))
                {
                    cookies = new StringSafeDictionary<bool>();

                    string[] allKeys = RequestCookiesCollection.AllKeys;

                    // Fill in request cookies table
                    foreach (string key in allKeys)
                    {
                        if (key != null)
                        {
                            cookies[key] = true;
                        }
                    }

                    RequestStockHelper.Add("RequestCookies", cookies, true);
                }

                return cookies;
            }
        }


        /// <summary>
        /// Response cookies collection [CookieName -> HttpCookie]
        /// </summary>
        private static StringSafeDictionary<IHttpCookie> ResponseCookies
        {
            get
            {
                if (!AllowCookies)
                {
                    return null;
                }

                // Get from the request
                var cookies = (StringSafeDictionary<IHttpCookie>)RequestStockHelper.GetItem("ResponseCookies", true);
                if ((cookies == null) && (CMSHttpContext.CurrentStandard != null))
                {
                    cookies = new StringSafeDictionary<IHttpCookie>();

                    // Fill in request cookies table
                    var responseCookies = ResponseCookiesCollection;

                    foreach (string key in responseCookies)
                    {
                        cookies[key] = responseCookies[key];
                    }

                    RequestStockHelper.Add("ResponseCookies", cookies, true);
                }

                return cookies;
            }
        }


        /// <summary>
        /// Returns true if some response cookie has been added.
        /// </summary>
        private static bool ResponseCookiesExist
        {
            get
            {
                // Get from the request stock
                object result = RequestStockHelper.GetItem("ResponseCookiesExist", true);
                if (result == null)
                {
                    // Check the number of cookies
                    result = (ResponseCookiesCollection != null) && (ResponseCookiesCollection.Count > 0);

                    RequestStockHelper.Add("ResponseCookiesExist", result, true);

                }

                return (bool)result;
            }
            set
            {
                RequestStockHelper.Add("ResponseCookiesExist", value, true);
            }
        }


        /// <summary>
        /// Indicates if the cookies are read-only
        /// </summary>
        public static bool ReadOnly
        {
            get
            {
                return ValidationHelper.GetBoolean(RequestStockHelper.GetItem("CookiesReadOnly", true), false);
            }
            set
            {
                RequestStockHelper.Add("CookiesReadOnly", value, true);
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Returns true if the cookie is allowed based on current cookie level
        /// </summary>
        /// <param name="name">Cookie name</param>
        private static bool IsCookieAllowed(string name)
        {
            return HelperObject.IsCookieAllowedInternal(name);
        }


        /// <summary>
        /// Returns the given cookie value.
        /// </summary>
        /// <param name="name">Cookie name to retrieve</param>
        public static string GetValue(string name)
        {
            return HelperObject.GetValueInternal(name, true, false);
        }


        /// <summary>
        /// Returns the given cookie value.
        /// </summary>
        /// <param name="name">Cookie name to retrieve</param>
        /// <param name="useDefaultValue">Indicates whether the registered cookie default value will be returned when the cookie is not present in the request</param>
        /// <param name="allowSensitiveData">Allows retrieving data from sensitive authentication cookies</param>
        public static string GetValue(string name, bool useDefaultValue, bool allowSensitiveData)
        {
            return HelperObject.GetValueInternal(name, useDefaultValue, allowSensitiveData);
        }


        /// <summary>
        /// Sets the cookie.
        /// </summary>
        /// <param name="name">Cookie name</param>
        /// <param name="value">Cookie value</param>
        /// <param name="expires">Expiration time</param>
        public static void SetValue(string name, string value, DateTime expires)
        {
            SetValue(name, value, null, expires);
        }


        /// <summary>
        /// Sets the cookie.
        /// </summary>
        /// <param name="name">Cookie name</param>
        /// <param name="value">Cookie value</param>
        /// <param name="path">Cookie virtual path</param>
        /// <param name="expires">Expiration time</param>
        public static void SetValue(string name, string value, string path, DateTime expires)
        {
            SetValue(name, value, path, expires, null);
        }


        /// <summary>
        /// Sets the cookie.
        /// </summary>
        /// <param name="name">Cookie name</param>
        /// <param name="value">Cookie value</param>
        /// <param name="path">Cookie virtual path</param>
        /// <param name="expires">Expiration time</param>
        /// <param name="httpOnly">Defines httpOnly flag.</param>
        public static void SetValue(string name, string value, string path, DateTime expires, bool? httpOnly)
        {
            SetValue(name, value, path, expires, httpOnly, null);
        }


        /// <summary>
        /// Sets the cookie.
        /// </summary>
        /// <param name="name">Cookie name</param>
        /// <param name="value">Cookie value</param>
        /// <param name="path">Cookie virtual path</param>
        /// <param name="expires">Expiration time</param>
        /// <param name="httpOnly">Defines httpOnly flag.</param>
        /// <param name="domain">Domain to associate the cookie with.</param>
        public static void SetValue(string name, string value, string path, DateTime expires, bool? httpOnly, string domain)
        {
            HelperObject.SetValueInternal(name, value, ref path, expires, httpOnly, domain);
        }


        /// <summary>
        /// Sets the specific cookie expiration time.
        /// </summary>
        /// <param name="cookieName">Cookie name</param>
        /// <param name="expiration">Expiration time</param>
        /// <param name="onlyIfShorter">If true, the expiration is set only when the expiration is shorter</param>
        public static void ChangeCookieExpiration(string cookieName, DateTime expiration, bool onlyIfShorter)
        {
            HelperObject.ChangeCookieExpirationInternal(cookieName, expiration, onlyIfShorter);
        }


        /// <summary>
        /// Removes all the user cookies above given level
        /// </summary>
        /// <param name="aboveLevel">Level above which the cookies should be removed</param>
        public static void RemoveAllCookies(int aboveLevel)
        {
            HelperObject.RemoveAllCookiesInternal(aboveLevel);
        }


        /// <summary>
        /// Removes the cookie from the client side.
        /// </summary>
        /// <param name="name">Cookie name to remove</param>
        public static void Remove(string name)
        {
            HelperObject.RemoveInternal(name);
        }


        /// <summary>
        /// Registers the given cookie within the system.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Cookies which are registered within the system are deleted automatically when the current cookie level is decreased below their registered value.
        /// </para>
        /// <para>
        /// Current cookie level must be decreased via the <see cref="ICurrentCookieLevelProvider.SetCurrentCookieLevel(int)"/> method to automatically handle the cookie deletion.
        /// </para>
        /// </remarks>
        /// <param name="name">Cookie name</param>
        /// <param name="level">Cookie level</param>
        /// <param name="isSensitive">Cookie sensitivity</param>
        public static void RegisterCookie(string name, int level, bool isSensitive = false)
        {
            CookieSettings[name] = new CookieSettings(level, null, isSensitive);
        }

        #endregion


        #region "Request / Response cookies"

        /// <summary>
        /// Returns the existing cookie.
        /// </summary>
        /// <param name="cookieName">Cookie name</param>
        public static IHttpCookie GetExistingCookie(string cookieName)
        {
            IHttpCookie responseCookie = null;

            if (ResponseCookieExists(cookieName, ref responseCookie))
            {
                return responseCookie;
            }

            if (RequestCookieExists(cookieName))
            {
                return RequestCookiesCollection[cookieName];
            }

            return null;
        }


        /// <summary>
        /// Checks if response cookie exists.
        /// </summary>
        /// <param name="name">Cookie name</param>
        public static bool ResponseCookieExists(string name)
        {
            IHttpCookie cookie = null;

            return ResponseCookieExists(name, ref cookie);
        }


        /// <summary>
        /// Checks if response cookie exists.
        /// </summary>
        /// <param name="name">Cookie name</param>
        /// <param name="cookie">Output the existing cookie</param>
        private static bool ResponseCookieExists(string name, ref IHttpCookie cookie)
        {
            if (!ResponseCookiesExist)
            {
                return false;
            }

            // Check response cookies
            var cookies = ResponseCookies;
            if (cookies != null)
            {
                cookie = cookies[name];

                return cookie != null;
            }

            return false;
        }


        /// <summary>
        /// Checks if request cookie exists.
        /// </summary>
        /// <param name="name">Cookie name</param>
        public static bool RequestCookieExists(string name)
        {
            // Check request cookies
            var cookies = RequestCookies;
            if (cookies != null)
            {
                var value = cookies[name];

                return value;
            }

            return false;
        }


        /// <summary>
        /// Restores response cookies.
        /// </summary>
        public static void RestoreResponseCookies()
        {
            // If CMSHttpContext or Response is null, returns null
            if (!AllowCookies || (CMSHttpContext.CurrentStandard == null) || ReadOnly)
            {
                return;
            }

            foreach (IHttpCookie cookie in ResponseCookies.Values)
            {
                if (!string.IsNullOrEmpty(cookie.Value))
                {
                    // Save cookie to the response
                    CMSHttpContext.CurrentStandard.Response.SetCookie(cookie);
                }
            }
        }


        /// <summary>
        /// Ensure response cookie.
        /// </summary>
        public static void EnsureResponseCookie(string cookieName)
        {
            // If CMSHttpContext or Response is null, returns null
            if (!AllowCookies || (CMSHttpContext.CurrentStandard == null))
            {
                return;
            }

            // Get cookie from response
            var cookie = ResponseCookiesCollection[cookieName];

            // Cookie exists add to the hash table
            if (cookie != null)
            {
                ResponseCookies[cookieName] = cookie;
            }
        }


        /// <summary>
        /// Gets the table of cookies.
        /// </summary>
        /// <param name="cookies">Cookies collection</param>
        /// <param name="name">Table name</param>
        private static DataTable GetCookieTable(IHttpCookieCollection cookies, string name)
        {
            // Create the cookie table
            DataTable dt = null;
            try
            {
                dt = new DataTable(name);

                dt.Columns.Add(new DataColumn("Name", typeof(string)));
                dt.Columns.Add(new DataColumn("Value", typeof(string)));
                dt.Columns.Add(new DataColumn("Expires", typeof(DateTime)));

                if (cookies != null)
                {
                    // Fill in cookies table
                    foreach (string key in cookies.AllKeys)
                    {
                        // Create new item for cookie
                        DataRow dr = dt.NewRow();

                        var cookie = cookies[key];
                        if (cookie != null)
                        {
                            // Set the values
                            dr["Name"] = key;
                            if (cookie.Expires != DateTime.MinValue)
                            {
                                dr["Expires"] = cookie.Expires;
                            }

                            // Add value
                            string value = cookie.Value;
                            if (IsSensitiveCookie(key))
                            {
                                value = "***";
                            }
                            dr["Value"] = value;

                            dt.Rows.Add(dr);
                        }
                    }
                }

                return dt;
            }
            catch
            {
                dt?.Dispose();
                throw;
            }
        }


        /// <summary>
        /// Gets the response cookies table.
        /// </summary>
        internal static DataTable GetResponseCookieTable()
        {
            return GetCookieTable(ResponseCookiesCollection, "ResponseCookies");
        }


        /// <summary>
        /// Gets the response cookies table.
        /// </summary>
        internal static DataTable GetRequestCookieTable()
        {
            return GetCookieTable(RequestCookiesCollection, "RequestCookies");
        }


        /// <summary>
        /// Clears all cookies from cookie collection.
        /// </summary>
        public static void ClearResponseCookies()
        {
            if (CMSHttpContext.CurrentStandard != null || ReadOnly)
            {
                ResponseCookiesCollection.Clear();
            }
        }


        /// <summary>
        /// Removes cookie from response.
        /// </summary>
        public static void RemoveResponseCookie(string name)
        {
            if (CMSHttpContext.CurrentStandard != null || ReadOnly)
            {
                ResponseCookiesCollection.Remove(name);
            }
        }


        /// <summary>
        /// Registers the default cookies
        /// </summary>
        private static void RegisterDefaultCookies()
        {
            // System cookies
            RegisterCookie(CookieName.CookieLevel, CookieLevel.System);
            RegisterCookie(CookieName.CsrfCookie, CookieLevel.System);

            // Essential cookies
            RegisterCookie(CookieName.WindowsUser, CookieLevel.Essential);

            RegisterCookie(CookieName.LiveID, CookieLevel.Essential, true);
            RegisterCookie(CookieName.PreferredCulture, CookieLevel.Essential);
            RegisterCookie(CookieName.MobileRedirected, CookieLevel.Essential);
            RegisterCookie(CookieName.CurrentTheme, CookieLevel.Essential);
            RegisterCookie(CookieName.VotedPolls, CookieLevel.Essential);
            RegisterCookie(CookieName.RatedDocuments, CookieLevel.Essential);
            RegisterCookie(CookieName.ForumPostAnswer, CookieLevel.Essential);
            RegisterCookie(CookieName.ShowDesktopVersion, CookieLevel.Essential);

            // Editor cookies
            RegisterCookie(CookieName.PreferredUICulture, CookieLevel.Editor);
            RegisterCookie(CookieName.ViewMode, CookieLevel.Editor);
            RegisterCookie(CookieName.SpellCheckUserWords, CookieLevel.Editor);
            RegisterCookie(CookieName.WebPartToolbarCategory, CookieLevel.Editor);
            RegisterCookie(CookieName.WebPartToolbarMinimized, CookieLevel.Editor);
            RegisterCookie(CookieName.PreviewState, CookieLevel.Editor);
            RegisterCookie(CookieName.SplitMode, CookieLevel.Editor);
            RegisterCookie(CookieName.UniGraph, CookieLevel.Editor);
            RegisterCookie(CookieName.SessionToken, CookieLevel.Editor);
            RegisterCookie(CookieName.CurrentDeviceProfileName, CookieLevel.Editor);
            RegisterCookie(CookieName.CurrentDeviceProfileRotate, CookieLevel.Editor);
            RegisterCookie(CookieName.MacroDesignerTab, CookieLevel.Editor);
            RegisterCookie(CookieName.Impersonation, CookieLevel.Editor);
            RegisterCookie(CookieName.DisplayContentInDesignMode, CookieLevel.Editor);
            RegisterCookie(CookieName.DisplayContentInUIElementDesignMode, CookieLevel.Editor);
            
            // All other cookies have the visitor cookie level - No need to set them up
        }


        /// <summary>
        /// Gets the cookie level from a string value
        /// </summary>
        /// <param name="value">Level value, number or string representation</param>
        /// <param name="defaultValue">Default value</param>
        public static int ConvertCookieLevelToIntegerValue(string value, int defaultValue)
        {
            if (value != null)
            {
                switch (value.ToLowerInvariant())
                {
                    case "none":
                        return CookieLevel.None;

                    case "system":
                        return CookieLevel.System;

                    case "essential":
                        return CookieLevel.Essential;

                    case "editor":
                        return CookieLevel.Editor;

                    case "visitor":
                        return CookieLevel.Visitor;

                    case "all":
                        return CookieLevel.All;
                }
            }

            return ValidationHelper.GetInteger(value, defaultValue);
        }

        #endregion


        #region "Internal methods"

        /// <summary>
        /// Removes all the user cookies
        /// </summary>
        /// <param name="aboveLevel">Level above which the cookies should be removed</param>
        protected virtual void RemoveAllCookiesInternal(int aboveLevel)
        {
            var allKeys = GetDistinctCookieNames();

            // Fill in request cookies table
            foreach (string key in allKeys)
            {
                int level = GetCookieLevelInternal(key);
                if (level > aboveLevel)
                {
                    Remove(key);
                }
            }
        }


        /// <summary>
        /// Gets the default value of a cookie
        /// </summary>
        /// <param name="name">Cookie name</param>
        protected virtual string GetCookieDefaultValueInternal(string name)
        {
            var settings = CookieSettings[name];
            return settings?.DefaultValue;
        }


        /// <summary>
        /// Returns true if the cookie is allowed based on current cookie level
        /// </summary>
        /// <param name="name">Cookie name</param>
        protected virtual bool IsCookieAllowedInternal(string name)
        {
            var cookieLevelProvider = Service.Resolve<ICurrentCookieLevelProvider>();

            int level = GetCookieLevelInternal(name);

            return (level <= cookieLevelProvider.GetCurrentCookieLevel());
        }

        /// <summary>
        /// Gets the cookie level for the given cookie
        /// </summary>
        /// <param name="cookieName">Cookie name</param>
        protected virtual int GetCookieLevelInternal(string cookieName)
        {
            int level = CookieLevel.Visitor;

            // Try get the cookie level
            if (CookieSettings.TryGetValue(cookieName, out var settings))
            {
                level = settings.Level;
            }
            else if (cookieName.StartsWith(CookieName.EditorPrefix, StringComparison.OrdinalIgnoreCase))
            {
                // Editor level for variant slider
                level = CookieLevel.Editor;
            }

            return level;
        }


        /// <summary>
        /// Sets the cookie.
        /// </summary>
        /// <param name="name">Cookie name</param>
        /// <param name="value">Cookie value</param>
        /// <param name="path">Cookie virtual path</param>
        /// <param name="expires">Expiration time</param>
        /// <param name="httpOnly">Defines httpOnly flag.</param>
        /// <param name="domain">Domain to associate the cookie with.</param>
        protected virtual void SetValueInternal(string name, string value, ref string path, DateTime expires, bool? httpOnly, string domain)
        {
            // If CMSHttpContext or Response is null, returns null
            if (!AllowCookies || (CMSHttpContext.CurrentStandard == null) || (CMSHttpContext.CurrentStandard.Response == null) || ReadOnly)
            {
                return;
            }

            // If cookie is not allowed, do not set
            if (IsCookieAllowed(name) || ((expires < DateTime.Now) && RequestCookieExists(name)))
            {
                // Save cookie to response for client
                var cookie = ResponseCookiesCollection[name];
                if (cookie == null)
                {
                    cookie = ObjectFactory<IHttpCookie>.New();
                    cookie.Name = name;

                    CMSHttpContext.CurrentStandard.Response.SetCookie(cookie);
                }

                // Set the cookie values
                SetCookieValues(cookie, value, ref path, expires, httpOnly, domain);

                ResponseCookies[name] = cookie;
                ResponseCookiesExist = true;
            }
        }


        /// <summary>
        /// Sets the cookie values
        /// </summary>
        /// <param name="cookie">Cookie to set</param>
        /// <param name="value">Cookie value</param>
        /// <param name="path">Cookie virtual path</param>
        /// <param name="expires">Expiration time</param>
        /// <param name="httpOnly">Defines httpOnly flag.</param>
        /// <param name="domain">Domain to associate the cookie with.</param>
        private static void SetCookieValues(IHttpCookie cookie, string value, ref string path, DateTime expires, bool? httpOnly, string domain)
        {
            cookie.Value = value;
            cookie.Expires = expires;

            if (httpOnly != null)
            {
                cookie.HttpOnly = httpOnly.Value;
            }

            if (domain != null)
            {
                cookie.Domain = domain;
            }

            // Set path to avoid loosing cookies on URL rewriting
            if (string.IsNullOrEmpty(path))
            {
                path = "/";
            }
            cookie.Path = path;
        }


        /// <summary>
        /// Returns the given cookie value.
        /// </summary>
        /// <param name="name">Cookie name to retrieve</param>
        /// <param name="useDefaultValue">If true, the cookie is allowed to use the configured default value</param>
        /// <param name="allowSensitiveData">Allows retrieving data from sensitive authentication cookies.</param>
        protected virtual string GetValueInternal(string name, bool useDefaultValue, bool allowSensitiveData)
        {
            // If CMSHttpContext, Request or Response is null, returns null
            if (!AllowCookies || (CMSHttpContext.CurrentStandard == null) || (CMSHttpContext.CurrentStandard.Response == null))
            {
                return null;
            }

            // Do not allow access to sensitive cookies
            if (!allowSensitiveData && IsSensitiveCookie(name))
            {
                return null;
            }

            // If cookie exists in response
            IHttpCookie responseCookie = null;

            if (ResponseCookieExists(name, ref responseCookie))
            {
                // And if is valid, returns it
                if ((responseCookie.Expires > DateTime.Now) || (responseCookie.Expires == DateTime.MinValue))
                {
                    return responseCookie.Value;
                }
            }
            // Else tries to find cookie in request
            else if (RequestCookieExists(name))
            {
                return RequestCookiesCollection[name].Value;
            }

            // Handle the default value if cookie is not allowed
            if (useDefaultValue && !IsCookieAllowed(name))
            {
                return GetCookieDefaultValueInternal(name);
            }

            return null;
        }


        /// <summary>
        /// Returns true for sensitive cookies.
        /// </summary>
        /// <param name="name">Name of the cookie</param>
        internal static bool IsSensitiveCookie(string name)
        {
            return CookieSettings.TryGetValue(name, out var settings) && settings.IsSensitive;
        }


        /// <summary>
        /// Removes the cookie from the client side.
        /// </summary>
        /// <param name="name">Cookie name to remove</param>
        protected virtual void RemoveInternal(string name)
        {
            // If CMSHttpContext or Response is null, returns null
            if (!AllowCookies || (CMSHttpContext.CurrentStandard == null) || ReadOnly)
            {
                return;
            }

            // If the cookie exists, sets expiration time to the past
            if (ResponseCookieExists(name))
            {
                ResponseCookiesCollection[name].Expires = DateTime.Now.AddYears(-1);
            }
            else
            {
                SetValue(name, "", DateTime.Now.AddYears(-1));
            }
        }


        /// <summary>
        /// Sets the specific cookie expiration time.
        /// </summary>
        /// <param name="cookieName">Cookie name</param>
        /// <param name="expiration">Expiration time</param>
        /// <param name="onlyIfShorter">If true, the expiration is set only when the expiration is shorter</param>
        protected virtual void ChangeCookieExpirationInternal(string cookieName, DateTime expiration, bool onlyIfShorter)
        {
            // Set cookie expiration
            var existing = GetExistingCookie(cookieName);
            if (existing != null)
            {
                if (((existing.Expires == DateTimeHelper.ZERO_TIME) || (existing.Expires > DateTime.Now)) && (!onlyIfShorter || (existing.Expires < expiration)))
                {
                    existing.Expires = expiration;
                }
            }
        }

        #endregion


        #region "ISimpleDataContainer Members"

        /// <summary>
        /// Gets or sets the value of the column.
        /// </summary>
        /// <param name="columnName">Column name</param>
        object ISimpleDataContainer.this[string columnName]
        {
            get
            {
                return GetValue(columnName);
            }
            set
            {
                throw new NotImplementedException();
            }
        }


        /// <summary>
        /// Gets the value from Cookie.
        /// </summary>
        /// <param name="cookieName">Cookie name</param>
        object ISimpleDataContainer.GetValue(string cookieName)
        {
            TryGetValue(cookieName, out var value);

            return value;
        }


        /// <summary>
        /// Not implemented.
        /// </summary>
        /// <param name="cookieName">Cookie name</param>
        /// <param name="value">New value</param>
        bool ISimpleDataContainer.SetValue(string cookieName, object value)
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
                var names = TypeHelper.NewList(ResponseCookies.Keys);

                foreach (string key in RequestCookies.Keys)
                {
                    if (!IsSensitiveCookie(key))
                    {
                        names.Add(key);
                    }
                }

                return names;
            }
        }


        /// <summary>
        /// Gets names of both request and response cookies without duplicates.
        /// </summary>
        public static IEnumerable<string> GetDistinctCookieNames()
        {
            var cookieNames = new HashSet<string>(ResponseCookies.TypedKeys);

            foreach (string key in RequestCookies.Keys)
            {
                cookieNames.Add(key);
            }

            return cookieNames;
        }


        /// <summary>
        /// Returns value of column.
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="value">Returns the value</param>
        /// <returns>Returns true if the operation was successful (the value was present)</returns>
        public bool TryGetValue(string columnName, out object value)
        {
            value = GetValue(columnName);
            return value != null;
        }


        /// <summary>
        /// Returns true if the object contains specified column.
        /// </summary>
        /// <param name="columnName">Column name</param>
        public bool ContainsColumn(string columnName)
        {
            return GetValue(columnName) != null;
        }

        #endregion
    }
}
