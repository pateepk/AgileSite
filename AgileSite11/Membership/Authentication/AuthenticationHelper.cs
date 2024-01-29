using System;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Configuration;
using System.Security.Principal;
using System.DirectoryServices;
using System.Collections.Generic;
using System.DirectoryServices.AccountManagement;

using CMS.DataEngine;
using CMS.EmailEngine;
using CMS.EventLog;
using CMS.Helpers;
using CMS.LicenseProvider;
using CMS.MacroEngine;
using CMS.Protection;
using CMS.Base;
using CMS.SiteProvider;
using CMS.Core;

namespace CMS.Membership
{
    /// <summary>
    /// Contains method connected with user authentication
    /// </summary>
    public static class AuthenticationHelper
    {
        #region "Constants"

        /// <summary>
        /// Setting value for displaying warning message to user with expired password
        /// </summary>
        public const string PASSWORD_EXPIRATION_WARNING = "SHOWWARNING";


        /// <summary>
        /// Setting value for locking user account to user with expired password
        /// </summary>
        public const string PASSWORD_EXPIRATION_LOCK = "LOCKACCOUNT";

        /// <summary>
        /// URL to default logon page.
        /// </summary>
        public const string DEFAULT_LOGON_PAGE = "~/cmspages/logon.aspx";

        private const string CURRENT_USER = "CurrentUser";

        // Constant used for identify user within AD.
        private const string USER = "user";
        // Constant used for identify group within AD.
        private const string GROUP = "group";

        #endregion


        #region "Private fields"

        private static bool? mUseSessionCookies;
        private static bool? mSynchronizeUserGUIDs;
        private static bool? mFederationAuthentication;
        private static bool? mImportWindowsRoles;
        private static bool? mUpdateLastLogonForExternalUsers;
        private static readonly CMSStatic<CurrentUserInfo> mGlobalPublicUser = new CMSStatic<CurrentUserInfo>();
        private static readonly CMSStatic<SafeDictionary<int, DateTime>> mKickUsers = new CMSStatic<SafeDictionary<int, DateTime>>(() => new SafeDictionary<int, DateTime>());
        private static string mNetBiosDomainName = String.Empty;

        // Specifies whether new session ID is generated after user log in/out.
        private static bool? mRenewSessionAuthChange;

        // Encapsulates the server or domain against which all operations are performed.
        private static PrincipalContext mPrincipal;

        #endregion


        #region "Private properties"

        /// <summary>
        /// Gets the value that indicates whether last logon information should be updated for external users
        /// </summary>
        private static bool UpdateLastLogonForExternalUsers
        {
            get
            {
                if (mUpdateLastLogonForExternalUsers == null)
                {
                    mUpdateLastLogonForExternalUsers = ValidationHelper.GetBoolean(SettingsHelper.AppSettings["CMSUpdateLastLogonForExternalUsers"], true);
                }
                return mUpdateLastLogonForExternalUsers.Value;
            }
        }


        /// <summary>
        /// Returns whether new session ID is generated after user log in/out.
        /// </summary>
        private static bool RenewSessionAuthChange
        {
            get
            {
                if (mRenewSessionAuthChange == null)
                {
                    mRenewSessionAuthChange = ValidationHelper.GetBoolean(SettingsHelper.AppSettings["CMSRenewSessionAuthChange"], false);
                }

                return mRenewSessionAuthChange.Value;
            }
        }


        /// <summary>
        /// If true, the session cookies will be used for the authentication.
        /// </summary>
        public static bool UseSessionCookies
        {
            get
            {
                if (mUseSessionCookies == null)
                {
                    mUseSessionCookies = ValidationHelper.GetBoolean(SettingsHelper.AppSettings["CMSUseSessionCookies"], true);
                }
                return mUseSessionCookies.Value;
            }
            set
            {
                mUseSessionCookies = value;
            }
        }


        /// <summary>
        /// Collection of the users which should be kicked.
        /// </summary>
        private static SafeDictionary<int, DateTime> KickUsers
        {
            get
            {
                return mKickUsers;
            }
        }


        /// <summary>
        /// Global public user object.
        /// </summary>
        public static CurrentUserInfo GlobalPublicUser
        {
            get
            {
                var user = mGlobalPublicUser.Value;
                if ((user == null) || !user.Generalized.IsObjectValid)
                {
                    DebugHelper.SetContext("GlobalPublicUser");

                    UserInfo sourceInfo = UserInfoProvider.GetUserInfo("public");
                    // If some source user info found, create CurrentUserInfo object
                    if (sourceInfo != null)
                    {
                        user = new CurrentUserInfo(sourceInfo, false);
                    }
                    else
                    {
                        throw new InvalidOperationException("User 'public' not found!");
                    }

                    DebugHelper.ReleaseContext();

                    mGlobalPublicUser.Value = user;
                }

                return user;
            }
        }

        #endregion


        #region "Public properties"

        /// <summary>
        /// Encapsulates the server or domain against which all operations are performed.
        /// </summary>
        public static PrincipalContext PrincipalContext
        {
            get
            {
                return mPrincipal ?? (mPrincipal = new PrincipalContext(ContextType.Domain));
            }
        }


        /// <summary>
        /// Get domain name in NetBios format
        /// </summary>
        /// <returns>NetBiosDomainName</returns>
        public static string NetBiosDomainName
        {
            get
            {
                if (mNetBiosDomainName == String.Empty)
                {
                    string dnsDomainName = PrincipalContext.ConnectedServer;
                    int firstDelimiter = dnsDomainName.IndexOf('.');
                    int length = dnsDomainName.Length - (firstDelimiter + 1);
                    dnsDomainName = dnsDomainName.Substring(firstDelimiter + 1, length);

                    var rootDSE = new DirectoryEntry($"LDAP://{dnsDomainName}/RootDSE");
                    string configurationNamingContext = rootDSE.Properties["configurationNamingContext"][0].ToString();

                    var searchRoot = new DirectoryEntry("LDAP://cn=Partitions," + configurationNamingContext);
                    var searcher = new DirectorySearcher(searchRoot);
                    searcher.SearchScope = SearchScope.OneLevel;
                    searcher.PropertiesToLoad.Add("netbiosname");
                    searcher.Filter = $"(&(objectcategory=Crossref)(dnsRoot={dnsDomainName})(netBIOSName=*))";

                    SearchResult result = searcher.FindOne();

                    if (result != null)
                    {
                        mNetBiosDomainName = result.Properties["netbiosname"][0].ToString();
                    }
                }
                return mNetBiosDomainName;
            }
        }


        /// <summary>
        /// If true, the system imports the external user roles.
        /// </summary>
        public static bool ImportExternalRoles
        {
            get;
            set;
        } = true;


        /// <summary>
        /// If true, the system imports the external users.
        /// </summary>
        public static bool ImportExternalUsers
        {
            get;
            set;
        } = true;


        /// <summary>
        /// Name of connection string used by AD membership provider.
        /// </summary>
        public static string ADConnectionStringName
        {
            get;
            set;
        }


        /// <summary>
        /// Name of user used to authenticate against AD by membership provider.
        /// </summary>
        public static string ADUsername
        {
            get;
            set;
        }


        /// <summary>
        /// Password of user used to authenticate against AD by membership provider.
        /// </summary>
        public static string ADPassword
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates whether to synchronize user's guid during import from Active Directory.
        /// </summary>
        public static bool SynchronizeUserGUIDs
        {
            get
            {
                if (mSynchronizeUserGUIDs == null)
                {
                    mSynchronizeUserGUIDs = ValidationHelper.GetBoolean(SettingsHelper.AppSettings["CMSSynchronizeUserGUIDs"], true);
                }
                return mSynchronizeUserGUIDs.Value;

            }
            set
            {
                mSynchronizeUserGUIDs = value;
            }
        }


        /// <summary>
        /// Gets or sets whether roles are imported during the Windows/Active directory authentication.
        /// </summary>
        public static bool ImportWindowsRoles
        {
            get
            {
                if (mImportWindowsRoles == null)
                {
                    mImportWindowsRoles = ValidationHelper.GetBoolean(SettingsHelper.AppSettings["CMSImportWindowsRoles"], true);
                }

                return mImportWindowsRoles.Value;
            }
            set
            {
                mImportWindowsRoles = value;
            }
        }


        /// <summary>
        /// Gets or sets whether federation authentication is used.
        /// </summary>
        public static bool FederationAuthentication
        {
            get
            {
                if (mFederationAuthentication == null)
                {
                    mFederationAuthentication = ValidationHelper.GetBoolean(SettingsHelper.AppSettings["CMSFederationAuthentication"], false);
                }
                return mFederationAuthentication.Value;
            }
            set
            {
                mFederationAuthentication = value;
            }
        }

        #endregion


        #region "Current user methods"

        /// <summary>
        /// Returns true if the current user is authenticated.
        /// </summary>
        public static bool IsAuthenticated()
        {
            return RequestContext.IsUserAuthenticated;
        }


        /// <summary>
        /// Gets the current user.
        /// </summary>
        /// <remarks>Do not call this method before <see cref="HttpApplication.AuthenticateRequest" /> event is executed because result won't be valid</remarks>
        internal static CurrentUserInfo GetCurrentUser()
        {
            // Get the source user info from the database
            CurrentUserInfo uInfo = null;

            if ((CMSHttpContext.Current != null) && (CMSHttpContext.Current.User != null))
            {
                DebugHelper.SetContext(CURRENT_USER);

                UserInfo sourceInfo = null;

                if (RequestContext.IsUserAuthenticated)
                {
                    // Do not initialize context with current user -> this could lead to stack overflow
                    using (new CMSActionContext { AllowInitUser = false })
                    {
                        sourceInfo =
                            RequestHelper.IsWindowsAuthentication()
                                ? AuthenticateUserWindows(CMSHttpContext.Current.User, SiteContext.CurrentSiteName)
                                : UserInfoProvider.GetUserInfo(RequestContext.UserName);
                    }

                    // If current user is "Public" and is authenticated => Sign out user
                    SignOutPublicUser(sourceInfo);
                }

                // If some source user info found, create CurrentUserInfo object
                if (sourceInfo != null)
                {
                    uInfo = new CurrentUserInfo(sourceInfo, false);
                }
            }

            // If no info found, get the public user
            if (uInfo == null)
            {
                uInfo = GlobalPublicUser;
            }

            DebugHelper.ReleaseContext();

            return uInfo;
        }


        /// <summary>
        /// Sign out public user.
        /// </summary>
        /// <param name="ui">User info</param>
        private static void SignOutPublicUser(UserInfo ui)
        {
            // If current user is "Public" => Sign out him
            if ((ui == null) || (ui.IsPublic() && !VirtualContext.IsPreviewLinkInitialized) || !ui.Enabled)
            {
                MembershipContext.AuthenticatedUser = null;

                // Do not sign out user, if authentication is not handled by CMS
                if (SystemContext.IsCMSRunningAsMainApplication)
                {
#pragma warning disable BH1012 // 'FormsAuthentication.SignOut()' should not be used. Use 'AuthenticationHelper.SignOut()' instead.
                    FormsAuthentication.SignOut();
#pragma warning restore BH1012 // 'FormsAuthentication.SignOut()' should not be used. Use 'AuthenticationHelper.SignOut()' instead.

                    if (!RequestHelper.IsAsyncPostback() && RequestHelper.IsFormsAuthentication())
                    {
                        URLHelper.Redirect(URLHelper.ResolveUrl(RequestContext.CurrentURL));
                    }
                }
            }
        }


        /// <summary>
        /// Signs the user out.
        /// </summary>
        public static void SignOut(string originalSignOutUrl = null)
        {
            string signOutUrl = originalSignOutUrl;
            SignOut(ref signOutUrl);

            if (!String.IsNullOrEmpty(signOutUrl))
            {
                URLHelper.Redirect(signOutUrl);
            }
        }


        /// <summary>
        /// Signs the user out.
        /// </summary>
        public static void SignOut(ref string signOutUrl)
        {
            // Initiate the SignOut event
            using (var e = SecurityEvents.SignOut.StartEvent(MembershipContext.AuthenticatedUser, ref signOutUrl))
            {
                if (e.CanContinue())
                {
                    SignOutUser();

                    // Ensure user reload
                    MembershipContext.AuthenticatedUser = null;

                    // Keep sign out flag
                    MembershipContext.SignOutPending = true;
                }

                e.FinishEvent();
            }

        }

        #endregion


        #region "Authentication methods"

