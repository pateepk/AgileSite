using System;

using CMS;
using CMS.Core;
using CMS.DocumentEngine;
using CMS.Helpers;
using CMS.SiteProvider;
using CMS.WebAnalytics;
using CMS.WebAnalytics.Internal;

[assembly: RegisterImplementation(typeof(ILogHitsProvider), typeof(DefaultLogHitsProvider), Priority = CMS.Core.RegistrationPriority.SystemDefault)]

namespace CMS.WebAnalytics
{
    /// <summary>
    /// Default implementation of <see cref="ILogHitsProvider"/>. Contains methods for performing logging of visitor hits.
    /// </summary>
    internal class DefaultLogHitsProvider : ILogHitsProvider
    {
        /// <summary>
        /// Performs logging of general hit.
        /// </summary>
        /// <param name="logHitParameters">Parameters required for hit logging</param>
        /// <exception cref="ArgumentException">Attempt to found page according to the given <paramref name="logHitParameters"/> was not successful (no page was found).</exception>
        /// <exception cref="InvalidOperationException">Javascript logging is not enabled on the current site.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="logHitParameters"/> is null</exception>
        public void LogHit(LogHitParameters logHitParameters)
        {
            if (logHitParameters == null)
            {
                throw new ArgumentNullException("logHitParameters");
            }

            string siteName = SiteContext.CurrentSiteName;
            if (!AnalyticsHelper.JavascriptLoggingEnabled(siteName))
            {
                throw new InvalidOperationException("[DefaultLogHitsProvider.LogHit]: Javascript logging is not enabled.");
            }

            // Need to fake referrer and current page
            RequestStockHelper.Add("AnalyticsReferrerString", logHitParameters.UrlReferrer);
            PageInfo currentPage = PageInfoProvider.GetPageInfo(siteName, logHitParameters.NodeAliasPath, logHitParameters.DocumentCultureCode, null, true);

            if (currentPage == null)
            {
                throw new ArgumentException("[DefaultLogHitsProvider.LogHit]: Page was not found.");
            }
                
            DocumentContext.CurrentPageInfo = currentPage;

            // Start event
            using (var h = WebAnalyticsEvents.ProcessAnalyticsService.StartEvent(logHitParameters.PlainParameters))
            {
                if (h.CanContinue())
                {
                    if (AnalyticsHelper.IsLoggingEnabled(siteName, currentPage.NodeAliasPath) && Service.Resolve<IAnalyticsConsentProvider>().HasConsentForLogging())
                    {
                        // PAGE VIEW
                        HitLogProvider.LogPageView(siteName, currentPage.DocumentCulture, currentPage.NodeAliasPath, currentPage.NodeID);


                        // VISITOR
                        // BROWSER TYPE
                        // MOBILE DEVICES
                        // BROWSER TYPE
                        // COUNTRIES
                        // Cookies VisitorStatus, VisitStatus (a obsolete CurrentVisitStatus)
                        // IP
                        // method uses AnalyticsHelper.GetContextStatus and AnalyticsHelper.SetContextStatus and also logs all mobile devices
                        AnalyticsMethods.LogVisitor(siteName);


                        // URL REFFERALS
                        // ALL TRAFFIC SOURCES (REFERRINGSITE + "_local") (REFERRINGSITE + "_direct") (REFERRINGSITE + "_search") (REFERRINGSITE + "_referring")
                        // SEARCH KEYWORDS
                        // LANDING PAGE
                        // EXIT PAGE
                        // TIME ON PAGE
                        AnalyticsMethods.LogAnalytics(currentPage, siteName);


                        // CONVERSION
                        AnalyticsMethods.LogConversion(siteName, currentPage.DocumentTrackConversionName, currentPage.DocumentConversionValue);


                        // AGGREGATED VIEW
                        // not logged by JavaScript call


                        // FILE DOWNLOADS
                        // file downloads are logged via special WebPart that directly outputs specified file (and before that logs a hit), so there is no way we can log this via JS


                        // BROWSER CAPABILITIES
                        // are already logged by JavaScript via WebPart AnalayicsBrowserCapabilities


                        // SEARCH CRAWLER
                        // not logged by JavaScript call


                        // INVALID PAGES
                        // throw 404 and there's no need to call it via JavaScript (even if RSS gets 404, we should know about it)


                        // ON-SITE SEARCH KEYWORDS
                        // separate method


                        // BANNER HITS
                        // separate method


                        // BANNER CLICKS
                    }
                    h.FinishEvent();
                }
            }
        }


        /// <summary>
        /// Performs logging of banner hit.
        /// </summary>
        /// <param name="bannerID">ID of the banner the visitor clicked on</param>
        /// <exception cref="InvalidOperationException">Javascript logging is not enabled on the current site.</exception>
        public void LogBannerHit(int bannerID)
        {
            string siteName = SiteContext.CurrentSiteName;
            if (!AnalyticsHelper.JavascriptLoggingEnabled(siteName))
            {
                throw new InvalidOperationException("[DefaultLogHitsProvider.LogBannerHit]: Javascript logging is not enabled.");
            }

            HitLogProvider.LogHit("bannerhit", SiteContext.CurrentSiteName, null, null, bannerID);
        }


        /// <summary>
        /// Performs logging of search event hit. 
        /// </summary>
        /// <param name="logSearchHitParameters">Parameters required for hit logging</param>
        /// <exception cref="ArgumentException">Attempt to found page according to the given <paramref name="logSearchHitParameters"/> was not successful (no page was found).</exception>
        /// <exception cref="InvalidOperationException">Javascript logging is not enabled on the current site.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="logSearchHitParameters"/> is null</exception>
        public void LogSearchHit(LogSearchHitParameters logSearchHitParameters)
        {
            if (logSearchHitParameters == null)
            {
                throw new ArgumentNullException("logSearchHitParameters");
            }

            string siteName = SiteContext.CurrentSiteName;
            if (!AnalyticsHelper.JavascriptLoggingEnabled(siteName))
            {
                throw new InvalidOperationException("[DefaultLogHitsProvider.LogSearchHit]: Javascript logging is not enabled.");
            }

            var currentPage = PageInfoProvider.GetPageInfo(siteName, logSearchHitParameters.NodeAliasPath, logSearchHitParameters.DocumentCultureCode, null, true);
            if (currentPage == null)
            {
                throw new ArgumentException("[DefaultLogHitsProvider.LogSearchHit]: Page was not found.");
            }

            DocumentContext.CurrentPageInfo = currentPage;
            AnalyticsHelper.LogOnSiteSearchKeywords(SiteContext.CurrentSiteName, currentPage.NodeAliasPath, currentPage.DocumentCulture, logSearchHitParameters.Keyword, 0, 1);
        }
    }
}
