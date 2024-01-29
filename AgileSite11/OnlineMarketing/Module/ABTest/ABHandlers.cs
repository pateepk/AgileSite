using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using CMS.Base;
using CMS.Core;
using CMS.DataEngine;
using CMS.DocumentEngine;
using CMS.Helpers;
using CMS.LicenseProvider;
using CMS.SiteProvider;
using CMS.WebAnalytics;

namespace CMS.OnlineMarketing
{
    /// <summary>
    /// AB testing event handlers.
    /// </summary>
    internal class ABHandlers
    {
        #region "Constants"

        /// <summary>
        /// Code name of the first session conversion used in log files.
        /// </summary>
        private const string ABSESSIONCONVERSION_FIRST = "absessionconversionfirst";


        /// <summary>
        /// Code name of the recurring session conversion used in log files.
        /// </summary>
        private const string ABSESSIONCONVERSION_RECURRING = "absessionconversionrecurring";


        /// <summary>
        /// Conversion name.
        /// </summary>
        private const string ABCONVERSION = "abconversion";

        
        /// <summary>
        /// Used as a query parameter when logging analytics via JavaScript. 
        /// </summary>
        private const string PARAM_ABTEST_NAME = "ABTestName";


        /// <summary>
        /// Used as a query parameter when logging analytics via JavaScript. 
        /// </summary>
        private const string PARAM_ABTEST_VARIANT_NAME = "ABTestVariantName";


        /// <summary>
        /// Used as a query parameter when logging analytics via JavaScript. 
        /// </summary>
        private const string PARAM_ABVISIT_FIRST = "ABTestFirstVisit";
        
        #endregion


        #region "Methods"

        /// <summary>
        /// Initializes the events handlers.
        /// </summary>
        public static void Init()
        {
            DocumentEvents.Update.Before += UpdateAliasPaths;

            if (SystemContext.IsCMSRunningAsMainApplication)
            {
                WebAnalyticsEvents.LogConversion.Before += LogABConversion;

                // Excluding visitors based on macro has to be on PostMapRequestHandler event because it is not too late to change current PageInfo if visitor did not meet traffic or macro conditions
                // This is also the last event that switching DocumentContext properties is possible (because of permission checks)
                // This event is not called when using Full page caching
                RequestEvents.PostMapRequestHandler.Execute += ExcludeVisitorFromAB;

                // Log AB request has to be on AcquireRequestState event because all needed context (session for getting already logged
                // conversions that the contact has done) information are available. This event is not called when using Full page caching
                RequestEvents.PostAcquireRequestState.Execute += LogABVisit;
                
                WebAnalyticsEvents.InsertAnalyticsJS.After += InsertABTestJavaScriptWebServiceParameters;
                WebAnalyticsEvents.ProcessAnalyticsService.Before += LogABVisitViaJavascript;
            }
        }
        

        /// <summary>
        /// Checks traffic and macro condition to decide whether exclude visitor from test or not.
        /// </summary>
        private static void ExcludeVisitorFromAB(object sender, EventArgs e)
        {
            var test = ABTestContext.CurrentABTest;
            if (test == null)
            {
                return;
            }

            var manager = new ABUserStateManager(test.ABTestName);

            if (!RequestHelper.IsPostBack() && ABTestContext.IsFirstABRequest)
            {
                ABRequestManager.ExcludeVisitorsBasedOnSegmentationConditions(test, manager);
            }
        }


        /// <summary>
        /// Checks whether the request is on AB test and user got assigned AB test variant.
        /// Context properties (<see cref="ABTestContext.CurrentABTest"/>, <see cref="ABTestContext.CurrentABTestVariant"/>, <see cref="ABTestContext.IsFirstABRequest"/>) 
        /// used are assigned in ABTestInfoProvider when AB variant for a PageInfo is requested (not in this handler).
        /// </summary>
        private static void LogABVisit(object sender, EventArgs e)
        {
            var test = ABTestContext.CurrentABTest;
            if (test == null)
            {
                return;
            }

            string testName = test.ABTestName;

            var manager = new ABUserStateManager(testName);
            bool isFirstVisit = ABTestContext.IsFirstABRequest;

            // If logging via Javascript is enabled then don't log AB visit now
            if (AnalyticsHelper.JavascriptLoggingEnabled(SiteContext.CurrentSiteName))
            {
                return;
            }

            if (!Service.Resolve<IAnalyticsConsentProvider>().HasConsentForLogging())
            {
                return;
            }

            // Check if the page is excluded
            if (manager.IsExcluded || !manager.SetVisit())
            {
                return;
            }

            string variantName = manager.GetVariantName();

            // Log the visit
            LogABVisit(testName, variantName, isFirstVisit);
        }


