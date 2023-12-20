using System;
using System.Net;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

using CMS.Core;
using CMS.SiteProvider;
using CMS.WebAnalytics.Internal;

namespace CMS.WebAnalytics.Web.UI.Filters
{
    /// <summary>
    /// Checks whether the request was made by crawler. If so, returns 403 Forbidden message.
    /// </summary>
    internal class FilterCrawlersAttribute : AuthorizationFilterAttribute
    {
        private readonly ISearchEnginesDetector mSearchEnginesDetector;


        /// <summary>
        /// Creates new instance of <see cref="FilterCrawlersAttribute"/> with <see cref="ISearchEnginesDetector"/> resolved from service locator.
        /// </summary>
        public FilterCrawlersAttribute()
            : this(Service.Resolve<ISearchEnginesDetector>())
        {
        }


        /// <summary>
        /// Creates new instance of <see cref="FilterCrawlersAttribute"/>.
        /// </summary>
        /// <param name="searchEnginesDetector">Implementation of <see cref="ISearchEnginesDetector"/> used to detect seach engines.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="searchEnginesDetector"/> is <c>null</c>.</exception>
        public FilterCrawlersAttribute(ISearchEnginesDetector searchEnginesDetector)
        {
            if (searchEnginesDetector == null)
            {
                throw new ArgumentNullException("searchEnginesDetector");
            }

            mSearchEnginesDetector = searchEnginesDetector;
        }


        /// <summary>
        /// Calls when a process requests authorization.
        /// </summary>
        /// <param name="actionContext">The action context, which encapsulates information for using <see cref="AuthorizationFilterAttribute"/>.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="actionContext"/> is null.</exception>
        public override void OnAuthorization(HttpActionContext actionContext)
        {
            if (actionContext == null)
            {
                throw new ArgumentNullException("actionContext");
            }

            var siteName = SiteContext.CurrentSite.SiteName;

            if (mSearchEnginesDetector.IsSearchEngine(siteName))
            {
                actionContext.Response = actionContext.Request.CreateResponse(HttpStatusCode.Forbidden);
            }
        }
    }
}
