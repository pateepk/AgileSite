using System;

using CMS.Base;
using CMS.Core;
using CMS.DataEngine;
using CMS.DocumentEngine;
using CMS.DocumentEngine.Internal;
using CMS.EventLog;
using CMS.Helpers;
using CMS.LicenseProvider;
using CMS.Modules;
using CMS.OnlineMarketing.Internal;
using CMS.OutputFilter;
using CMS.PortalEngine;
using CMS.Search;
using CMS.SiteProvider;
using CMS.URLRewritingEngine;
using CMS.WebAnalytics;

namespace CMS.OnlineMarketing.Web.UI
{
    internal class ABHandlers
    {
        #region "Constants"

        /// <summary>
        /// Caching set to 6 hours.
        /// </summary>
        public const int CACHE_MINUTES = 360;

        /// <summary>
        /// Log via JavaScript key name.
        /// </summary>
        private const string LOGVIAJAVASCRIPTKEYNAME = "CMSWebAnalyticsUseJavascriptLogging";

        /// <summary>
        /// Test type for caching purposes.
        /// </summary>
        private const string ABTESTTYPE = "ABTEST";

        /// <summary>
        /// Cookie prefix.
        /// </summary>
        private const string ABTESTCOOKIE_PREFIX = "CMSAB";

        #endregion


        private static readonly object abUserStateManagerFactoryInitializationLock = new object();
        private static IABUserStateManagerFactory mABUserStateManagerFactory;


        private static IABUserStateManagerFactory ABUserStateManagerFactory
        {
            get
            {
                if (mABUserStateManagerFactory == null)
                {
                    lock (abUserStateManagerFactoryInitializationLock)
                    {
                        if (mABUserStateManagerFactory == null)
                        {
                            mABUserStateManagerFactory = Service.Resolve<IABUserStateManagerFactory>();
                        }
                    }
                }
                return mABUserStateManagerFactory;
            }
        }


        /// <summary>
        /// Initializes the events handlers.
        /// </summary>
        public static void Init()
        {
            DocumentEvents.GetDocumentMark.Execute += MarkDocumentInContentTree;
            if (SystemContext.IsCMSRunningAsMainApplication)
            {
                DocumentEventsInternal.GeneratePreviewLink.Execute += OnPreviewLinkGeneration_Execute;
                OutputFilterEvents.SaveOutputToCache.Before += SaveABToOutputCache;
                OutputFilterEvents.SendCacheOutput.Before += SendPageFromCache;

                URLRewritingEvents.ProcessABTest.Execute += ProcessABTest;
            }
        }


        /// <summary>
        /// Fires when PageInfo potentially using A/B test is required
        /// </summary>
        private static void ProcessABTest(object sender, ProcessABTestEventArgs args)
        {
            // Apply page variants only when AB Testing, Web analytics, Track conversions are enabled && current view mode is live site
            if (AnalyticsHelper.AnalyticsEnabled(args.SiteName) &&
                LicenseKeyInfoProvider.IsFeatureAvailable(RequestContext.CurrentDomain, FeatureEnum.ABTesting) &&
                SettingsKeyInfoProvider.GetBoolValue(args.SiteName + ".CMSABTestingEnabled") &&
                ResourceSiteInfoProvider.IsResourceOnSite("cms.abtest", SiteContext.CurrentSiteName) &&
                !SearchCrawler.IsCrawlerRequest() &&
                args.ViewMode.IsLiveSite()
            )
            {
                args.ReturnedPageInfo = ABRequestManager.GetABTestPage(args.PageInfo);
            }
        }


        /// <summary>
        /// Marks documents in content tree with icons.
        /// </summary>
        private static void MarkDocumentInContentTree(object sender, DocumentMarkEventArgs e)
        {
            if (!SettingsKeyInfoProvider.GetBoolValue(e.SiteName + ".CMSABTestingEnabled"))
            {
                return;
            }

            // Check license here manually because otherwise it would be checked automatically and content tree wouldn't load
            if (!LicenseHelper.CheckFeature(RequestContext.CurrentDomain, FeatureEnum.ABTesting))
            {
                return;
            }

            string path = ValidationHelper.GetString(e.Container.GetValue("NodeAliasPath"), string.Empty);
            try
            {
                var documentMarker = GetDocumentMarker(e, path);
                e.MarkContent += documentMarker.GetIcons();
            }
            catch (Exception ex)
            {
                EventLogProvider.LogException("ABHandlers", "MarkDocumentInContentTree", ex);
            }
        }


        private static IABDocumentMarker GetDocumentMarker(DocumentMarkEventArgs e, string path)
        {
            if (SiteInfoProvider.GetSiteInfo(e.SiteName)?.SiteIsContentOnly == true)
            {
                return new ContentOnlySiteABDocumentMarker(path, e.SiteName, e.PreferredCultureCode);
            }

            return new ABDocumentMarker(path, e.SiteName, e.PreferredCultureCode);
        }