        /// <summary>
        /// Impersonates current user.
        /// </summary>
        /// <param name="impersonatedUser">User to which impersonate the current user</param>
        /// <param name="redirectionUrl">Custom redirection relative URL</param>
        /// <param name="forceRedirect">Indicates if redirect should be forced even if no redirect URL is specified</param>
        public static void ImpersonateUser(UserInfo impersonatedUser, string redirectionUrl = null, bool forceRedirect = true)
        {
            if (impersonatedUser == null)
            {
                CancelImpersonation();
                return;
            }

            // User is already impersonated or user is not global administrator or is not enabled
            if (MembershipContext.CurrentUserIsImpersonated()
                || !MembershipContext.AuthenticatedUser.CheckPrivilegeLevel(UserPrivilegeLevelEnum.Admin)
                || impersonatedUser.CheckPrivilegeLevel(UserPrivilegeLevelEnum.Admin)
                || !impersonatedUser.Enabled)
            {
                return;
            }

            if (RequestHelper.IsFormsAuthentication())
            {
                SetImpersonatedUser(impersonatedUser);

                // Ensure correct value for first logged-in users
                if (impersonatedUser.UserPasswordLastChanged == DateTime.MinValue)
                {
                    impersonatedUser.UserPasswordLastChanged = DateTime.Now;
                    UserInfoProvider.SetUserInfo(impersonatedUser);
                }

                // Set authentication cookie and context values
                AuthenticateUser(impersonatedUser.UserName, false, false);
            }
            else
            {
                SetImpersonatedUser(impersonatedUser);
            }

            if (!String.IsNullOrEmpty(redirectionUrl))
            {
                URLHelper.Redirect(URLHelper.ResolveUrl(redirectionUrl));
            }
            else if (forceRedirect)
            {
                // If user is global admin, redirect to the current page
                if (impersonatedUser.CheckPrivilegeLevel(UserPrivilegeLevelEnum.Admin))
                {
                    URLHelper.Redirect(RequestContext.CurrentURL);
                }
                // If user is editor, redirect to the Admin page
                else if (impersonatedUser.CheckPrivilegeLevel(UserPrivilegeLevelEnum.Editor, SiteContext.CurrentSiteName))
                {
                    URLHelper.Redirect(URLHelper.ResolveUrl("~/CMSMessages/Redirect.aspx?frame=top&url=~/Admin/cmsadministration.aspx"));
                }
                // Redirect to the live site
                else
                {
                    URLHelper.Redirect(URLHelper.ResolveUrl("~/CMSMessages/Redirect.aspx?frame=top&url=~/&livesite=1"));
                }
            }
        }


        /// <summary>
        /// Signs out the impersonated user and switches to the original user.
        /// </summary>
        /// <returns>Original user if impersonation was used, otherwise returns null</returns>
        public static UserInfo CancelImpersonation()
        {
            if (MembershipContext.CurrentUserIsImpersonated())
            {
                var data = ImpersonationHelper.GetDataFromCookie();
                var originalUser = UserInfoProvider.GetUserInfoByGUID(data.OriginalUserId);
                var impersonatedUser = UserInfoProvider.GetUserInfoByGUID(data.ImpersonatedUserId);

                ImpersonationHelper.RemoveCookie();
                MembershipContext.AuthenticatedUser = null;

                var message = $"User {originalUser.UserName} canceled impersonation of user {impersonatedUser.UserName}.";
                LogImpersonationEvent(originalUser, message, "ImpersonationCanceled");

                // Forms authentication requires to switch impersonated user by original one
                if (RequestHelper.IsFormsAuthentication() && originalUser != null)
                {
                    AuthenticateUser(originalUser.UserName, false, false);
                }

                return originalUser;
            }

            return null;
        }


        /// <summary>
        /// Sets given user as impersonated one, current user is set as original one.
        /// </summary>
        /// <param name="impersonatedUser">impersonated user</param>
        private static void SetImpersonatedUser(IUserInfo impersonatedUser)
        {
            var currentUser = MembershipContext.AuthenticatedUser;

            // Both cookies are required, original in forms authentication
            ImpersonationHelper.SetCookie(impersonatedUser, currentUser);
            MembershipContext.AuthenticatedUser = null;

            var message = $"User {currentUser.UserName} is impersonated as user {impersonatedUser.UserName}.";
            LogImpersonationEvent(currentUser, message, "ImpersonationStarted");
        }


        /// <summary>
        /// Logs impersonation event
        /// </summary>
        /// <param name="user">event source user</param>
        /// <param name="message">event message</param>
        /// <param name="eventCode">event code</param>
        private static void LogImpersonationEvent(IUserInfo user, string message, string eventCode)
        {
            EventLogProvider.LogEvent(EventType.INFORMATION, "Administration", eventCode, message,
                RequestContext.CurrentURL, user.UserID, user.UserName, 0, null, null, SiteContext.CurrentSiteID);
        }


        /// <summary>
        /// Authenticate given user.
        /// </summary>
        /// <param name="userName">User name</param>
        /// <param name="createPersistentCookie">Indicates if persistent cookie should be created</param>
        /// <param name="loadCultures">If true, the preferred cultures of the user are loaded</param>
        public static void AuthenticateUser(string userName, bool createPersistentCookie, bool loadCultures = true)
        {
            UserInfo ui = UserInfoProvider.GetUserInfo(userName);

            if (SecurityEvents.Authenticate.IsBound)
            {
                // Initiate the authentication event
                SecurityEvents.Authenticate.StartEvent(ref ui, userName);
            }

            if (ui != null)
            {
                // Set authentication cookie
                FormsAuthentication.SetAuthCookie(ui.UserName, createPersistentCookie);

                // Set context values
                MembershipContext.AuthenticatedUser = new CurrentUserInfo(ui, true);

                if (loadCultures)
                {
                    UserInfoProvider.SetPreferredCultures(ui);
                }

                // Log authentication event
                EventLogProvider.LogEvent(EventType.INFORMATION, "Authentication", "AUTHENTICATED", null, RequestContext.RawURL, ui.UserID, ui.UserName, 0, null, RequestContext.UserHostAddress, SiteContext.CurrentSiteID);
            }
        }


        /// <summary>
        /// Authenticates user against the database.
        /// </summary>
        /// <param name="userName">User name to authenticate</param>
        /// <param name="password">Password to authenticate</param>
        /// <param name="siteName">Site name</param>
        /// <param name="login">Indicates if methods is called during user login</param>
        /// <param name="source">Source of calling</param>
        public static UserInfo AuthenticateUser(string userName, string password, string siteName, bool login = true, AuthenticationSourceEnum source = AuthenticationSourceEnum.Standard)
        {
            UserInfo result = null;
            SiteInfo si = SiteInfoProvider.GetSiteInfo(siteName);

            // Authenticate only user who is not public
            if ((userName != null) && !userName.Equals("public", StringComparison.InvariantCultureIgnoreCase))
            {
                result = UserInfoProvider.GetUserInfoForSitePrefix(userName, si);
            }

            // Authentication for existing user
            if (result != null)
            {
                // Not enabled or external or domain user has never valid password
                if (result.Enabled && !result.IsExternal && !result.UserIsDomain)
                {
                    string returnUrl = URLHelper.AddParameterToUrl(RequestContext.CurrentURL, "username", userName);

                    // Check if password hasn't expired yet
                    CheckPasswordExpiration(result, siteName, true, returnUrl);

                    if (UserInfoProvider.IsUserPasswordDifferent(result, password))
                    {
                        CheckInvalidPasswordAttempts(result, siteName, returnUrl);

                        result = null;
                    }

                    // Check password policy
                    if ((result != null) && SettingsKeyInfoProvider.GetBoolValue(siteName + ".CMSPolicyForcePolicyOnLogon") && !SecurityHelper.CheckPasswordPolicy(password, siteName))
                    {
                        if (source == AuthenticationSourceEnum.Standard)
                        {
                            // Generate request GUID
                            DateTime time = DateTime.Now;

                            // Set to user
                            result.UserPasswordRequestHash = SecurityHelper.GenerateConfirmationEmailHash(result.UserGUID.ToString(), time);

                            // Update the user record
                            UserInfoProvider.SetUserInfo(result);

                            // Prepare URL for password reset
                            string requestUrl = GetResetPasswordUrl(siteName);
                            requestUrl = URLHelper.AddParameterToUrl(requestUrl, "hash", result.UserPasswordRequestHash);
                            requestUrl = URLHelper.AddParameterToUrl(requestUrl, "datetime", DateTimeUrlFormatter.Format(time));
                            requestUrl = URLHelper.AddParameterToUrl(requestUrl, "policyreq", "1");
                            requestUrl = URLHelper.AddParameterToUrl(requestUrl, "returnurl", returnUrl);

                            // Redirect to force changing password
                            URLHelper.Redirect(requestUrl);
                        }
                        else
                        {
                            result = null;
                        }
                    }
                }
                else
                {
                    MembershipContext.UserAccountLockedDueToInvalidLogonAttempts = (UserAccountLockCode.ToEnum(result.UserAccountLockReason) == UserAccountLockEnum.MaximumInvalidLogonAttemptsReached);
                    MembershipContext.UserAccountLockedDueToPasswordExpiration = (UserAccountLockCode.ToEnum(result.UserAccountLockReason) == UserAccountLockEnum.PasswordExpired);
                    result = null;
                }
            }

            // Run custom authentication event if set
            if (SecurityEvents.Authenticate.IsBound)
            {
                bool wasAuthenticated = (result != null);

                // Initiate the authentication event
                SecurityEvents.Authenticate.StartEvent(ref result, userName, password, siteName: siteName);

                // If local authentication not passed and external did, add the user as an external user
                if (!wasAuthenticated && (result != null) && (result.IsExternal) && login)
                {
                    EnsureExternalUser(result);
                }
            }

            // Get the site id
            int siteId = 0;
            if (si != null)
            {
                siteId = si.SiteID;
            }

            // Do site check
            result = UserInfoProvider.CheckUserBelongsToSite(result, siteName);

            // Check whether user is kicked
            if ((result != null) && !CanUserLogin(result.UserID))
            {
                result = null;
            }

            // Process the result
            if (result == null)
            {
                if (!RequestHelper.IsMixedAuthentication() && login)
                {
                    // Insert information about unsuccessful authentication to eventlog.
                    EventLogProvider.LogEvent(EventType.INFORMATION, "Authentication", "AUTHENTICATIONFAIL", null, RequestContext.RawURL, 0, userName, 0, null, RequestContext.UserHostAddress, siteId);
                }
            }
            else
            {
                if (login)
                {
                    FinalizeAuthenticationProcess(result, siteId);
                }
            }

            // Return the result
            return result;
        }


        /// <summary>
        /// Finalizes the authentication process. Does all the actions that are needed
        /// after successful authentication.
        /// </summary>
        /// <param name="user">User info.</param>
        /// <param name="siteId">Site ID.</param>
        public static void FinalizeAuthenticationProcess(UserInfo user, int siteId)
        {
            // Set the cultures
            UserInfoProvider.SetPreferredCultures(user);

            UpdateLastLogonInformation(user);

            // Reset invalid log in attempts
            user.UserInvalidLogOnAttempts = 0;

            // Update last password change for first time user was authenticated
            if (user.UserPasswordLastChanged == DateTime.MinValue)
            {
                user.UserPasswordLastChanged = DateTime.Now;
            }

            // Disable logging
            using (CMSActionContext context = new CMSActionContext())
            {
                context.LogSynchronization = false;
                context.LogExport = false;
                context.LogEvents = false;

                // Update the user record
                UserInfoProvider.SetUserInfo(user);
            }

            RemoveSessionID();

            // Insert information about successful authentication to event log.
            EventLogProvider.LogEvent(EventType.INFORMATION, "Authentication", "AUTHENTICATIONSUCC", null, RequestContext.RawURL, user.UserID, user.UserName, 0, null, RequestContext.UserHostAddress, siteId);
        }


        /// <summary>
        /// Sets the authentication cookie expiration
        /// </summary>
        public static void ExtendAuthenticationCookieExpiration()
        {
            // Extend the expiration of the authentication cookie if required
            if (!UseSessionCookies && (HttpContext.Current != null) && (HttpContext.Current.Session != null))
            {
                CookieHelper.ChangeCookieExpiration(FormsAuthentication.FormsCookieName, DateTime.Now.AddMinutes(HttpContext.Current.Session.Timeout), true);
            }
        }


        /// <summary>
        /// Authenticates user against the database.
        /// </summary>
        /// <param name="username">User name to authenticate</param>
        /// <param name="password">Password to authenticate</param>
        /// <param name="siteName">Site name</param>
        /// <param name="membershipProvider">Instance of the <see cref="MembershipProvider"/> to use. If not specified the membership provider specified in .config file is used</param>
        /// <param name="roleProvider">Instance of <see cref="System.Web.Security.RoleProvider" /> to use. If not specified the role provider specified in .config file is used.</param>
        public static UserInfo AuthenticateUserAD(string username, string password, string siteName, MembershipProvider membershipProvider = null, RoleProvider roleProvider = null)
        {
            membershipProvider = membershipProvider ?? System.Web.Security.Membership.Providers["CMSADProvider"];
            if (membershipProvider != null)
            {
                // Get the site id
                int siteId = 0;
                SiteInfo si = SiteInfoProvider.GetSiteInfo(siteName);
                if (si != null)
                {
                    siteId = si.SiteID;
                }

                // Validate the user against AD
                if (membershipProvider.ValidateUser(username, password))
                {
                    // Get the roles if the roles are to be imported
                    string[] roles = null;
                    if (!FederationAuthentication && ImportWindowsRoles)
                    {
                        roles = GetUserADRoles(username, roleProvider);
                    }
                    SecurityIdentifier sid = null;
                    if (!FederationAuthentication && SynchronizeUserGUIDs)
                    {
                        MembershipUser user = membershipProvider.GetUser(username, true);
                        if (user?.ProviderUserKey != null)
                        {
                            sid = new SecurityIdentifier(user.ProviderUserKey.ToString());
                        }
                    }

                    UserInfo result = EnsureADUser(username, sid, siteName, roles);

                    // Write result to the event log
                    if (result != null)
                    {
                        // Insert information about successful authentication to event log.
                        EventLogProvider.LogEvent(EventType.INFORMATION, "Authentication", "AUTHENTICATIONSUCC", null, RequestContext.RawURL, result.UserID, username, 0, null, RequestContext.UserHostAddress, siteId);

                        return result;
                    }
                }

                // Insert information about unsuccessful authentication to event log.
                EventLogProvider.LogEvent(EventType.INFORMATION, "Authentication", "AUTHENTICATIONFAIL", null, RequestContext.RawURL, 0, username, 0, null, RequestContext.UserHostAddress, siteId);

                return null;
            }
            else
            {
                throw new Exception("[UserInfoProvider.AuthenticateUserAD]: Membership provider 'CMSADProvider' is not configured.");
            }
        }


