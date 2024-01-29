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
    /// Base page for the CMS Polls pages to apply global settings to the pages.
    /// </summary>
    public class CMSPollsPage : CMSDeskPage
    {
        /// <summary>
        /// Indicates if current user is authorized for reading global polls.
        /// </summary>
        protected bool AuthorizedForGlobalPolls
        {
            get;
            private set;
        }


        /// <summary>
        /// Indicates if current user is authorized for reading site polls.
        /// </summary>
        protected bool AuthorizedForSitePolls
        {
            get;
            private set;
        }


        /// <summary>
        /// Page OnInit event.
        /// </summary>
        /// <param name="e">Event args</param>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            CheckDocPermissions = false;
            
            // Check license
            if (DataHelper.GetNotEmpty(RequestContext.CurrentDomain, "") != "")
            {
                LicenseHelper.CheckFeatureAndRedirect(RequestContext.CurrentDomain, FeatureEnum.Polls);
            }

            // Check site availability
            if (!ResourceSiteInfoProvider.IsResourceOnSite("CMS.Polls", SiteContext.CurrentSiteName))
            {
                RedirectToResourceNotAvailableOnSite("CMS.Polls");
            }

            CurrentUserInfo user = MembershipContext.AuthenticatedUser;

            // Check permissions for CMS Desk -> Tools -> Polls
            if (!user.IsAuthorizedPerUIElement("CMS.Polls", "Polls"))
            {
                RedirectToUIElementAccessDenied("CMS.Polls", "Polls");
            }

            // Check permissions for global and site polls
            AuthorizedForSitePolls = user.IsAuthorizedPerResource("CMS.Polls", CMSAdminControl.PERMISSION_READ);
            AuthorizedForGlobalPolls = user.IsAuthorizedPerResource("CMS.Polls", CMSAdminControl.PERMISSION_GLOBALREAD);

            // Check if global polls should be displayed on this site
            AuthorizedForGlobalPolls &= SettingsKeyInfoProvider.GetBoolValue(SiteContext.CurrentSiteName + ".CMSPollsAllowGlobal");

            if (!AuthorizedForGlobalPolls && !AuthorizedForSitePolls)
            {
                RedirectToAccessDenied("CMS.Polls", "Read");
            }
        }


        /// <summary>
        /// Checks "read" permission of site/global poll for current user.
        /// </summary>
        /// <param name="siteId">Poll site ID</param>
        public void CheckPollsReadPermission(int siteId)
        {
            // Check if site poll belongs to current site
            if ((siteId > 0) && (siteId != SiteContext.CurrentSiteID))
            {
                EditedObject = null;
            }

            // Check "read" permission for global poll
            if (siteId <= 0 && !AuthorizedForGlobalPolls)
            {
                RedirectToAccessDenied("CMS.Polls", CMSAdminControl.PERMISSION_GLOBALREAD);
            }

            // Check "read" permission for site poll
            if (siteId > 0 && !AuthorizedForSitePolls)
            {
                RedirectToAccessDenied("CMS.Polls", CMSAdminControl.PERMISSION_READ);
            }
        }


        /// <summary>
        /// Checks "modify" permission of site/global poll for current user.
        /// </summary>
        /// <param name="siteId">Poll site ID</param>
        public bool CheckPollsModifyPermission(int siteId)
        {
            return CheckPollsModifyPermission(siteId, true);
        }


        /// <summary>
        /// Checks "modify" permission of site/global poll for current user.
        /// </summary>
        /// <param name="siteId">Poll site ID</param>
        /// <param name="autoRedirect">Indicates if page should be automatically redirected to access denied page if permission check fails.</param>
        public bool CheckPollsModifyPermission(int siteId, bool autoRedirect)
        {
            // Check "modify" permission for global poll
            if (siteId <= 0 && !MembershipContext.AuthenticatedUser.IsAuthorizedPerResource("CMS.Polls", CMSAdminControl.PERMISSION_GLOBALMODIFY))
            {
                if (autoRedirect)
                {
                    RedirectToAccessDenied("CMS.Polls", CMSAdminControl.PERMISSION_GLOBALMODIFY);
                }
                return false;
            }

            // Check "modify" permission for site poll
            if (siteId > 0 && !MembershipContext.AuthenticatedUser.IsAuthorizedPerResource("CMS.Polls", CMSAdminControl.PERMISSION_MODIFY))
            {
                if (autoRedirect)
                {
                    RedirectToAccessDenied("CMS.Polls", CMSAdminControl.PERMISSION_MODIFY);
                }
                return false;
            }
            return true;
        }
    }
}