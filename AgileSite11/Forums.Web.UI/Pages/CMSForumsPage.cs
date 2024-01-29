using System;

using CMS.Helpers;
using CMS.LicenseProvider;
using CMS.SiteProvider;
using CMS.Membership;
using CMS.DataEngine;
using CMS.Modules;
using CMS.UIControls;

namespace CMS.Forums.Web.UI
{
    /// <summary>
    /// Summary description for ForumsPage.
    /// </summary>
    public class CMSForumsPage : CMSDeskPage
    {
        /// <summary>
        /// OnLoad override.
        /// </summary>
        /// <param name="e">Event agrs</param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            
            CheckDocPermissions = false;

            // Check the license
            if (DataHelper.GetNotEmpty(RequestContext.CurrentDomain, "") != "")
            {
                LicenseHelper.CheckFeatureAndRedirect(RequestContext.CurrentDomain, FeatureEnum.Forums);
            }

            // Check site availability
            if (!ResourceSiteInfoProvider.IsResourceOnSite("CMS.Forums", SiteContext.CurrentSiteName))
            {
                RedirectToResourceNotAvailableOnSite("CMS.Forums");
            }

            CurrentUserInfo user = MembershipContext.AuthenticatedUser;

            // Check permissions for CMS Desk -> Tools -> Forums
            if (!user.IsAuthorizedPerUIElement("CMS.Forums", "Forums"))
            {
                RedirectToUIElementAccessDenied("CMS.Forums", "Forums");
            }

            // Check 'Read' permission
            if (!user.IsAuthorizedPerResource("CMS.Forums", "Read"))
            {
                RedirectToAccessDenied("CMS.Forums", "Read");
            }
        }
    }
}