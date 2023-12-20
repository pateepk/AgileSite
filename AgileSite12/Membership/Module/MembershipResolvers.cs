using System.Data;

using CMS.DataEngine;
using CMS.MacroEngine;

namespace CMS.Membership
{
    /// <summary>
    /// Resolvers used in e-mail templates and other macro visual components.
    /// </summary>
    public class MembershipResolvers : ResolverDefinition
    {
        /// <summary>
        /// Returns the basic membership resolver.
        /// </summary>
        /// <param name="user">User to be used as a data source</param>
        /// <param name="buildFrom">Resolver to build the result from</param>
        /// <param name="addUserFields">If true, FirstName, UserFullName, LastName, Email and UserName are registered to resolver</param>
        /// <param name="setCulture">If true, newly created resolver has culture set to preferred culture of <paramref name="user"/></param>
        private static MacroResolver GetMembershipResolverBase(UserInfo user, MacroResolver buildFrom, bool addUserFields = false, bool setCulture = true)
        {
            if (user == null)
            {
                user = (UserInfo)ModuleManager.GetReadOnlyObject(UserInfo.OBJECT_TYPE);
            }
            else
            {
                user = user.Clone();
            }

            var resolver = buildFrom ?? MacroContext.CurrentResolver.CreateChild();
            resolver.SetNamedSourceData("User", user);
            resolver.SetAnonymousSourceData(user);

            if (addUserFields)
            {
                resolver.SetNamedSourceData("FirstName", user.FirstName);
                resolver.SetNamedSourceData("UserFullName", user.FullName);
                resolver.SetNamedSourceData("LastName", user.LastName);
                resolver.SetNamedSourceData("Email", user.Email);
                resolver.SetNamedSourceData("UserName", UserInfoProvider.TrimSitePrefix(user.UserName));
            }

            if (setCulture && (buildFrom == null))
            {
                // Set culture to given user's preferred culture
                resolver.Culture = user.PreferredCultureCode;
            }

            return resolver;
        }


        /// <summary>
        /// Builds and returns a resolver for email templates of type 'Membership change password'
        /// </summary>
        /// <param name="user">User to be used as a data source</param>
        /// <param name="resetPasswordUrl">The URL used to reset the password</param>
        /// <param name="cancelUrl">URL allowing user to cancel the request</param>
        /// <param name="buildFrom">Resolver to build the result from</param>
        /// <param name="ipAddress">The IP address where the password reset request came from</param>
        /// <remarks>If <paramref name="buildFrom"/> is null then newly created resolver has culture set to the preferred culture of <paramref name="user"/></remarks>
        public static MacroResolver GetMembershipChangePasswordResolver(UserInfo user, string resetPasswordUrl, string cancelUrl, MacroResolver buildFrom = null, string ipAddress = null)
        {
            var resolver = GetMembershipResolverBase(user, buildFrom);
            resolver.SetNamedSourceData("ResetPasswordUrl", resetPasswordUrl);
            resolver.SetNamedSourceData("CancelUrl", cancelUrl);
            resolver.SetNamedSourceData("IP", !string.IsNullOrEmpty(ipAddress) ? ipAddress : "N/A");

            return resolver;
        }


        /// <summary>
        /// Builds and returns a resolver for email templates of type 'Membership password reset confirmation'
        /// </summary>
        /// <param name="user">User to be used as a data source</param>
        /// <param name="buildFrom">Resolver to build the result from</param>
        /// <remarks>If <paramref name="buildFrom"/> is null then newly created resolver has culture set to the preferred culture of <paramref name="user"/></remarks>
        public static MacroResolver GetMembershipPasswordResetConfirmationResolver(UserInfo user, MacroResolver buildFrom = null)
        {
            var resolver = GetMembershipResolverBase(user, buildFrom, true);

            return resolver;
        }


        /// <summary>
        /// Builds and returns a resolver for email templates of type 'Password'
        /// </summary>
        /// <param name="user">User to be used as a data source</param>
        /// <param name="password">User's password</param>
        /// <param name="buildFrom">Resolver to build the result from</param>
        /// <remarks>If <paramref name="buildFrom"/> is null then newly created resolver has culture set to the preferred culture of <paramref name="user"/></remarks>
        public static MacroResolver GetPasswordResolver(UserInfo user, string password, MacroResolver buildFrom = null)
        {
            var resolver = GetMembershipResolverBase(user, buildFrom, true);
            resolver.SetNamedSourceData("Password", password);

            return resolver;
        }


