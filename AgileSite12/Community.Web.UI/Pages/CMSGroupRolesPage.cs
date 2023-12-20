using System;

using CMS.Modules;
using CMS.SiteProvider;

namespace CMS.Community.Web.UI
{
    /// <summary>
    /// Base page for roles under groups pages.
    /// </summary>
    public class CMSGroupRolesPage : CMSGroupPage
    {
        /// <summary>
        /// Page OnInit event.
        /// </summary>
        /// <param name="e">Event args</param>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            // Check site availability
            if (!ResourceSiteInfoProvider.IsResourceOnSite("CMS.Roles", SiteContext.CurrentSiteName))
            {
                RedirectToResourceNotAvailableOnSite("CMS.Roles");
            }
        }
    }
}