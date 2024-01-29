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
    /// Base page for the Custom table modal pages.
    /// </summary>
    public abstract class CMSCustomTablesModalPage : CMSModalPage
    {
        /// <summary>
        /// Page OnInit event.
        /// </summary>
        /// <param name="e">Event args</param>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            // Check license
            if (DataHelper.GetNotEmpty(RequestContext.CurrentDomain, "") != "")
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