        /// <summary>
        /// Logs AB visit.
        /// </summary>
        /// <param name="testName">AB test name</param>
        /// <param name="variantName">AB variant name</param>
        /// <param name="isFirstVisit">Indicates whether the request is user's first visit of the page</param>
        /// <param name="page">Page the user visits, uses DocumentContext.CurrentPageInfo if left empty</param>
        private static void LogABVisit(string testName, string variantName, bool isFirstVisit, PageInfo page = null)
        {
            if (page == null)
            {
                page = DocumentContext.CurrentPageInfo;
            }

            var visitType = isFirstVisit ? "first" : "return";
            string codeName = string.Format("abvisit{0};{1};{2}", visitType, testName, variantName);
            HitLogProvider.LogHit(codeName, page.SiteName, page.DocumentCulture, page.NodeAliasPath, 0);
        }


        /// <summary>
        /// Logs AB visit via JavaScript snippet.
        /// </summary>
        private static void LogABVisitViaJavascript(object sender, AnalyticsJSEventArgs e)
        {
            if (!AnalyticsHelper.JavascriptLoggingEnabled(SiteContext.CurrentSiteName))
            {
                return;
            }

            if (!Service.Resolve<IAnalyticsConsentProvider>().HasConsentForLogging())
            {
                return;
            }

            Dictionary<string, string> queryParams = e.QueryParameters;
            if ((queryParams == null) || !queryParams.ContainsKey(PARAM_ABTEST_NAME) || !queryParams.ContainsKey(PARAM_ABVISIT_FIRST))
            {
                return;
            }

            string testName = queryParams[PARAM_ABTEST_NAME];

            if (String.IsNullOrEmpty(testName))
            {
                return;
            }

            var manager = new ABUserStateManager(testName);
            if (manager.IsExcluded || !manager.SetVisit())
            {
                return;
            }

            // If the variant name is empty, the user has cookies disabled and we don't know which variant he or she has been given
            string variantName = manager.GetVariantName();
            if (String.IsNullOrEmpty(variantName))
            {
                return;
            }

            bool isFirstVisit = queryParams[PARAM_ABVISIT_FIRST].ToBoolean(true);

            LogABVisit(testName, variantName, isFirstVisit);
        }


        /// <summary>
        /// Inserts additional query parameters to the web analytics Javascript snippet.
        /// </summary>
        private static void InsertABTestJavaScriptWebServiceParameters(object sender, AnalyticsJSEventArgs e)
        {
            var test = ABTestContext.CurrentABTest;
            if (test == null)
            {
                return;
            }

            string testName = test.ABTestName;
            Dictionary<string, string> queryParams = e.QueryParameters;

            if (string.IsNullOrEmpty(testName) || (queryParams == null))
            {
                return;
            }

            var variant = ABTestContext.CurrentABTestVariant;
            if (variant != null)
            {
                queryParams.Add(PARAM_ABTEST_VARIANT_NAME, variant.ABVariantName);
            }

            queryParams.Add(PARAM_ABTEST_NAME, testName);
            queryParams.Add(PARAM_ABVISIT_FIRST, ABTestContext.IsFirstABRequest.ToString());
        }


        /// <summary>
        /// Logs AB conversions.
        /// </summary>
        private static void LogABConversion(object sender, CMSEventArgs<LogRecord> processLogRecordEventArgs)
        {
            if (!LicenseHelper.CheckFeature(RequestContext.CurrentDomain, FeatureEnum.ABTesting))
            {
                return;
            }

            var logRecord = processLogRecordEventArgs.Parameter;

            // Check whether the context is available so we can use cookies
            if (HttpContext.Current != null)
            {
                string siteName = logRecord.SiteName;
                string culture = logRecord.Culture;
                string objectName = logRecord.ObjectName;
                int objectId = logRecord.ObjectId;
                int count = logRecord.Hits;
                double value = logRecord.Value;

                // Check whether AB testing is enabled and try log AB testing conversion
                if (ABTestInfoProvider.ABTestingEnabled(siteName))
                {
                    LogABTestConversion(siteName, culture, objectName, objectId, count, value);
                }
            }
        }


