using System;
using System.Security;

using CMS.Base;
using CMS.Helpers;

namespace CMS.Membership
{
    /// <summary>
    /// Membership related context methods and variables.
    /// </summary>
    [RegisterAllProperties]
    public class MembershipContext : AbstractContext<MembershipContext>
    {
        #region "Constants"

        /// <summary>
        /// Session key used for information about partially authentication.
        /// </summary>
        private const string MF_AUTHENICATION_INFO = "PartiallyAuthUser";

        #endregion


        #region "Variables"

        private CurrentUserInfo mCurrentUser;
        private UserInfo mCurrentUserProfile;

        private bool mUserAccountLockedInvalidLogonAttempts;
        private bool mUserAccountLockedPasswordExpired;
        private bool mUserAccountLockedDueToInvalidPasscode;
        private bool mMFAuthenticationTokenNotInitialized;
        private bool mSignOutPending;
        private bool mUserIsBanned;
        private bool? mAllowOnlyRead;
        private bool mUserIsPartiallyAuthenticated;
        private SessionInfo mCurrentSessionInfo;
        private string mOriginalUserName;

        #endregion


        #region "Public properties"

        /// <summary>
        /// Current user info object according the URL parameter of the current request.
        /// It is available when the request contains parameters "userid", "username" or "userguid" with valid value of the user.
        /// </summary>
        [RegisterProperty("CurrentUser", Hidden = true)]
        public static UserInfo CurrentUserProfile
        {
            get
            {
                return GetCurrentUserProfile();
            }
            set
            {
                Current.mCurrentUserProfile = value;
            }
        }


        /// <summary>
        /// Gets user name of impersonated user.
        /// </summary>
        public static string ImpersonatedUserName
        {
            get
            {
                if (CurrentUserIsImpersonated())
                {
                    var data = ImpersonationHelper.GetDataFromCookie();
                    var impersonatedUser = UserInfoProvider.GetUserInfoByGUID(data.ImpersonatedUserId);
                    return impersonatedUser?.UserName;
                }

                return null;
            }
        }


        /// <summary>
        /// Gets original user name in case that current user is impersonated.
        /// </summary>
        public static string OriginalUserName
        {
            get
            {
                if (Current.mOriginalUserName == null && CMSHttpContext.Current != null && CurrentUserIsImpersonated())
                {
                    var data = ImpersonationHelper.GetDataFromCookie();
                    var impersonatedUser = UserInfoProvider.GetUserInfoByGUID(data.OriginalUserId);
                    Current.mOriginalUserName = impersonatedUser?.UserName;
                }

                return Current.mOriginalUserName;
            }
        }


        /// <summary>
        /// Returns current authenticated user.
        /// </summary>
        public static CurrentUserInfo AuthenticatedUser
        {
            get
            {
                var current = Current;
                if (current.mCurrentUser == null)
                {
                    LogUnsupportedUsageForCurrentRequestStage();

                    // When context or user information is not available, return public user as default, do not cache the user in that case
                    // User information is not available before HttpApplication.AuthenticateRequest is called
                    if ((CMSHttpContext.Current == null) || (CMSHttpContext.Current.User == null))
                    {
                        return AuthenticationHelper.GlobalPublicUser;
                    }

                    current.mCurrentUser = AuthenticationHelper.GetCurrentUser();
                }

                return current.mCurrentUser;
            }
            set
            {
                Current.mCurrentUser = value;
            }
        }


        /// <summary>
        /// Logs error to the eventlog when property called prior <see cref="RequestEvents.Authenticate"/> event.
        /// </summary>
        private static void LogUnsupportedUsageForCurrentRequestStage()
        {
            if (SystemContext.DiagnosticLogging && IsBeforeAuthenticateRequest())
            {
                EventLog.EventLogProvider.LogEvent(
                    eventType: EventLog.EventType.ERROR,
                    source: "Membership",
                    eventCode: "GetAuthenticatedUser",
                    eventDescription: $"MembershipContext.AuthenticatedUser is called prior HttpApplication.AuthenticateRequest stage. {Environment.NewLine} {Environment.StackTrace}",
                    userName: "public");
            }
        }


        /// <summary>
        /// Indicates whether application stage is before authentication request.
        /// </summary>
        private static bool IsBeforeAuthenticateRequest()
        {
            var result = false;

            try
            {
                result = CMSHttpContext.Current != null && CMSHttpContext.Current.CurrentNotification < System.Web.RequestNotification.AuthenticateRequest;
            }
            catch
            {
            }

            return result;
        }


