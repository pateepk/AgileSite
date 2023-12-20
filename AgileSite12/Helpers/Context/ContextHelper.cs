using System;

namespace CMS.Helpers
{
    /// <summary>
    /// Wrapper around RequestStockHelper, SessionHelper and CookieHelper to access the context values. It always uses the case sensitive keys.
    /// </summary>
    public static class ContextHelper
    {
        #region "Methods"

        /// <summary>
        /// Returns the given session value.
        /// </summary>
        /// <param name="key">Value key</param>
        /// <param name="request">Use request storage</param>
        /// <param name="session">Use session storage</param>
        /// <param name="cookie">Use cookies</param>
        public static object GetItem(string key, bool request, bool session, bool cookie)
        {
            object value = null;
            bool saveToRequest = false;
            bool saveToSession = false;

            // Try to get from request
            if (request)
            {
                value = RequestStockHelper.GetItem(key, true);
            }

            // Try to get from session
            if (value == null)
            {
                saveToRequest = request;
                if (session)
                {
                    value = SessionHelper.GetValue(key);
                }
            }

            // Try to get from cookie
            if (value == null)
            {
                saveToSession = session;
                if (cookie)
                {
                    value = CookieHelper.GetValue(key);
                }
            }

            // Save the result to the higher contexts
            if (value != null)
            {
                if (saveToRequest)
                {
                    RequestStockHelper.Add(key, value, true);
                }
                if (saveToSession)
                {
                    SessionHelper.SetValue(key, value);
                }
            }

            return value;
        }


        /// <summary>
        /// Sets the session value.
        /// </summary>
        /// <param name="key">Item key</param>
        /// <param name="value">Value</param>
        /// <param name="request">Use request storage</param>
        /// <param name="session">Use session storage</param>
        /// <param name="cookie">Use cookies</param>
        /// <param name="cookieExpires">Time when the cookie expires</param>
        /// <param name="cookieHttpOnly">Defines httpOnly flag.</param>
        public static void Add(string key, object value, bool request, bool session, bool cookie, DateTime cookieExpires, bool? cookieHttpOnly = null)
        {
            if (request)
            {
                RequestStockHelper.Add(key, value, true);
            }
            if (session)
            {
                SessionHelper.SetValue(key, value);
            }
            if (cookie)
            {
                CookieHelper.SetValue(key, value.ToString(), null, cookieExpires, cookieHttpOnly);
            }
        }


        /// <summary>
        /// Removes the session value
        /// </summary>
        /// <param name="key">Item key</param>
        /// <param name="request">Remove from request storage</param>
        /// <param name="session">Remove from session storage</param>
        /// <param name="cookie">Remove from cookies</param>
        public static void Remove(string key, bool request, bool session, bool cookie)
        {
            if (request)
            {
                RequestStockHelper.Remove(key, true);
            }
            if (session)
            {
                SessionHelper.Remove(key);
            }
            if (cookie)
            {
                CookieHelper.Remove(key);
            }
        }

        #endregion
    }
}