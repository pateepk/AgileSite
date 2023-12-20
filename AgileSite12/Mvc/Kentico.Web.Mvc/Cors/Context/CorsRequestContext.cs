using System;

using CMS.Base;
using CMS.SiteProvider;

namespace Kentico.Web.Mvc
{
    /// <summary>
    /// Provides access to CORS-related informations of the request.
    /// </summary>
    internal sealed class CorsRequestContext : ICorsRequestContext
    {
        private readonly IRequest request;
        private bool? isCurrentSiteOrigin;


        /// <summary>
        /// Gets the Origin header value.
        /// </summary>
        public string Origin => request.Headers[CorsConstants.ORIGIN];


        /// <summary>
        /// Gets the Access-Control-Request-Method header value.
        /// </summary>
        public string AccessControlRequestMethod => request.Headers[CorsConstants.ACCESS_CONTROL_REQUEST_METHOD];


        /// <summary>
        /// Gets the request HTTP method.
        /// </summary>
        public string HttpMethod => request.HttpMethod;


        /// <summary>
        /// Gets a value indicating whether this is a preflight request.
        /// </summary>
        public bool IsPreflight
        {
            get
            {
                return !String.IsNullOrEmpty(Origin)
                    && !String.IsNullOrEmpty(AccessControlRequestMethod)
                    && String.Equals(HttpMethod, CorsConstants.PREFLIGHT_HTTP_METHOD, StringComparison.OrdinalIgnoreCase);
            }
        }


        /// <summary>
        /// Gets a value indicating if the requestHeaders origin domain is one of current site's domains. 
        /// </summary>
        public bool IsCurrentSiteOrigin
        {
            get
            {
                isCurrentSiteOrigin = isCurrentSiteOrigin ?? IsCurrentSiteOriginInternal(Origin);
                return isCurrentSiteOrigin == true;
            }
        }


        /// <summary>
        /// Creates a new instance of <see cref="CorsRequestContext"/>.
        /// </summary>
        /// <param name="httpContext">HTTP Context</param>
        /// <exception cref="ArgumentNullException">Throws when <paramref name="httpContext"/> is <c>null</c>.</exception>
        public CorsRequestContext(IHttpContext httpContext)
        {
            httpContext = httpContext ?? throw new ArgumentNullException(nameof(httpContext));
            this.request = httpContext.Request;
        }


        /// <summary>
        /// Finds out if the <paramref name="requestOrigin"/> (URI) belongs to current site's administration
        /// so the request origin can be considered as trusted.
        /// </summary>
        /// <param name="requestOrigin">URI of the request origin</param>
        /// <returns>True, if the requestHeaders origin domain is one of current site's domains</returns>
        private bool IsCurrentSiteOriginInternal(string requestOrigin)
        {
            if (requestOrigin == null)
            {
                return false;
            }

            string originSiteName = SiteInfoProvider.GetSiteNameFromUrl(requestOrigin);
            string currentSiteName = SiteContext.CurrentSite.SiteName;

            return String.Equals(originSiteName, currentSiteName, StringComparison.InvariantCultureIgnoreCase);
        }
    }
}
