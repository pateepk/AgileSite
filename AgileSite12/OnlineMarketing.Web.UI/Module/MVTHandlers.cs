using System;

using CMS.Base;
using CMS.Base.Web.UI;
using CMS.Core;
using CMS.DataEngine;
using CMS.DocumentEngine;
using CMS.Helpers;
using CMS.LicenseProvider;
using CMS.Modules;
using CMS.OutputFilter;
using CMS.PortalEngine;
using CMS.SiteProvider;

namespace CMS.OnlineMarketing.Web.UI
{
    /// <summary>
    /// MV testing event handlers.
    /// </summary>
    internal class MVTHandlers
    {
        /// <summary>
        /// Test type constant for caching purposes.
        /// </summary>
        private const string MVTESTTYPE = "MVTEST";

        /// <summary>
        /// Cookie prefix.
        /// </summary>
        private const string MVTCOOKIE_PREFIX = "CMSMVT";

        /// <summary>
        /// Log via JavaScript key name.
        /// </summary>
        private const string LogViaJavaScriptKeyName = "CMSWebAnalyticsUseJavascriptLogging";


        #region "Methods"

        /// <summary>
        /// Initializes the events handlers.
        /// </summary>
        public static void Init()
        {
            PortalEngineEvents.MVTVariantsEnabled.Execute += MVTVariantsEnabled;

            OutputFilterEvents.SaveOutputToCache.Before += SaveMVTToOutputCache;
            OutputFilterEvents.SendCacheOutput.Before += SendMVTFromCache;
        }


        /// <summary>
        /// Event to find whether MVT variants are enabled
        /// </summary>
        private static void MVTVariantsEnabled(object sender, PortalEngineEventArgs args)
        {
            if ((PortalContext.ViewMode != ViewModeEnum.UI)
                && SettingsKeyInfoProvider.GetBoolValue(SiteContext.CurrentSiteName + ".CMSMVTEnabled")
                && ModuleEntryManager.IsModuleLoaded(ModuleName.ONLINEMARKETING)
                && LicenseKeyInfoProvider.IsFeatureAvailable(RequestContext.CurrentDomain, FeatureEnum.MVTesting)
                && ResourceSiteInfoProvider.IsResourceOnSite("cms.mvtest", SiteContext.CurrentSiteName))
            {
                // If not found from document context
                ITreeNode currentDocument = DocumentContext.CurrentDocument;
                if (currentDocument == null)
                {
                    try
                    {
                        // Try page
                        ICMSPage page = PageContext.CurrentPage as ICMSPage;
                        if ((page != null) && (page.DocumentManager != null))
                        {
                            currentDocument = page.DocumentManager.Node;
                        }
                    }
                    catch
                    {
                        args.Enabled = false;
                        return;
                    }
                }

                if ((currentDocument != null) && (PortalContext.ViewMode != ViewModeEnum.DashboardWidgets))
                {
                    // Check whether MVT is active for the current document
                    args.Enabled = MVTestInfoProvider.ContainsMVTest(currentDocument.NodeAliasPath, currentDocument.NodeSiteID, currentDocument.DocumentCulture, PortalContext.ViewMode.IsLiveSite());
                }
            }
        }


        /// <summary>
        /// Applies MVT testing on the output cache.
        /// </summary>
        private static void SaveMVTToOutputCache(object sender, OutputCacheEventArgs e)
        {
            var output = e.Output;
            CMSCacheDependency cd = null;

            var list = output.CacheDependencies;

            // Check whether MV testing is defined an if so, create dummy record for combination type
            var mvTestName = MVTContext.CurrentMVTestName;

            if (String.IsNullOrEmpty(mvTestName))
            {
                return;
            }

            var mvCombinationName = MVTContext.CurrentMVTCombinationName;
            if (String.IsNullOrEmpty(mvCombinationName))
            {
                return;
            }

            // Add dependencies specific for MVT
            if (list != null)
            {
                list.Add(MVTestInfo.OBJECT_TYPE + "|byname|" + mvTestName.ToLowerCSafe());
                list.Add(MVTCombinationInfo.OBJECT_TYPE + "|byname|" + mvCombinationName.ToLowerCSafe());
                list.Add(MVTVariantInfo.OBJECT_TYPE + "|all");
                list.Add(SettingsKeyInfo.OBJECT_TYPE + "|byname|" + LogViaJavaScriptKeyName.ToLowerCSafe());

                // Add tracked cache dependencies
                cd = list.GetCacheDependency();
            }

            // Create simple cached output
            CachedOutput testoutput = new CachedOutput();
            testoutput.TestCookieName = MVTCOOKIE_PREFIX + mvTestName;

            output.CopyCacheSettingsTo(testoutput);

            // Save to the cache
            OutputHelper.AddToCache(testoutput, cd);

            // Add test and variant name to the cache key 
            output.CacheKey += CacheHelper.SEPARATOR + CacheHelper.GetCacheItemName(null, MVTESTTYPE, mvTestName, mvCombinationName);
        }


        /// <summary>
        /// Tries to fetch correct content from output cache if page MVT testing is enabled.
        /// If page content is not present in cache, indicates that cache should be skipped (fallback to regular load).
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="ea">Event args needed to perform loading from output cache</param>
        private static void SendMVTFromCache(object sender, OutputCacheEventArgs ea)
        {
            CachedOutput cachedOutput = ea.Output;

            if ((cachedOutput == null) || (String.IsNullOrEmpty(cachedOutput.TestCookieName)))
            {
                return;
            }

            // Get test name and type
            if (!cachedOutput.TestCookieName.StartsWithCSafe(MVTCOOKIE_PREFIX))
            {
                return;
            }

            string testName = cachedOutput.TestCookieName.Substring(6);

            // Try get variant name from cookies
            string variantName = CookieHelper.GetValue(cachedOutput.TestCookieName);

            // Generate new variant name
            if (string.IsNullOrEmpty(variantName))
            {
                variantName = GetMVTVariantName(cachedOutput.SiteName, ea.ViewMode, testName);
            }

            if (String.IsNullOrEmpty(variantName))
            {
                ea.Output = null;
                ea.FallbackToRegularLoad = true;
                return;
            }

            // Set MVT Context
            MVTContext.CurrentMVTestName = testName;
            MVTContext.CurrentMVTCombinationName = variantName;

            // Create cache key part
            string cacheKeyPart = CacheHelper.SEPARATOR + CacheHelper.GetCacheItemName(null, MVTESTTYPE, testName, variantName);

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
        /// Gets name of the MV test variant.
        /// </summary>
        /// <param name="siteName">Site name</param>
        /// <param name="viewMode">View mode of the page</param>
        /// <param name="testName">Name of the MVT test</param>
        /// <returns>Returns empty string if variant is not found or is not active.</returns>
        private static string GetMVTVariantName(string siteName, ViewModeEnum viewMode, string testName)
        {
            MVTestInfo mvTest = MVTestInfoProvider.GetMVTestInfo(testName, siteName);
            if ((mvTest == null) || !MVTestInfoProvider.MVTestIsRunning(mvTest))
            {
                return null;
            }

            MVTCombinationInfo combination = MVTCombinationInfoProvider.GetMVTestCombination(mvTest, CultureHelper.GetPreferredCulture(), viewMode);

            return (combination != null) ? combination.MVTCombinationName : null;
        }
        
        #endregion
    }
}
