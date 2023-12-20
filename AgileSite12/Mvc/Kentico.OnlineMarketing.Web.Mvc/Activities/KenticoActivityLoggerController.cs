using System;
using System.Web.Mvc;

using CMS.Core;
using CMS.Helpers;
using CMS.WebAnalytics;

namespace Kentico.Activities.Web.Mvc
{
    /// <summary>
    /// Controller responsible for activity logging via javascript. Provides method for logging an activity
    /// and method which returns script which logs activity via javascript AJAX on client side.
    /// </summary>
    /// <remarks>
    /// Call <c>routes.Kentico().MapActivitiesRoutes();</c> before you register all your routes. Furthermore, include 
    /// script tag in your layout <c>@Scripts.Render(Url.RouteUrl("KenticoLogActivityScript"))</c> which is
    /// <c>@Scripts.Render("Kentico.Resource/Activities/Logger/Logger.js")</c> or create AJAX post request <c>Kentico.Activities/Logger/Log</c>.
    /// Make sure that the AJAX call is performed at every page all required fields are filled correctly (<see cref="KenticoActivityLoggerController.Log"/>).
    /// </remarks>
    public class KenticoActivityLoggerController : Controller
    {
        private readonly IPagesActivityLogger mPagesActivityLogger;


        /// <summary>
        /// Creates an instance of <see cref="KenticoActivityLoggerController"/>
        /// </summary>
        public KenticoActivityLoggerController()
            : this(Service.Resolve<IPagesActivityLogger>())
        {
        }


        /// <summary>
        ///  Creates an instance of <see cref="KenticoActivityLoggerController"/>
        /// </summary>
        /// <param name="pagesActivityLogger">Logger used for activity logging.</param>
        internal KenticoActivityLoggerController(IPagesActivityLogger pagesActivityLogger)
        {
            mPagesActivityLogger = pagesActivityLogger;
        }


#pragma warning disable SCS0016 // Controller method is vulnerable to CSRF

        /// <summary>
        /// Logs activities (pagevisit, landingpage).
        /// </summary>
        [HttpPost]
        public void Log(string title, string url, string referrer)
        {
            LogExternalSearchActivity(url, referrer);
            LogLandingPageActivity(title, url, referrer);
            LogPageVisitActivity(title, url, referrer);

            Response.ContentType = "text/plain";
        }

#pragma warning restore SCS0016 // Controller method is vulnerable to CSRF


        /// <summary>
        /// Returns javascript file which calls <see cref="Log"/> action via AJAX immediately after it is loaded.
        /// </summary>
        public ActionResult Script()
        {
            if (VirtualContext.IsInitialized)
            {
                return new EmptyResult();
            }

            var logUrl = Url.Action("Log");            
            return Content(Scripts.Logger + $"('{logUrl}');", "application/javascript");
        }


        private void LogExternalSearchActivity(string url, string referrer)
        {
            Uri refererUri;
            if (Uri.TryCreate(referrer, UriKind.Absolute, out refererUri))
            {
                mPagesActivityLogger.LogExternalSearch(refererUri, null, url, referrer);
            }
        }


        private void LogLandingPageActivity(string title, string url, string referrer)
        {
            mPagesActivityLogger.LogLandingPage(title, null, url, referrer);
        }


        private void LogPageVisitActivity(string title, string url, string referrer)
        {
            mPagesActivityLogger.LogPageVisit(title, null, null, url, referrer);
        }
    }
}