using System.Web;
using System.Web.Http;
using System.Web.Http.WebHost;
using System.Web.Routing;
using System.Web.SessionState;

namespace CMS.WebApi
{
    /// <summary>
    /// A <see cref="IHttpAsyncHandler"/> that passes ASP.NET requests into the <see cref="HttpServer"/>
    /// pipeline and write the result back.
    /// </summary>
    /// <remarks>
    /// Specifies that the target HTTP handler requires access to session-state values. 
    /// </remarks>
    internal class RequiredSessionStateControllerHandler : HttpControllerHandler, IRequiresSessionState
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HttpControllerHandler"/> class.
        /// </summary>
        /// <param name="routeData">The route data.</param>
        public RequiredSessionStateControllerHandler(RouteData routeData)
            : base(routeData)
        {
        }
    }
}