        /// <summary>
        /// Tries to fetch correct content from output cache if page AB testing is enabled.
        /// If page content is not present in cache, indicates that cache should be skipped (fallback to regular load).
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="ea">Event args needed to perform loading from output cache</param>
        private static void SendPageFromCache(object sender, OutputCacheEventArgs ea)
        {
            CachedOutput cachedOutput = ea.Output;

            if (String.IsNullOrEmpty(cachedOutput.TestCookieName))
            {
                return;
            }

            // Ensure that cached output is A/B test.
            if (!cachedOutput.TestCookieName.StartsWithCSafe(ABTESTCOOKIE_PREFIX))
            {
                return;
            }

            var test = ABTestInfoProvider.GetABTestInfo(cachedOutput.TestCookieName.Substring(ABTESTCOOKIE_PREFIX.Length), SiteContext.CurrentSiteName);

            ABTestContext.CurrentABTest = test;

            if (test == null)
            {
                return;
            }

            // Try get variant name from cookies
            var manager = ABUserStateManagerFactory.Create<string>(test.ABTestName);
            string variantName = manager.GetVariantIdentifier();
            if (String.IsNullOrEmpty(variantName))
            {
                // Do not use cache if visitor targeting macro is filled and it's user's first request.
                if (!String.IsNullOrEmpty(test.ABTestVisitorTargeting) && !manager.IsABTestCookieDefined())
                {
                    ea.Output = null;
                    ea.FallbackToRegularLoad = true;
                    return;
                }

                // Generate new variant
                if (ABTestStatusEvaluator.ABTestIsRunning(test))
                {
                    variantName = GenerateNewABVariant(test);
                }
            }

            // Set ABTest variant correctly from ABTest and variant name
            ABTestContext.CurrentABTestVariant = ABVariantInfoProvider.GetABVariantInfo(variantName, test.ABTestName, cachedOutput.SiteName);

            // Is null or empty when visitor is excluded from test
            string cacheKeyPart;
            if (String.IsNullOrEmpty(variantName))
            {
                cacheKeyPart = CacheHelper.SEPARATOR + CacheHelper.GetCacheItemName(null, ABTESTTYPE, test.ABTestName, string.Empty, "excluded");
            }
            else
            {
                cacheKeyPart = CacheHelper.SEPARATOR + CacheHelper.GetCacheItemName(null, ABTESTTYPE, test.ABTestName, variantName);
            }

            // Try get new cached output
            if (OutputHelper.TryGetFromCache(cachedOutput.CacheKey + cacheKeyPart, cachedOutput.SiteName, out cachedOutput))
            {
                ea.Output = cachedOutput;

                // Output is not defined
                if (cachedOutput == null)
                {
                    ea.FallbackToRegularLoad = true;
                }
            }
            // Variant was not found => regular load
            else
            {
                ea.FallbackToRegularLoad = true;
            }
        }

        /// <summary>
        /// Applies AB testing on the output cache.
        /// </summary>
        private static void SaveABToOutputCache(object sender, OutputCacheEventArgs e)
        {
            var output = e.Output;

            // Check whether AB testing is defined an if so create dummy record for variant type
            var test = ABTestContext.CurrentABTest;

            if (test == null)
            {
                return;
            }

            SaveDummyItemToCache(test.ABTestName, output);

            if (ABUserStateManagerFactory.Create<string>(test.ABTestName).IsExcluded)
            {
                output.CacheKey += CacheHelper.SEPARATOR + CacheHelper.GetCacheItemName(null, ABTESTTYPE, test.ABTestName, string.Empty, "excluded");
            }
            else
            {
                var variant = ABTestContext.CurrentABTestVariant;
                if (variant == null)
                {
                    return;
                }

                // Add test and variant name to the cache key
                output.CacheKey += CacheHelper.SEPARATOR + CacheHelper.GetCacheItemName(null, ABTESTTYPE, test.ABTestName, variant.ABVariantName);
            }
        }


        /// <summary>
        /// Creates dummy cache item with A/B test name in <see cref="CachedOutput.TestCookieName"/> property.
        /// </summary>
        /// <param name="testName">A/B test name</param>
        /// <param name="output">Cached output</param>
        private static void SaveDummyItemToCache(string testName, CachedOutput output)
        {
            CMSCacheDependency cd = null;

            // Create simple cached output
            CachedOutput simplePageCache = new CachedOutput();
            output.CopyCacheSettingsTo(simplePageCache);

            var cacheDependencies = output.CacheDependencies;

            // Add dependencies specific for ABTest
            if (cacheDependencies != null)
            {
                cacheDependencies.Add(ABVariantInfo.OBJECT_TYPE + "|all|");
                cacheDependencies.Add(ABTestInfo.OBJECT_TYPE + "|byname|" + testName.ToLowerCSafe());
                cacheDependencies.Add(SettingsKeyInfo.OBJECT_TYPE + "|byname|" + LOGVIAJAVASCRIPTKEYNAME.ToLowerCSafe());

                // Add tracked cache dependencies
                cd = cacheDependencies.GetCacheDependency();
            }

            simplePageCache.OriginalAliasPath = output.OriginalAliasPath;
            simplePageCache.TestCookieName = ABTESTCOOKIE_PREFIX + testName;

            // Save to the cache
            OutputHelper.AddToCache(simplePageCache, cd);
        }



        /// <summary>
        /// Generates new AB test variant and returns its name.
        /// </summary>
        /// <param name="abTest">AB test</param>
        /// <returns>Returns null if variant is not found or is not active.</returns>
        private static string GenerateNewABVariant(ABTestInfo abTest)
        {
            ABVariantInfo variant = ABRequestManager.GetABTestVariant(abTest);
            if (variant != null)
            {
                if (ABSegmentationEvaluator.CheckVisitorTargetingMacro(abTest))
                {
                    return variant.ABVariantName;
                }

                // Exclude from test
                var manager = ABUserStateManagerFactory.Create<string>(abTest.ABTestName);
                manager.Exclude();
            }
            return null;
        }


        /// <summary>
        /// Adds query string parameter to the preview link being generated.
        /// </summary>
        internal static void OnPreviewLinkGeneration_Execute(object sender, GeneratePreviewLinkEventArgs e)
        {
            var targetVariant = ABTestUIVariantHelper.GetPersistentVariantIdentifier(e.Page.DocumentID);
            if (targetVariant.HasValue)
            {
                e.QueryStringParameters.Add(ABTestConstants.AB_TEST_VARIANT_QUERY_STRING_PARAMETER_NAME, targetVariant.Value.ToString());
            }
        }
    }
}