        /// <summary>
        /// Authenticates the external user with Windows authentication.
        /// </summary>
        /// <param name="user">User to authenticate</param>
        /// <param name="siteName">Site name for authentication</param>
        internal static UserInfo AuthenticateUserWindows(IPrincipal user, string siteName)
        {
            UserInfo userInfo = null;

            // Check if the windows user was already recognized and synced
            var userName = WindowsUserCookieHelper.GetKnownWindowsUserName();
            var windowsUserName = user.Identity.Name;

            if (!String.IsNullOrEmpty(userName) && (userName == windowsUserName))
            {
                var safeUserName = EnsureSafeUserName(windowsUserName, siteName);

                userInfo = UserInfoProvider.GetUserInfo(safeUserName);

                // Do not handle disabled or not domain user 
                if (userInfo != null && (!userInfo.Enabled || !userInfo.UserIsDomain))
                {
                    userInfo = null;
                }
            }

            if (userInfo == null)
            {
                // Get the roles if the roles are to be imported
                string[] roles = null;
                if (!FederationAuthentication && ImportWindowsRoles)
                {
                    roles = GetUserWindowsRoles(windowsUserName);
                }

                var windowsIdentity = user.Identity as WindowsIdentity;
                var sid = windowsIdentity?.User;

                userInfo = EnsureADUser(windowsUserName, sid, siteName, roles);

                WindowsUserCookieHelper.SetKnownWindowsUserName(windowsUserName);
            }

            userInfo = HandleImpersonationForADUser(userInfo);

            return userInfo;
        }


        private static UserInfo HandleImpersonationForADUser(UserInfo userInfo)
        {
            if (userInfo != null)
            {
                // Handle impersonation
                if (userInfo.CheckPrivilegeLevel(UserPrivilegeLevelEnum.Admin) && MembershipContext.CurrentUserIsImpersonated())
                {
                    var impersonatedUserInfo = UserInfoProvider.GetUserInfo(MembershipContext.ImpersonatedUserName);
                    if ((impersonatedUserInfo != null) && !impersonatedUserInfo.CheckPrivilegeLevel(UserPrivilegeLevelEnum.Admin))
                    {
                        // Impersonate user
                        userInfo = impersonatedUserInfo;
                    }
                    else
                    {
                        CancelImpersonation();
                    }
                }
            }

            return userInfo;
        }


        /// <summary>
        /// Sets preferred cultures for a windows user
        /// </summary>
        /// <param name="siteName">Site name</param>
        public static void SetWindowsUserCultures(string siteName)
        {
            // If authentication mode is Windows, set user UI culture
            if (!String.IsNullOrEmpty(siteName) && RequestHelper.IsWindowsAuthentication() && IsAuthenticated())
            {
                UserInfo currentUser = MembershipContext.AuthenticatedUser;
                if (!currentUser.IsPublic())
                {
                    UserInfoProvider.SetPreferredCultures(currentUser);
                }
            }
        }


        /// <summary>
        /// Prepares the user authentication GUID, adds it as a query string parameter to supplied Url.
        /// </summary>
        /// <param name="user">UserInfo of particular user</param>
        /// <param name="targetUrl">Target URL</param>
        /// <returns>Target URL with authentication GUID parameter</returns>
        public static string GetUserAuthenticationUrl(UserInfo user, string targetUrl)
        {
            // Create new authentication GUID for user
            Guid authGuid = Guid.NewGuid();
            user.UserAuthenticationGUID = authGuid;

            using (CMSActionContext context = new CMSActionContext())
            {
                // Disable version creation
                context.CreateVersion = false;
                // Disable logging of tasks
                context.DisableLogging();

                user.Generalized.SetObject();
            }

            // Add authentication GUID token to URL
            return URLHelper.AddParameterToUrl(targetUrl, "authenticationGuid", authGuid.ToString());
        }


        /// <summary>
        /// Authenticate user with provided OpenID parameters. Create new user if createNew is set to TRUE and user doesn't
        /// exist in DB.
        /// </summary>
        /// <param name="claimedID">OpenID Claimed Identifier</param>
        /// <param name="providerUrl">OpenID provider URL</param>
        /// <param name="siteName">Site name</param>
        /// <param name="generatePassword">Indicates if random password should be generated</param>
        /// <param name="disableConfirmation">Indicates if e-mail confirmation of newly registered user is disabled</param>
        /// <param name="error">Error message which will be filled when error occurred</param>
        /// <returns>UserInfo with authenticated user or null if user is not found</returns>
        public static UserInfo AuthenticateOpenIDUser(string claimedID, string providerUrl, string siteName, bool generatePassword, bool disableConfirmation, ref string error)
        {
            // Do not initialize context with current user -> this could lead to stack overflow
            using (new CMSActionContext { AllowInitUser = false })
            {
                // Check if license permits OpenID
                LicenseHelper.CheckFeatureAndRedirect(RequestContext.CurrentDomain, FeatureEnum.OpenID);

                // Check if parameters are set
                if (!String.IsNullOrEmpty(claimedID) && !String.IsNullOrEmpty(providerUrl))
                {
                    // Try to find Claimed ID in DB
                    UserInfo ui = OpenIDUserInfoProvider.GetUserInfoByOpenID(claimedID);

                    // User doesn't exist in DB = create a new one only if user with specified OpenID name doesn't exist
                    if (ui == null)
                    {
                        // Check if IP address is not banned for possible registration
                        BannedIPInfoProvider.CheckIPandRedirect(siteName, BanControlEnum.Registration);

                        // Create user info
                        ui = new UserInfo();

                        // OpenID user will have special prefix + GUID as a user name, unless they change it later
                        string guid = Guid.NewGuid().ToString();
                        ui.UserName = UserInfoProvider.OPENID_USERS_PREFIX + guid;

                        // OpenID full name
                        ui.FullName = UserInfoProvider.OPENID_FULLNAME_PREFIX + guid;

                        ui.IsExternal = true;

                        UserInfoProvider.SetUserInfo(ui);

                        // Generate random password for newly created OpenID user
                        if (generatePassword)
                        {
                            UserInfoProvider.SetPassword(ui.UserName, UserInfoProvider.GenerateNewPassword(siteName));
                            // Update/reload password to user info class
                            ui = UserInfoProvider.GetUserInfo(ui.UserID);
                        }

                        // Add current site
                        var rolesTable = ui.SitesRoles[siteName.ToLowerInvariant()];
                        if (rolesTable == null)
                        {
                            rolesTable = ui.CreateNewRolesDictionary();
                            ui.SitesRoles[siteName.ToLowerInvariant()] = rolesTable;
                        }

                        // Assign the default roles
                        string[] roles = SettingsKeyInfoProvider.GetValue(siteName + ".CMSOpenIDRoles").Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (string role in roles)
                        {
                            string roleName = role.Trim().ToLowerInvariant();
                            if (rolesTable[roleName] == null)
                            {
                                rolesTable[roleName] = 0;
                            }
                        }

                        // Ensure the user roles
                        UserInfoProvider.EnsureRolesAndSites(ui);

                        // Create new OpenID <-> User record
                        OpenIDUserInfo oui = new OpenIDUserInfo();
                        oui.OpenID = claimedID;
                        oui.OpenIDProviderUrl = providerUrl;
                        oui.UserID = ui.UserID;
                        OpenIDUserInfoProvider.SetOpenIDUserInfo(oui);

                        return AuthenticateMembershipUser(ui, true, siteName, disableConfirmation, ref error);
                    }
                    // User already exists in DB = authenticate user
                    else
                    {
                        // Ban IP addresses which are blocked for login
                        BannedIPInfoProvider.CheckIPandRedirect(siteName, BanControlEnum.Login);

                        return AuthenticateMembershipUser(ui, false, siteName, disableConfirmation, ref error);
                    }
                }
            }

            return null;
        }


        /// <summary>
        /// Authenticates user with LinkedID parameters. It will create new user if not found in DB.
        /// </summary>
        /// <param name="profileID">LinkedID profile identifier</param>
        /// <param name="firstName">User's first name</param>
        /// <param name="lastName">User's last name</param>
        /// <param name="siteName">Site name</param>
        /// <param name="generatePassword">Indicates if random password should be generated</param>
        /// <param name="disableConfirmation">Indicates if e-mail confirmation of newly registered user is disabled</param>
        /// <param name="error">Error message which will be filled when error occurred</param>
        /// <returns>UserInfo with authenticated user or null if user is not found</returns>
        public static UserInfo AuthenticateLinkedInUser(string profileID, string firstName, string lastName, string siteName, bool generatePassword, bool disableConfirmation, ref string error)
        {
            // Do not initialize context with current user -> this could lead to stack overflow
            using (new CMSActionContext { AllowInitUser = false })
            {
                // Check if license permits LinkedIn
                LicenseHelper.CheckFeatureAndRedirect(RequestContext.CurrentDomain, FeatureEnum.LinkedIn);

                // Check if parameters are set
                if (!String.IsNullOrEmpty(profileID))
                {
                    // Try to find profile ID in DB
                    UserInfo ui = UserInfoProvider.GetUserInfoByLinkedInID(profileID);

                    // User doesn't exist in DB = create a new one only if user with specified LinkedIn name doesnt' exist
                    if (ui == null)
                    {
                        // Check if IP address is not banned for possible registration
                        BannedIPInfoProvider.CheckIPandRedirect(siteName, BanControlEnum.Registration);

                        // Create user info
                        ui = new UserInfo();

                        // LinkedIn user will have special prefix + GUID as a user name, unless they change it later
                        string guid = Guid.NewGuid().ToString();
                        ui.UserName = UserInfoProvider.LINKEDIN_USERS_PREFIX + guid;

                        // LinkedIn full name
                        if (!String.IsNullOrEmpty(firstName) || !String.IsNullOrEmpty(lastName))
                        {
                            string fullName = String.Empty;
                            if (!String.IsNullOrEmpty(firstName))
                            {
                                fullName = firstName + " ";
                            }
                            if (!String.IsNullOrEmpty(lastName))
                            {
                                fullName += lastName;
                            }
                            ui.FullName = fullName.Trim();
                        }
                        else
                        {
                            ui.FullName = UserInfoProvider.LINKEDIN_FULLNAME_PREFIX + guid;
                        }

                        // First name
                        if (!String.IsNullOrEmpty(firstName))
                        {
                            ui.FirstName = firstName;
                        }

                        // Last name
                        if (!String.IsNullOrEmpty(lastName))
                        {
                            ui.LastName = lastName;
                        }

                        ui.IsExternal = true;

                        // Save LinkedIn profile ID
                        ui.UserSettings.UserLinkedInID = profileID;

                        UserInfoProvider.SetUserInfo(ui);

                        // Generate random password for newly created LinkedIn user
                        if (generatePassword)
                        {
                            UserInfoProvider.SetPassword(ui.UserName, UserInfoProvider.GenerateNewPassword(siteName));
                            // Update/reload password to user info class
                            ui = UserInfoProvider.GetUserInfo(ui.UserID);
                        }

                        // Add current site
                        var rolesTable = ui.SitesRoles[siteName.ToLowerInvariant()];
                        if (rolesTable == null)
                        {
                            rolesTable = ui.CreateNewRolesDictionary();
                            ui.SitesRoles[siteName.ToLowerInvariant()] = rolesTable;
                        }

                        // Assign the default roles
                        string[] roles = SettingsKeyInfoProvider.GetValue(siteName + ".CMSLinkedInRoles").Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (string role in roles)
                        {
                            string roleName = role.Trim().ToLowerInvariant();
                            if (rolesTable[roleName] == null)
                            {
                                rolesTable[roleName] = 0;
                            }
                        }

                        // Ensure the user roles
                        UserInfoProvider.EnsureRolesAndSites(ui);

                        return AuthenticateMembershipUser(ui, true, siteName, disableConfirmation, ref error);
                    }
                    // User already exists in DB = authenticate user
                    else
                    {
                        // Ban IP addresses which are blocked for login
                        BannedIPInfoProvider.CheckIPandRedirect(siteName, BanControlEnum.Login);

                        return AuthenticateMembershipUser(ui, false, siteName, disableConfirmation, ref error);
                    }
                }
            }

            return null;
        }


