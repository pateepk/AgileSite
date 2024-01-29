using System;
using System.Security.Cryptography;
using System.Text;
using System.Web.Security;

using CMS.EventLog;
using CMS.Helpers;
using CMS.SiteProvider;

namespace CMS.Membership
{
    /// <summary>
    /// Provides support for storing Windows user information in cookies.
    /// </summary>
    internal static class WindowsUserCookieHelper
    {
        #region "Constants"

        /// <summary>
        /// Cookie key for authenticated Windows user
        /// </summary>
        private const string WINDOWS_USER_KEY = CookieName.WindowsUser;

        /// <summary>
        /// MachineKey protection purpose
        /// </summary>
        private const string PURPOSE = "Windows user";

        #endregion


        #region "Methods"

        internal static string GetKnownWindowsUserName()
        {
            var encodedUserName = CookieHelper.GetValue(WINDOWS_USER_KEY);

            if (String.IsNullOrEmpty(encodedUserName))
            {
                return null;
            }

            try
            {
                return Encoding.UTF8.GetString(MachineKey.Unprotect(Convert.FromBase64String(encodedUserName), PURPOSE));
            }
            catch (Exception ex) when (ex is FormatException || ex is CryptographicException)
            {
                EventLogProvider.LogException("Authentication", "AUTHENTICATIONFAIL", ex, SiteContext.CurrentSiteID);

                return null;
            }
        }


        internal static void SetKnownWindowsUserName(string userName)
        {
            if (userName == null)
            {
                return;
            }

            var encodedUserName = Convert.ToBase64String(MachineKey.Protect(Encoding.UTF8.GetBytes(userName), PURPOSE));

            CookieHelper.SetValue(CookieName.WindowsUser, encodedUserName, DateTimeHelper.ZERO_TIME);
        }


        internal static void ClearKnownWindowsUserName()
        {
            CookieHelper.SetValue(CookieName.WindowsUser, "", DateTime.Now.AddDays(-1));
        }

        #endregion
    }
}