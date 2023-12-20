using System;
using System.Collections.Generic;
using System.Linq;

using CMS.Helpers;

namespace CMS.OnlineMarketing
{
    /// <summary>
    /// Class providing access to cached AB tests and variants.
    /// </summary>
    public sealed class ABCachedObjects
    {
        /// <summary>
        /// Caching is set to 24 hours.
        /// </summary>
        public const int CACHE_MINUTES = 1440;


        /// <summary>
        /// Returns all AB tests using the cached section.
        /// </summary>
        public static List<ABTestInfo> GetTests()
        {
            // Get all AB tests, cached
            var tests = new List<ABTestInfo>();

            using (var cs = new CachedSection<List<ABTestInfo>>(ref tests, CACHE_MINUTES, true, null, "allabtests"))
            {
                if (cs.LoadData)
                {
                    tests = ABTestInfoProvider.GetABTests()
                                              .ToList();

                    // Save the result to the cache
                    if (cs.Cached)
                    {
                        cs.CacheDependency = CacheHelper.GetCacheDependency(new[] { ABTestInfo.OBJECT_TYPE + "|all" });
                    }

                    cs.Data = tests;
                }
            }
            return tests;
        }


        /// <summary>
        /// Returns AB variants for the given test using the cached section.
        /// </summary>
        public static List<ABVariantInfo> GetVariants(ABTestInfo abTest)
        {
            var variants = new List<ABVariantInfo>();

            // Try to get data from cache
            using (var cs = new CachedSection<List<ABVariantInfo>>(ref variants, CACHE_MINUTES, true, null, "abvariants", abTest.ABTestName, abTest.ABTestSiteID))
            {
                if (cs.LoadData)
                {
                    // Get the variant ID's for current test
                    variants = ABVariantInfoProvider.GetVariants()
                                                    .WhereEquals("ABVariantTestID", abTest.ABTestID)
                                                    .ToList();

                    // Save the result to the cache
                    if (cs.Cached)
                    {
                        // Prepare cache dependencies - we want to refresh cache if some A/B test is changed
                        string[] dependencies = new string[2];
                        dependencies[0] = ABTestInfo.OBJECT_TYPE + "|byid|" + abTest.ABTestID;
                        dependencies[1] = ABVariantInfo.OBJECT_TYPE + "|all";

                        cs.CacheDependency = CacheHelper.GetCacheDependency(dependencies);
                    }

                    cs.Data = variants;
                }
            }

            return variants;
        }


        /// <summary>
        /// Returns cached variants for the given AB test.
        /// </summary>
        public static List<ABVariantDataInfo> GetVariantsData(ABTestInfo abTest)
        {
            var variants = new List<ABVariantDataInfo>();

            if (abTest != null)
            {
                variants = CacheHelper.Cache(() => ABVariantDataInfoProvider.GetVariantsData()
                                                                            .WhereEquals("ABVariantTestID", abTest.ABTestID)
                                                                            .ToList(),
                    new CacheSettings(CACHE_MINUTES, ABVariantDataInfo.OBJECT_TYPE, abTest.ABTestName, abTest.ABTestSiteID)
                    {
                        CacheDependency = CacheHelper.GetCacheDependency(new[]
                        {
                            ABTestInfo.OBJECT_TYPE + "|byid|" + abTest.ABTestID,
                            ABVariantDataInfo.OBJECT_TYPE + "|all"
                        })
                    });
            }

            return variants;
        }


        /// <summary>
        /// Returns cached <see cref="ABVariantDataInfo.ABVariantDisplayName"/> of A/B test variant with given <paramref name="variantIdentifier"/> on site with given <paramref name="siteId"/>.
        /// </summary>
        internal static string GetVariantDisplayName(int siteId, Guid variantIdentifier)
        {
            return CacheHelper.Cache(() => ABVariantDataInfoProvider.GetVariantsData()
                                                                    .Source(s => s.InnerJoin<ABTestInfo>("ABVariantTestID", "ABTestID"))
                                                                    .WhereEquals("ABTestSiteID", siteId)
                                                                    .WhereEquals("ABVariantGUID", variantIdentifier)
                                                                    .Column("ABVariantDisplayName")
                                                                    .GetScalarResult<string>(null),
                    new CacheSettings(CACHE_MINUTES, ABVariantDataInfo.OBJECT_TYPE, siteId, variantIdentifier)
                    {
                        CacheDependency = CacheHelper.GetCacheDependency(new[]
                        {
                            CacheHelper.GetCacheItemName(null, ABVariantDataInfo.OBJECT_TYPE, "all")
                        })
                    });
        }
    }
}