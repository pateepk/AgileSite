using System;

using CMS.Base;
using CMS.Helpers;
using CMS.Membership;
using CMS.Modules;
using CMS.SiteProvider;

namespace CMS.UIControls
{
    /// <summary>
    /// Base page for the scheduled tasks tab of administration pages to apply global settings to the pages.
    /// </summary>
    [UIElement("CMS.ScheduledTasks", "ScheduledTasks")]
    public abstract class CMSScheduledTasksPage : CMSAdministrationPage
    {
        #region "Variables"

        private SiteInfo mSiteInfo = null;
        private int? mSiteID = null;

        #endregion


        #region "Properties"

        /// <summary>
        /// Site ID
        /// </summary>
        public override int SiteID
        {
            get
            {
                if (mSiteID == null)
                {
                    mSiteID = ValidationHelper.GetInteger(UIContext.GetValue("SiteID"), 0);
                    if (mSiteID == 0)
                    {
                        mSiteID = SiteContext.CurrentSiteID;
                    }
                }
                return mSiteID.Value;
            }
        }


        /// <summary>
        /// Current site info
        /// </summary>
        protected SiteInfo SiteInfo
        {
            get
            {
                return mSiteInfo ?? (mSiteInfo = SiteInfoProvider.GetSiteInfo(SiteID));
            }
        }

        #endregion


        /// <summary>
        /// PreInit event handler
        /// </summary>
        protected override void OnPreInit(EventArgs e)
        {
            base.OnPreInit(e);

            if (!DebugHelper.DebugScheduler)
            {
                DisableDebugging();
            }
        }


        /// <summary>
        /// Init event handler
        /// </summary>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            CurrentUserInfo user = MembershipContext.AuthenticatedUser;
            if (user != null)
            {
                // Check site availability
                if (!user.CheckPrivilegeLevel(UserPrivilegeLevelEnum.Admin))
                {
                    if (!ResourceSiteInfoProvider.IsResourceOnSite("CMS.ScheduledTasks", SiteContext.CurrentSiteName))
                    {
                        RedirectToResourceNotAvailableOnSite("CMS.ScheduledTasks");
                    }
                }

                // Check "read" permission
                if (!user.IsAuthorizedPerResource("CMS.ScheduledTasks", "Read"))
                {
                    RedirectToAccessDenied("CMS.ScheduledTasks", "Read");
                }
            }
        }


        /// <summary>
        /// Returns element name for given element on CMS Desk or in Site Manager.
        /// </summary>
        /// <param name="element">Element name</param>
        protected string GetElementName(string element)
        {
            if (ValidationHelper.GetBoolean(UIContext.GetValue("IsSiteManager"), false))
            {
                element = "Administration." + element;
            }

            return element;
        }
    }
}