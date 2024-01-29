using System;

using CMS.Core;
using CMS.DataEngine;
using CMS.UIControls;

namespace CMS.WebAnalytics.Web.UI
{
    /// <summary>
    /// Base page for campaigns.
    /// </summary>
    [CheckLicence(FeatureEnum.CampaignAndConversions)]
    [UIElement(ModuleName.WEBANALYTICS, "Campaign")]
    [Security(Resource = ModuleName.WEBANALYTICS, ResourceSite = true, Permission = "ManageCampaigns")]
    [Security(Resource = ModuleName.REPORTING, ResourceSite = true)]
    public class CMSCampaignPage : CMSDeskPage
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
    }
}