        /// <summary>
        /// Authenticates the Windows Live user with user's LiveID. If user doesn't exists yet and createNew set
        /// to true, then it is created.
        /// </summary>
        /// <param name="userLiveId">User LiveID</param>
        /// <param name="siteName">Site name</param>
        /// <param name="disableConfirmation">Indicates if e-mail confirmation of newly registered user is disabled</param>
        /// <param name="error">Error message which will be filled when error occurred</param>
        /// <returns>Returns UserInfo</returns>
        public static UserInfo AuthenticateWindowsLiveUser(string userLiveId, string siteName, bool disableConfirmation, ref string error)
        {
            // Do not initialize context with current user -> this could lead to stack overflow
            using (new CMSActionContext { AllowInitUser = false })
            {
                // Check if license permits Live ID
                LicenseHelper.CheckFeatureAndRedirect(RequestContext.CurrentDomain, FeatureEnum.WindowsLiveID);

                // LiveID and site name must be set
                if (String.IsNullOrEmpty(userLiveId) || String.IsNullOrEmpty(siteName))
                {
                    return null;
                }

                // Try to find user with such ID
                UserSettingsInfo userSettings = UserSettingsInfoProvider.GetUserSettings()
                    .WhereEquals("WindowsLiveID", userLiveId)
                    .TopN(1)
                    .FirstOrDefault();
                UserInfo ui;

                // User already visited the site
                if (userSettings != null)
                {
                    // Ban IP addresses which are blocked for login
                    BannedIPInfoProvider.CheckIPandRedirect(siteName, BanControlEnum.Login);

                    // Get user
                    ui = UserInfoProvider.GetUserInfo(userSettings.UserSettingsUserID);
                    ui.UserSettings = userSettings;

                    return AuthenticateMembershipUser(ui, false, siteName, disableConfirmation, ref error);
                }
                // New Windows Live user
                else
                {
                    // Check if IP address is not banned for possible registration
                    BannedIPInfoProvider.CheckIPandRedirect(siteName, BanControlEnum.Registration);

                    // Create user info
                    ui = new UserInfo();

                    // Live user will have special prefix, unless they change it later
                    ui.UserName = UserInfoProvider.LIVEID_USERS_PREFIX + userLiveId;
                    ui.FullName = ui.UserName;
                    ui.IsExternal = true;
                    ui.UserSettings.WindowsLiveID = userLiveId;

                    UserInfoProvider.SetUserInfo(ui);

                    // Add current site
                    var rolesTable = ui.SitesRoles[siteName.ToLowerInvariant()];
                    if (rolesTable == null)
                    {
                        rolesTable = ui.CreateNewRolesDictionary();
                        ui.SitesRoles[siteName.ToLowerInvariant()] = rolesTable;
                    }

                    // Assign the default roles
                    string[] roles = SettingsKeyInfoProvider.GetValue(siteName + ".CMSLiveIDRoles").Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string role in roles)
                    {
                        string roleName = role.Trim().ToLowerInvariant();
                        if (rolesTable[roleName] == null)
                        {
                            rolesTable[roleName] = 0;
                        }
                    }

                    // Ensure the user roles
                    UserInfoProvider.EnsureRolesAndSites(ui);

                    return AuthenticateMembershipUser(ui, true, siteName, disableConfirmation, ref error);
                }
            }
        }


        /// <summary>
        /// Authenticate user with provided Facebook Connect parameters. Create new user if createNew
        /// is set to TRUE and user doesn't exist in DB.
        /// </summary>
        /// <param name="facebookUserID">Facebook user ID</param>
        /// <param name="siteName">Site name</param>
        /// <param name="generatePassword">Indicates if random password should be generated</param>
        /// <param name="disableConfirmation">Indicates if e-mail confirmation of newly registered user is disabled</param>
        /// <param name="error">Error message which will be filled when error occurred</param>
        /// <returns>UserInfo with authenticated user or null if user is not found</returns>
        public static UserInfo AuthenticateFacebookConnectUser(string facebookUserID, string siteName, bool generatePassword, bool disableConfirmation, ref string error)
        {
            // Do not initialize context with current user -> this could lead to stack overflow
            using (new CMSActionContext { AllowInitUser = false })
            {
                // Check if license permits FacebookConnect
                LicenseHelper.CheckFeatureAndRedirect(RequestContext.CurrentDomain, FeatureEnum.FaceBookConnect);

                // Check if parameters are set
                if (!String.IsNullOrEmpty(facebookUserID))
                {
                    // Try to find Facebook user ID in DB
                    UserInfo ui = UserInfoProvider.GetUserInfoByFacebookConnectID(facebookUserID);

                    if (ui == null)
                    {
                        // Check if IP address is not banned for possible registration
                        BannedIPInfoProvider.CheckIPandRedirect(siteName, BanControlEnum.Registration);

                        // User doesn't exist in DB = create a new one only if user with specified FacebookID name doesn't exist
                        if (UserInfoProvider.GetUserInfo(UserInfoProvider.FACEBOOKID_USERS_PREFIX + facebookUserID) == null)
                        {
                            // Create user info
                            ui = new UserInfo();

                            // Facebook user will have special prefix, unless they change it later
                            ui.UserName = UserInfoProvider.FACEBOOKID_USERS_PREFIX + facebookUserID;
                            ui.FullName = UserInfoProvider.FACEBOOKID_FULLNAME_PREFIX + facebookUserID;

                            ui.IsExternal = true;
                            ui.UserSettings.UserFacebookID = facebookUserID;

                            UserInfoProvider.SetUserInfo(ui);

                            // Generate random password for newly created Facebook user
                            if (generatePassword)
                            {
                                UserInfoProvider.SetPassword(ui.UserName, UserInfoProvider.GenerateNewPassword(siteName));
                                // Update/reload password to user info class
                                ui = UserInfoProvider.GetUserInfo(ui.UserID);
                            }

                            // Add current site
                            var rolesTable = ui.SitesRoles[siteName.ToLowerInvariant()];
                            if (rolesTable == null)
                            {
                                rolesTable = ui.CreateNewRolesDictionary();
                                ui.SitesRoles[siteName.ToLowerInvariant()] = rolesTable;
                            }

                            // Assign the default roles
                            string[] roles = SettingsKeyInfoProvider.GetValue(siteName + ".CMSFacebookRoles").Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                            foreach (string role in roles)
                            {
                                string roleName = role.Trim().ToLowerInvariant();
                                if (rolesTable[roleName] == null)
                                {
                                    rolesTable[roleName] = 0;
                                }
                            }

                            // Ensure the user roles
                            UserInfoProvider.EnsureRolesAndSites(ui);

                            return AuthenticateMembershipUser(ui, true, siteName, disableConfirmation, ref error);
                        }
                    }
                    // User already exists in DB = authenticate user
                    else
                    {
                        // Ban IP addresses which are blocked for login
                        BannedIPInfoProvider.CheckIPandRedirect(siteName, BanControlEnum.Login);

                        return AuthenticateMembershipUser(ui, false, siteName, disableConfirmation, ref error);
                    }
                }
            }

            return null;
        }


        /// <summary>
        /// Signs out user and removes their session.
        /// </summary>
        private static void SignOutUser()
        {
#pragma warning disable BH1012 // 'FormsAuthentication.SignOut()' should not be used. Use 'AuthenticationHelper.SignOut()' instead.
            FormsAuthentication.SignOut();
#pragma warning restore BH1012 // 'FormsAuthentication.SignOut()' should not be used. Use 'AuthenticationHelper.SignOut()' instead.
            ImpersonationHelper.RemoveCookie();
            WindowsUserCookieHelper.ClearKnownWindowsUserName();
            RemoveSessionID();
        }


        /// <summary>
        /// Creates authentication cookie of user with specified custom data.
        /// </summary>
        /// <param name="username">User name</param>
        /// <param name="isPersistent">Persistency of cookie</param>
        /// <param name="timeOut">Cookie timeout</param>
        /// <param name="userData">Array of user data</param>
        public static void SetAuthCookieWithUserData(string username, bool isPersistent, int timeOut, string[] userData)
        {
            // Parameters must be valid
            if (String.IsNullOrEmpty(username) || (userData == null))
            {
                return;
            }

            // Get userData in one string concatenated with ';'
            string data = String.Empty;
            foreach (string str in userData)
            {
                data += str + ";";
            }
            data = data.TrimEnd(';');

            // Create forms ticket
            FormsAuthenticationTicket ticket = new FormsAuthenticationTicket(1, username, DateTime.Now, DateTime.Now.AddMinutes(timeOut), false, data, FormsAuthentication.FormsCookiePath);

            // Encrypt the ticket
            string encTicket = FormsAuthentication.Encrypt(ticket);

            // Create the cookie
            CookieHelper.SetValue(FormsAuthentication.FormsCookieName, encTicket, ticket.CookiePath, ticket.Expiration, true, FormsAuthentication.CookieDomain);
        }


        /// <summary>
        /// Returns an array with userdata from authentication cookie.
        /// </summary>
        public static string[] GetUserDataFromAuthCookie()
        {
            string cookieName = FormsAuthentication.FormsCookieName;

            // Get auth cookie
            string encrCookie = CookieHelper.GetValue(cookieName, useDefaultValue: true, allowSensitiveData: true);

            if (encrCookie != null)
            {
                // Decrypt cookie value and get ticket
                FormsAuthenticationTicket ticket = FormsAuthentication.Decrypt(encrCookie);

                // Ticket is ok and there are some userdata
                if (!String.IsNullOrEmpty(ticket?.UserData))
                {
                    // Return split values
                    return ticket.UserData.Split(';');
                }
            }

            return null;
        }


        /// <summary>
        /// Creates or updates user with given SID.
        /// Rises authentication event when user is ready to update/insert.
        /// </summary>
        /// <param name="userName">User to authenticate</param>
        /// <param name="sid">User's Active Directory SID</param>
        /// <param name="siteName">Site name for authentication</param>
        /// <param name="roles">User roles</param>
        private static UserInfo EnsureADUser(string userName, SecurityIdentifier sid, string siteName, string[] roles)
        {
            // Get desired username
            userName = EnsureSafeUserName(userName, siteName);

            // Try to get current user record
            var externalUser = UserInfoProvider.GetUserInfo(userName);
            bool isNew = externalUser == null;

            // Authentication is valid for domain users only
            if (!isNew && !externalUser.UserIsDomain)
            {
                return null;
            }

            // Do the import within locked section
            using (new LockedSection(isNew, userName))
            {
                // Ensure thread safety by double checking new user
                if (isNew)
                {
                    externalUser = UserInfoProvider.GetUserInfo(userName);
                }

                // Create new external user
                if (externalUser == null)
                {
                    // Prepare external user record
                    externalUser = new UserInfo
                    {
                        UserName = userName,
                        FullName = userName,
                        IsExternal = true,
                        UserIsDomain = true,
                        Enabled = true,
                    };

                    externalUser.SetValue("UserCreated", DateTime.Now);

                    // Synchronize AD GUID
                    if (!FederationAuthentication && SynchronizeUserGUIDs && (sid != null))
                    {
                        Guid userGuid = GetUserADGuid(userName, sid);

                        // Ensure GUID does not belong to any existing user
                        if ((userGuid != Guid.Empty) && (UserInfoProvider.GetUserInfoByGUID(userGuid) == null))
                        {
                            externalUser.UserGUID = userGuid;
                        }
                    }
                }

                // Add current site and site roles
                if (!String.IsNullOrEmpty(siteName))
                {
                    // Add current site
                    var rolesTable = externalUser.SitesRoles[siteName.ToLowerInvariant()];
                    if (rolesTable == null)
                    {
                        rolesTable = externalUser.CreateNewRolesDictionary();
                        externalUser.SitesRoles[siteName.ToLowerInvariant()] = rolesTable;
                    }

                    // Add site roles
                    if (roles != null)
                    {
                        foreach (string role in roles)
                        {
                            var roleKey = UserInfoProvider.UseSafeRoleName ? ValidationHelper.GetSafeRoleName(role, siteName).ToLowerInvariant() : role.ToLowerInvariant();
                            if (rolesTable[roleKey] == null)
                            {
                                rolesTable[roleKey] = 0;
                            }
                        }
                    }
                }

                // Ensure empty container for global roles
                externalUser.SitesRoles[UserInfo.GLOBAL_ROLES_KEY] = externalUser.SitesRoles[UserInfo.GLOBAL_ROLES_KEY] ?? externalUser.CreateNewRolesDictionary();

                // Initiate the authentication event
                if (SecurityEvents.Authenticate.IsBound)
                {
                    SecurityEvents.Authenticate.StartEvent(ref externalUser, userName, null);
                }

                // Check whether user is kicked or disabled
                if ((externalUser != null) && (!CanUserLogin(externalUser.UserID) || !externalUser.Enabled))
                {
                    externalUser = null;
                }

                // Ensure the user is valid
                if ((externalUser != null) && (externalUser.IsExternal))
                {
                    // Ensure the user
                    EnsureExternalUser(externalUser);

                    // Event that's triggered when User Authentication is complete is needed, we'll bind to that in Online Marketing
                    ModuleCommands.OnlineMarketingLogLogin(externalUser);
                }
            }

            return externalUser;
        }


        private static string EnsureSafeUserName(string userName, string siteName)
        {
            return UserInfoProvider.UseSafeUserName ? ValidationHelper.GetSafeUserName(userName, siteName) : userName;
        }


