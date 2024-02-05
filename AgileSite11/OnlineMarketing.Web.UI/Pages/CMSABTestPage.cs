using System;

using CMS.DataEngine;
using CMS.Helpers;
using CMS.SiteProvider;
using CMS.UIControls;

namespace CMS.OnlineMarketing.Web.UI
{
    /// <summary>
    /// Base class for UI pages of AB test.
    /// </summary>
    [CheckLicence(FeatureEnum.ABTesting)]
    [Security(Resource = "cms.abtest", Permission = "Read", ResourceSite = true)]
    [Security(Resource = "CMS.abtest", UIElements = "ABTestListing")]
    [Security(Resource = "cms", UIElements = "CMSDesk.OnlineMarketing")]
    public class CMSABTestPage : CMSDeskPage
    {
        /// <summary>
        /// Load event handler
        /// </summary>
        protected override void OnLoad(EventArgs e)
        {
            int ABTestID = QueryHelper.GetInteger("objectid", 0);
            if (ABTestID != 0)
            {
                GeneralizedInfo abTestInfo = ProviderHelper.GetInfoById(PredefinedObjectType.ABTEST, ABTestID);
                if ((abTestInfo == null) || (abTestInfo.ObjectSiteID != SiteContext.CurrentSiteID))
                {
                    RedirectToAccessDenied(GetString("cmsmessages.accessdenied"));
                }
            }

            int ABTestVariantID = QueryHelper.GetInteger("variantId", 0);
            if (ABTestVariantID != 0)
            {
                GeneralizedInfo abVariantInfo = ProviderHelper.GetInfoById(PredefinedObjectType.ABVARIANT, ABTestVariantID);
                if ((abVariantInfo == null) || (abVariantInfo.ObjectSiteID != SiteContext.CurrentSiteID))
                {
                    RedirectToAccessDenied(GetString("cmsmessages.accessdenied"));
                }
            }

            base.OnLoad(e);
        }
    }
}