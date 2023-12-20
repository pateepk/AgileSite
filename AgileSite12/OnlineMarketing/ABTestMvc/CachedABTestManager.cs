using System;
using System.Collections.Generic;
using CMS.Core.Internal;
using CMS.DocumentEngine;
using CMS.Helpers;
using CMS.OnlineMarketing.Internal;

namespace CMS.OnlineMarketing
{
    /// <summary>
    /// Acts as a decorator for <see cref="IABTestManager"/> and adds caching to methods.
    /// </summary>
    /// <remarks>
    /// The implementation caches the <see cref="GetRunningABTest(TreeNode)"/> method's return value only.
    /// </remarks>
    public class CachedABTestManager : ICachedABTestManager
    {
        private readonly IABTestManager abTestManager;
        private readonly IDateTimeNowService dateTimeNowService;


        /// <summary>
        /// Gets the number of minutes used for absolute cache expiration.
        /// </summary>
        /// <remarks>
        /// The implementation returns 1440 minutes (24 hours).
        /// </remarks>
        protected internal virtual int CacheMinutes
        {
            get
            {
                return ABCachedObjects.CACHE_MINUTES;
            }
        }


        /// <summary>
        /// Initializes a new instance of <see cref="CachedABTestManager"/>.
        /// </summary>
        /// <param name="abTestManager"><see cref="IABTestManager"/> implementation to be decorated with caching.</param>
        /// <param name="dateTimeNowService">Service used for getting current datetime.</param>
        public CachedABTestManager(IABTestManager abTestManager, IDateTimeNowService dateTimeNowService)
        {
            this.abTestManager = abTestManager;
            this.dateTimeNowService = dateTimeNowService;
        }


        /// <summary>
        /// Adds a new A/B test variant into <paramref name="page"/> based on an existing source variant.
        /// </summary>
        /// <param name="page">Page for which to add a new variant.</param>
        /// <param name="sourceVariantGuid">GUID of the source variant (if null or <see cref="Guid.Empty"/>, original is assumed as the source).</param>
        /// <returns>Returns the new A/B test variant.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="page"/> is null.</exception>
        /// <exception cref="ArgumentException">Throw when <paramref name="sourceVariantGuid"/> does not identify an existing variant within <paramref name="page"/>.</exception>
        public IABTestVariant AddVariant(TreeNode page, Guid? sourceVariantGuid)
        {
            return abTestManager.AddVariant(page, sourceVariantGuid);
        }


        /// <summary>
        /// Creates a new A/B test for <paramref name="page"/> with included traffic set to 100
        /// and stores it to the database. If the page has any A/B test variants defined,
        /// they are cleared.
        /// </summary>
        /// <param name="page">Page to create A/B test for.</param>
        /// <returns>Returns a new A/B for <paramref name="page"/>.</returns>
        /// <remarks>
        /// The A/B test's name is inferred using <see cref="ABTestNameHelper.GetDefaultDisplayName"/>.
        /// </remarks>
        public ABTestInfo CreateABTest(TreeNode page)
        {
            return abTestManager.CreateABTest(page);
        }


        /// <summary>
        /// Gets A/B test without a winner for a given <paramref name="page"/>.
        /// Returns null if no A/B test is associated with the <paramref name="page"/> or associated A/B test has a winner.
        /// </summary>
        /// <param name="page">Page to retrieve an unconclued A/B test for.</param>
        /// <returns>Returns an unconcluded A/B test for the page, or null.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="page"/> is null.</exception>
        public ABTestInfo GetABTestWithoutWinner(TreeNode page)
        {
            return abTestManager.GetABTestWithoutWinner(page);
        }


        /// <summary>
        /// Gets A/B test for <paramref name="page"/> which is still running.
        /// Returns null if no running A/B is available for the page.
        /// Results of this method are cached.
        /// </summary>
        /// <param name="page">Page to retrieve running A/B test for.</param>
        /// <returns>Returns running A/B test for the page, or null.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="page"/> is null.</exception>
        public ABTestInfo GetRunningABTest(TreeNode page)
        {
            ABTestInfo test = null;

            using (var cs = new CachedSection<ABTestInfo>(ref test, CacheMinutes, true, null, "pageabtest", page.NodeSiteID, page.NodeAliasPath, page.DocumentCulture))
            {
                if (cs.LoadData)
                {
                    test = abTestManager.GetRunningABTest(page);
                    cs.Data = test;
                    if (cs.Cached)
                    {
                        if (test == null)
                        {
                            cs.CacheDependency = CacheHelper.GetCacheDependency(new[] { ABTestInfo.OBJECT_TYPE + "|all" });
                        }
                        else
                        {
                            if (test.ABTestOpenTo != DateTime.MinValue)
                            {
                                var remainingTestDuration = (test.ABTestOpenTo - dateTimeNowService.GetDateTimeNow()).TotalMinutes;
                                cs.CacheMinutes = Math.Min(remainingTestDuration, CacheMinutes);
                            }

                            cs.CacheDependency = CacheHelper.GetCacheDependency(new[] { ABTestInfo.OBJECT_TYPE + $"|byid|{test.ABTestID}" });
                        }
                    }
                }
            }

            return test;
        }