        /// <summary>
        /// Indicates if user reached maximal allowed number of invalid logon attempts.
        /// </summary>
        [NotRegisterProperty]
        public static bool UserAccountLockedDueToInvalidLogonAttempts
        {
            get
            {
                return Current.mUserAccountLockedInvalidLogonAttempts;
            }
            set
            {
                Current.mUserAccountLockedInvalidLogonAttempts = value;
            }
        }


        /// <summary>
        /// Indicates if user account was locked because its password expired.
        /// </summary>
        [NotRegisterProperty]
        public static bool UserAccountLockedDueToPasswordExpiration
        {
            get
            {
                return Current.mUserAccountLockedPasswordExpired;
            }
            set
            {
                Current.mUserAccountLockedPasswordExpired = value;
            }
        }


        /// <summary>
        /// Indicates if multi-factor token ID was initialized.
        /// </summary>
        [NotRegisterProperty]
        public static bool MFAuthenticationTokenNotInitialized
        {
            get
            {
                return Current.mMFAuthenticationTokenNotInitialized;
            }
            set
            {
                Current.mMFAuthenticationTokenNotInitialized = value;
            }
        }


        /// <summary>
        /// Indicates if login failed due to invalid passcode.
        /// </summary>
        [NotRegisterProperty]
        public static bool UserAuthenticationFailedDueToInvalidPasscode
        {
            get
            {
                return Current.mUserAccountLockedDueToInvalidPasscode;
            }
            set
            {
                Current.mUserAccountLockedDueToInvalidPasscode = value;
            }
        }


        /// <summary>
        /// Indicates if user is partially authenticated. Previously inserted username and password were correct.
        /// </summary>
        [NotRegisterProperty]
        public static bool UserIsPartiallyAuthenticated
        {
            get
            {
                return Current.mUserIsPartiallyAuthenticated;
            }
            set
            {
                Current.mUserIsPartiallyAuthenticated = value;
            }
        }


        /// <summary>
        /// Current session info.
        /// </summary>
        [NotRegisterProperty]
        public static SessionInfo CurrentSession
        {
            get
            {
                return Current.mCurrentSessionInfo ?? SessionManager.GetCurrentSessionInfoFromHashtable();
            }
            set
            {
                Current.mCurrentSessionInfo = value;
            }
        }


        /// <summary>
        /// If true, SignOut method was called in this method and user is figuratively signed out.
        /// </summary>
        [NotRegisterProperty]
        public static bool SignOutPending
        {
            get
            {
                return Current.mSignOutPending;
            }
            set
            {
                Current.mSignOutPending = value;
            }
        }


        /// <summary>
        /// Indicates whether the user is banned using BannedIP modules.
        /// </summary>
        [NotRegisterProperty]
        public static bool UserIsBanned
        {
            get
            {
                return Current.mUserIsBanned;
            }
            set
            {
                Current.mUserIsBanned = value;
            }
        }


