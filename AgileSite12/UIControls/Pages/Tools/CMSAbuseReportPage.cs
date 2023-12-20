using System;

using CMS.Membership;
using CMS.Modules;
using CMS.SiteProvider;

namespace CMS.UIControls
{
    /// <summary>
    /// Base page for the CMS Abuse report pages to apply global settings to the pages.
    /// </summary>
    public abstract class CMSAbuseReportPage : CMSDeskPage
    {
        /// <summary>
        /// Page OnInit event.
        /// </summary>
        /// <param name="e">Event args</param>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            CheckDocPermissions = false;

            // Check site availability
            if (!ResourceSiteInfoProvider.IsResourceOnSite("CMS.AbuseReport", SiteContext.CurrentSiteName))
            {
                RedirectToResourceNotAvailableOnSite("CMS.AbuseReport");
            }

            CurrentUserInfo user = MembershipContext.AuthenticatedUser;

            // Check permissions for CMS Desk -> Tools ->Abuse report
            if (!user.IsAuthorizedPerUIElement("CMS.AbuseReport", "AbuseReport"))
            {
                RedirectToUIElementAccessDenied("CMS.AbuseReport", "AbuseReport");
            }

            // Check permissions
            if (!user.IsAuthorizedPerResource("CMS.AbuseReport", "Read"))
            {
                RedirectToAccessDenied("CMS.AbuseReport", "Read");
            }
        }
    }
}