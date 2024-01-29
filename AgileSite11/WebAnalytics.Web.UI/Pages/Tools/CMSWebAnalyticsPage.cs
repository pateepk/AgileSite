using System;

using CMS.Base;
using CMS.Core;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Membership;
using CMS.Modules;
using CMS.UIControls;

namespace CMS.WebAnalytics.Web.UI
{
    /// <summary>
    /// Base page for the CMS Web analytics pages to apply global settings to the pages.
    /// </summary>
    [CheckLicence(FeatureEnum.WebAnalytics)]

    [Security(Resource = ModuleName.WEBANALYTICS, ResourceSite = true, Permission = "Read")]
    [Security(Resource = ModuleName.REPORTING, ResourceSite = true)]
    [UIElement("CMS.WebAnalytics", "CMSWebAnalytics")]
    public abstract class CMSWebAnalyticsPage : CMSDeskPage
    {
        /// <summary>
        /// Checks all permissions for web analytics
        /// </summary>
        public static void CheckAllPermissions()
        {
            // Test reporting module
            if (!ModuleManager.IsModuleLoaded(ModuleName.REPORTING))
            {
                RedirectToInformation("analytics.noreporting");
            }
        }


        /// <summary>
        /// Init event handler
        /// </summary>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            CheckDocPermissions = false;
            CheckAllPermissions();
        }


        /// <summary>
        /// Check hierarchical UI permission for analytics codename. Redirects if check fails.
        /// </summary>
        /// <param name="codeName">Analytics code name</param>
        public void CheckWebAnalyticsUI(string codeName)
        {
            CheckUIElementAccessHierarchical("CMS.WebAnalytics", codeName);
        }


        /// <summary>
        /// Check UI permission for codename based on query 'datacodename' param. Redirects if check fails
        /// </summary>
        public void CheckWebAnalyticsUI()
        {
            // No check for global admin
            if (MembershipContext.AuthenticatedUser.CheckPrivilegeLevel(UserPrivilegeLevelEnum.Admin))
            {
                return;
            }

            String dataCodeName = QueryHelper.GetString("datacodename", String.Empty);
            if (String.IsNullOrEmpty(dataCodeName))
            {
                return;
            }

            // Get root for Web Analytics
            UIElementInfo root = UIElementInfoProvider.GetRootUIElementInfo("CMS.WebAnalytics");

            // Test if data code name is not custom name
            var elements = UIElementInfoProvider
                .GetUIElements()
                .WhereEquals("ElementName", dataCodeName)
                .WhereStartsWith("ElementIDPath", root.ElementIDPath + "/")
                .Columns("ElementName");

            // If datacodename found - test its UI permissions
            if (elements.HasResults())
            {
                CheckWebAnalyticsUI(dataCodeName);
            }
            else
            {
                // Else test 'custom' UI permission
                CheckWebAnalyticsUI("Custom");
            }
        }
    }
}