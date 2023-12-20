using System;
using System.Web.Mvc;

using CMS.Base;
using CMS.Core;
using CMS.Helpers;
using CMS.OnlineMarketing;
using CMS.OnlineMarketing.Internal;

using Kentico.Content.Web.Mvc.Routing;
using Kentico.OnlineMarketing.Web.Mvc.ABTest.Content;

namespace Kentico.OnlineMarketing.Web.Mvc
{
    /// <summary>
    /// Controller responsible for A/B test conversion logging via javascript. Provides method for logging conversions
    /// and method which returns script for logging via javascript AJAX on client side.
    /// </summary>
    /// <remarks>
    /// Enable A/B testing via application builder <c>IApplicationBuilder.UseABTesting()</c>.
    /// Furthermore, include script in your layout <c>@Html.Kentico().ABTestLoggerScript()</c>.
    /// </remarks>
    public class KenticoABTestLoggerController : Controller
    {
        private readonly IABTestLogger mAbTestLogger;
        private readonly ISiteService mSiteService;
        private readonly IAlternativeUrlsService mAlternativeUrlsService;


        /// <summary>
        /// Creates an instance of <see cref="KenticoABTestLoggerController"/>
        /// </summary>
        public KenticoABTestLoggerController()
            : this(Service.Resolve<IABTestLogger>(), Service.Resolve<ISiteService>(), Service.Resolve<IAlternativeUrlsService>())
        {
        }


        /// <summary>
        ///  Creates an instance of <see cref="KenticoABTestLoggerController"/>
        /// </summary>
        /// <param name="abTestLogger">Logger used for A/B test conversion logging.</param>
        /// <param name="siteService">Site service.</param>
        /// <param name="alternativeUrlsService">Service providing work with alternative URLs.</param>
        internal KenticoABTestLoggerController(IABTestLogger abTestLogger, ISiteService siteService, IAlternativeUrlsService alternativeUrlsService)
        {
            mAbTestLogger = abTestLogger;
            mSiteService = siteService;
            mAlternativeUrlsService = alternativeUrlsService;
        }


#pragma warning disable SCS0016 // Controller method is vulnerable to CSRF

        /// <summary>
        /// Logs page visit conversion.
        /// </summary>
        [HttpPost]
        public void LogPageVisit(string url, string culture)
        {
            LogPageVisitConversion(url, culture);

            Response.ContentType = "text/plain";
        }

#pragma warning restore SCS0016 // Controller method is vulnerable to CSRF


        /// <summary>
        /// Returns javascript file which calls logging action via AJAX immediately after it is loaded.
        /// </summary>
        /// <param name="abtest">Optional A/B test visit test name</param>
        public ContentResult Script(string abtest = null)
        {
            LogABVisit(abtest);

            var culture = RouteData.Values[ABTestRouteConstants.CULTURE_ROUTE_DATA_KEY];

            var logUrl = Url.Action("LogPageVisit");

            return Content(ConversionScripts.ConversionLogger + $"('{logUrl}', '{culture}');", "application/javascript");
        }


        private void LogABVisit(string testName)
        {
            if (!VirtualContext.IsInitialized && !String.IsNullOrWhiteSpace(testName))
            {
                var abTest = ABTestInfoProvider.GetABTestInfo(testName, mSiteService.CurrentSite?.SiteName);
                if (abTest != null)
                {
                    var stateManager = Service.Resolve<IABUserStateManagerFactory>().Create<Guid?>(testName);
                    var variantIdentifier = stateManager.GetVariantIdentifier();

                    if (!stateManager.IsExcluded && variantIdentifier.HasValue)
                    {
                        stateManager.SetVisitToSession();
                    }
                }
            }
        }


        private void LogPageVisitConversion(string url, string culture)
        {
            if (VirtualContext.IsInitialized || !Uri.TryCreate(url, UriKind.Absolute, out var uri))
            {
                return;
            }

            if (!uri.TryGetRelativePath(out var relativePath))
            {
                return;
            }

            var siteName = mSiteService.CurrentSite.SiteName;

            var mainDocumentData = mAlternativeUrlsService.GetMainDocumentData(relativePath);
            if (mainDocumentData != null)
            {
                mAbTestLogger.LogConversion(ABTestConversionNames.PAGE_VISIT, siteName, mainDocumentData.Url.TrimStart('~'), culture: mainDocumentData.Culture);
            }
            else
            {
                mAbTestLogger.LogConversion(ABTestConversionNames.PAGE_VISIT, siteName, relativePath, culture: culture);
            }
        }
    }
}
