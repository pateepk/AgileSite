using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;

using CMS.Core;
using CMS.EventLog;
using CMS.WebAnalytics.Internal;
using CMS.WebAnalytics.Web.UI;
using CMS.WebAnalytics.Web.UI.Filters;
using CMS.WebApi;

[assembly: RegisterCMSApiController(typeof(WebAnalyticsController), RequiresSessionState = false)]

namespace CMS.WebAnalytics.Web.UI
{
    /// <summary>
    /// Handles logging of visitor hits.
    /// </summary>
    /// <remarks>
    /// Only real visitors ban be logged, crawlers which the system is able to recognize are omitted.
    /// Adds information to the header to tell the crawlers not to track this controller. This applies only to the crawlers that was
    /// not recognized in the <see cref="FilterCrawlersAttribute"/>.
    /// </remarks>
    [DoNotTrack]
    [FilterCrawlers]
    public sealed class WebAnalyticsController : CMSApiController
    {
        private readonly ILogHitsProvider mLogHitsProvider = Service.Resolve<ILogHitsProvider>();

        /// <summary>
        /// Performs logging of general hit.
        /// </summary>
        /// <param name="logHitParameters">Parameters required for hit logging</param>
        /// <returns>
        /// HTTP response message containing the status code dependent on whether the action was successful or not.
        /// Returns 200 OK status if logging was successful.
        /// Returns 400 Bad request if exception was thrown while trying to log the hit.
        /// </returns>
        [HttpPost]
        public HttpResponseMessage LogHit([FromBody]LogHitParameters logHitParameters)
        {
            try
            {
                mLogHitsProvider.LogHit(logHitParameters);
            }
            catch (Exception ex)
            {
                EventLogProvider.LogException("Web analytics", "LogHit", ex);
                return new HttpResponseMessage(HttpStatusCode.BadRequest);
            }

            return new HttpResponseMessage(HttpStatusCode.OK);
        }


        /// <summary>
        /// Performs logging of search event hit. 
        /// </summary>
        /// <param name="logSearchHitParameters">Parameters required for hit logging</param>
        /// <returns>
        /// HTTP response message containing the status code dependent on whether the action was successful or not.
        /// Returns 200 OK status if logging was successful.
        /// Returns 400 Bad request if exception was thrown while trying to log the hit.
        /// </returns>
        [HttpPost]
        public HttpResponseMessage LogSearchHit([FromBody]LogSearchHitParameters logSearchHitParameters)
        {
            try
            {
                mLogHitsProvider.LogSearchHit(logSearchHitParameters);
            }
            catch (Exception ex)
            {
                EventLogProvider.LogException("Web analytics", "LogSearchHit", ex);
                return new HttpResponseMessage(HttpStatusCode.BadRequest);
            }

            return new HttpResponseMessage(HttpStatusCode.OK);
        }


        /// <summary>
        /// Performs logging of banner hit.
        /// </summary>
        /// <param name="bannerID">ID of the banner the visitor clicked on</param>
        /// <returns>
        /// HTTP response message containing the status code dependent on whether the action was successful or not.
        /// Returns 200 OK status if logging was successful.
        /// Returns 400 Bad request if exception was thrown while trying to log the hit.
        /// </returns>
        [HttpPost]
        public HttpResponseMessage LogBannerHit([FromBody]int bannerID)
        {
            try
            {
                mLogHitsProvider.LogBannerHit(bannerID);
            }
            catch (Exception ex)
            {
                EventLogProvider.LogException("Web analytics", "LogBannerHit", ex);
                return new HttpResponseMessage(HttpStatusCode.BadRequest);
            }

            return new HttpResponseMessage(HttpStatusCode.OK);
        }
    }
}