        /// <summary>
        /// Returns true if the current context allows only reading of the data
        /// </summary>
        [Obsolete("Use VirtualContext.ReadonlyMode property instead.")]
        public static bool AllowOnlyRead
        {
            get
            {
                var c = Current;
                if (c.mAllowOnlyRead == null)
                {
                    // Allow only read in preview mode to prevent unwanted changes by user
                    c.mAllowOnlyRead = VirtualContext.ReadonlyMode;
                }

                return c.mAllowOnlyRead.Value;
            }
            set
            {
                Current.mAllowOnlyRead = value;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Indicates whether currently authenticated user is impersonated.
        /// </summary>
        public static bool CurrentUserIsImpersonated()
        {
            var cookie = ImpersonationHelper.GetCookie();
            if (cookie == null || string.IsNullOrEmpty(cookie.Value))
            {
                return false;
            }

            var data = ImpersonationHelper.GetDataFromCookie();
            var isValid = data.IsValid();

            if (!isValid)
            {
                ImpersonationHelper.RemoveCookie();
            }

            return isValid;
        }


        /// <summary>
        /// Returns information on the current user according user ID/ user name specified as an URL parameter of the current request
        /// </summary>
        private static UserInfo GetCurrentUserProfile()
        {
            var c = Current;

            UserInfo ui = c.mCurrentUserProfile;
            if (ui == null)
            {
                // Get all the necessary information from the URL parameters collection
                int userId = QueryHelper.GetInteger("userid", 0);
                if (userId > 0)
                {
                    // Try to get the user info from the DB by the user ID first
                    ui = UserInfoProvider.GetUserInfo(userId);
                }

                if (ui == null)
                {
                    string userName = QueryHelper.GetString("username", "");
                    if (userName != "")
                    {
                        ui = UserInfoProvider.GetUserInfo(userName);
                    }
                }

                if (ui == null)
                {
                    Guid userGuid = QueryHelper.GetGuid("userguid", Guid.Empty);
                    if (userGuid != Guid.Empty)
                    {
                        ui = UserInfoProvider.GetUserInfoByGUID(userGuid);
                    }
                }

                // Save the info to the Session and request items
                c.mCurrentUserProfile = ui;
            }

            // Hidden users are not visible
            if ((ui != null) && (ui.UserIsHidden))
            {
                return null;
            }

            // Return user info
            return ui;
        }


        /// <summary>
        /// Gets impersonating user if the current <see cref="AuthenticatedUser"/> is impersonated, otherwise null.
        /// </summary>
        /// <exception cref="SecurityException">Data from impersonation cookie are invalid.</exception>
        public static UserInfo GetImpersonatingUser()
        {
            if (CurrentUserIsImpersonated())
            {
                var data = ImpersonationHelper.GetDataFromCookie();

                return UserInfoProvider.GetUserInfoByGUID(data.OriginalUserId);
            }

            return null;
        }

        #endregion


        #region "Multi-factor authentication methods"

        /// <summary>
        /// Get hash stamp which represents valid period for user.
        /// </summary>
        /// <param name="time">DateTime.</param>
        private static string CreatePartialAuthenticationInfo(DateTime time)
        {
            var shiftedIntervalInMinutes = MFAuthenticationHelper.HelperObject.ClockDriftTolerance.TotalMinutes + 1;

            // Create timestamp
            long timestamp = time.AddMinutes(shiftedIntervalInMinutes).Ticks;
#pragma warning disable 618
            return EncryptionHelper.EncryptData(timestamp.ToString());
#pragma warning restore 618
        }


        /// <summary>
        /// Partially authenticates the user.
        /// </summary>
        public static void SetUserPartialAuthentication()
        {
            SetUserPartialAuthentication(DateTime.Now);
        }


        /// <summary>
        /// Partially authenticates the user.
        /// </summary>
        /// <param name="dateTime">Datetime.</param>
        public static void SetUserPartialAuthentication(DateTime dateTime)
        {
            UserIsPartiallyAuthenticated = true;
            var authInfo = CreatePartialAuthenticationInfo(dateTime);

            SessionHelper.SetValue(MF_AUTHENICATION_INFO, authInfo);
        }


        /// <summary>
        /// Clear authentication info.
        /// </summary>
        public static void ClearUserPartialAuthentication()
        {
            UserIsPartiallyAuthenticated = false;
            SessionHelper.Remove(MF_AUTHENICATION_INFO);
        }


        /// <summary>
        /// Checks if user is partially authenticated.
        /// </summary>
        public static bool IsUserPartiallyAuthenticated()
        {
            return IsUserPartiallyAuthenticated(DateTime.Now);
        }


        /// <summary>
        /// Checks if user is partially authenticated.
        /// </summary>
        /// <param name="dateTime">Datetime.</param>
        public static bool IsUserPartiallyAuthenticated(DateTime dateTime)
        {
            UserIsPartiallyAuthenticated = false;
            var authInfo = SessionHelper.GetValue(MF_AUTHENICATION_INFO) as string;

            var decryptedMessage = EncryptionHelper.DecryptData(authInfo);

            long decryptedTimestamp;

            if (!long.TryParse(decryptedMessage, out decryptedTimestamp))
            {
                return false;
            }

            long actualTimestamp = dateTime.Ticks;

            if (actualTimestamp <= decryptedTimestamp)
            {
                UserIsPartiallyAuthenticated = true;
            }

            return UserIsPartiallyAuthenticated;
        }

        #endregion


        /// <summary>
        /// Clones the object for new thread
        /// </summary>
        public override object CloneForNewThread()
        {
            Ensure(OriginalUserName);

            return base.CloneForNewThread();
        }
    }
}