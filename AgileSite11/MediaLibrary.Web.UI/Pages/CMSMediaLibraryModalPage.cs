using System;

using CMS.Membership;
using CMS.Modules;
using CMS.SiteProvider;
using CMS.UIControls;

namespace CMS.MediaLibrary.Web.UI
{
    /// <summary>
    /// Base page for the CMS Media library modal pages to apply global settings to the pages.
    /// </summary>
    public abstract class CMSMediaLibraryModalPage : CMSModalPage
    {
        /// <summary>
        /// Page OnInit event.
        /// </summary>
        /// <param name="e">Event args</param>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            // Check site availability
            if (!ResourceSiteInfoProvider.IsResourceOnSite("CMS.MediaLibrary", SiteContext.CurrentSiteName))
            {
                RedirectToResourceNotAvailableOnSite("CMS.MediaLibrary");
            }

            CurrentUserInfo user = MembershipContext.AuthenticatedUser;

            // Check permissions for CMS Desk -> Tools -> MediaLibrary
            if (!user.IsAuthorizedPerUIElement("CMS.MediaLibrary", "MediaLibrary"))
            {
                RedirectToUIElementAccessDenied("CMS.MediaLibrary", "MediaLibrary");
            }

            // Check media library 'Read' permission
            if (!user.IsAuthorizedPerResource("CMS.MediaLibrary", "Read"))
            {
                RedirectToAccessDenied("CMS.MediaLibrary", "Read");
            }
        }
    }
}