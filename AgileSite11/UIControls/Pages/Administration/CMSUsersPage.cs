using System;

using CMS.Base;
using CMS.Membership;
using CMS.Modules;
using CMS.SiteProvider;

namespace CMS.UIControls
{
    /// <summary>
    /// Base page for the users tab of administration pages to apply global settings to the pages.
    /// </summary>
    public abstract class CMSUsersPage : CMSAdministrationPage
    {
        #region "Events"

        /// <summary>
        /// Init event handler
        /// </summary>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            CheckResources();
            CheckPermissions();
            CheckUIPermissions();
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Checks resources.
        /// </summary>
        protected virtual void CheckResources()
        {
            CurrentUserInfo user = MembershipContext.AuthenticatedUser;
            if (user != null)
            {
                // Check site availability
                if (!user.CheckPrivilegeLevel(UserPrivilegeLevelEnum.Admin))
                {
                    if (!ResourceSiteInfoProvider.IsResourceOnSite("CMS.Users", SiteContext.CurrentSiteName))
                    {
                        RedirectToResourceNotAvailableOnSite("CMS.Users");
                    }
                }
            }
        }


        /// <summary>
        /// Checks UI permissions.
        /// </summary>
        protected virtual void CheckUIPermissions()
        {
            CheckUIElementAccessHierarchical("CMS.Users", "Users");
        }


        /// <summary>
        /// Checks permissions.
        /// </summary>
        protected virtual void CheckPermissions()
        {
            CurrentUserInfo user = MembershipContext.AuthenticatedUser;
            if (user != null)
            {
                // Check "read" permission
                if (!user.IsAuthorizedPerResource("CMS.Users", "Read"))
                {
                    RedirectToAccessDenied("CMS.Users", "Read");
                }
            }
        }


        /// <summary>
        /// Returns false if edited user is global admin and current user can't edit admin's account.
        /// </summary>
        /// <param name="editedUser">Edited user info</param>
        protected bool CheckGlobalAdminEdit(UserInfo editedUser)
        {
            var currentUser = MembershipContext.AuthenticatedUser;
            if (!currentUser.CheckPrivilegeLevel(UserPrivilegeLevelEnum.GlobalAdmin) 
                && (editedUser != null) && editedUser.CheckPrivilegeLevel(UserPrivilegeLevelEnum.Admin)
                && (editedUser.UserID != currentUser.UserID))
            {
                return false;
            }

            return true;
        }


        /// <summary>
        /// Test if edited user belongs to current site
        /// </summary>
        /// <param name="ui">User info object</param>
        public void CheckUserAvaibleOnSite(UserInfo ui)
        {
            if (ui != null)
            {
                if (!MembershipContext.AuthenticatedUser.CheckPrivilegeLevel(UserPrivilegeLevelEnum.Admin) && !ui.IsInSite(SiteContext.CurrentSiteName))
                {
                    RedirectToInformation(GetString("user.notinsite"));
                }
            }
        }

        #endregion
    }
}