        /// <summary>
        /// Checks cookies and logs AB test conversions.
        /// </summary>
        /// <param name="siteName">Site name</param>
        /// <param name="culture">Culture</param>
        /// <param name="conversionName">Conversion name</param>
        /// <param name="objectId">Object ID</param>
        /// <param name="count">Conversions count</param>
        /// <param name="value">Conversions value</param>
        private static void LogABTestConversion(string siteName, string culture, string conversionName, int objectId, int count, double value)
        {
            if (!Service.Resolve<IAnalyticsConsentProvider>().HasConsentForLogging())
            {
                return;
            }

            // Loop through user's AB test cookies
            foreach (string testName in ABUserStateManager.GetUsersTests())
            {
                // Get the test
                ABTestInfo abTest = ABTestInfoProvider.GetABTestInfo(testName, siteName);

                // Log AB conversion if the test is running and user is not excluded from it
                if (abTest != null && ABTestStatusEvaluator.ABTestIsRunning(abTest))
                {
                    var manager = new ABUserStateManager(abTest.ABTestName);
                    if (manager.IsExcluded)
                    {
                        continue;
                    }

                    var variantName = manager.GetVariantName();
                    if (!string.IsNullOrEmpty(variantName))
                    {
                        // Log first conversion if the conversion isn't saved as permanent
                        if (!manager.GetPermanentConversions().Contains(conversionName))
                        {
                            HitLogProvider.LogHit(ABSESSIONCONVERSION_FIRST + ";" + testName + ";" + variantName, siteName, culture, conversionName, objectId, count, value);
                        }
                        // Log recurring conversion if the conversion isn't saved as session
                        else if (!manager.GetSessionConversions().Contains(conversionName) && manager.IsABVisit())
                        {
                            HitLogProvider.LogHit(ABSESSIONCONVERSION_RECURRING + ";" + testName + ";" + variantName, siteName, culture, conversionName, objectId, count, value);
                        }

                        manager.AddConversion(conversionName);

                        // Always log transaction conversion
                        HitLogProvider.LogHit(ABCONVERSION + ";" + testName + ";" + variantName, siteName, culture, conversionName, objectId, count, value);
                    }
                }
            }
        }


        /// <summary>
        /// Updates paths of AB variants and AB tests when underlying document is moved or updated (document/node and AB variant/test are linked together just by alias path, not FK).
        /// </summary>
        private static void UpdateAliasPaths(object sender, DocumentEventArgs e)
        {
            if (!LicenseHelper.CheckFeature(RequestContext.CurrentDomain, FeatureEnum.ABTesting))
            {
                return;
            }

            var originalPath = e.Node.NodeAliasPath;

            e.CallWhenFinished(() =>
            {
                var currentPath = e.Node.NodeAliasPath;

                if (originalPath == currentPath)
                {
                    return;
                }

                var variants = ABVariantInfoProvider.GetVariants()
                                                    .OnSite(SiteContext.CurrentSiteID)
                                                    .Where(v => v.ABVariantPath.Equals(originalPath, StringComparison.InvariantCulture) || v.ABVariantPath.StartsWith(originalPath + "/", StringComparison.InvariantCulture));
                foreach (var variant in variants)
                {
                    variant.ABVariantPath = currentPath + variant.ABVariantPath.Substring(originalPath.Length);
                    ABVariantInfoProvider.SetABVariantInfo(variant);
                }

                var tests = ABCachedObjects.GetTests()
                                           .Where(t => (t.ABTestSiteID == SiteContext.CurrentSiteID) &&
                                                       (t.ABTestOriginalPage.Equals(originalPath, StringComparison.InvariantCulture) || t.ABTestOriginalPage.StartsWith(originalPath + "/", StringComparison.InvariantCulture)));
                foreach (var test in tests)
                {
                    test.ABTestOriginalPage = currentPath + test.ABTestOriginalPage.Substring(originalPath.Length);
                    ABTestInfoProvider.SetABTestInfo(test);
                }
            });
        }
        
        #endregion
    }
}