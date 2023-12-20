using System;

using CMS.DataEngine;
using CMS.Helpers;
using CMS.LicenseProvider;
using CMS.Membership;
using CMS.Modules;
using CMS.SiteProvider;
using CMS.UIControls;

namespace CMS.Synchronization.Web.UI
{
    /// <summary>
    /// Base page for the Content staging pages.
    /// </summary>
    public abstract class CMSStagingPage : CMSDeskPage
    {
        /// <summary>
        /// Page OnInit event.
        /// </summary>
        /// <param name="e">Event args</param>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            if (DataHelper.GetNotEmpty(RequestContext.CurrentDomain, "") != "")
            {
                LicenseHelper.CheckFeatureAndRedirect(RequestContext.CurrentDomain, FeatureEnum.Staging);
            }

            // Check site availability
            if (!ResourceSiteInfoProvider.IsResourceOnSite("CMS.Staging", SiteContext.CurrentSiteName))
            {
                RedirectToResourceNotAvailableOnSite("CMS.Staging");
            }

            // Check permissions for CMS Desk -> Tools -> Staging
            CurrentUserInfo user = MembershipContext.AuthenticatedUser;
            if (!user.IsAuthorizedPerUIElement("CMS.Staging", "Staging"))
            {
                RedirectToUIElementAccessDenied("CMS.Staging", "Staging");
            }
        }
    }
}