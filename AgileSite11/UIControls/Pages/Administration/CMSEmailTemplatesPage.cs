using System;

using CMS.Base;
using CMS.Membership;
using CMS.Modules;
using CMS.SiteProvider;

namespace CMS.UIControls
{
    /// <summary>
    /// Base page for the e-mail templates tab of administration pages to apply global settings to the pages.
    /// </summary>
    [UIElement("CMS.EmailTemplates", "EmailTemplates")]
    public abstract class CMSEmailTemplatesPage : CMSAdministrationPage
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
                    if (!ResourceSiteInfoProvider.IsResourceOnSite("CMS.EmailTemplates", SiteContext.CurrentSiteName))
                    {
                        RedirectToResourceNotAvailableOnSite("CMS.EmailTemplates");
                    }
                }

                // Check "read" permission
                if (!user.IsAuthorizedPerResource("CMS.EmailTemplates", "Read"))
                {
                    RedirectToAccessDenied("CMS.EmailTemplates", "Read");
                }
            }
        }
    }
}