        /// <summary>
        /// Authenticates user. If new user is being created then analytics logs are recorded.
        /// </summary>
        /// <param name="ui">UserInfo of newly created or already existing user</param>
        /// <param name="isNew">Indicates if authenticated user is newly created</param>
        /// <param name="siteName">Site name</param>
        /// <param name="disableConfirmation">Indicates if email confirmation of newly registered user is disabled</param>
        /// <param name="error">Error which occurred during authentication process</param>
        /// <returns>Returns authenticated user</returns>
        private static UserInfo AuthenticateMembershipUser(UserInfo ui, bool isNew, string siteName, bool disableConfirmation, ref string error)
        {
            if (ui != null)
            {
                // Authenticate existing user
                if (!isNew)
                {
                    // Do site check
                    ui = UserInfoProvider.CheckUserBelongsToSite(ui, siteName);

                    // Check whether user is kicked or disabled
                    if ((ui != null) && (!CanUserLogin(ui.UserID) || !ui.Enabled))
                    {
                        ui = null;
                        error = ResHelper.GetString("membership.userdisabled");
                    }
                }
                // Authenticate new user
                else
                {
                    bool requiresConfirmation = SettingsKeyInfoProvider.GetBoolValue(siteName + ".CMSRegistrationEmailConfirmation");
                    requiresConfirmation &= !disableConfirmation;
                    bool requiresAdminApprove = SettingsKeyInfoProvider.GetBoolValue(siteName + ".CMSRegistrationAdministratorApproval");

                    // Check if user is enabled after creation
                    if (requiresConfirmation || requiresAdminApprove)
                    {
                        ui.Enabled = false;

                        // User is waiting for admin's approval
                        if (requiresAdminApprove && !requiresConfirmation)
                        {
                            ui.UserSettings.UserWaitingForApproval = true;
                            if (String.IsNullOrEmpty(error))
                            {
                                error = ResHelper.GetString("membership.usercreateddisabled");
                            }
                        }
                        // User is waiting for e-mail confirmation
                        else if (String.IsNullOrEmpty(error))
                        {
                            error = ResHelper.GetString("membership.requiresconfirmation");
                        }
                    }
                    else
                    {
                        ui.Enabled = true;
                    }

                    UserInfoProvider.SetUserInfo(ui);


                    // Check license limitation
                    UserInfoProvider.CheckLicenseLimitation(ui, ref error);

                    // Update registration information
                    ui.UserSettings.UserRegistrationInfo.IPAddress = RequestContext.UserHostAddress;
                    ui.UserSettings.UserRegistrationInfo.Agent = HttpContext.Current.Request.UserAgent;
                }
            }

            // Update the user record
            if (ui != null)
            {
                UpdateLastLogonInformation(ui);

                using (CMSActionContext context = new CMSActionContext())
                {
                    // Disable logging of tasks
                    context.DisableLogging();
                    UserInfoProvider.SetUserInfo(ui);
                }

                if (ui.Enabled)
                {
                    UserInfoProvider.SetPreferredCultures(ui);
                }
            }

            return ui;
        }

        #endregion


        #region "Authentication helper methods"

        /// <summary>
        /// Get domain name in NetBiosDomainName\SamAccountName
        /// </summary>
        /// <param name="principal">Principal object (User or Group)</param>
        /// <returns>Domain name in format </returns>
        public static string GetDomainSamAccountName(Principal principal)
        {
            string principalNamePart = principal.SamAccountName;
            string domainPart = NetBiosDomainName;
            string distinguishedName = principal.DistinguishedName;

            if (principal.StructuralObjectClass == GROUP)
            {
                // Try to parse domain from distinguished name if NetBIOSName not found
                if (string.IsNullOrEmpty(domainPart) && (distinguishedName != null))
                {
                    const string DC = "DC=";
                    string DCName = null;

                    // Parse distinguished name
                    foreach (string name in distinguishedName.Split(','))
                    {
                        if (name.StartsWith(DC, StringComparison.OrdinalIgnoreCase) && DCName == null)
                        {
                            DCName = name.Split('=')[1];
                        }
                    }

                    domainPart = (DCName);
                }
            }
            else if (principal.StructuralObjectClass == USER)
            {
                // Try to parse domain from UPN if NetBIOSName not found
                if (string.IsNullOrEmpty(domainPart) && (principal.UserPrincipalName != null))
                {
                    string userPrincipalName = principal.UserPrincipalName;
                    // Split username and domain part
                    int indexOfDomainSeparator = userPrincipalName.IndexOf('@');
                    // If '@' is present and is not last char
                    if ((indexOfDomainSeparator != -1) && (userPrincipalName.Length != (indexOfDomainSeparator + 1)))
                    {
                        // Get domain name
                        string domainName = userPrincipalName.Substring(indexOfDomainSeparator + 1);
                        int indexOfDot = domainName.IndexOf('.');
                        if (indexOfDot == -1)
                        {
                            domainPart = domainName;
                        }
                        else
                        {
                            domainName = domainName.Substring(0, indexOfDot);
                            domainPart = domainName;
                        }
                    }
                }
            }
            return domainPart + "\\" + principalNamePart;
        }


        /// <summary>
        /// Checks whether request is being redirected to a logon page.
        /// </summary>
        /// <remarks>
        /// Logon page can be defined in <see cref="GetSecuredAreasLogonPage"/>, <see cref="FormsAuthentication.LoginUrl"/>  or <see cref="DEFAULT_LOGON_PAGE"/>.
        /// </remarks>
        /// <seealso cref="SecurityEvents.AuthenticationRequested"/>
        public static bool IsAuthenticationRedirect()
        {
            if (HttpContext.Current.Response.StatusCode != 302)
            {
                return false;
            }

            var redirectPath = URLHelper.GetAbsoluteUrl(HttpContext.Current.Response.RedirectLocation);
            var formLogonUrl = ValidationHelper.GetString(GetSecuredAreasLogonPage(SiteContext.CurrentSiteName), FormsAuthentication.LoginUrl);
            var array = new[]
            {
                URLHelper.GetAbsoluteUrl(formLogonUrl),
                URLHelper.GetAbsoluteUrl(DEFAULT_LOGON_PAGE)
            };
            return URLHelper.CheckPrefixes(ref redirectPath, array, false);
        }


        /// <summary>
        /// Ensures that the external user record is present in the database.
        /// </summary>
        /// <param name="uInfo">User info object with the user data</param>
        public static void EnsureExternalUser(UserInfo uInfo)
        {
            if (uInfo.IsExternal && ImportExternalUsers && !RequestHelper.IsWebDAVRequest())
            {
                // Set properties for existing user
                UserInfo currentUser = UserInfoProvider.GetUserInfo(uInfo.UserName);
                if (currentUser != null)
                {
                    uInfo.UserID = currentUser.UserID;
                    uInfo.UserSettings.UserSettingsID = currentUser.UserSettings.UserSettingsID;
                    uInfo.UserSettings.UserSettingsUserGUID = currentUser.UserGUID;
                    uInfo.UserSettings.UserSettingsUserID = currentUser.UserID;
                    uInfo.UserGUID = currentUser.UserGUID;
                }

                if ((uInfo.UserID == 0) || UpdateLastLogonForExternalUsers)
                {
                    // Disable logging and additional actions
                    using (new CMSActionContext
                    {
                        LogSynchronization = false,
                        LogExport = false,
                        LogEvents = false,
                        CreateVersion = false
                    })
                    {
                        UpdateLastLogonInformation(uInfo);

                        // Temporary info must be used because of object invalidation which is removing site roles.
                        var temporaryUserInfo = new UserInfo(uInfo, true);

                        // Update / insert the record
                        UserInfoProvider.SetUserInfo(temporaryUserInfo);
                    }
                }

                // Update roles and sites
                if (ImportExternalRoles)
                {
                    if (RequestHelper.IsWindowsAuthentication())
                    {
                        UserInfoProvider.EnsureRolesAndSitesForWindowsAuthentication(uInfo);
                    }
                    else
                    {
                        UserInfoProvider.EnsureRolesAndSites(uInfo);
                    }
                }
            }
        }


        /// <summary>
        /// Checks if given user can login to system.
        /// </summary>
        /// <param name="userID">User id</param>
        public static bool CanUserLogin(int userID)
        {
            // If user is in kicked
            if (KickUsers.Contains(userID))
            {
                // Can he already login?
                if (DateTime.Now > ValidationHelper.GetDateTime(KickUsers[userID], DateTimeHelper.ZERO_TIME))
                {
                    KickUsers.Remove(userID);
                    return true;
                }

                return false;
            }

            return true;
        }


        /// <summary>
        /// Gets the user roles.
        /// </summary>
        /// <param name="userName">User name</param>
        public static string[] GetUserWindowsRoles(string userName)
        {
            // Get roles using standard Windows authentication model
            var tokenRoleProvider = Service.Resolve<IWindowsTokenRoleService>();
            return tokenRoleProvider.GetRolesForUser(userName);
        }


        /// <summary>
        /// Gets the user roles from AD.
        /// </summary>
        /// <param name="userName">User name</param>
        /// <param name="roleProvider">Instance of <see cref="System.Web.Security.RoleProvider" /> to use. If not specified the role provider specified in .config file is used.</param>
        public static string[] GetUserADRoles(string userName, RoleProvider roleProvider = null)
        {
            roleProvider = roleProvider ?? Roles.Providers["CMSADRoleProvider"];
            if (roleProvider != null)
            {
                // Get roles using AD role provider
                return roleProvider.GetRolesForUser(userName);
            }
            else
            {
                throw new Exception("[UserInfoProvider.GetUserADRoles]: Role provider 'CMSADRoleProvider' is not configured.");
            }
        }


        /// <summary>
        /// Removes cookie with session ID which enforces ASP.NET to generate new session ID if RenewSessionAuthChange property is set to true.
        /// </summary>
        public static void RemoveSessionID()
        {
            if (RenewSessionAuthChange)
            {
                SessionHelper.Abandon();
                CookieHelper.SetValue(CookieName.ASPNETSessionID, "", DateTimeHelper.ZERO_TIME);
            }
        }


        /// <summary>
        /// Updates last logon information of user = last logon time, IP address and agent.
        /// Doesn't call SetUserInfo!!
        /// </summary>
        /// <param name="userInfo">User info</param>
        public static void UpdateLastLogonInformation(UserInfo userInfo)
        {
            // Update last logon property
            userInfo.LastLogon = DateTime.Now;

            // Set last logon additional information
            if (CMSHttpContext.Current != null)
            {
                userInfo.UserLastLogonInfo.IPAddress = RequestContext.UserHostAddress;
                userInfo.UserLastLogonInfo.Agent = CMSHttpContext.Current.Request.ServerVariables["HTTP_USER_AGENT"];
            }
        }


        /// <summary>
        /// Tries to retrieve GUID from Active Directory.
        /// </summary>
        /// <param name="userName">SAM account name</param>
        /// <param name="sid">Security identifier</param>
        /// <returns>GUID of </returns>
        private static Guid GetUserADGuid(string userName, SecurityIdentifier sid)
        {
            Guid userGuid = Guid.Empty;

            try
            {
                DirectoryEntry searchRoot;

                if (RequestHelper.IsMixedAuthentication())
                {
                    // Get connection string
                    ConnectionStringSettings connSettings = SettingsHelper.ConnectionStrings[ADConnectionStringName];
                    string connStr = connSettings?.ConnectionString;

                    // Create search root
                    searchRoot = new DirectoryEntry(connStr, ADUsername, ADPassword);
                }
                else
                {
                    searchRoot = new DirectoryEntry();
                }

                DirectorySearcher ds = new DirectorySearcher(searchRoot);

                // Initialize filter
                ds.Filter = "(objectSid=" + sid.Value + ")";

                userGuid = SearchADForGuid(ds);

                // If not found by SID
                if (userGuid == Guid.Empty)
                {
                    ds.Filter = "(sAMAccountName=" + userName + ")";
                    userGuid = SearchADForGuid(ds);
                }

            }
            catch (Exception ex)
            {
                EventLogProvider.LogException("Membership", "RETRIEVEGUIDFAILED", ex);
            }

            return userGuid;
        }


        /// <summary>
        /// Retrieves GUID of a user specified by filter of given directory searcher.
        /// </summary>
        /// <param name="ds">Initialized directory searcher</param>
        /// <returns>GUID of AD user or Guid.Empty</returns>
        private static Guid SearchADForGuid(DirectorySearcher ds)
        {
            Guid userGuid = Guid.Empty;

            // Try to find
            SearchResult singleResult = ds.FindOne();
            if (singleResult != null)
            {
                DirectoryEntry user = singleResult.GetDirectoryEntry();
                userGuid = user.Guid;
            }
            return userGuid;
        }


        /// <summary>
        /// Find users specified by Email
        /// </summary>
        /// <param name="userEmail">Email assigned to the user.</param>
        private static IEnumerable<UserInfo> FindUsersByEmail(string userEmail)
        {
            return UserInfoProvider.GetUsers()
                .WhereEquals("Email", userEmail)
                .ToList();
        }

        #endregion


        #region "Password settings methods"

        /// <summary>
        /// Returns URL of page where user can reset his password.
        /// </summary>
        /// <param name="siteName">Site name.</param>
        public static string GetResetPasswordUrl(string siteName)
        {
            return URLHelper.GetAbsoluteUrl(DataHelper.GetNotEmpty(SettingsKeyInfoProvider.GetValue(siteName + ".CMSResetPasswordURL"), "~/CMSModules/Membership/CMSPages/ResetPassword.aspx"));
        }


