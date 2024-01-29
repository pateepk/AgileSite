using System;

using CMS.Helpers;
using CMS.LicenseProvider;
using CMS.SiteProvider;
using CMS.DataEngine;
using CMS.Modules;
using CMS.UIControls;

namespace CMS.Blogs.Web.UI
{
    /// <summary>
    /// Base page for the CMS Blogs pages to apply global settings to the pages.
    /// </summary>
    [Security(Resource = "CMS.Blog", Permission = "Read", ResourceSite = true)]
    public abstract class CMSBlogsPage : CMSDeskPage
    {
        /// <summary>
        /// Page OnInit event.
        /// </summary>
        /// <param name="e">Event args</param>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            CheckDocPermissions = false;

            CheckLicence();

            CheckSiteAvailability();
        }


        private static void CheckSiteAvailability()
        {
            if (!ResourceSiteInfoProvider.IsResourceOnSite("CMS.Blog", SiteContext.CurrentSiteName))
            {
                RedirectToResourceNotAvailableOnSite("CMS.Blog");
            }
        }


        private static void CheckLicence()
        {
            if (DataHelper.GetNotEmpty(RequestContext.CurrentDomain, "") != "")
            {
                LicenseHelper.CheckFeatureAndRedirect(RequestContext.CurrentDomain, FeatureEnum.Blogs);
            }
        }
    }
}