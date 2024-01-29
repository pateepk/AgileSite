using System;

using CMS.Base;
using CMS.Core;
using CMS.Helpers;
using CMS.SiteProvider;

namespace CMS.UIControls
{
    /// <summary>
    /// Base page for email queue pages.
    /// </summary>
    public class EmailQueuePage : CMSDeskPage
    {
        /// <summary>
        /// Permission for modifying email queue in the UI.
        /// </summary>
        public const string MODIFY_PERMISSION = "ModifyCMSEmailQueue";

        /// <summary>
        /// Permission for accessing email queue in the UI.
        /// </summary>
        public const string READ_PERMISSION = "ReadCMSEmailQueue";


        private bool? mUserIsAdmin;
        private bool? mUserHasModify;
        private int? siteId;


        /// <summary>
        /// Indicates the site in context of which is the email queue working.
        /// </summary>
        protected int SiteId
        {
            get
            {
                if (!siteId.HasValue)
                {
                    siteId = CMSActionContext.CurrentUser.CheckPrivilegeLevel(UserPrivilegeLevelEnum.GlobalAdmin) ? QueryHelper.GetInteger("siteid", -1) : SiteContext.CurrentSiteID;
                }

                return siteId.Value;
            }
        }


        /// <summary>
        /// Indicates whether current user has privilege level at least <see cref="UserPrivilegeLevelEnum.Admin"/> on current site.
        /// </summary>
        protected bool UserIsAdmin
        {
            get
            {
                if (!mUserIsAdmin.HasValue)
                {
                    mUserIsAdmin = CMSActionContext.CurrentUser.CheckPrivilegeLevel(UserPrivilegeLevelEnum.Admin, SiteContext.CurrentSiteName);
                }

                return mUserIsAdmin.Value;
            }
        }


        /// <summary>
        /// Indicates whether current user can modify email queue.
        /// </summary>
        protected bool UserHasModify
        {
            get
            {
                if (!mUserHasModify.HasValue)
                {
                    mUserHasModify = CMSActionContext.CurrentUser.IsAuthorizedPerResource(ModuleName.EMAILENGINE, MODIFY_PERMISSION, SiteContext.CurrentSiteName, false);
                }

                return mUserHasModify.Value;
            }
        }


        /// <summary>
        /// OnPreLoad event. 
        /// </summary>
        protected override void OnPreLoad(EventArgs e)
        {
            base.OnPreLoad(e);

            // Ensure page accessibility when there is no site running
            RequireSite = false;
        }
    }
}
