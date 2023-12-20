using System.Web;
using System.Web.Http;
using System.Web.Http.WebHost;
using System.Web.Routing;

namespace CMS.WebApi
{
    /// <summary>
    /// A <see cref="IRouteHandler"/> that returns instances of <see cref="HttpControllerHandler"/> that
    /// can pass requests to a given <see cref="HttpServer"/> instance.
    /// </summary>
    internal class RequiredSessionStateRouteHandler : HttpControllerRouteHandler
    {
        /// <summary>
        /// Provides the object that processes the request.
        /// </summary>
        /// <remarks>
        /// Specifies that the target HTTP handler requires access to session-state values. 
        /// </remarks>
        /// <param name="requestContext">An object that encapsulates information about the request.</param>
        /// <returns>
        /// An object that processes the request.
        /// </returns>
        protected override IHttpHandler GetHttpHandler(RequestContext requestContext)
        {
            return new RequiredSessionStateControllerHandler(requestContext.RouteData);
        }
    }
}