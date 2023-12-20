using System;
using System.Collections.Specialized;
using System.Linq;
using System.Web.Security;

using CMS.Helpers;
using CMS.Base;
using CMS.SiteProvider;
using CMS.Membership;
using CMS.DataEngine;

namespace CMS.MembershipProvider
{
    /// <summary>
    /// Class providing membership management.
    /// </summary>
    public class CMSMembershipProvider : System.Web.Security.MembershipProvider
    {
        #region "Variables"

        private string mApplicationName = "a";
        private string mDescription = "";
        private bool mEnablePasswordReset = false;
        private bool mEnablePasswordRetrieval = false;
        private int mMaxInvalidPasswordAttempts = 0;
        private int mMinRequiredNonAlphanumericCharacters = 0;
        private int mMinRequiredPasswordLength = 0;
        private string mName = "";
        private int mPasswordAttemptWindow = 0;
        private MembershipPasswordFormat mPasswordFormat = MembershipPasswordFormat.Hashed;
        private string mPasswordStrengthRegularExpression = "";
        private bool mRequiresQuestionAndAnswer = false;
        private bool mRequiresUniqueEmail = false;

        #endregion


        #region "Properties"

        /// <summary>
        /// Applicaton name.
        /// </summary>
        public override string ApplicationName
        {
            get
            {
                return mApplicationName;
            }
            set
            {
                mApplicationName = value;
            }
        }


        /// <summary>
        /// Description.
        /// </summary>
        public override string Description
        {
            get
            {
                return mDescription;
            }
        }


        /// <summary>
        /// Enable password reset.
        /// </summary>
        public override bool EnablePasswordReset
        {
            get
            {
                return mEnablePasswordReset;
            }
        }


        /// <summary>
        /// Enable password retrieval.
        /// </summary>
        public override bool EnablePasswordRetrieval
        {
            get
            {
                return mEnablePasswordRetrieval;
            }
        }


        /// <summary>
        /// Maximum invalid password attempts.
        /// </summary>
        public override int MaxInvalidPasswordAttempts
        {
            get
            {
                return mMaxInvalidPasswordAttempts;
            }
        }


        /// <summary>
        /// Minimum required nonalphanumeric characters.
        /// </summary>
        public override int MinRequiredNonAlphanumericCharacters
        {
            get
            {
                return mMinRequiredNonAlphanumericCharacters;
            }
        }


        /// <summary>
        /// Minimum required password length.
        /// </summary>
        public override int MinRequiredPasswordLength
        {
            get
            {
                return mMinRequiredPasswordLength;
            }
        }


        /// <summary>
        /// Name.
        /// </summary>
        public override string Name
        {
            get
            {
                return mName;
            }
        }


        /// <summary>
        /// Password attempt window.
        /// </summary>
        public override int PasswordAttemptWindow
        {
            get
            {
                return mPasswordAttemptWindow;
            }
        }


        /// <summary>
        /// Password format.
        /// </summary>
        public override MembershipPasswordFormat PasswordFormat
        {
            get
            {
                return mPasswordFormat;
            }
        }


        /// <summary>
        /// Password strength regular expression.
        /// </summary>
        public override string PasswordStrengthRegularExpression
        {
            get
            {
                return mPasswordStrengthRegularExpression;
            }
        }


        /// <summary>
        /// Required question and aswer.
        /// </summary>
        public override bool RequiresQuestionAndAnswer
        {
            get
            {
                return mRequiresQuestionAndAnswer;
            }
        }


