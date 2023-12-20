using System;
using System.Data;

using CMS.Base;
using CMS.Helpers;
using CMS.Membership;

namespace CMS.UIControls
{
    /// <summary>
    /// Base page for the design pages.
    /// </summary>
    public abstract class CMSDesignPage : CMSContentPage
    {
        #region "Methods"

        /// <summary>
        /// OnPreInit event handler
        /// </summary>
        protected override void OnPreInit(EventArgs e)
        {
            UserInfo ui = MembershipContext.AuthenticatedUser;
            RequireSite = !ui.CheckPrivilegeLevel(UserPrivilegeLevelEnum.GlobalAdmin);

            base.OnPreInit(e);
        }


        /// <summary>
        /// Constructor
        /// </summary>
        protected CMSDesignPage()
        {
            Load += new EventHandler(CMSDesignPage_Load);
            PreInit += new EventHandler(CMSDesignPage_PreInit);
        }


        private void CMSDesignPage_PreInit(object sender, EventArgs e)
        {
            // Check permissions for CMS Desk -> Content -> Design
            var user = MembershipContext.AuthenticatedUser;

            // Check design permission
            if (!user.IsAuthorizedPerUIElement("CMS.Design", "Design"))
            {
                RedirectToUIElementAccessDenied("CMS.Design", "Design");
            }

            CheckAdministrationInterface();
        }


        /// <summary>
        /// Load event handler
        /// </summary>
        protected void CMSDesignPage_Load(object sender, EventArgs e)
        {
            SetRTL();
            SetBrowserClass();
            AddNoCacheTag();

            // Do not check unsaved content in the design pages
            DocumentManager.RegisterSaveChangesScript = false;

            // Check design permission
            CheckDesign("Design");
        }


        /// <summary>
        /// Checks the given permission for CMS.Design module.
        /// </summary>
        protected void CheckDesign(string permissionName)
        {
            CurrentUserInfo currentUser = CurrentUser;
            if (currentUser == null)
            {
                RedirectToAccessDenied(null);
            }
            else
            {
                // Reserve the security item
                DataRow dr = SecurityDebug.StartSecurityOperation("CheckDesign");
                bool isAllowed = true;

                // Check the design permission
                if (!currentUser.CheckPrivilegeLevel(UserPrivilegeLevelEnum.Admin))
                {
                    isAllowed = (currentUser.CheckPrivilegeLevel(UserPrivilegeLevelEnum.Editor, CurrentSiteName) && currentUser.IsAuthorizedPerResource("CMS.Design", permissionName));
                }

                // Log the security item
                if (dr != null)
                {
                    SecurityDebug.FinishSecurityOperation(dr, currentUser.UserName, null, null, isAllowed, CurrentSiteName);
                }

                // Redirect if fail
                if (!isAllowed)
                {
                    RedirectToAccessDenied("CMS.Design", permissionName);
                }
            }
        }

        #endregion
    }
}