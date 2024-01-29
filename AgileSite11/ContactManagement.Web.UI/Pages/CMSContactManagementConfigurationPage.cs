using System;

using CMS.Base;
using CMS.Helpers;
using CMS.Membership;
using CMS.SiteProvider;

namespace CMS.ContactManagement.Web.UI
{
    /// <summary>
    /// Base page for CMS ContactManagement - Configuration page.
    /// </summary>
    public abstract class CMSContactManagementConfigurationPage : CMSContactManagementPage
    {
        #region "Methods"

        /// <summary>
        /// Gets valid site id for current user.
        /// </summary>
        /// <param name="queryStringSiteId">Site id from querystring</param>
        public override int GetSiteID(string queryStringSiteId)
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
        /// OnInit event handler.
        /// </summary>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            // Check UI elements
            if (!CurrentUser.IsAuthorizedPerUIElement("CMS.OnlineMarketing", "Configuration"))
            {
                RedirectToUIElementAccessDenied("CMS.OnlineMarketing", "Configuration");
            }
        }

        #endregion
    }
}