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
    /// Base page for content personalization section.
    /// </summary>
    public abstract class CMSContentPersonalizationContentPage : CMSPropertiesPage
    {
        /// <summary>
        /// OnInit() event.
        /// </summary>
        /// <param name="e">Arguments.</param>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            if (DataHelper.GetNotEmpty(RequestContext.CurrentDomain, "") != "")
            {
                LicenseHelper.CheckFeatureAndRedirect(RequestContext.CurrentDomain, FeatureEnum.ContentPersonalization);
            }

            // Check module availability on site
            if (!ResourceSiteInfoProvider.IsResourceOnSite("cms.contentpersonalization", SiteContext.CurrentSiteName))
            {
                RedirectToResourceNotAvailableOnSite("CMS.ContentPersonalization");
            }

            CurrentUserInfo cui = MembershipContext.AuthenticatedUser;
            if ((cui == null) || !cui.IsAuthorizedPerResource("cms.contentpersonalization", "Read"))
            {
                RedirectToAccessDenied(String.Format(GetString("general.permissionresource"), "Read", "Content personalization"));
            }

            // Check UI Permissions
            if (!cui.IsAuthorizedPerUIElement("CMS.Content", "Properties.Variants"))
            {
                RedirectToUIElementAccessDenied("CMS.Content", "Properties.Variants");
            }
        }
    }
}