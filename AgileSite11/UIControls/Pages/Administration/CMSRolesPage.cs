using System;

using CMS.Base;
using CMS.Membership;
using CMS.Modules;
using CMS.SiteProvider;

namespace CMS.UIControls
{
    /// <summary>
    /// Base page for the roles tab of administration pages to apply global settings to the pages.
    /// </summary>
    [UIElement("CMS.Roles", "Roles")]
    public abstract class CMSRolesPage : CMSAdministrationPage
    {
        /// <summary>
        /// Init event handler
        /// </summary>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            CurrentUserInfo user = MembershipContext.AuthenticatedUser;
            if (user != null)
            {
                // Check site availability
                if (!user.CheckPrivilegeLevel(UserPrivilegeLevelEnum.Admin))
                {
                    if (!ResourceSiteInfoProvider.IsResourceOnSite("CMS.Roles", SiteContext.CurrentSiteName))
                    {
                        RedirectToResourceNotAvailableOnSite("CMS.Roles");
                    }
                }

                // Check "read" permission
                if (!user.IsAuthorizedPerResource("CMS.Roles", "Read"))
                {
                    RedirectToAccessDenied("CMS.Roles", "Read");
                }
            }
        }
    }
}