        /// <summary>
        /// Requires unique email.
        /// </summary>
        public override bool RequiresUniqueEmail
        {
            get
            {
                return mRequiresUniqueEmail;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Changes Password of user specified by username.
        /// </summary>
        /// <param name="username">User name</param>
        /// <param name="oldPassword">Old password</param>
        /// <param name="newPassword">New password</param>
        public override bool ChangePassword(string username, string oldPassword, string newPassword)
        {
            UserInfoProvider.SetPassword(username, newPassword);
            return true;
        }


        /// <summary>
        /// As password questions and answers are not implemented in UserInfo, this metod returns true.
        /// </summary>
        /// <param name="username">Not used</param>
        /// <param name="password">Not used</param>
        /// <param name="newPasswordQuestion">Not used</param>
        /// <param name="newPasswordAnswer">Not used</param>
        public override bool ChangePasswordQuestionAndAnswer(string username, string password, string newPasswordQuestion, string newPasswordAnswer)
        {
            return true;
        }


        /// <summary>
        /// Creates new user.
        /// </summary>
        /// <param name="username">User name</param>
        /// <param name="password">Password</param>
        /// <param name="email">E-mail</param>
        /// <param name="passwordQuestion">Not used</param>
        /// <param name="passwordAnswer">Not used</param>
        /// <param name="isApproved">'isApproved' parameter is considered as 'Enabled' property in UserInfo;</param>
        /// <param name="providerUserKey">Not used</param>
        /// <param name="status">If UserInfoProvider's SetUserInfo method throws exception then status is set to 'ProviderError', else it is set to 'Success'</param>
        public override MembershipUser CreateUser(string username, string password, string email, string passwordQuestion, string passwordAnswer, bool isApproved, Object providerUserKey, out MembershipCreateStatus status)
        {
            UserInfo newUser = new UserInfo();
            newUser.Email = email;
            newUser.Enabled = isApproved;
            newUser.FirstName = "";
            newUser.FullName = "";
            newUser.LastName = "";
            newUser.MiddleName = "";
            newUser.PreferredCultureCode = "";
            newUser.UserName = username;
            newUser.UserPasswordLastChanged = DateTime.Now;

            newUser.SiteIndependentPrivilegeLevel = UserPrivilegeLevelEnum.None;

            try
            {
                UserInfoProvider.SetUserInfo(newUser);
                UserInfoProvider.SetPassword(newUser, password);

                status = MembershipCreateStatus.Success;
            }
            catch
            {
                status = MembershipCreateStatus.ProviderError;
            }

            return new CMSMembershipUser(newUser);
        }


        /// <summary>
        /// Deletes user specified by username.
        /// </summary>
        /// <param name="username">User name</param>
        /// <param name="deleteAllRelatedData">Not used</param>
        public override bool DeleteUser(string username, bool deleteAllRelatedData)
        {
            try
            {
                UserInfoProvider.DeleteUser(username);
                return true;
            }
            catch
            {
                return false;
            }
        }


        /// <summary>
        /// Gets a collection of membership users where the e-mail address contains the specified e-mail address to match.
        /// </summary>
        /// <param name="emailToMatch">User e-mail</param>
        /// <param name="pageIndex">Not used</param>
        /// <param name="pageSize">Not used</param>
        /// <param name="totalRecords">Not used</param>
        public override MembershipUserCollection FindUsersByEmail(string emailToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            totalRecords = 0;
            MembershipUserCollection userCollection = new MembershipUserCollection();

            if (emailToMatch != null)
            {
                var users = UserInfoProvider.GetUsers()
                    .WhereContains("Email", emailToMatch);

                foreach (UserInfo ui in users)
                {
                    userCollection.Add(GetUser(ui));
                }
                totalRecords = users.Count;
            }

            return userCollection;
        }


        /// <summary>
        /// Gets a collection of membership users where the user name contains the specified user name to match.
        /// </summary>
        /// <param name="usernameToMatch">User name</param>
        /// <param name="pageIndex">Not used</param>
        /// <param name="pageSize">Not used</param>
        /// <param name="totalRecords">Not used</param>
        public override MembershipUserCollection FindUsersByName(string usernameToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            MembershipUserCollection userCollection = new MembershipUserCollection();

            if (!String.IsNullOrEmpty(usernameToMatch))
            {
                usernameToMatch = SqlHelper.GetSafeQueryString(usernameToMatch, false);
            }

            var users = UserInfoProvider.GetUsers()
                .WhereContains("UserName", usernameToMatch);

            foreach (UserInfo ui in users)
            {
                userCollection.Add(GetUser(ui));
            }
            totalRecords = users.Count;

            return userCollection;

        }


        /// <summary>
        /// Gets a collection of all the users in DB.
        /// </summary>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="totalRecords">Total number of users</param>
        public override MembershipUserCollection GetAllUsers(int pageIndex, int pageSize, out int totalRecords)
        {
            // Get the users
            var usersSet = UserInfoProvider.GetUsers();
            var userCollection = new MembershipUserCollection();

            foreach (var user in usersSet)
            {
                userCollection.Add(GetUser(user));
            }

            totalRecords = usersSet.Count;
            return userCollection;
        }


        /// <summary>
        /// Returns number of online users. Monitor online users feature must be enabled.
        /// </summary>
        public override int GetNumberOfUsersOnline()
        {
            return SessionManager.PublicUsers + SessionManager.AuthenticatedUsers;
        }


        /// <summary>
        /// Gets password of user specified by username.
        /// </summary>
        /// <param name="username">User name</param>
        /// <param name="answer">Not used</param>
        public override string GetPassword(string username, string answer)
        {
            UserInfo ui = UserInfoProvider.GetUserInfo(username);
            if (ui != null)
            {
                return ValidationHelper.GetString(ui.GetValue("UserPassword"), "");
            }

            return "";
        }


        /// <summary>
        /// Returns MembershipUser object containing data of user specified by providerUserKey.
        /// </summary>
        /// <param name="providerUserKey">User key</param>
        /// <param name="userIsOnline">Not used</param>
        public override MembershipUser GetUser(Object providerUserKey, bool userIsOnline)
        {
            int userId = Convert.ToInt32(providerUserKey);
            UserInfo ui = UserInfoProvider.GetUserInfo(userId);
            return GetUser(ui);
        }


        /// <summary>
        /// Returns MembershipUser object containing data of user specified by his name.
        /// </summary>
        /// <param name="username">User name</param>
        /// <param name="userIsOnline">Not used</param>
        public override MembershipUser GetUser(string username, bool userIsOnline)
        {
            UserInfo ui = UserInfoProvider.GetUserInfo(username);
            return GetUser(ui);
        }


        /// <summary>
        /// Returns MembershipUser object containing data of user specified by his name.
        /// </summary>
        /// <param name="ui">User info</param>
        public MembershipUser GetUser(UserInfo ui)
        {
            CMSMembershipUser resultUser = new CMSMembershipUser(ui);
            return resultUser;
        }


        /// <summary>
        /// Gets user name of user with given email.
        /// </summary>
        /// <param name="email">User e-mail</param>
        public override string GetUserNameByEmail(string email)
        {
            UserInfo user = UserInfoProvider.GetUsers()
               .WhereEquals("Email", email)
               .Columns("UserName")
               .TopN(1).FirstOrDefault();

            if (user != null)
            {
                return user.UserName;
            }
            return "";
        }


        /// <summary>
        /// Sets password of user specified by user name to empty string.
        /// </summary>
        /// <param name="username">User name</param>
        /// <param name="answer">Not used</param>
        public override string ResetPassword(string username, string answer)
        {
            UserInfoProvider.SetPassword(username, "");
            return "";
        }


        /// <summary>
        /// Sets 'Enabled' property of user specified by user name to 'True'.
        /// </summary>
        /// <param name="userName">User name</param>
        public override bool UnlockUser(string userName)
        {
            UserInfo user = UserInfoProvider.GetUserInfo(userName);

            if (user != null)
            {
                AuthenticationHelper.UnlockUserAccount(user);
                return true;
            }

            return false;
        }


        /// <summary>
        /// Updates data of specified user.
        /// </summary>
        /// <param name="user">User to update</param>
        public override void UpdateUser(MembershipUser user)
        {
            if (user is CMSMembershipUser)
            {
                CMSMembershipUser cmsUser = (CMSMembershipUser)user;
                UserInfoProvider.SetUserInfo(cmsUser.UserInfoMembership);
            }
            else
            {
                UserInfo CMSuser = UserInfoProvider.GetUserInfo(user.UserName);
                CMSuser.Email = user.Email;
                UserInfoProvider.SetEnabled(CMSuser, user.IsApproved);
                UserInfoProvider.SetUserInfo(CMSuser);
            }
        }


        /// <summary>
        /// Validates entered passcode for user. Partial validation is checked.
        /// This method should be used when validating passcode in multi-step scenario.
        /// </summary>
        /// <param name="username">User name.</param>
        /// <param name="passcode">Passcode.</param>
        /// <returns>True if passcode is valid. False otherwise.</returns>
        public bool MFValidatePasscode(string username, string passcode)
        {
            // Check partial authentication validity
            if (!MembershipContext.IsUserPartiallyAuthenticated())
            {
                return false;
            }

            // Get the user
            SiteInfo si = SiteInfoProvider.GetSiteInfo(SiteContext.CurrentSiteName);
            UserInfo user = UserInfoProvider.GetUserInfoForSitePrefix(username, si);

            return MFValidatePasscode(user, passcode);
        }


        /// <summary>
        /// Validates entered passcode for user. Checks passcode only.
        /// </summary>
        /// <param name="user">User.</param>
        /// <param name="passcode">Passcode.</param>
        /// <param name="finalize">Finalization of authentication process will NOT be performed if set to false.</param>
        /// <returns>True if passcode is valid. False otherwise.</returns>
        public bool MFValidatePasscode(UserInfo user, string passcode, bool finalize = true)
        {
            // Always set membership context
            MembershipContext.UserAccountLockedDueToInvalidLogonAttempts = (UserAccountLockCode.ToEnum(user.UserAccountLockReason) == UserAccountLockEnum.MaximumInvalidLogonAttemptsReached);
            MembershipContext.UserAccountLockedDueToPasswordExpiration = (UserAccountLockCode.ToEnum(user.UserAccountLockReason) == UserAccountLockEnum.PasswordExpired);

            // Checking disabled users should always return false
            if ((user == null) || !user.Enabled)
            {
                return false;
            }

            // Check passcode
            if (MFAuthenticationHelper.IsPasscodeValid(user, passcode))
            {
                MembershipContext.ClearUserPartialAuthentication();

                if (finalize)
                {
                    AuthenticationHelper.FinalizeAuthenticationProcess(user, SiteContext.CurrentSiteID);
                }

                return true;
            }

            // Passcode was wrong
            if (passcode != null)
            {
                AuthenticationHelper.CheckInvalidPasswordAttempts(user, SiteContext.CurrentSiteName);
                MembershipContext.UserAuthenticationFailedDueToInvalidPasscode = true;
            }

            return false;
        }


        /// <summary>
        /// Checks if user can be authenticated.
        /// </summary>
        /// <param name="username">User name.</param>
        /// <param name="password">Password.</param>
        /// <returns>True if user can be authenticated. False otherwise.</returns>
        public bool MFValidateCredentials(string username, string password)
        {
            // Try to authenticate user
            UserInfo result = AuthenticationHelper.AuthenticateUser(username, password, SiteContext.CurrentSiteName, false);

            // Mixed mode authentication - authenticate against AD if not authenticated or external
            if ((result == null) && AuthenticationMode.IsMixedAuthentication())
            {
                // Authenticate the user using AD
                result = AuthenticationHelper.AuthenticateUserAD(username, password, SiteContext.CurrentSiteName);
            }

            if (result != null)
            {
                // Partially authenticate the user
                MembershipContext.SetUserPartialAuthentication();

                MFAuthenticationHelper.IssuePasscode(result.UserName);

                return true;
            }

            return false;
        }


        /// <summary>
        /// Checks whether given password matches the password of user specified by username.
        /// </summary>
        /// <param name="username">User name</param>
        /// <param name="password">Password</param>
        public override bool ValidateUser(string username, string password)
        {
            // Get the site name
            string siteName = SiteContext.CurrentSiteName;

            UserInfo result = AuthenticationHelper.AuthenticateUser(username, password, siteName);

            // Mixed mode authentication - authenticate against AD if not authenticated or external
            if ((result == null) && AuthenticationMode.IsMixedAuthentication())
            {
                // Authenticate the user using AD
                result = AuthenticationHelper.AuthenticateUserAD(username, password, siteName);
            }

            // Check if user is enabled just for sure, authenticate methods should return null if disabled
            return ((result != null) && (result.Enabled));
        }


        /// <summary>
        /// Initializes the provider.
        /// </summary>
        /// <param name="strName">Name used to refer to the provider</param>
        /// <param name="config">Ignored</param>
        public override void Initialize(string strName, NameValueCollection config)
        {
            mName = strName;
            mApplicationName = "/";
        }

        #endregion
    }
}