using CMS.Base;
using CMS.Core;
using CMS.Membership;
using CMS.UIControls;

namespace CMS.ContactManagement.Web.UI
{
    /// <summary>
    /// Helps authorize current user to read or modify resources contained in module CMS.ContactManagement.
    /// </summary>
    public static class AuthorizationHelper
    {
        #region "Public methods"

        /// <summary>
        /// Indicates if current user is authorized for contacts, contact groups and accounts.
        /// </summary>
        /// <param name="redirectIfNotAuthorized">Indicates if redirect should be done when not enough permissions</param>
        public static bool AuthorizedReadContact(bool redirectIfNotAuthorized)
        {
            return IsAuthorizedForContactManagement("Read", redirectIfNotAuthorized);
        }


        /// <summary>
        /// Returns <c>true</c> if current user is authorized to modify contact, contact groups and accounts.
        /// </summary>
        /// <param name="redirectIfNotAuthorized">Indicates if redirect should be done when not enough permissions</param>
        public static bool AuthorizedModifyContact(bool redirectIfNotAuthorized)
        {
            return IsAuthorizedForContactManagement("Modify", redirectIfNotAuthorized);
        }


        /// <summary>
        /// Indicates if current user is authorized for configuration.
        /// </summary>
        /// <param name="redirectIfNotAuthorized">Indicates if redirect should be done when not enough permissions</param>
        public static bool AuthorizedReadConfiguration(bool redirectIfNotAuthorized)
        {
            return IsAuthorizedForContactManagement("ReadConfiguration", redirectIfNotAuthorized);
        }


        /// <summary>
        /// Returns <c>true</c> if current user is authorized to modify configuration.
        /// </summary>
        /// <param name="redirectIfNotAuthorized">Indicates if redirect should be done when not enough permissions</param>
        public static bool AuthorizedModifyConfiguration(bool redirectIfNotAuthorized)
        {
            return IsAuthorizedForContactManagement("ModifyConfiguration", redirectIfNotAuthorized);
        }

        #endregion


        #region "Private methods"
        
        /// <summary>
        /// Redirects user to access denied page, if <paramref name="redirectIfNotAuthorized"/> is true and user is not <paramref name="authorized"/>.
        /// </summary>
        /// <param name="permissionName">Name of the permission that was not granted to the current user.</param>
        /// <param name="redirectIfNotAuthorized">Indicates if user will be redirected to access denied page.</param>
        /// <param name="authorized">Indicates if user was granted with <paramref name="permissionName"/>.</param>
        private static void RedirectIfNotAuthorized(string permissionName, bool redirectIfNotAuthorized, bool authorized)
        {
            if (redirectIfNotAuthorized && !authorized)
            {
                CMSPage.RedirectToAccessDenied(ModuleName.CONTACTMANAGEMENT, permissionName);
            }
        }


        /// <summary>
        /// Indicates if user is granted with <paramref name="permissionName"/> in module <see cref="ModuleName.CONTACTMANAGEMENT"/>.
        /// If not and <paramref name="redirectIfNotAuthorized"/> is true, user will be redirected to access denied page.
        /// </summary>
        /// <param name="permissionName">Name of the permission that is checked for current user.</param>
        /// <param name="redirectIfNotAuthorized">Indicates if user will be redirected to access denied page.</param>
        private static bool IsAuthorizedForContactManagement(string permissionName, bool redirectIfNotAuthorized)
        {
            var user = MembershipContext.AuthenticatedUser;

            // Allow access to global administrator
            if (user.CheckPrivilegeLevel(UserPrivilegeLevelEnum.GlobalAdmin))
            {
                return true;
            }

            // Allow global objects only for authorized users
            bool authorized = user.IsAuthorizedPerResource(ModuleName.CONTACTMANAGEMENT, permissionName);
            RedirectIfNotAuthorized(permissionName, redirectIfNotAuthorized, authorized);

            return authorized;
        }

        #endregion
    }
}
