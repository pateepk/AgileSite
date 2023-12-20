using System;
using System.Linq;

using CMS.Activities;
using CMS.Base;
using CMS.OnlineMarketing.Internal;
using CMS.SiteProvider;

namespace CMS.OnlineMarketing
{
    /// <summary>
    /// Class for updating A/B test related properties in activities.
    /// </summary>
    internal class ABTestActivityModifier : IActivityModifier
    {
        private readonly ISiteService mSiteService;


        /// <summary>
        /// Creates instance of <see cref="ABTestActivityModifier"/>.
        /// </summary>
        /// <param name="siteService">Site service.</param>
        public ABTestActivityModifier(ISiteService siteService)
        {
            mSiteService = siteService ?? throw new ArgumentNullException(nameof(siteService));
        }


        /// <summary>
        /// Updates <see cref="IActivityInfo.ActivityABVariantName"/> with name of current A/B test variant if one is present.
        /// </summary>
        /// <remarks>Only activities of type <see cref="PredefinedActivityType.PAGE_VISIT"/> or <see cref="PredefinedActivityType.LANDING_PAGE"/> are updated.</remarks>
        /// <param name="activity">Activity which property to update.</param>
        public void Modify(IActivityInfo activity)
        {
            if (activity.ActivityType == PredefinedActivityType.PAGE_VISIT || activity.ActivityType == PredefinedActivityType.LANDING_PAGE)
            {
                var siteInfo = SiteInfoProvider.GetSiteInfo(mSiteService.CurrentSite.SiteID);
                if (!siteInfo.SiteIsContentOnly)
                {
                    activity.ActivityABVariantName = ABTestContext.CurrentABTestVariant?.ABVariantName;
                }
                else
                {
                    var variantsIdentifiers = ABTestHelper.GetValidVariants().ToList();
                    if (variantsIdentifiers.Any())
                    {
                        activity.ActivityABVariantName = ABCachedObjects.GetVariantDisplayName(activity.ActivitySiteID, variantsIdentifiers.First());
                    }
                }
            }
        }
    }
}
