using System;
using System.Collections.Generic;
using System.Linq;

using CMS.Base;
using CMS.Core;
using CMS.DataEngine;
using CMS.DocumentEngine;
using CMS.Helpers;
using CMS.LicenseProvider;
using CMS.Modules;
using CMS.OnlineMarketing.Internal;
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


        #region "Fields"

        private static readonly object abUserStateManagerFactoryInitializationLock = new object();
        private static IABUserStateManagerFactory mABUserStateManagerFactory;
        private static IABTestConversionLogger aBTestConversionLogger;

        #endregion


        #region "Properties"

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


        private static IABTestConversionLogger ABTestConversionLogger => aBTestConversionLogger ?? (aBTestConversionLogger = Service.Resolve<IABTestConversionLogger>());

        #endregion


        #region "Methods"

        /// <summary>
        /// Initializes the events handlers.
        /// </summary>
        public static void Init()
        {
            DocumentEvents.Update.Before += DocumentUpdateOnBefore;

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


        private static void DocumentUpdateOnBefore(object sender, DocumentEventArgs e)
        {
            UpdateAliasPaths(e);
            MaterializeABVariantData(e);
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

            var manager = ABUserStateManagerFactory.Create<string>(test.ABTestName);

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

            var manager = ABUserStateManagerFactory.Create<string>(testName);
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

            string variantName = manager.GetVariantIdentifier();

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

            var manager = ABUserStateManagerFactory.Create<string>(testName);
            if (manager.IsExcluded || !manager.SetVisit())
            {
                return;
            }

            // If the variant name is empty, the user has cookies disabled and we don't know which variant he or she has been given
            string variantName = manager.GetVariantIdentifier();
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
            var logRecord = processLogRecordEventArgs.Parameter;

            // Check whether the context for cookies is available
            if (CMSHttpContext.Current == null)
            {
                return;
            }

            ABTestConversionLogger.LogConversion<string>(RequestContext.CurrentDomain, logRecord.SiteName, logRecord.Culture, logRecord.ObjectName, String.Empty, logRecord.ObjectId, logRecord.Hits, logRecord.Value);
        }


        /// <summary>
        /// Updates paths of AB variants and AB tests when underlying document is moved or updated (document/node and AB variant/test are linked together just by alias path, not FK).
        /// </summary>
        private static void UpdateAliasPaths(DocumentEventArgs e)
        {
            var siteName = e.Node.NodeSiteName;

            if (SiteInfoProvider.GetSiteInfo(siteName).SiteIsContentOnly ||
                !LicenseHelper.CheckFeature(RequestContext.CurrentDomain, FeatureEnum.ABTesting))
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
                                                    .Where(new WhereCondition()
                                                           .Where("ABVariantPath", QueryOperator.Like, originalPath)
                                                           .Or()
                                                           .WhereStartsWith("ABVariantPath", $"{originalPath}/")
                                                    );

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


        /// <summary>
        /// Returns a <see cref="TreeNode"/> which represents an original AB variant for given <paramref name="variantNode"/>.
        /// </summary>
        /// <param name="variantNode">Node which represents AB variant</param>
        internal static TreeNode GetOriginalVariantNode(TreeNode variantNode)
        {
            if (variantNode == null)
            {
                return null;
            }

            if (!ABTestInfoProvider.ABTestingEnabled(variantNode.NodeSiteName)
                || !ResourceSiteInfoProvider.IsResourceOnSite(ModuleName.ABTEST, variantNode.NodeSiteName)
                || !LicenseHelper.CheckFeature(RequestContext.CurrentDomain, FeatureEnum.ABTesting)
                )
            {
                return null;
            }

            var runningTests = ABTestInfoProvider
                               .GetABTests()
                               .Source(s =>
                                   s.Join<ABVariantInfo>("ABTestID", "ABVariantTestID",
                                       additionalCondition: new WhereCondition()
                                                            .WhereEquals("ABVariantPath", variantNode.NodeAliasPath)
                                                            .WhereEquals("ABTestSiteID", variantNode.NodeSiteID)
                                   )
                               )
                               .Columns("ABTestID", "ABTestOriginalPage", "ABTestOpenFrom", "ABTestOpenTo", "ABTestCulture")
                               .Where(ABTestStatusEvaluator.ABTestIsRunning)
                               .Where(t => String.IsNullOrEmpty(t.ABTestCulture) || t.ABTestCulture.Equals(variantNode.DocumentCulture, StringComparison.InvariantCultureIgnoreCase)).ToList();

            if (!runningTests.Any())
            {
                return null;
            }

            var abTest = runningTests.First();

            if (variantNode.NodeAliasPath.Equals(abTest.ABTestOriginalPage, StringComparison.InvariantCultureIgnoreCase))
            {
                return variantNode;
            }

            var originalNode = PageInfoProvider.GetPageInfo(variantNode.NodeSiteName, abTest.ABTestOriginalPage, variantNode.DocumentCulture, null, false);

            return DocumentHelper.GetDocuments().WhereEquals("DocumentID", originalNode.DocumentID).FirstOrDefault();
        }


        private static void MaterializeABVariantData(DocumentEventArgs e)
        {
            var siteName = e.Node.NodeSiteName;

            if (!SiteInfoProvider.GetSiteInfo(siteName).SiteIsContentOnly
                || !ABTestInfoProvider.ABTestingEnabled(siteName)
                || !ResourceSiteInfoProvider.IsResourceOnSite(ModuleName.ABTEST, siteName)
                || !LicenseHelper.CheckFeature(RequestContext.CurrentDomain, FeatureEnum.ABTesting)
            )
            {
                return;
            }

            if (e.Node.ChangedColumns().Contains("DocumentABTestConfiguration", StringComparer.OrdinalIgnoreCase))
            {
                e.CallWhenFinished(() =>
                {
                    var manager = Service.Resolve<IABTestManager>();
                    var abTest = manager?.GetABTestWithoutWinner(e.Node);

                    if (abTest != null)
                    {
                        var variants = manager.GetVariants(e.Node).ToList();
                        ABVariantDataInfoProvider.MaterializeVariants(abTest, variants);
                    }
                });
            }
        }

        #endregion
    }
}