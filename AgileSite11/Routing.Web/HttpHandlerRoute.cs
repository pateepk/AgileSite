using System.Web.Routing;

namespace CMS.Routing.Web
{
    /// <summary>
    /// Represents a route for Kentico HTTP handlers.
    /// </summary>
    internal sealed class HttpHandlerRoute : Route
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HttpHandlerRoute"/> class.
        /// </summary>
        /// <param name="url">The URL pattern for the route.</param>
        /// <param name="routeHandler">The object that processes requests for the route.</param>
        public HttpHandlerRoute(string url, IRouteHandler routeHandler) : base(url, routeHandler)
        {

        }


        /// <summary>
        /// Returns information about the URL that is associated with the route.
        /// </summary>
        /// <param name="requestContext">An object that encapsulates information about the requested route.</param>
        /// <param name="values">An object that contains the parameters for a route.</param>
        /// <remarks>
        /// This method always returns <c>null</c> to prevent generation of links to Kentico HTTP handlers in external web applications.
        /// </remarks>
        public override VirtualPathData GetVirtualPath(RequestContext requestContext, RouteValueDictionary values)
        {
            return null;
        }
    }
}