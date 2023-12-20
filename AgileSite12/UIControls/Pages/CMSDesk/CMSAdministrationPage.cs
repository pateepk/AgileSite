using System;

using CMS.Base;
using CMS.Helpers;
using CMS.Membership;
using CMS.SiteProvider;

namespace CMS.UIControls
{
    /// <summary>
    /// Base page for the Administration section to apply global settings to the pages.
    /// </summary>
    public abstract class CMSAdministrationPage : CMSPage
    {
        #region "Variables"

        private int mSiteID = -1;
        private int mSelectedSiteID = -2;
        private string mSiteName;

        #endregion


        #region "Properties"

        /// <summary>
        /// Returns the site name for current user, based on SiteID.
        /// </summary>
        public virtual string SiteName
        {
            get
            {
                return mSiteName ?? (mSiteName = SiteInfoProvider.GetSiteName(SiteID));
            }
        }


        /// <summary>
        /// Returns correct site id for current user.
        /// </summary>
        public virtual int SiteID
        {
            get
            {
                if (mSiteID == -1)
                {
                    mSiteID = GetSiteID(QueryHelper.GetString("siteid", string.Empty));
                }

                return mSiteID;
            }
            set
            {
                mSiteID = value;
            }
        }


        /// <summary>
        /// Returns correct selected site id for current user.
        /// </summary>
        public virtual int SelectedSiteID
        {
            get
            {
                if (mSelectedSiteID == -2)
                {
                    mSelectedSiteID = GetSiteID(QueryHelper.GetString("selectedsiteid", string.Empty));
                }

                return mSelectedSiteID;
            }
            set
            {
                mSelectedSiteID = value;
            }
        }

        #endregion


        /// <summary>
        /// Constructor
        /// </summary>
        public CMSAdministrationPage()
        {
            Load += BasePage_Load;
            PreInit += CMSAdministrationPage_PreInit;
        }


        /// <summary>
        /// PreInit event handler
        /// </summary>
        private void CMSAdministrationPage_PreInit(object sender, EventArgs e)
        {
            CheckAdministrationInterface();
        }


        /// <summary>
        /// Load event handler
        /// </summary>
        protected void BasePage_Load(object sender, EventArgs e)
        {
            CheckGlobalAdministrator();

            RedirectToSecured();
            SetRTL();
            SetBrowserClass();
            AddNoCacheTag();
        }


        /// <summary>
        /// Check whether current page is displayed for specific site 
        /// and if not check whether global app is not disable for global admin
        /// </summary>
        protected new void CheckGlobalAdministrator()
        {
            var currentUser = MembershipContext.AuthenticatedUser;

            // Check if site id is presented in query string.
            if ((SiteID <= 0) && !currentUser.CheckPrivilegeLevel(UserPrivilegeLevelEnum.GlobalAdmin))
            {
                RedirectToAccessDenied(ResHelper.GetString("accessdeniedtopage.globaladminrequired"));
            }
        }


        /// <summary>
        /// Gets valid site id for current user.
        /// </summary>
        /// <param name="queryStringSiteId">Site id from querystring</param>
        public int GetSiteID(string queryStringSiteId)
        {
            // Get site id from querystring
            int siteId = ValidationHelper.GetInteger(queryStringSiteId, Int32.MinValue);

            // Global administrator can edit everything
            if (MembershipContext.AuthenticatedUser.CheckPrivilegeLevel(UserPrivilegeLevelEnum.GlobalAdmin))
            {
                // There is site id in the querystring
                if (siteId != Int32.MinValue)
                {
                    return siteId;
                }
                return 0;
            }

            // Editor can edit only current site
            if (MembershipContext.AuthenticatedUser.CheckPrivilegeLevel(UserPrivilegeLevelEnum.Editor, SiteContext.CurrentSiteName))
            {
                if (SiteContext.CurrentSite != null)
                {
                    return SiteContext.CurrentSiteID;
                }
            }

            return -1;
        }
    }
}