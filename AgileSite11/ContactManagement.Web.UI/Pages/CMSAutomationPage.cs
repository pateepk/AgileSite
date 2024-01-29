using System;

using CMS.Base;
using CMS.Core;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.LicenseProvider;
using CMS.Membership;
using CMS.Modules;
using CMS.SiteProvider;
using CMS.UIControls;

namespace CMS.ContactManagement.Web.UI
{
    /// <summary>
    /// Base contact management page.
    /// </summary>
    public class CMSAutomationPage : CMSDeskPage
    {
        #region "Properties"


        /// <summary>
        /// Indicates if current user is authorized for reading contacts.
        /// </summary>
        protected bool AuthorizedForContacts
        {
            get
            {
                return CurrentUser.IsAuthorizedPerResource(ModuleName.CONTACTMANAGEMENT, "Read");
            }
        }

        #endregion


        #region "Methods"
        
        /// <summary>
        /// Page OnInit event handler.
        /// </summary>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            LicenseHelper.CheckFeatureAndRedirect(RequestContext.CurrentDomain, FeatureEnum.MarketingAutomation);
            CheckPermissions(true);

            // Disable checking for site in site manager
            RequireSite = !CurrentUser.CheckPrivilegeLevel(UserPrivilegeLevelEnum.Admin);
        }


        /// <summary>
        /// Checks if on-line marketing is available on site.
        /// </summary>
        /// <param name="redirectOnError">If set to <c>true</c> then redirect to error permission page when not authorized</param>
        public static bool CheckPermissions(bool redirectOnError)
        {
            // Check permissions
            bool authorized = true;
            CurrentUserInfo user = MembershipContext.AuthenticatedUser;
            if (user != null)
            {
                // Check permissions for CMS Desk -> On-line Marketing -> Automation
                if (!user.IsAuthorizedPerUIElement(ModuleName.ONLINEMARKETING, new string[] { "Processes" }, SiteContext.CurrentSiteName))
                {
                    RedirectToUIElementAccessDenied(ModuleName.ONLINEMARKETING, "Processes");
                }

                if (!user.IsAuthorizedPerUIElement(ModuleName.CMS, "CMSDesk.OnlineMarketing"))
                {
                    RedirectToUIElementAccessDenied(ModuleName.CMS, "CMSDesk.OnlineMarketing");
                }

                // Check read permission for CMS Desk -> On-line Marketing -> Automation
                if (!user.IsAuthorizedPerResource(ModuleName.ONLINEMARKETING, "ReadProcesses"))
                {
                    RedirectToAccessDenied(ModuleName.ONLINEMARKETING, "ReadProcesses");
                }

                // Check site availability
                if (!user.CheckPrivilegeLevel(UserPrivilegeLevelEnum.Admin) && !ResourceSiteInfoProvider.IsResourceOnSite(ModuleName.ONLINEMARKETING, SiteContext.CurrentSiteName))
                {
                    authorized = false;
                    if (redirectOnError)
                    {
                        RedirectToAccessDenied(ModuleName.ONLINEMARKETING, String.Empty);
                    }
                }
            }
            else
            {
                authorized = false;
            }

            return authorized;
        }

        #endregion
    }
}