        /// <summary>
        /// Returns URL of page where user can unlock her account.
        /// </summary>
        /// <param name="siteName">Site name.</param>
        public static string GetUnlockAccountUrl(string siteName)
        {
            string unlockUrl = SettingsKeyInfoProvider.GetURLValue(siteName + ".CMSUserAccountUnlockPath", "~/CMSModules/Membership/CMSPages/UnlockUserAccount.aspx");

            // Sets absolute unsubscribe link
            return URLHelper.GetAbsoluteUrl(unlockUrl);
        }


        /// <summary>
        /// Gets limit of invalid password attempts before user account is locked
        /// </summary>
        /// <param name="siteName">Site name for which the check is made</param>
        public static int MaximumInvalidPasswordAttemps(string siteName)
        {
            return SettingsKeyInfoProvider.GetIntValue(siteName + ".CMSMaximumInvalidLogonAttempts");
        }


        /// <summary>
        /// Indicates if additional info about account lock should be displayed
        /// </summary>
        /// <param name="siteName">Site name for which the check is made</param>
        public static bool DisplayAccountLockInformation(string siteName)
        {
            return SettingsKeyInfoProvider.GetBoolValue(siteName + ".CMSDisplayAccountLockInformation");
        }


        /// <summary>
        /// Indicates if password expiration is enabled
        /// </summary>
        /// <param name="siteName">Site to check</param>
        /// <param name="expDays">Number of days before password expiration</param>
        /// <returns>True if password expiration is enabled, False otherwise</returns>
        public static bool IsPasswordExpirationEnabled(string siteName, out int expDays)
        {
            bool pwdExpiration = SettingsKeyInfoProvider.GetBoolValue(siteName + ".CMSPasswordExpiration");
            expDays = SettingsKeyInfoProvider.GetIntValue(siteName + ".CMSPasswordExpirationPeriod");

            return (pwdExpiration && (expDays > 0));
        }

        #endregion


        #region "Password methods"

        /// <summary>
        /// Returns a message that the entered password doesn't meet the defined password policy.
        /// </summary>
        /// <param name="siteName">Code name of the site that has the password policy defined.</param>
        public static string GetPolicyViolationMessage(string siteName)
        {
            // Returns custom message
            string customMessage = SettingsKeyInfoProvider.GetValue(siteName + ".CMSPolicyViolationMessage");
            if (!String.IsNullOrEmpty(customMessage))
            {
                return ResHelper.LocalizeString(customMessage);
            }

            // Get password requirements
            int length = SettingsKeyInfoProvider.GetIntValue(siteName + ".CMSPolicyMinimalLength");
            int numberOfNonAlphaNum = SettingsKeyInfoProvider.GetIntValue(siteName + ".CMSPolicyNumberOfNonAlphaNumChars");

            string message;

            // Both minimum length and number of special characters are set
            if ((length > 0) && (numberOfNonAlphaNum > 0))
            {
                message = String.Format(ResHelper.GetString("passwordpolicy.notaccetable"), length, numberOfNonAlphaNum);
            }
            // Only minimum length is set
            else if (length > 0)
            {
                message = String.Format(ResHelper.GetString("passwordpolicy.notacceptablelength"), length);
            }
            // Only number of special characters is set
            else if (numberOfNonAlphaNum > 0)
            {
                message = String.Format(ResHelper.GetString("passwordpolicy.notacceptablenonalphanum"), numberOfNonAlphaNum);
            }
            else
            {
                message = ResHelper.GetString("passwordpolicy.policynotmet");
            }

            return message;
        }


        /// <summary>
        /// Check if user password is expired and optionally according to settings lock expired password account.
        /// </summary>
        /// <param name="ui">User account to check</param>
        /// <param name="siteName">Site name</param>
        /// <param name="lockAccount">Indicates if account should be locked if maximum invalid logon attempts was reached</param>
        /// <returns>Returns true if user password expired, false otherwise</returns>
        /// <param name="returnUrl">URL using which user can log on to system, after password change</param>
        public static bool CheckPasswordExpiration(UserInfo ui, string siteName, bool lockAccount = false, string returnUrl = null)
        {
            bool passExpired = false;

            // Check if user exists
            if ((ui != null) && (ui.UserPasswordLastChanged > DateTime.MinValue))
            {
                // Use simple checking for disabled user
                if (ui.Enabled)
                {
                    int expDays;

                    if (IsPasswordExpirationEnabled(siteName, out expDays))
                    {
                        // Check if password expired
                        if ((DateTime.Now - ui.UserPasswordLastChanged).Days >= expDays)
                        {
                            if (lockAccount && SettingsKeyInfoProvider.GetValue(siteName + ".CMSPasswordExpirationBehaviour") == PASSWORD_EXPIRATION_LOCK)
                            {
                                ui.Enabled = false;
                                ui.UserAccountLockReason = UserAccountLockCode.FromEnum(UserAccountLockEnum.PasswordExpired);
                                UserInfoProvider.SetUserInfo(ui);

                                if (SettingsKeyInfoProvider.GetBoolValue(siteName + ".CMSPasswordExpirationEmail"))
                                {
                                    SendPasswordRequest(ui, siteName, "Password expiration check", SettingsKeyInfoProvider.GetValue(siteName + ".CMSSendPasswordEmailsFrom"), "Membership.PasswordExpired", null, GetResetPasswordUrl(siteName));
                                }
                            }

                            passExpired = true;
                        }
                    }
                }
                else if (ui.UserAccountLockReason == UserAccountLockCode.FromEnum(UserAccountLockEnum.PasswordExpired))
                {
                    passExpired = true;
                }
            }

            return passExpired;
        }


        /// <summary>
        /// Check if logging invalid password attempts is enabled and if so, invalid attempt counter is incremented and
        /// if user exceeded maximal number of invalid attempts, her account is locked
        /// </summary>
        /// <param name="user">User to log invalid password attempt</param>
        /// <param name="siteName">Site name to check invalid password attempts functionality</param>
        /// <param name="returnUrl">URL using which user can log on to system, after password change</param>
        public static void CheckInvalidPasswordAttempts(UserInfo user, string siteName, string returnUrl = null)
        {
            if ((user != null) && !String.IsNullOrEmpty(siteName))
            {
                int maxAttempts = MaximumInvalidPasswordAttemps(siteName);
                if (maxAttempts > 0)
                {
                    user.UserInvalidLogOnAttempts++;
                    if (user.UserInvalidLogOnAttempts >= maxAttempts)
                    {
                        user.Enabled = false;
                        user.UserAccountLockReason = UserAccountLockCode.FromEnum(UserAccountLockEnum.MaximumInvalidLogonAttemptsReached);
                        if (SettingsKeyInfoProvider.GetBoolValue(siteName + ".CMSSendAccountUnlockEmail"))
                        {
                            SendUnlockAccountRequest(user, siteName, "USERAUTHENTICATION", SettingsKeyInfoProvider.GetValue(siteName + ".CMSSendPasswordEmailsFrom"), null, returnUrl);
                        }

                        // Log event
                        EventLogProvider.LogEvent(EventType.WARNING, "Membership", "USERACCOUNTLOCKED");

                        MembershipContext.UserAccountLockedDueToInvalidLogonAttempts = true;
                    }
                    UserInfoProvider.SetUserInfo(user);
                }
            }
        }


        /// <summary>
        /// Sends e-mail to user to inform her about her account was locked due to reaching maximum invalid logon attempts.
        /// </summary>
        /// <param name="user">User</param>
        /// <param name="siteName">Site name</param>
        /// <param name="source">Source of calling, will be used for log event</param>
        /// <param name="sendEmailFrom">Email address of the sender</param>
        /// <param name="resolver">Macro resolver</param>
        /// <param name="returnUrl">URL using which user can log on to system, after password change</param>
        public static string SendUnlockAccountRequest(UserInfo user, string siteName, string source, string sendEmailFrom, MacroResolver resolver, string returnUrl)
        {
            string failed = ResHelper.GetString("membership.unlockaccountfailed");

            // Check if from e-mail address is set
            if (String.IsNullOrEmpty(sendEmailFrom))
            {
                EventLogProvider.LogEvent(EventType.ERROR, source, "UnlockAccountRequest", "Missing the 'From' e-mail address for sent e-mails.");

                return ResHelper.GetAPIString("membership.unlockaccountfailed", "Failed to send the unlock account e-mail");
            }

            // Get password email template
            EmailTemplateInfo emailTemplate = EmailTemplateProvider.GetEmailTemplate("Membership.UserAccountLock", siteName);
            if (emailTemplate != null)
            {
                // Prepare e-mail
                EmailMessage message = new EmailMessage
                {
                    EmailFormat = EmailFormatEnum.Default,
                    From = sendEmailFrom,
                    Subject = emailTemplate.TemplateSubject,
                    Recipients = user.Email,
                    Body = emailTemplate.TemplateText
                };

                // Generate request GUID
                DateTime time = DateTime.Now;

                // Set to user
                user.UserPasswordRequestHash = SecurityHelper.GenerateConfirmationEmailHash(user.UserGUID.ToString(), time);
                if (String.IsNullOrEmpty(user.UserInvalidLogOnAttemptsHash))
                {
                    user.UserInvalidLogOnAttemptsHash = SecurityHelper.GenerateConfirmationEmailHash(user.UserGUID.ToString(), time);
                }
                UserInfoProvider.SetUserInfo(user);

                // Reset URL
                string passwordResetUrl = GetResetPasswordUrl(siteName);
                passwordResetUrl = URLHelper.AddParameterToUrl(passwordResetUrl, "hash", user.UserPasswordRequestHash);
                passwordResetUrl = URLHelper.AddParameterToUrl(passwordResetUrl, "datetime", DateTimeUrlFormatter.Format(time));

                if (!String.IsNullOrEmpty(returnUrl))
                {
                    // Add return URL
                    passwordResetUrl = URLHelper.AddParameterToUrl(passwordResetUrl, "returnurl", HttpUtility.UrlEncode(returnUrl));
                }

                // Unlock URL
                string unlockAccountUrl = GetUnlockAccountUrl(siteName);
                unlockAccountUrl = URLHelper.AddParameterToUrl(unlockAccountUrl, "unlockaccounthash", user.UserInvalidLogOnAttemptsHash);

                if (!String.IsNullOrEmpty(returnUrl))
                {
                    // Add return URL
                    unlockAccountUrl = URLHelper.AddParameterToUrl(unlockAccountUrl, "returnurl", HttpUtility.UrlEncode(returnUrl));
                }

                resolver = MembershipResolvers.GetMembershipUnlockAccountResolver(user, passwordResetUrl, unlockAccountUrl, resolver);

                try
                {
                    // Attach template metafiles to e-mail
                    EmailHelper.ResolveMetaFileImages(message, emailTemplate.TemplateID, EmailTemplateInfo.OBJECT_TYPE, ObjectAttachmentsCategories.TEMPLATE);

                    // Send email
                    EmailSender.SendEmailWithTemplateText(siteName, message, emailTemplate, resolver, true);
                }
                catch (Exception ex)
                {
                    // Log error
                    EventLogProvider.LogException("Unlock account request", source, ex);

                    return failed;
                }

                return ResHelper.GetString("membership.unlockaccountsent");
            }
            else
            {
                // Log missing template
                EventLogProvider.LogEvent(EventType.ERROR, source, "UnlockAccountRequest", "Email template 'Membership.UserAccountLock' wasn't found.");

                return failed;
            }
        }


        /// <summary>
        /// Unlock specified user account
        /// </summary>
        /// <param name="user">User account to be unlocked</param>
        public static void UnlockUserAccount(UserInfo user)
        {
            if (user != null)
            {
                user.UserInvalidLogOnAttempts = 0;
                user.Enabled = true;
                user.UserInvalidLogOnAttemptsHash = null;
                user.UserPasswordRequestHash = null;
                user.UserAccountLockReason = 0;

                UserInfoProvider.SetUserInfo(user);

                EventLogProvider.LogEvent(EventType.INFORMATION, "Membership", "USERACCOUNTUNLOCKED");
            }
        }


        /// <summary>
        /// Handles admin emergency reset
        /// </summary>
        public static void HandleAdminEmergencyReset()
        {
            string adminReset = ValidationHelper.GetString(SettingsHelper.AppSettings["CMSAdminEmergencyReset"], null);
            if (!String.IsNullOrEmpty(adminReset))
            {
                string[] resetParams = adminReset.Split(';');
                if ((resetParams.Length >= 1) && (resetParams.Length <= 3))
                {
                    // Check if create user if she doesn't exist
                    bool forceCreate = (resetParams.Length == 3) && ValidationHelper.GetBoolean(resetParams[2], false);
                    string userName = resetParams[0];
                    UserInfo ui = UserInfoProvider.GetUserInfo(userName);

                    // Create new user
                    if ((ui == null) && forceCreate)
                    {
                        if (UserInfoProvider.LicenseVersionCheck(RequestContext.CurrentDomain, FeatureEnum.Administrators, ObjectActionEnum.Insert, false))
                        {
                            if (ValidationHelper.IsUserName(userName))
                            {
                                ui = new UserInfo();
                                ui.UserName = resetParams[0];
                                ui.SiteIndependentPrivilegeLevel = UserPrivilegeLevelEnum.GlobalAdmin;
                                ui.Enabled = true;

                                string error = null;
                                UserInfoProvider.CheckLicenseLimitation(ui, ref error);
                                if (!String.IsNullOrEmpty(error))
                                {
                                    throw new Exception(error);
                                }
                            }
                            else
                            {
                                throw new Exception("Specified username for newly created user is not valid.");
                            }
                        }
                    }

                    // Unlock account and set new specified password
                    if (ui != null)
                    {
                        UserInfoProvider.SetPassword(ui, (resetParams.Length > 1) ? resetParams[1] : "", false);
                        UnlockUserAccount(ui);
                    }

                    // Remove key from web.config
                    SettingsHelper.RemoveConfigValue("CMSAdminEmergencyReset");
                    URLHelper.Redirect(RequestContext.CurrentURL);
                }
            }
        }


