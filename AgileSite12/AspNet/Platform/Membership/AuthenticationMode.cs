using System;
using System.Configuration;
using System.Security.Principal;
using System.Web.Security;

using CMS.Base;
using CMS.Core;
using CMS.Helpers;

namespace CMS.Membership
{
    /// <summary>
    /// Specifies the ASP.NET authentication scheme that is used to identify users.
    /// </summary>
    public static class AuthenticationMode
    {
        #region "Constants & Fields"

        private const string AUTH_TYPE_DEFAULT = "default";
        private const string AUTH_TYPE_FORMS = "forms";
        private const string AUTH_TYPE_WINDOWS = "windows";
        private const string AUTH_TYPE_BOTH = "both";

        private static string mAuthenticationTypeCheck;

        #endregion


        #region "Properties"

        /// <summary>
        /// Gets the value that sets the type of current authentication
        /// AUTH_TYPE_DEFAULT  - Use default
        /// AUTH_TYPE_FORMS    - Forcibly sets Forms authentication
        /// AUTH_TYPE_WINDOWS  - Forcibly sets Windows authentication
        /// AUTH_TYPE_BOTH     - Forcibly sets Windows authentication  and Forms authentication
        /// </summary>
        private static string AuthenticationTypeCheck
        {
            get
            {
                // Check whether authentication type check is set
                if (mAuthenticationTypeCheck == null)
                {
                    // Get authentication type from settings
                    mAuthenticationTypeCheck = ValidationHelper.GetString(SettingsHelper.AppSettings["CMSAuthenticationType"], AUTH_TYPE_DEFAULT).ToLowerInvariant();
                }

                return mAuthenticationTypeCheck;
            }
        }

        #endregion


        #region "Authentication modes"

        /// <summary>
        /// Returns true if the authentication mode is Windows authentication.
        /// </summary>
        public static bool IsWindowsAuthentication()
        {
            // Default checking
            if (AuthenticationTypeCheck == AUTH_TYPE_DEFAULT)
            {
                // Get authentication mode from current request
                var context = CMSHttpContext.Current;
                if ((context != null) && (context.User != null))
                {
                    var identity = context.User.Identity;
                    return (identity.AuthenticationType != "Forms")
                        && !String.IsNullOrEmpty(identity.AuthenticationType)
                        && (identity is WindowsIdentity);
                }

                // Return false if context is not available
                return false;
            }

            // Return true if type of authentication check is set to Windows or Both
            return ((AuthenticationTypeCheck == AUTH_TYPE_WINDOWS) || (AuthenticationTypeCheck == AUTH_TYPE_BOTH));
        }


        /// <summary>
        /// Returns true if the authentication mode is Forms authentication.
        /// </summary>
        public static bool IsFormsAuthentication()
        {
            if (AuthenticationTypeCheck == AUTH_TYPE_DEFAULT)
            {
                // Get authentication mode from current request
                var context = CMSHttpContext.Current;
                if ((context != null) && (context.User != null))
                {
                    return (context.User.Identity.AuthenticationType == "Forms") || String.IsNullOrEmpty(context.User.Identity.AuthenticationType);
                }

                // Return false if context is not available
                return false;
            }

            // Return true if type of authentication check is set to Forms or Both
            return ((AuthenticationTypeCheck == AUTH_TYPE_FORMS) || (AuthenticationTypeCheck == AUTH_TYPE_BOTH));
        }


        /// <summary>
        /// If true, mixed authentication is used.
        /// </summary>
        public static bool IsMixedAuthentication()
        {
            try
            {
                return (System.Web.Security.Membership.Providers["CMSADProvider"] != null) && (Roles.Providers["CMSADRoleProvider"] != null);
            }
            catch(ConfigurationException ex)
            {
                CoreServices.EventLog.LogException("MixedAuthentication", "CHECK", ex);
                return false;
            }
        }

        #endregion
    }
}
