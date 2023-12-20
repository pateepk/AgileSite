using System;
using System.Collections.Generic;

using CMS.Core;
using CMS.Core.Internal;
using CMS.DocumentEngine;
using CMS.Helpers;
using CMS.OnlineMarketing;
using CMS.OnlineMarketing.Internal;

namespace Kentico.OnlineMarketing.Web.Mvc
{
    /// <summary>
    /// Class serves as a cache which maps URLs to specific pages for usage in output caching.
    /// </summary>
    internal class OutputCacheUrlToPageMapper : IOutputCacheUrlToPageMapper
    {
        /// <summary>
        /// Lifetime of the cached page in minutes.
        /// </summary>
        public const double CACHED_PAGE_DURATION = 1440;


        private readonly IDateTimeNowService dateTimeNowService;
        private readonly IABTestVariantSelectionArbiter variantArbiter;
        private readonly ICachedABTestManager abTestManager;
        

        /// <summary>
        /// Creates an instance of <see cref="OutputCacheUrlToPageMapper"/>.
        /// </summary>
        public OutputCacheUrlToPageMapper()
            : this(Service.Resolve<ICachedABTestManager>(), Service.Resolve<IABTestVariantSelectionArbiter>(), Service.Resolve<IDateTimeNowService>())
        {
        }


        internal OutputCacheUrlToPageMapper(ICachedABTestManager abTestManager, IABTestVariantSelectionArbiter variantArbiter, IDateTimeNowService dateTimeNowService)
        {
            this.abTestManager = abTestManager ?? throw new ArgumentNullException(nameof(abTestManager));
            this.variantArbiter = variantArbiter ?? throw new ArgumentNullException(nameof(variantArbiter));
            this.dateTimeNowService = dateTimeNowService ?? throw new ArgumentNullException(nameof(dateTimeNowService));
        }


        /// <summary>
        /// Gets A/B test variant for the page specified by <paramref name="uri"/>.
        /// </summary>
        /// <param name="uri">URL to get variant for.</param>
        /// <returns>A/B test variant if user has a variant assigned, otherwise null.</returns>
        public ABTestVariant GetABTestVariantForUrl(Uri uri)
        {
            var cacheKey = GetCacheKey(uri);
            if (CacheHelper.TryGetItem<TreeNode>(cacheKey, out var treeNode))
            {
                return variantArbiter.SelectVariant(treeNode);
            }

            return null;
        }


        /// <summary>
        /// Adds a new URL to page mapping.
        /// </summary>
        /// <param name="url">URL used as a key for the page in the cache.</param>
        /// <param name="page">Page to be cached.</param>
        public void Add(Uri url, TreeNode page)
        {
            var abTest = abTestManager.GetRunningABTest(page);

            var cacheDuration = CACHED_PAGE_DURATION;
            if (abTest != null && abTest.ABTestOpenTo != DateTime.MinValue)
            {
                // Duration is set here to prevent the pages from occupying the cache when A/B test is no longer running.
                var remainingTestDuration = (abTest.ABTestOpenTo - dateTimeNowService.GetDateTimeNow()).TotalMinutes;
                cacheDuration = Math.Min(remainingTestDuration, cacheDuration);
            }

            var cacheKey = GetCacheKey(url);

            CacheHelper.Cache(() => page, new CacheSettings(cacheDuration, cacheKey)
            {
                GetCacheDependency = () =>
                {
                    var keys = new List<string>()
                    {
                        // This dependecy is sufficient because change to A/B test touches its related page.
                        CacheHelper.BuildCacheItemName(new object[] { "nodeid", page.NodeID })
                    };

                    return CacheHelper.GetCacheDependency(keys);
                }
            });

            CMSHttpContext.Current.Response.AddCacheItemDependency(cacheKey);
        }


        private string GetCacheKey(Uri url)
        {
            return CacheHelper.BuildCacheItemName( new object[] {nameof(OutputCacheUrlToPageMapper), url.AbsoluteUri });
        }
    }
}
