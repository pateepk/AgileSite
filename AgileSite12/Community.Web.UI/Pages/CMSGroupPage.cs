using System;

using CMS.DataEngine;
using CMS.Helpers;
using CMS.LicenseProvider;
using CMS.Membership;
using CMS.Modules;
using CMS.SiteProvider;
using CMS.UIControls;

namespace CMS.Community.Web.UI
{
    /// <summary>
    ///  Base page for the CMS Group pages to apply global settings to the pages.
    /// </summary>
    public class CMSGroupPage : CMSDeskPage
    {
        /// <summary>
        /// Page OnInit event.
        /// </summary>
        /// <param name="e">Event args</param>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            CheckDocPermissions = false;

            OnCheckGroupPermissions += CMSDeskPage_OnCheckGroupPermissions;

            // Check the license
            if (DataHelper.GetNotEmpty(RequestContext.CurrentDomain, "") != "")
            {
                LicenseHelper.CheckFeatureAndRedirect(RequestContext.CurrentDomain, FeatureEnum.Groups);
            }

            // Check site availability
            if (!ResourceSiteInfoProvider.IsResourceOnSite("CMS.Groups", SiteContext.CurrentSiteName))
            {
                RedirectToResourceNotAvailableOnSite("CMS.Groups");
            }

            CurrentUserInfo user = MembershipContext.AuthenticatedUser;

            // Check permissions for CMS Desk -> Tools -> Groups
            if (!user.IsAuthorizedPerUIElement("CMS.Groups", "Groups"))
            {
                RedirectToUIElementAccessDenied("CMS.Groups", "Groups");
            }

            // Check permissions
            if (!MembershipContext.AuthenticatedUser.IsAuthorizedPerResource("CMS.Groups", "Read"))
            {
                RedirectToAccessDenied("CMS.Groups", "Read");
            }
        }


        private bool CMSDeskPage_OnCheckGroupPermissions(CheckGroupPermissionArgs arg)
        {
            return GroupSecurityHelper.IsUserAuthorizedPerGroup(arg.User, arg.GroupId, arg.PermissionName, arg.SiteId);
        }
    }
}