        /// <summary>
        /// Tries to find user based on email address and if it finds the user, a password reset link is sent.
        /// </summary>
        /// <param name="email">Email address of user requesting a new password.</param>
        /// <param name="siteName">Site name used to process e-mail templates.</param>
        /// <param name="source">Source of the request, used for logging.</param>
        /// <param name="sendEmailFrom">Email address of the sender.</param>
        /// <param name="resolver">Macro resolver used to resolve values in e-mail template.</param>
        /// <param name="requestUrl">URL which will be send to user to finish the password reset process.</param>
        /// <param name="returnUrl">URL to which the user will be redirected after finishing the password reset process.</param>
        public static void ForgottenEmailRequest(string email, string siteName, string source, string sendEmailFrom, MacroResolver resolver = null, string requestUrl = null, string returnUrl = null)
        {
            // Trim user identification
            email = email.Trim();

            // Check banned IP
            if (!BannedIPInfoProvider.IsAllowed(siteName, BanControlEnum.AllNonComplete))
            {
                EventLogProvider.LogEvent(EventType.WARNING, source, "PASSWORDRETRIEVAL", $"Password reset request for '{email}'.\nResult: {ResHelper.GetString("General.BannedIP")}", source);
                return;
            }

            // Check if from e-mail address is set
            if (String.IsNullOrEmpty(sendEmailFrom))
            {
                EventLogProvider.LogEvent(EventType.ERROR, source, "PASSWORDRETRIEVAL", "Missing the 'From' e-mail address for sent e-mails.");
                return;
            }

            var users = FindUsersByEmail(email);

            // Don't consider users prefixed by other sites
            if (UserInfoProvider.UserNameSitePrefixEnabled(siteName))
            {
                var sitePrefix = UserInfoProvider.GetUserNameSitePrefix(SiteInfoProvider.GetSiteInfo(siteName));
                if (!String.IsNullOrEmpty(sitePrefix))
                {
                    users = users.Where(x => !UserInfoProvider.IsSitePrefixedUser(x.UserName) || x.UserName.StartsWith(sitePrefix, StringComparison.InvariantCultureIgnoreCase));
                }
            }

            // Don't consider site bindings in case accounts are shared across all sites
            bool isSharedAccountsEnabled = SettingsKeyInfoProvider.GetBoolValue("CMSSiteSharedAccounts");
            UserInfo user = isSharedAccountsEnabled ? users.FirstOrDefault() : users.FirstOrDefault(item => item.IsInSite(siteName));

            // Wrong email, user doesn't belong to site, or user is disabled
            if ((user == null) || !user.Enabled)
            {
                EventLogProvider.LogEvent(EventType.INFORMATION, source, "PASSWORDRETRIEVAL", $"Password reset request for '{email}'.\nResult: {ResHelper.GetAPIString("LogonForm.NoUser", "No user found.")}", source);
                return;
            }

            // Send a password reset link
            SendPasswordRequest(user, siteName, source, sendEmailFrom, "Membership.ChangePasswordRequest", resolver, requestUrl, returnUrl);
        }


        /// <summary>
        /// Sends e-mail to user for approval request of new password.
        /// </summary>
        /// <param name="user">User who should receive the password request.</param>
        /// <param name="siteName">Site name used to process e-mail templates.</param>
        /// <param name="source">Source of the request, used for logging.</param>
        /// <param name="sendEmailFrom">Email address of the sender.</param>
        /// <param name="emailTemplateName">Email template name to be used for the email.</param>
        /// <param name="resolver">Macro resolver used to resolve values in e-mail template.</param>
        /// <param name="requestUrl">URL which will be send to user to finish the password reset process.</param>
        /// <param name="returnUrl">URL to which the user will be redirected after finishing the password reset process.</param>
        /// <returns>Message with result description.</returns>
        public static string SendPasswordRequest(UserInfo user, string siteName, string source, string sendEmailFrom, string emailTemplateName, MacroResolver resolver, string requestUrl, string returnUrl = null)
        {
            string failed = ResHelper.GetString("membership.passwreqfailed");

            // Get password email template
            EmailTemplateInfo emailTemplate = EmailTemplateProvider.GetEmailTemplate(emailTemplateName, siteName);
            if (emailTemplate == null)
            {
                EventLogProvider.LogEvent(EventType.ERROR, source, "PasswordRetrievalRequest", $"Email template '{emailTemplateName}' wasn't found.");
                return failed;
            }

            // Prepare e-mail
            EmailMessage message = new EmailMessage
            {
                EmailFormat = EmailFormatEnum.Default,
                From = sendEmailFrom,
                Recipients = user.Email,
                Body = emailTemplate.TemplateText
            };

            // Generate request guid
            DateTime time = DateTime.Now;

            // Set to user
            user.UserPasswordRequestHash = SecurityHelper.GenerateConfirmationEmailHash(user.UserGUID.ToString(), time);
            UserInfoProvider.SetUserInfo(user);

            requestUrl = URLHelper.AddParameterToUrl(requestUrl, "hash", user.UserPasswordRequestHash);
            requestUrl = URLHelper.AddParameterToUrl(requestUrl, "datetime", DateTimeUrlFormatter.Format(time));

            // Adding return URL
            if (!String.IsNullOrEmpty(returnUrl))
            {
                requestUrl = URLHelper.AddParameterToUrl(requestUrl, "returnurl", HttpUtility.UrlEncode(returnUrl));
            }

            resolver = MembershipResolvers.GetMembershipChangePasswordResolver(user, requestUrl, URLHelper.AddParameterToUrl(requestUrl, "cancel", "1"), resolver, RequestContext.UserHostAddress);

            try
            {
                // Attach template metafiles to e-mail
                EmailHelper.ResolveMetaFileImages(message, emailTemplate.TemplateID, EmailTemplateInfo.OBJECT_TYPE, ObjectAttachmentsCategories.TEMPLATE);

                // Send email
                EmailSender.SendEmailWithTemplateText(siteName, message, emailTemplate, resolver, true);
            }
            catch (Exception ex)
            {
                // Log error
                EventLogProvider.LogException(source, "PasswordRetrievalRequest", ex);

                return failed;
            }

            return String.Format(ResHelper.GetString("membership.passwreqsent"), HTMLHelper.HTMLEncode(user.Email));
        }


        /// <summary>
        /// Resets password for user based on his request.
        /// </summary>
        /// <param name="hash">Request hash (identifier).</param>
        /// <param name="requestTime">Request time.</param>
        /// <param name="userID">User id.</param>
        /// <param name="interval">Interval in which user can reset her password.</param>
        /// <param name="newPassword">New password.</param>
        /// <param name="siteName">Site name.</param>
        /// <param name="sendEmailFrom">Email address of the sender.</param>
        /// <param name="resolver">Macro resolver used to resolve values in confirmation e-mail template.</param>
        /// <param name="source">Source of the request, used for logging.</param>
        /// <param name="success">Returns whether sending of request was successful.</param>
        /// <param name="exceededTimeText">Text returned by this method if time request is exceeded.</param>
        /// <param name="invalidRequestText">Text returned by this method if request is invalid.</param>
        /// <returns>Message with result description.</returns>
        /// <remarks>In case the CMSSendPasswordResetConfirmation setting key is set to true, a confirmation email is sent to the user after successful password reset.</remarks>
        public static string ResetPassword(string hash, string requestTime, int userID, double interval, string newPassword, string source, string sendEmailFrom, string siteName, MacroResolver resolver, out bool success, string invalidRequestText, string exceededTimeText)
        {
            success = false;

            UserInfo ui;

            // Get user info
            if (userID > 0)
            {
                // Invalidation forces user info to load user settings from DB and not use cached values.
                ui = UserInfoProvider.GetUserInfo(userID);
                ui?.Generalized.Invalidate(false);
            }
            else
            {
                ui = UserInfoProvider.GetUsersDataWithSettings().WhereEquals("UserPasswordRequestHash", hash).FirstObject;
            }

            // Validate request
            ResetPasswordResultEnum result = ValidateResetPassword(ui, hash, requestTime, interval, source);

            // Prepare messages
            string securedAreasLogonUrl = GetSecuredAreasLogonPage(siteName);
            securedAreasLogonUrl = URLHelper.ResolveUrl(URLHelper.AddParameterToUrl(securedAreasLogonUrl, "forgottenpassword", "1"));
            string invalidRequestMessage = DataHelper.GetNotEmpty(invalidRequestText, String.Format(ResHelper.GetString("membership.passwresetfailed"), securedAreasLogonUrl));
            string timeExceededMessage = DataHelper.GetNotEmpty(exceededTimeText, String.Format(ResHelper.GetString("membership.passwreqinterval"), securedAreasLogonUrl));
            string resultMessage = String.Empty;

            // Check result
            switch (result)
            {
                case ResetPasswordResultEnum.Success:
                    success = true;
                    break;

                case ResetPasswordResultEnum.TimeExceeded:
                    resultMessage = timeExceededMessage;
                    break;

                default:
                    resultMessage = invalidRequestMessage;
                    break;
            }

            if (success)
            {
                // Unlock account disabled due to maximum invalid logon attempts
                if (!ui.Enabled && (ui.UserAccountLockReason != UserAccountLockCode.FromEnum(UserAccountLockEnum.DisabledManually)))
                {
                    ui.UserInvalidLogOnAttempts = 0;
                    ui.UserInvalidLogOnAttemptsHash = null;
                    ui.UserAccountLockReason = 0;
                    ui.Enabled = true;
                }

                resultMessage = ResHelper.GetString("membership.passwreset");
                UserInfoProvider.SetPassword(ui, newPassword);

                if (SettingsKeyInfoProvider.GetBoolValue(siteName + ".CMSSendPasswordResetConfirmation"))
                {
                    SendPasswordResetConfirmation(ui, siteName, source, "Membership.PasswordResetConfirmation", sendEmailFrom, resolver);
                }
            }

            return resultMessage;
        }


        /// <summary>
        /// Sends a confirmation e-mail informing the user about a recent password change.
        /// </summary>
        /// <param name="ui">The user to be informed.</param>
        /// <param name="siteName">Current site name.</param>
        /// <param name="source">Context where the password change was initiated.</param>
        /// <param name="emailTemplateName">E-mail template that should be used.</param>
        /// <param name="sendEmailFrom">E-mail address of the sender. If not specified, the CMSSendPasswordEmailsFrom setting value is used.</param>
        /// <param name="resolver">Optional macro resolver used to resolve macros in the specified template.</param>
        public static void SendPasswordResetConfirmation(UserInfo ui, string siteName, string source, string emailTemplateName, string sendEmailFrom = null, MacroResolver resolver = null)
        {
            if (String.IsNullOrEmpty(ui.Email))
            {
                EventLogProvider.LogEvent(EventType.ERROR, source, "SendPasswordResetConfirmation", $"User '{ui.UserName}' has no email address specified.", RequestContext.CurrentURL);

                return;
            }

            if (sendEmailFrom == null)
            {
                sendEmailFrom = SettingsKeyInfoProvider.GetValue("CMSSendPasswordEmailsFrom", siteName);
            }

            EmailTemplateInfo emailTemplate = EmailTemplateProvider.GetEmailTemplate(emailTemplateName, siteName);
            if (emailTemplate == null)
            {
                EventLogProvider.LogEvent(EventType.ERROR, source, "SendPasswordResetConfirmation", $"Email template '{emailTemplateName}' wasn't found.", RequestContext.CurrentURL);

                return;
            }

            if (String.IsNullOrEmpty(sendEmailFrom))
            {
                sendEmailFrom = emailTemplate.TemplateFrom;
            }

            if (!String.IsNullOrEmpty(sendEmailFrom))
            {
                EmailMessage message = new EmailMessage
                {
                    EmailFormat = EmailFormatEnum.Default,
                    From = sendEmailFrom,
                    Subject = ResHelper.GetAPIString("LogonForm.ResetPasswordConfirmationSubject", "Your password has been changed"),
                    Recipients = ui.Email,
                    Body = emailTemplate.TemplateText
                };

                resolver = resolver ?? MembershipResolvers.GetMembershipPasswordResetConfirmationResolver(ui);

                try
                {
                    // Attach template metafiles to e-mail
                    EmailHelper.ResolveMetaFileImages(message, emailTemplate.TemplateID, EmailTemplateInfo.OBJECT_TYPE, ObjectAttachmentsCategories.TEMPLATE);
                    // Send email
                    EmailSender.SendEmailWithTemplateText(siteName, message, emailTemplate, resolver, true);
                }
                catch (Exception ex)
                {
                    EventLogProvider.LogException(source, "SendPasswordResetConfirmation", ex);

                    return;
                }

                EventLogProvider.LogEvent(EventType.INFORMATION, source, "SendPasswordResetConfirmation", $"A password reset confirmation has been sent to specified address '{HTMLHelper.HTMLEncode(ui.Email)}'.", RequestContext.CurrentURL);
            }
            else
            {
                EventLogProvider.LogEvent(EventType.ERROR, source, "SendPasswordResetConfirmation", "Reset password confirmation wasn't send, because no 'From' address was specified.", RequestContext.CurrentURL);
            }
        }


