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
    /// Base page for the CMS Message board pages to apply global settings to the pages.
    /// </summary>
    public abstract class CMSMessageBoardPage : CMSDeskPage
    {
        /// <summary>
        /// Page OnInit event.
        /// </summary>
        /// <param name="e">Event args</param>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            CheckDocPermissions = false;

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

            CurrentUserInfo user = MembershipContext.AuthenticatedUser;

            // Check permissions for CMS Desk -> Tools -> MessageBoards
            if (!user.IsAuthorizedPerUIElement("CMS.MessageBoards", "MessageBoards"))
            {
                RedirectToUIElementAccessDenied("CMS.MessageBoards", "MessageBoards");
            }

            // Check 'Read' permission
            if (!user.IsAuthorizedPerResource("CMS.MessageBoards", "Read"))
            {
                RedirectToAccessDenied("CMS.MessageBoards", "Read");
            }
        }
    }
}