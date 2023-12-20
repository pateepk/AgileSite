using System;

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
        #region "Properties"

        /// <summary>
        /// Indicates whether dialog content class should be added to the content panel in dialog mode
        /// </summary>
        [Obsolete("Not used anymore, will be removed in the next major version.")]
        public bool UseDialogContentClass
        {
            get;
            set;
        } = true;

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

        #endregion
    }
}
