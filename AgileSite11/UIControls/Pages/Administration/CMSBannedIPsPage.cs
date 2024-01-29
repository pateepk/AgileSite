using System;

using CMS.Base;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.LicenseProvider;
using CMS.Membership;
using CMS.Modules;
using CMS.SiteProvider;

namespace CMS.UIControls
{
    /// <summary>
    /// Base page for the banned IPs tab of administration pages to apply global settings to the pages.
    /// </summary>
    [UIElement("CMS.BannedIP", "BannedIP")]
    public abstract class CMSBannedIPsPage : CMSAdministrationPage
    {
        /// <summary>
        /// Init event handler
        /// </summary>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            // Check license
            LicenseHelper.CheckFeatureAndRedirect(RequestContext.CurrentDomain, FeatureEnum.BannedIP);

            CurrentUserInfo user = MembershipContext.AuthenticatedUser;
            if (user != null)
            {
                // Check site availability
                if (!user.CheckPrivilegeLevel(UserPrivilegeLevelEnum.Admin))
                {
                    if (!ResourceSiteInfoProvider.IsResourceOnSite("CMS.BannedIP", SiteContext.CurrentSiteName))
                    {
                        RedirectToResourceNotAvailableOnSite("CMS.BannedIP");
                    }
                }

                // Check "read" permission
                if (!user.IsAuthorizedPerResource("CMS.BannedIP", "Read"))
                {
                    RedirectToAccessDenied("CMS.BannedIP", "Read");
                }
            }
        }


        /// <summary>
        /// Check specifies permission.
        /// </summary>
        /// <param name="permissionName">Name of permission</param>
        protected void CheckPermissions(string permissionName)
        {
            //Check permissions
            if (SiteID == 0)
            {
                // Must be global admin for manage all bannedip
                CheckGlobalAdministrator();
            }
            // Check 'Read' permission for specified site
            else if (!MembershipContext.AuthenticatedUser.IsAuthorizedPerResource("cms.bannedip", permissionName, SiteName))
            {
                RedirectToAccessDenied("cms.bannedip", permissionName);
            }
        }
    }
}