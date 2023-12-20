using System;
using System.Web;
using System.Web.Routing;

using CMS.Base;
using CMS.DataEngine;
using CMS.DocumentEngine;
using CMS.Helpers;

namespace Kentico.Content.Web.Mvc.Routing
{
    /// <summary>
    /// Constraint that matches provided URL with defined Alternative URLs <seealso cref="AlternativeUrlInfo"/>. Should be used within a catch-all route.
    /// </summary>
    internal class AlternativeUrlConstraint : IRouteConstraint
    {
        private readonly IAlternativeUrlsService mAlternativeUrlsService;
        private readonly ISiteService mSiteService;
        private readonly Func<int, string> mGetBehaviorMode;


        /// <summary>
        /// Initializes a new instance of <see cref="AlternativeUrlConstraint"/>.
        /// </summary>
        public AlternativeUrlConstraint(IAlternativeUrlsService alternativeUrlsService, ISiteService siteService)
            : this (alternativeUrlsService, siteService, (siteId) => SettingsKeyInfoProvider.GetValue("CMSAlternativeURLsMode", siteId))
        {
        }


        /// <summary>
        /// Initializes a new instance of <see cref="AlternativeUrlConstraint"/>.
        /// </summary>
        /// <remarks>This constructor is meant for testing purposes only.</remarks>
        internal AlternativeUrlConstraint(IAlternativeUrlsService alternativeUrlsService, ISiteService siteService, Func<int, string> getBehaviorMode)
        {
            mAlternativeUrlsService = alternativeUrlsService;
            mSiteService = siteService;
            mGetBehaviorMode = getBehaviorMode;
        }


        /// <summary>
        /// Determines whether the parameter matches with any of Alternative URLs <seealso cref="AlternativeUrlInfo"/>. 
        /// </summary>
        /// <param name="httpContext">Object that encapsulates information about the HTTP request.</param>
        /// <param name="route">Object that this constraint belongs to.</param>
        /// <param name="parameterName">Name of the parameter that is being checked.</param>
        /// <param name="values">Object that contains the parameters for the URL.</param>
        /// <param name="routeDirection">Object that indicates whether the constraint check is being performed when an incoming request is being handled or when a URL is being generated.</param>
        public bool Match(HttpContextBase httpContext, Route route, string parameterName, RouteValueDictionary values, RouteDirection routeDirection)
        {
            if (routeDirection == RouteDirection.UrlGeneration)
            {
                return false;
            }

            var currentSiteID = mSiteService.CurrentSite?.SiteID ?? 0;

            if (currentSiteID <= 0)
            {
                return false;
            }

            var path = Convert.ToString(values[parameterName]);
            if (String.IsNullOrEmpty(path))
            {
                return false;
            }

            var documentData = mAlternativeUrlsService.GetMainDocumentData(path);
            if (documentData != null)
            {
                var documentUrlWithQueryString = URLHelper.AppendQuery(documentData.Url, httpContext.Request.QueryString.ToQueryString());

                var mode = mGetBehaviorMode(currentSiteID);

                switch (mode.ToLowerInvariant())
                {
                    case "rewrite":
                    {
                        httpContext.Items[AlternativeUrlsRouteConstants.PAGE_MAIN_URL_CONTEXT_ITEM_NAME] = documentData.Url;
                        httpContext.RewritePath(documentUrlWithQueryString);

                        return false;
                    }
                    default:
                    {
                        // Remember the redirect url for route handler
                        httpContext.Items[AlternativeUrlsRouteConstants.REDIRECT_URL_CONTEXT_ITEM_NAME] = documentUrlWithQueryString;

                        return true;
                    }
                }                            
            }

            return false;
        }
    }
}