        /// <summary>
        /// Validates request of password reset.
        /// </summary>
        /// <param name="ui">User info object.</param>
        /// <param name="hash">Request hash (identifier).</param>
        /// <param name="requestTime">Request time.</param>
        /// <param name="interval">Interval in which user can reset her password.</param>
        /// <param name="source">Source of calling, will be used for log event</param>
        public static ResetPasswordResultEnum ValidateResetPassword(UserInfo ui, string hash, string requestTime, double interval, string source)
        {
            if (ui != null)
            {
                // Parse datetime from string
                DateTime time;
                try
                {
                    time = DateTimeUrlFormatter.Parse(requestTime);
                }
                catch
                {
                    return ResetPasswordResultEnum.ValidationFailed;
                }

                // Validate request
                if (SecurityHelper.ValidateConfirmationEmailHash(hash, ui.UserGUID.ToString(), time))
                {
                    // Get difference between request and current time
                    DateTime now = DateTime.Now;
                    TimeSpan span = now.Subtract(time);

                    // Check if user can reset password
                    if ((span.TotalHours <= interval) || (interval == 0))
                    {
                        return ResetPasswordResultEnum.Success;
                    }

                    return ResetPasswordResultEnum.TimeExceeded;
                }

                EventLogProvider.LogException(source, "VALIDATIONFAIL", new Exception("Hash validation failed."));

                return ResetPasswordResultEnum.ValidationFailed;
            }

            EventLogProvider.LogException(source, "VALIDATIONFAIL", new Exception("Request identifier hasn't been found."));

            return ResetPasswordResultEnum.ValidationFailed;
        }

        #endregion


        #region "User registration methods"

        /// <summary>
        /// Gets URL for user registration approval page (Page where user is required to confirm his registration).
        /// </summary>
        /// <param name="customApprovalUrl">Base URL for user registration approval URL. When empty URL is taken from settings</param>
        /// <param name="userGuid">Registered user GUID</param>
        /// <param name="siteName">Site name</param>
        /// <param name="notifyAdmin">Indicates if admin should be notified about new user registration</param>
        /// <returns>Returns string with URL in absolute format. URL parameters contains hash.</returns>
        public static string GetRegistrationApprovalUrl(string customApprovalUrl, Guid userGuid, string siteName, bool notifyAdmin = false)
        {
            customApprovalUrl = GetNotHashedRegistrationApprovalUrl(customApprovalUrl, userGuid, siteName, notifyAdmin);

            // Append security hash
            return URLHelper.AddParameterToUrl(customApprovalUrl, "hash", QueryHelper.GetHash(customApprovalUrl, false));
        }


        /// <summary>
        /// Sends registration emails. Returns string with error if any exception occurred.
        /// </summary>
        /// <param name="ui">Send e-mail to this UserInfo</param>
        /// <param name="approvalPage">Registration approval page URL</param>
        /// <param name="requiresConfirmation">Indicates if e-mail with confirmation link should be send</param>
        /// <param name="sendWelcomeEmail">Indicates if user welcome e-mail should be sent</param>
        /// <returns>Returns error if any exception occurred during sending e-mails.</returns>
        public static string SendRegistrationEmails(UserInfo ui, string approvalPage, bool requiresConfirmation, bool sendWelcomeEmail)
        {
            if (ui != null)
            {
                bool error = false;
                EmailTemplateInfo template = null;
                string subject = null;
                string currentSiteName = SiteContext.CurrentSiteName;

                // Requires e-mail confirmation only if specifically requested and settings require confirmation
                requiresConfirmation &= SettingsKeyInfoProvider.GetBoolValue(currentSiteName + ".CMSRegistrationEmailConfirmation");
                // Requires admin's approval only if settings require approval
                bool requiresAdminApprove = SettingsKeyInfoProvider.GetBoolValue(currentSiteName + ".CMSRegistrationAdministratorApproval");

                // Send welcome message with username and password, with confirmation link, user must confirm registration
                if (requiresConfirmation)
                {
                    template = EmailTemplateProvider.GetEmailTemplate("RegistrationConfirmation", currentSiteName);
                    subject = ResHelper.GetString("RegistrationForm.RegistrationConfirmationEmailSubject");
                }
                // Send welcome message with username and password, with information that user must be approved by administrator
                else if (sendWelcomeEmail)
                {
                    if (requiresAdminApprove)
                    {
                        template = EmailTemplateProvider.GetEmailTemplate("Membership.RegistrationWaitingForApproval", currentSiteName);
                        subject = ResHelper.GetString("RegistrationForm.RegistrationWaitingForApprovalSubject");
                    }
                    // Send welcome message with username and password, user can logon directly
                    else
                    {
                        template = EmailTemplateProvider.GetEmailTemplate("Membership.Registration", currentSiteName);
                        subject = ResHelper.GetString("RegistrationForm.RegistrationSubject");
                    }
                }

                // Send email
                if (template != null)
                {
                    // Email message
                    EmailMessage emailMessage = new EmailMessage()
                    {
                        EmailFormat = EmailFormatEnum.Default,
                        Recipients = ui.Email,
                        From = SettingsKeyInfoProvider.GetValue(currentSiteName + ".CMSNoreplyEmailAddress"),
                        Subject = subject
                    };

                    try
                    {
                        var resolver = MembershipResolvers.GetMembershipRegistrationResolver(ui, GetRegistrationApprovalUrl(approvalPage, ui.UserGUID, currentSiteName));
                        EmailSender.SendEmailWithTemplateText(currentSiteName, emailMessage, template, resolver, true);
                    }
                    catch (Exception ex)
                    {
                        EventLogProvider.LogException("RegistrationForm", "SendEmail", ex);
                        error = true;
                    }
                }

                // If there was some error, user must be deleted
                if (error)
                {
                    // Email was not send, user can't be approved - delete it
                    UserInfoProvider.DeleteUser(ui);
                    return ResHelper.GetString("RegistrationForm.UserWasNotCreated");
                }
                else
                {
                    return null;
                }
            }
            // Empty UserInfo
            else
            {
                return ResHelper.GetString("adm.user.notexist");
            }
        }


        /// <summary>
        /// Sends notification e-mail to administrator about user registration.
        /// </summary>
        public static void NotifyAdministrator(UserInfo ui, string fromAddress, string toAddress)
        {
            string currentSiteName = SiteContext.CurrentSiteName;
            bool requiresAdminApprove = SettingsKeyInfoProvider.GetBoolValue(currentSiteName + ".CMSRegistrationAdministratorApproval");

            // Get template
            EmailTemplateInfo mEmailTemplate = EmailTemplateProvider.GetEmailTemplate(requiresAdminApprove ? "Registration.Approve" : "Registration.New", currentSiteName);

            if (mEmailTemplate == null)
            {
                EventLogProvider.LogEvent(EventType.ERROR, "RegistrationForm", "GetEmailTemplate", eventUrl: RequestContext.RawURL);
            }
            else
            {
                EmailMessage message = new EmailMessage();
                message.EmailFormat = EmailFormatEnum.Default;
                message.From = fromAddress;
                message.Recipients = toAddress;
                message.Subject = ResHelper.GetString("RegistrationForm.EmailSubject");

                try
                {
                    var resolver = MembershipResolvers.GetRegistrationResolver(ui);
                    EmailSender.SendEmailWithTemplateText(currentSiteName, message, mEmailTemplate, resolver, false);
                }
                catch
                {
                    EventLogProvider.LogEvent(EventType.ERROR, "Membership", "RegistrationEmail");
                }
            }
        }


        private static string GetNotHashedRegistrationApprovalUrl(string customApprovalUrl, Guid userGuid, string siteName, bool notifyAdmin)
        {
            // Set approval page
            if (String.IsNullOrEmpty(customApprovalUrl))
            {
                // Get approval page from settings if exists
                customApprovalUrl = SettingsKeyInfoProvider.GetURLValue(siteName + ".CMSRegistrationApprovalPath", "~/CMSPages/Dialogs/UserRegistration.aspx");
            }

            // Get absolute URL
            customApprovalUrl = URLHelper.GetAbsoluteUrl(customApprovalUrl);

            // Append user guid to URL
            customApprovalUrl = URLHelper.AddParameterToUrl(customApprovalUrl, "userguid", userGuid.ToString());

            // Append admin notification parameter to URL
            if (notifyAdmin)
            {
                customApprovalUrl = URLHelper.AddParameterToUrl(customApprovalUrl, "notifyadmin", true.ToString());
            }
            return customApprovalUrl;
        }

        #endregion


        #region "Online users methods"

        /// <summary>
        /// Checks whether the given user is kicked or not.
        /// </summary>
        /// <param name="userID">User id</param>
        public static bool UserKicked(int userID)
        {
            if (KickUsers.Contains(userID))
            {
                return true;
            }

            return false;
        }


        /// <summary>
        /// Removes user from kicked.
        /// </summary>
        /// <param name="UserID">User id</param>
        public static void RemoveUserFromKicked(int UserID)
        {
            if (KickUsers.Contains(UserID))
            {
                KickUsers.Remove(UserID);
            }
        }


        /// <summary>
        /// Adds user to kicked.
        /// </summary>
        /// <param name="userID">User id</param>
        public static void AddUserToKicked(int userID)
        {
            if (!KickUsers.Contains(userID))
            {
                KickUsers.Add(userID, (DateTime.Now.AddMinutes(SettingsKeyInfoProvider.GetIntValue("CMSDenyLoginInterval"))));
            }
        }


        /// <summary>
        /// Removes expired records from KickUsers hashtable.
        /// </summary>
        public static void RemoveExpiredKickedUsers()
        {
            // Prepare keys
            object[] keys;

            lock (KickUsers)
            {
                keys = new object[KickUsers.Keys.Count];
                KickUsers.Keys.CopyTo(keys, 0);
            }

            // Loop thru hashtable
            foreach (object k in keys)
            {
                int userID = ValidationHelper.GetInteger(k, 0);
                // Remove record from hashtable?
                if (DateTime.Now > ValidationHelper.GetDateTime(KickUsers[userID], DateTimeHelper.ZERO_TIME))
                {
                    KickUsers.Remove(userID);
                }
            }
        }


        /// <summary>
        /// Returns ID's of kicked users in string in format: ID1,ID2 ...
        /// </summary>
        public static string GetKickedUsers()
        {
            // Prepare keys
            object[] keys;

            lock (KickUsers)
            {
                keys = new object[KickUsers.Keys.Count];
                KickUsers.Keys.CopyTo(keys, 0);
            }

            string users = String.Empty;

            // Loop thru hashtable
            foreach (object k in keys)
            {
                int userID = ValidationHelper.GetInteger(k, 0);
                // Remove record from hashtable?
                if (DateTime.Now < ValidationHelper.GetDateTime(KickUsers[userID], DateTimeHelper.ZERO_TIME))
                {
                    users += userID + ",";
                }
            }

            return users.TrimEnd(',');
        }

        #endregion


        #region "URL methods"

        /// <summary>
        /// Check URL query string for authentication parameter and authenticate user.
        /// </summary>
        public static void HandleAutomaticSignIn()
        {
            // Check for authentication token
            if (QueryHelper.Contains("authenticationGuid") && SettingsKeyInfoProvider.GetBoolValue("CMSAutomaticallySignInUser"))
            {
                UserInfo ui = null;

                if (!IsAuthenticated())
                {
                    // Get authentication token
                    Guid authGuid = QueryHelper.GetGuid("authenticationGuid", Guid.Empty);
                    if (authGuid != Guid.Empty)
                    {
                        // Get users with found authentication token
                        ui = UserInfoProvider.GetUsersDataWithSettings().WhereEquals("UserAuthenticationGUID", authGuid).TopN(1).First();
                        if (ui != null)
                        {
                            // Authenticate user
                            AuthenticateUser(ui.UserName, false, false);
                        }
                    }
                }
                else
                {
                    // Get current user info
                    ui = MembershipContext.AuthenticatedUser;
                }

                // Remove authentication GUID
                if ((ui != null) && (ui.UserAuthenticationGUID != Guid.Empty))
                {
                    using (CMSActionContext context = new CMSActionContext())
                    {
                        context.DisableAll();

                        ui.UserAuthenticationGUID = Guid.Empty;
                        ui.Generalized.SetObject();
                    }
                }

                // Redirect to URL without authentication token
                URLHelper.Redirect(URLHelper.RemoveParameterFromUrl(RequestContext.CurrentURL, "authenticationGuid"));
            }
        }


        /// <summary>
        /// Returns value of CMSSecuredAreasLogonPage setting key, or returns default logon URL if this setting key is empty.
        /// </summary>
        /// <param name="site">site identifier bound with setting key</param>
        /// <returns>value of setting key, or default logon URL</returns>
        public static string GetSecuredAreasLogonPage(SiteInfoIdentifier site)
        {
            string logonUrl = SettingsKeyInfoProvider.GetValue("CMSSecuredAreasLogonPage", site);
            if (string.IsNullOrWhiteSpace(logonUrl))
            {
                logonUrl = DEFAULT_LOGON_PAGE;
            }

            return logonUrl;
        }

        #endregion
    }
}