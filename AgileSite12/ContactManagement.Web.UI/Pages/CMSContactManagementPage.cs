using System;

using CMS.Base;
using CMS.Core;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.SiteProvider;
using CMS.UIControls;

namespace CMS.ContactManagement.Web.UI
{
    /// <summary>
    /// Base contact management page.
    /// </summary>
    [Security(Resource = ModuleName.CONTACTMANAGEMENT, Permission = "Read")]
    [CheckLicence(FeatureEnum.SimpleContactManagement)]
    public class CMSContactManagementPage : CMSDeskPage
    {
        #region "Variables"

        private int mSiteID = 0;

        #endregion


        #region "Properties"

        /// <summary>
        /// Returns correct site id for current user.
        /// </summary>
        public virtual int SiteID
        {
            get
            {
                if (mSiteID == 0)
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

        #endregion


        #region "Methods"

        /// <summary>
        /// Gets valid site id for current user.
        /// </summary>
        /// <param name="queryStringSiteId">Site id from querystring</param>
        public virtual int GetSiteID(string queryStringSiteId)
        {
            // Get site id from querystring
            int siteId = ValidationHelper.GetInteger(queryStringSiteId, Int32.MinValue);

            // Global administrator can edit everything
            if (CurrentUser.CheckPrivilegeLevel(UserPrivilegeLevelEnum.GlobalAdmin))
            {
                // There is site id in the querystring
                if (siteId != Int32.MinValue)
                {
                    return siteId;
                }
            }

            return SiteContext.CurrentSiteID;
        }


        /// <summary>
        /// Page OnInit event handler.
        /// </summary>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            // Disable checking for site in site manager
            RequireSite = !CurrentUser.CheckPrivilegeLevel(UserPrivilegeLevelEnum.Admin);
        }


        /// <summary>
        /// Adds 'siteid' query parameter to provided URL.
        /// </summary>
        /// <param name="url">Existing url</param>
        /// <param name="currentSiteID">Current site ID</param>
        /// <returns>Returns modified URL</returns>
        protected string AddSiteQuery(string url, int? currentSiteID)
        {
            if (currentSiteID != null)
            {
                url = URLHelper.AddParameterToUrl(url, "siteid", currentSiteID.ToString());
            }
            else
            {
                url = URLHelper.AddParameterToUrl(url, "siteid", SiteID.ToString());
            }

            return url;
        }

        #endregion
    }
}