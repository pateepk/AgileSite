using System;

using CMS.DataEngine;
using CMS.Helpers;
using CMS.LicenseProvider;
using CMS.Membership;
using CMS.Modules;
using CMS.SiteProvider;

namespace CMS.UIControls
{
    /// <summary>
    /// Base page for the Custom table pages in the tools section.
    /// </summary>
    public class CMSCustomTablesToolsPage : CMSDeskPage
    {
        /// <summary>
        /// Page OnInit event.
        /// </summary>
        /// <param name="e">Event args</param>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            CheckDocPermissions = false;

            // Check license
            if (DataHelper.GetNotEmpty(RequestContext.CurrentDomain, string.Empty) != string.Empty)
            {
                LicenseHelper.CheckFeatureAndRedirect(RequestContext.CurrentDomain, FeatureEnum.CustomTables);
            }

            // Check site availability
            if (!ResourceSiteInfoProvider.IsResourceOnSite("CMS.CustomTables", SiteContext.CurrentSiteName))
            {
                RedirectToResourceNotAvailableOnSite("CMS.CustomTables");
            }

            // Check permissions for CMS Desk -> Tools -> Custom tables
            CurrentUserInfo user = MembershipContext.AuthenticatedUser;
            if (!user.IsAuthorizedPerUIElement("CMS.CustomTables", "CustomTables"))
            {
                RedirectToUIElementAccessDenied("CMS.CustomTables", "CustomTables");
            }
        }
    }
}