using System;

using CMS.Helpers;
using CMS.LicenseProvider;
using CMS.SiteProvider;
using CMS.Membership;
using CMS.DataEngine;
using CMS.Modules;
using CMS.UIControls;

namespace CMS.MessageBoards.Web.UI
{
    /// <summary>
    /// Base page for the CMS Content Message Boards modal pages to apply global settings to the pages.
    /// </summary>
    public abstract class CMSContentMessageBoardsPage : CMSContentModalPage
    {
        /// <summary>
        /// Page OnInit event.
        /// </summary>
        /// <param name="e">Event args</param>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            // Check license for message boards
            if (DataHelper.GetNotEmpty(RequestContext.CurrentDomain, "") != "")
            {
                LicenseHelper.CheckFeatureAndRedirect(RequestContext.CurrentDomain, FeatureEnum.MessageBoards);
            }

            // Check site availability
            if (!ResourceSiteInfoProvider.IsResourceOnSite("CMS.MessageBoards", SiteContext.CurrentSiteName))
            {
                RedirectToResourceNotAvailableOnSite("CMS.MessageBoards");
            }

            // Check 'Read' permission
            if (!MembershipContext.AuthenticatedUser.IsAuthorizedPerResource("CMS.MessageBoards", "Read"))
            {
                RedirectToAccessDenied("CMS.MessageBoards", "Read");
            }
        }
    }
}