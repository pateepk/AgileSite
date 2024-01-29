using System;

using CMS.Base;
using CMS.Core;
using CMS.Membership;
using CMS.Modules;
using CMS.SiteProvider;

namespace CMS.UIControls
{
    /// <summary>
    /// Base page for the UI personalization tab of administration pages to apply global settings to the pages.
    /// </summary>
    public abstract class CMSUIPersonalizationPage : CMSAdministrationPage
    {
        /// <summary>
        /// Page PreRender.
        /// </summary>
        protected override void OnPreRender(EventArgs e)
        {
            var uiElement = new UIElementAttribute(ModuleName.UIPERSONALIZATION, "UIPersonalization");
            uiElement.Check(this);

            base.OnPreRender(e);
        }


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
                    if (!ResourceSiteInfoProvider.IsResourceOnSite("CMS.UIPersonalization", SiteContext.CurrentSiteName))
                    {
                        RedirectToResourceNotAvailableOnSite("CMS.UIPersonalization");
                    }
                }

                // Check "read" permission
                if (!user.IsAuthorizedPerResource("CMS.UIPersonalization", "Read"))
                {
                    RedirectToAccessDenied("CMS.UIPersonalization", "Read");
                }
            }
        }
    }
}