        /// <summary>
        /// Builds and returns a resolver for email templates of type 'Forgotten password'
        /// </summary>
        /// <param name="user">User to be used as a data source</param>
        /// <param name="password">User's password</param>
        /// <param name="logonUrl">URL of the logon page</param>
        /// <param name="buildFrom">Resolver to build the result from</param>
        /// <remarks>If <paramref name="buildFrom"/> is null then newly created resolver has culture set to the preferred culture of <paramref name="user"/></remarks>
        public static MacroResolver GetForgottenPasswordResolver(UserInfo user, string password, string logonUrl, MacroResolver buildFrom = null)
        {
            var resolver = GetPasswordResolver(user, password, buildFrom);
            resolver.SetNamedSourceData("LogonURL", logonUrl);

            return resolver;
        }


        /// <summary>
        /// Builds and returns a resolver for email templates of type 'Registration'
        /// </summary>
        /// <param name="user">User to be used as a data source</param>
        /// <param name="buildFrom">Resolver to build the result from</param>
        public static MacroResolver GetRegistrationResolver(UserInfo user, MacroResolver buildFrom = null)
        {
            var resolver = GetMembershipResolverBase(user, buildFrom, true, false);

            return resolver;
        }



        /// <summary>
        /// Builds and returns a resolver for email templates of type 'Registration approval'
        /// </summary>
        /// <param name="user">User to be used as a data source</param>
        /// <param name="homePageUrl">homePageUrl to be used as a data source</param>
        /// <param name="buildFrom">Resolver to build the result from</param>
        /// <remarks>If <paramref name="buildFrom"/> is null then newly created resolver has culture set to the preferred culture of <paramref name="user"/></remarks>
        public static MacroResolver GetRegistrationApprovalResolver(UserInfo user, string homePageUrl, MacroResolver buildFrom = null)
        {
            var resolver = GetMembershipResolverBase(user, buildFrom, true);
            resolver.SetNamedSourceData("HomePageURL", homePageUrl);

            return resolver;
        }


        /// <summary>
        /// Builds and returns a resolver for email templates of type 'Membership registration'
        /// </summary>
        /// <param name="user">User to be used as a data source</param>
        /// <param name="confirmaddress">Confirmation address</param>
        /// <param name="buildFrom">Resolver to build the result from</param>
        /// <remarks>If <paramref name="buildFrom"/> is null then newly created resolver has culture set to the preferred culture of <paramref name="user"/></remarks>
        public static MacroResolver GetMembershipRegistrationResolver(UserInfo user, string confirmaddress, MacroResolver buildFrom = null)
        {
            var resolver = GetMembershipResolverBase(user, buildFrom, true);
            resolver.SetNamedSourceData("ConfirmAddress", confirmaddress);

            return resolver;
        }


        /// <summary>
        /// Builds and returns a resolver for email templates of type 'Membership expiration'
        /// </summary>
        /// <param name="user">User to be used as a data source</param>
        /// <param name="membershipTable">Table of user's memberships</param>
        /// <param name="buildFrom">Resolver to build the result from</param>
        /// <remarks>If <paramref name="buildFrom"/> is null then newly created resolver has culture set to the preferred culture of <paramref name="user"/></remarks>
        public static MacroResolver GetMembershipExpirationResolver(UserInfo user, DataTable membershipTable, MacroResolver buildFrom = null)
        {
            var resolver = GetMembershipResolverBase(user, buildFrom);
            if (membershipTable == null)
            {
                membershipTable = new DataTable();
            }
            // Expiring memberships
            resolver.SetNamedSourceData("MembershipsTable", membershipTable.Rows);

            return resolver;
        }


        /// <summary>
        /// Builds and returns a resolver for email templates of type 'Account unlock'
        /// </summary>
        /// <param name="user">User to be used as a data source</param>
        /// <param name="resetPasswordUrl">URL to reset password page</param>
        /// <param name="unlockAccountUrl">URL to unlock account page</param>
        /// <param name="buildFrom">Resolver to build the result from</param>
        /// <remarks>If <paramref name="buildFrom"/> is null then newly created resolver has culture set to the preferred culture of <paramref name="user"/></remarks>
        public static MacroResolver GetMembershipUnlockAccountResolver(UserInfo user, string resetPasswordUrl, string unlockAccountUrl, MacroResolver buildFrom = null)
        {
            var resolver = GetMembershipResolverBase(user, buildFrom);
            resolver.SetNamedSourceData("ResetPasswordUrl", resetPasswordUrl);
            resolver.SetNamedSourceData("UnlockAccountUrl", unlockAccountUrl);

            return resolver;
        }
    }
}