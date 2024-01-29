using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Web.SessionState;
using System.Web;

using CMS.Base;
using CMS.Core;

namespace CMS.Helpers
{
    /// <summary>
    /// Session management.
    /// </summary>
    public static class SessionHelper
    {
#pragma warning disable BH1001 // 'Session[]' should not be used. Use 'SessionHelper.GetValue()' instead.
#pragma warning disable BH1002 // 'Session[]' should not be used. Use 'SessionHelper.SetValue()' instead.
        
        #region "Variables"

        private static bool? mAllowSessionState;
        private static int mSessionTimeout = -1;

        private static bool sessionTimeoutInitialized;

        #endregion


        #region "Properties"

        /// <summary>
        /// Returns true if the session is available
        /// </summary>
        public static bool SessionIsAvailable
        {
            get
            {
                return CurrentSession != null;
            }
        }


        /// <summary>
        /// Current session
        /// </summary>
        private static HttpSessionStateBase CurrentSession
        {
            get
            {
                return CMSHttpContext.Current?.Session;
            }
        }


        /// <summary>
        /// Gets or sets the session timeout value (this value doesn't change the timeout period).
        /// </summary>
        public static int SessionTimeout
        {
            get
            {
                if (CurrentSession != null)
                {
                    return CurrentSession.Timeout;
                }

                return mSessionTimeout;
            }
            set
            {
                mSessionTimeout = value;
            }
        }


        /// <summary>
        /// If true, the session state storage is allowed to be used.
        /// </summary>
        public static bool AllowSessionState
        {
            get
            {
                if (mAllowSessionState == null)
                {
                    mAllowSessionState = ValidationHelper.GetBoolean(SettingsHelper.AppSettings["CMSAllowSessionState"], true);
                }

                return mAllowSessionState.Value;
            }
            set
            {
                mAllowSessionState = value;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Cancels the current session.
        /// </summary>
        public static void Abandon()
        {
            if (!AllowSessionState || (CurrentSession == null) || VirtualContext.IsInitialized)
            {
                return;
            }

            CurrentSession.Abandon();
        }


        /// <summary>
        /// Clears the session content starting with given string
        /// </summary>
        /// <param name="startsWith">If null, removes all session items, if set, remove only items starting with given string</param>
        public static void Clear(string startsWith = null)
        {
            if (!AllowSessionState || (CurrentSession == null) || VirtualContext.IsInitialized)
            {
                return;
            }

            CheckReadOnlyMode();

            // Clear all in case prefix not specified
            if (String.IsNullOrEmpty(startsWith))
            {
                CurrentSession.Clear();
                return;
            }

            List<string> keyList = new List<string>();

            // Build the items list
            foreach (string key in CurrentSession.Keys)
            {
                if (key.StartsWith(startsWith, StringComparison.InvariantCulture))
                {
                    keyList.Add(key);
                }
            }

            // Remove the items
            foreach (string key in keyList)
            {
                Remove(key);
            }
        }


        /// <summary>
        /// Returns the given session value.
        /// </summary>
        /// <param name="key">Value key</param>
        public static object GetValue(string key)
        {
            // if HttpContext, Request or Response is null, returns null
            if (!AllowSessionState || (CurrentSession == null))
            {
                return null;
            }

            return CurrentSession[key];
        }

        
        /// <summary>
        /// Sets the session value.
        /// </summary>
        /// <param name="key">Item key</param>
        /// <param name="value">Value</param>
        /// <param name="allowVirtualContext">If true, the value is set even when virtual context is initialized</param>
        public static void SetValue(string key, object value, bool allowVirtualContext = false)
        {
            // if HttpContext or Session is null, returns null
            if (!AllowSessionState || (CurrentSession == null) || (VirtualContext.IsInitialized && !allowVirtualContext))
            {
                return;
            }

            CheckReadOnlyMode();

            CurrentSession[key] = value;
        }


        /// <summary>
        /// Sets the session value.
        /// </summary>
        /// <param name="key">Item key</param>
        public static void Remove(string key)
        {
            // if HttpContext or Session is null, returns null
            if (!AllowSessionState || (CurrentSession == null) || VirtualContext.IsInitialized)
            {
                return;
            }

            CheckReadOnlyMode();

            // Remove the key from session if exists
            if (CurrentSession[key] != null)
            {
                CurrentSession.Remove(key);
            }
        }


        /// <summary>
        /// Returns the session ID.
        /// </summary>
        public static string GetSessionID()
        {
#pragma warning disable BH1000 // SessionID' should not be used. Use 'SessionHelper.GetSessionID()' instead.
            return CurrentSession?.SessionID;
#pragma warning restore BH1000 // SessionID' should not be used. Use 'SessionHelper.GetSessionID()' instead.
        }


        /// <summary>
        /// Initializes the session timeout variable
        /// </summary>
        public static void InitSessionTimeout()
        {
            // Keep session timeout within static variable
            if (!sessionTimeoutInitialized && (CMSHttpContext.Current != null) && (CMSHttpContext.Current.Session != null))
            {
                SessionTimeout = CMSHttpContext.Current.Session.Timeout;
                sessionTimeoutInitialized = true;
            }
        }


        private static void CheckReadOnlyMode()
        {
            if (CMSHttpContext.Current == null || !(CMSHttpContext.Current.Handler is IReadOnlySessionState))
            {
                return;
            }
            string errorMessage = "IReadOnlySessionState handler should not allow to update session.";
            if (SystemContext.DevelopmentMode)
            {
                throw new InvalidOperationException(errorMessage);
            }

            string eventDescription = String.Format(
@"{0}
Stack trace:
{1}", errorMessage, new StackTrace());


            CoreServices.EventLog.LogEvent("E", "SessionStorage", "READONLYSESSION", eventDescription);
        }

        #endregion

#pragma warning restore BH1001 // 'Session[]' should not be used. Use 'SessionHelper.GetValue()' instead.
#pragma warning restore BH1002 // 'Session[]' should not be used. Use 'SessionHelper.SetValue()' instead.
    }
}