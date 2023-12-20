using System;

using CMS.Membership;
using CMS.Modules;
using CMS.SiteProvider;

namespace CMS.UIControls
{
    /// <summary>
    /// Base page for the CMS Content modal pages to apply global settings to the pages.
    /// </summary>
    public abstract class CMSContentModalPage : CMSModalPage
    {
        /// <summary>
        /// Page OnInit event.
        /// </summary>
        /// <param name="e">Event args</param>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            // Check site availability
            if (!ResourceSiteInfoProvider.IsResourceOnSite("CMS.Content", SiteContext.CurrentSiteName))
            {
                RedirectToResourceNotAvailableOnSite("CMS.Content");
            }

            // Check permissions for CMS Desk -> Content tab
            CurrentUserInfo user = MembershipContext.AuthenticatedUser;
            if (!user.IsAuthorizedPerUIElement("CMS.Content", "Content"))
            {
                RedirectToUIElementAccessDenied("CMS.Content", "Content");
            }

            // Check 'Explore tree' permission
            if (!IsUserAuthorizedPerContent())
            {
                RedirectToAccessDenied("CMS.Content", "exploretree");
            }
        }
    }
}