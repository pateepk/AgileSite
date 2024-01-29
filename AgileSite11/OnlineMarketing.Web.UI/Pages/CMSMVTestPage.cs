using System;

using CMS.DataEngine;
using CMS.Helpers;
using CMS.LicenseProvider;
using CMS.Membership;
using CMS.Modules;
using CMS.SiteProvider;
using CMS.UIControls;

namespace CMS.OnlineMarketing.Web.UI
{
    /// <summary>
    /// Base class for UI pages of MVT test.
    /// </summary>
    public class CMSMVTestPage : CMSDeskPage
    {
        /// <summary>
        /// Load event handler
        /// </summary>
        protected override void OnLoad(EventArgs e)
        {
            // Check license
            if (DataHelper.GetNotEmpty(RequestContext.CurrentDomain, "") != "")
            {
                LicenseHelper.CheckFeatureAndRedirect(RequestContext.CurrentDomain, FeatureEnum.MVTesting);
            }

            // Check module availability on site
            if (!ResourceSiteInfoProvider.IsResourceOnSite("cms.mvtest", SiteContext.CurrentSiteName))
            {
                RedirectToResourceNotAvailableOnSite("CMS.MVTest");
            }

            CurrentUserInfo user = MembershipContext.AuthenticatedUser;
            // Check module 'Read' permission
            if (!user.IsAuthorizedPerResource("cms.mvtest", "Read"))
            {
                RedirectToAccessDenied("cms.MVTest", "Read");
            }

            int MVTestID = QueryHelper.GetInteger("objectID", 0);
            if (MVTestID != 0)
            {
                int siteID = ValidationHelper.GetInteger(ModuleCommands.OnlineMarketingGetMVTestSiteID(MVTestID), 0);
                if (siteID != SiteContext.CurrentSiteID)
                {
                    RedirectToAccessDenied(GetString("cmsmessages.accessdenied"));
                }
            }

            base.OnLoad(e);
        }
    }
}