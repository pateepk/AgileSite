using System;

using CMS.Modules;
using CMS.SiteProvider;
using CMS.UIControls;

namespace CMS.MediaLibrary.Web.UI
{
    /// <summary>
    /// Base page for the CMS Media library pages to apply global settings to the pages.
    /// </summary>
    [Security(Resource = "CMS.MediaLibrary", Permission = "Read", ResourceSite = true)]
    public abstract class CMSMediaLibraryPage : CMSDeskPage
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
            if (!ResourceSiteInfoProvider.IsResourceOnSite("CMS.MediaLibrary", SiteContext.CurrentSiteName))
            {
                RedirectToResourceNotAvailableOnSite("CMS.MediaLibrary");
            }
        }
    }
}