using System;
using System.Collections.Generic;
using System.Linq;

using CMS.Base;
using CMS.ContactManagement;
using CMS.DocumentEngine;
using CMS.SiteProvider;

namespace CMS.OnlineMarketing
{
    /// <summary>
    /// Class managing requests on AB test pages.
    /// </summary>
    internal static class ABRequestManager
    {
        #region "Internal methods"

        /// <summary>
        /// Returns the given page or variant page based on whether it has an AB test running.
        /// </summary>
        /// <param name="originalPage">Original page</param>
        internal static PageInfo GetABTestPage(PageInfo originalPage)
        {
            if (originalPage == null)
            {
                return null;
            }

            SiteInfo site = SiteInfoProvider.GetSiteInfo(originalPage.NodeSiteID);

            // Check whether AB testing is enabled
            if (site == null || !ABTestInfoProvider.ABTestingEnabled(site.SiteName))
            {
                return originalPage;
            }

            string originalPagePath = originalPage.NodeAliasPath;
            string originalPageCulture = originalPage.DocumentCulture;

            // Get AB tests on the page
            var abTests = ABCachedObjects.GetTests()
                                         .Where(t => t.ABTestSiteID == site.SiteID)
                                         .Where(t => t.ABTestOriginalPage.EqualsCSafe(originalPagePath, true))
                                         .Where(t => String.IsNullOrEmpty(t.ABTestCulture) || (t.ABTestCulture.EqualsCSafe(originalPageCulture, true)));

            foreach (var test in abTests)
            {
                if (ABTestStatusEvaluator.ABTestIsRunning(test))
                {
                    ABVariantInfo variant = GetABTestVariant(test);
                    ABTestContext.CurrentABTest = test;

                    if (variant != null)
                    {
                        // If the variant has the same path as the original page, return the page
                        if (variant.ABVariantPath.EqualsCSafe(originalPagePath, true))
                        {
                            ABTestContext.CurrentABTestVariant = variant;
                            return originalPage;
                        }

                        // Get page for the assigned AB variant
                        PageInfo variantPage = PageInfoProvider.GetPageInfo(site.SiteName, variant.ABVariantPath, originalPageCulture, null, false);

                        // If the page doesn't exist in the given culture (DocumentID == 0) or is not published, exclude the visitor from the test
                        if ((variantPage == null ) || (variantPage.DocumentID == 0) || !variantPage.IsPublished)
                        {
                            var manager = new ABUserStateManager(test.ABTestName);
                            manager.Exclude();

                            return originalPage;
                        }

                        // Set context and return the variant page
                        ABTestContext.CurrentABTestVariant = variant;
                        return variantPage;
                    }
                }
            }

            return originalPage;
        }


        /// <summary>
        /// Gets AB variant from the user's cookie or randomly selects one and assigns it to the user. Also checks whether the user should be excluded or not
        /// (see <see cref="ABTestInfo.ABTestIncludedTraffic"/> or <see cref="ABTestInfo.ABTestVisitorTargeting"/>).
        /// </summary>
        /// <param name="abTest">AB test</param>
        /// <exception cref="ArgumentNullException"><paramref name="abTest"/> is null</exception>
        internal static ABVariantInfo GetABTestVariant(ABTestInfo abTest)
        {
            if (abTest == null)
            {
                throw new ArgumentNullException("abTest");
            }

            var manager = new ABUserStateManager(abTest.ABTestName);
            if (manager.IsExcluded)
            {
                return null;
            }

            var variants = ABCachedObjects.GetVariants(abTest);
            if (!variants.Any())
            {
                manager.Exclude();
                return null;
            }

            // Get variant from cookies
            string variantName = manager.GetVariantName();

            // Check whether variant is defined
            if (!String.IsNullOrEmpty(variantName))
            {
                // Get variant info object
                var variant = variants.FirstOrDefault(v => (v.ABVariantName == variantName));

                // Check whether variant is defined and is assigned to specified ab test
                if (variant != null)
                {
                    // Extend cookie expiration to the next 2 months.
                    manager.AssignVariant(variant.ABVariantName);
                    return variant;
                }
            }

            ABVariantInfo selectedVariant = null;

            // Check if user should be included in A/B test. Exclude him when he shouldn't be.
            if (!ABSegmentationEvaluator.CheckUserIsEditor() && ABSegmentationEvaluator.CheckIncludedTrafficCondition(abTest))
            {
                selectedVariant = SelectRandomTestVariant(variants);
                manager.AssignVariant(selectedVariant.ABVariantName);
            }
            else
            {
                manager.Exclude();
            }

            ABTestContext.IsFirstABRequest = true;
            return selectedVariant;
        }


        /// <summary>
        /// Uses <see cref="ABSegmentationEvaluator"/> to check segmentation conditions.
        /// Excludes visitor from AB test if did not pass.
        /// Excludes also editors from test.
        /// </summary>
        /// <param name="abTest">AB test</param>
        /// <param name="manager">AB user state manager</param>
        /// <exception cref="ArgumentNullException"><paramref name="abTest"/> or <paramref name="manager"/> is null</exception>
        internal static void ExcludeVisitorsBasedOnSegmentationConditions(ABTestInfo abTest, ABUserStateManager manager)
        {
            if (abTest == null)
            {
                throw new ArgumentNullException("abTest");
            }
            if (manager == null)
            {
                throw new ArgumentNullException("manager");
            }

            if (!ABSegmentationEvaluator.CheckVisitorTargetingMacro(abTest))
            {
                ExcludeAndSetOriginalPage(manager);
            }
        }

        #endregion


        #region "Private methods"

        /// <summary>
        /// Returns random variant.
        /// </summary>
        /// <param name="variants">List of AB variants</param>
        private static ABVariantInfo SelectRandomTestVariant(IList<ABVariantInfo> variants)
        {
            // Generate random variant
            int randomVariant = StaticRandom.Next(0, variants.Count);

            // Return variant
            return variants[randomVariant];
        }


        /// <summary>
        /// Excludes visitor from ab test and sets him original page.
        /// </summary>
        /// <param name="manager">AB user state manager</param>
        private static void ExcludeAndSetOriginalPage(ABUserStateManager manager)
        {
            manager.Exclude();

            DocumentContext.CurrentPageInfo = DocumentContext.OriginalPageInfo;
            DocumentContext.CurrentAliasPath = DocumentContext.OriginalAliasPath;
            DocumentContext.OriginalPageInfo = null;
            DocumentContext.OriginalAliasPath = null;
        }

        #endregion
    }
}
