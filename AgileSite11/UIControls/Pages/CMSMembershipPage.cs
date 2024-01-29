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
    /// Base class for UI pages of membership.
    /// </summary>
    [UIElement("CMS.Membership", "Membership")]
    public class CMSMembershipPage : CMSAdministrationPage
    {
        /// <summary>
        /// Init event handler
        /// </summary>
        protected override void OnInit(EventArgs e)
        {
            // Check the license
            if (DataHelper.GetNotEmpty(RequestContext.CurrentDomain, "") != "")
            {
                LicenseHelper.CheckFeatureAndRedirect(RequestContext.CurrentDomain, FeatureEnum.Membership);
            }

            // Check module availability on site
            if (!ResourceSiteInfoProvider.IsResourceOnSite("CMS.Membership", SiteContext.CurrentSiteName))
            {
                RedirectToResourceNotAvailableOnSite("CMS.Membership");
            }

            CurrentUserInfo user = MembershipContext.AuthenticatedUser;

            // Check module 'Read' permission
            if (!user.IsAuthorizedPerResource("CMS.Membership", "Read"))
            {
                RedirectToAccessDenied("CMS.Membership", "Read");
            }

            base.OnInit(e);
        }


        /// <summary>
        /// Checks permission for given membership info object.
        /// </summary>
        /// <param name="mi">Membership info object.</param>
        public void CheckMembershipPermissions(MembershipInfo mi)
        {
            // Test security
            if ((mi != null) && !MembershipContext.AuthenticatedUser.CheckPrivilegeLevel(UserPrivilegeLevelEnum.GlobalAdmin))
            {
                // For global admin without access to site manager site must be positive number
                if (MembershipContext.AuthenticatedUser.CheckPrivilegeLevel(UserPrivilegeLevelEnum.Admin))
                {
                    if (SiteID <= 0)
                    {
                        RedirectToAccessDenied(GetString("general.actiondenied"));
                    }
                }
                else
                {
                    if (mi.MembershipSiteID != SiteContext.CurrentSiteID)
                    {
                        RedirectToAccessDenied(GetString("general.actiondenied"));
                    }
                }
            }
        }
    }
}