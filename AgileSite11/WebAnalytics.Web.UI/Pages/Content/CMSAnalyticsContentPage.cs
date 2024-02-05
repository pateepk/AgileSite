using System;

using CMS.Core;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.LicenseProvider;
using CMS.Membership;
using CMS.Modules;
using CMS.SiteProvider;
using CMS.UIControls;

namespace CMS.WebAnalytics.Web.UI
{
    /// <summary>
    /// Base page for the analytics pages.
    /// </summary>
    public abstract class CMSAnalyticsContentPage : CMSContentPage
    {
        /// <summary>
        /// Init event handler
        /// </summary>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            // Check permissions for CMS Desk -> Content -> Analytics tab
            CurrentUserInfo user = MembershipContext.AuthenticatedUser;
            if (!user.IsAuthorizedPerUIElement("CMS.Content", "Analytics"))
            {
                RedirectToUIElementAccessDenied("CMS.Content", "Analytics");
            }

            // Test reporting module
            if (!ModuleManager.IsModuleLoaded(ModuleName.REPORTING))
            {
                RedirectToInformation("analytics.noreporting");
            }

            if (!ResourceSiteInfoProvider.IsResourceOnSite(ModuleName.REPORTING, SiteContext.CurrentSiteName))
            {
                RedirectToAccessDeniedResourceNotAvailableOnSite("CMS.Reporting");
            }

            // Check license
            if (DataHelper.GetNotEmpty(RequestContext.CurrentDomain, "") != "")
            {
                LicenseHelper.CheckFeatureAndRedirect(RequestContext.CurrentDomain, FeatureEnum.WebAnalytics);
            }

            // Check site availability
            if (!ResourceSiteInfoProvider.IsResourceOnSite("CMS.WebAnalytics", SiteContext.CurrentSiteName))
            {
                RedirectToAccessDeniedResourceNotAvailableOnSite("CMS.WebAnalytics");
            }
        }
    }
}