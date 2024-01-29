using CMS.Base;
using CMS.Core;
using CMS.Membership;
using CMS.SiteProvider;
using CMS.UIControls;


namespace CMS.Activities.Web.UI
{
    /// <summary>
    /// Helps authorize current user to read or modify resources contained in the CMS.Activities module.
    /// </summary>
    public static class AuthorizationHelper
    {
        /// <summary>
        /// Indicates if current user is authorized for activities.
        /// </summary>
        /// <param name="siteID">SiteID of an activity</param>
        /// <param name="redirectIfNotAuthorized">Indicates if redirect should be done when not enough permissions</param>
        public static bool AuthorizedReadActivity(int siteID, bool redirectIfNotAuthorized)
        {
            return AuthorizedForObject(siteID, "ReadActivities", redirectIfNotAuthorized);
        }


        /// <summary>
        /// Returns <c>true</c> if current user is authorized to manage activities.
        /// </summary>
        /// <param name="siteID">SiteID of an activity</param>
        /// <param name="redirectIfNotAuthorized">Indicates if redirect should be done when not enough permissions</param>
        public static bool AuthorizedManageActivity(int siteID, bool redirectIfNotAuthorized)
        {
            return AuthorizedForObject(siteID, "ManageActivities", redirectIfNotAuthorized);
        }


        /// <summary>
        /// Returns <c>true</c> if user is authorized per resource.
        /// </summary>
        /// <param name="siteID">Object's site ID</param>
        /// <param name="permissionSite">Name of the permission for site objects</param>
        /// <param name="redirectIfNotAuthorized">Indicates if redirect should be done when not enough permissions</param>
        /// <returns>Returns <c>true</c> if user is authorized for the object</returns>
        private static bool AuthorizedForObject(int siteID, string permissionSite, bool redirectIfNotAuthorized)
        {
            var user = MembershipContext.AuthenticatedUser;

            // Allow access to global administrator
            if (user.CheckPrivilegeLevel(UserPrivilegeLevelEnum.GlobalAdmin))
            {
                return true;
            }

            var authorized = false;

            // Check site permission
            if (siteID > 0)
            {
                authorized = user.IsAuthorizedPerResource(ModuleName.ACTIVITIES, permissionSite, SiteInfoProvider.GetSiteName(siteID));
            }
            else
            {
                authorized = user.IsAuthorizedPerResource(ModuleName.ACTIVITIES, permissionSite);
            }

            // Redirect when requested
            if (redirectIfNotAuthorized && !authorized)
            {
                CMSPage.RedirectToAccessDenied(ModuleName.ACTIVITIES, permissionSite);
            }

            return authorized;
        }
    }
}
