using CMS.SiteProvider;
using System.Web;
using System.Web.Routing;

namespace Kentico.Web.Mvc.Internal
{
    /// <summary>
    /// Route constraint restricting the culture parameter to cultures allowed by the site.
    /// </summary>
    public class SiteCultureConstraint : IRouteConstraint
    {
        /// <summary>
        /// Determines whether the URL parameter contains an allowed culture name for this constraint.
        /// </summary>
        /// <param name="httpContext">Object that encapsulates information about the HTTP request.</param>
        /// <param name="route">Object that this constraint belongs to.</param>
        /// <param name="parameterName">Name of the parameter that is being checked.</param>
        /// <param name="values">Object that contains the parameters for the URL.</param>
        /// <param name="routeDirection">Object that indicates whether the constraint check is being performed when an incoming request is being handled or when a URL is being generated.</param>
        /// <returns>True if the URL parameter contains an allowed culture name; otherwise, false.</returns>
        public bool Match(HttpContextBase httpContext, Route route, string parameterName, RouteValueDictionary values, RouteDirection routeDirection)
        {
            string cultureCodeName = values[parameterName]?.ToString();

            return CultureSiteInfoProvider.IsCultureOnSite(cultureCodeName, SiteContext.CurrentSiteName);
        }
    }
}
