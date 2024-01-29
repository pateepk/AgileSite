using System;

using CMS.Helpers;
using CMS.Membership;
using CMS.Modules;
using CMS.SiteProvider;

namespace CMS.UIControls
{
    /// <summary>
    /// Base page for the content pages to apply global settings to the pages.
    /// </summary>
    public abstract class CMSContentPage : CMSDeskPage
    {
        #region "Variables"

        private bool mUseDialogContentClass = true;

        #endregion


        #region "Constants"

        /// <summary>
        /// View tab.
        /// </summary>
        protected const int TAB_VIEW = 0;

        /// <summary>
        /// Validate tab.
        /// </summary>
        protected const int TAB_VALIDATE = 1;

        #endregion


        #region "Properties"

        /// <summary>
        /// Indicates whether dialog content class should be added to the content panel in dialog mode
        /// </summary>
        public bool UseDialogContentClass
        {
            get
            {
                return mUseDialogContentClass;
            }
            set
            {
                mUseDialogContentClass = value;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// OnPreInit
        /// </summary>
        protected override void OnPreInit(EventArgs e)
        {
            EnsureDocumentManager = true;

            base.OnPreInit(e);
        }


        /// <summary>
        /// OnInit
        /// </summary>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            // Check site availability
            if (!ResourceSiteInfoProvider.IsResourceOnSite("CMS.Content", SiteContext.CurrentSiteName))
            {
                RedirectToResourceNotAvailableOnSite("CMS.Content");
            }

            CheckSecurity();
        }


        /// <summary>
        /// Checks the security.
        /// </summary>
        public static void CheckSecurity()
        {
            CurrentUserInfo user = MembershipContext.AuthenticatedUser;

            // Check permissions for CMS Desk -> Content tab
            if (!user.IsAuthorizedPerUIElement("CMS.Content", "Content"))
            {
                RedirectToUIElementAccessDenied("CMS.Content", "Content");
            }

            // Check 'Explore tree' permission
            if (!IsUserAuthorizedPerContent())
            {
                RedirectToAccessDenied("CMS.Content", "exploretree");
            }
        }


        /// <summary>
        /// Gets current view tab.
        /// </summary>
        protected int GetViewTab()
        {
            return ValidationHelper.GetInteger(ContextHelper.GetItem(CookieName.ViewTab, true, false, true), TAB_VIEW);
        }


        /// <summary>
        /// Sets current view tab.
        /// </summary>
        /// <param name="value">Value to store</param>
        protected void SetViewTab(int value)
        {
            ContextHelper.Add(CookieName.ViewTab, value, true, false, true, DateTime.Now.AddDays(1));
        }

        #endregion
    }
}