        /// <summary>
        /// Returns all A/B test variants existing for given <paramref name="page"/>.
        /// </summary>
        /// <returns>
        /// Existing variants or empty enumeration when document has empty A/B variant configuration.
        /// Variants are represented by <see cref="IABTestVariant"/> interface.
        /// </returns>
        /// <exception cref="ArgumentNullException">When passed <paramref name="page"/> is null.</exception>
        /// <exception cref="InvalidOperationException">When feature is not available by license, A/B testing is disabled, resource is not allowed on site, user does not have a Read permission, site isn't defined as content only or document has malformed A/B variant configuration data.</exception>
        public IEnumerable<IABTestVariant> GetVariants(TreeNode page)
        {
            return abTestManager.GetVariants(page);
        }


        /// <summary>
        /// Promotes a variant identified by <paramref name="variantGuid"/> as the winner variant.
        /// Winning variant is stored in the database within the A/B test.
        /// </summary>
        /// <param name="page">Page for which to promote the winner variant.</param>
        /// <param name="variantGuid">GUID of the variant to be promoted.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="page"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when there is no unconcluded A/B test or <paramref name="variantGuid"/> does not identify an existing variant within <paramref name="page"/>.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the A/B test for the current <paramref name="page"/> has not finished yet.</exception>
        /// <seealso cref="GetABTestWithoutWinner(TreeNode)"/>
        /// <seealso cref="ABTestStatusEvaluator.ABTestIsFinished(ABTestInfo)"/>
        public void PromoteVariant(TreeNode page, Guid variantGuid)
        {
            abTestManager.PromoteVariant(page, variantGuid);
        }


        /// <summary>
        /// Removes an A/B test variant identified by <paramref name="variantGuid"/> from <paramref name="page"/>.
        /// </summary>
        /// <param name="page">Page from which to remove a variant.</param>
        /// <param name="variantGuid">GUID of the variant to be removed.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="page"/> is null.</exception>
        /// <exception cref="ArgumentException">Throw when <paramref name="variantGuid"/> does not identify an existing variant within <paramref name="page"/>.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the original A/B variant is to be deleted.</exception>
        public void RemoveVariant(TreeNode page, Guid variantGuid)
        {
            abTestManager.RemoveVariant(page, variantGuid);
        }


        /// <summary>
        /// Renames an A/B test variant in <paramref name="page"/> identified by variant GUID with given name.
        /// </summary>
        /// <param name="page">Page in which to rename the variant.</param>
        /// <param name="variantGuid">Unique identifier of the variant for which the change the name.</param>
        /// <param name="newVariantName">New name for the variant identified with <paramref name="variantGuid"/>.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="page"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="newVariantName"/> is null, empty or has more than <see cref="ABTestManager.MAXIMUM_VARIANT_NAME_LENGTH"/> characters.</exception>
        /// <exception cref="InvalidOperationException">Thrown when there is no variant with <paramref name="variantGuid"/> identifier.</exception>
        public void RenameVariant(TreeNode page, Guid variantGuid, string newVariantName)
        {
            abTestManager.RenameVariant(page, variantGuid, newVariantName);
        }


        /// <summary>
        /// Updates an A/B test variant in <paramref name="page"/> identified by variant GUID with given Page builder widgets configuration.
        /// </summary>
        /// <param name="page">Page in which to update the variant.</param>
        /// <param name="variantGuid">Unique identifier of the variant for which to update the configuration.</param>
        /// <param name="configurationSource">Source of the configuration to update the variant with.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="page"/> or <paramref name="configurationSource"/> is null.</exception>
        /// <exception cref="ArgumentException">Throw when <paramref name="variantGuid"/> does not identify an existing variant within <paramref name="page"/>.</exception>
        public void UpdateVariant(TreeNode page, Guid variantGuid, VariantConfigurationSource configurationSource)
        {
            abTestManager.UpdateVariant(page, variantGuid, configurationSource);
        }
    }
}
