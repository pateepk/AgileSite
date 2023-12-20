using System.Globalization;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Kentico.PageBuilder.Web.Mvc
{
    /// <summary>
    /// Creates an object that implements the <see cref="IHttpHandler"/> interface and passes the request context to it.
    /// Configures the current thread to use the culture specified by the request's RouteData <see cref="PageBuilderConstants.CULTURE_ROUTE_KEY" /> parameter.
    /// </summary>
    internal class PageBuilderCultureRouteHandler : MvcRouteHandler
    {
        /// <summary>
        /// Creates a new instance of the <see cref="PageBuilderCultureRouteHandler"/> class.
        /// </summary>
        public PageBuilderCultureRouteHandler()
        {
        }


        /// <summary>
        /// Returns the HTTP handler by using the specified HTTP context. 
        /// <see cref="Thread.CurrentCulture"/> and <see cref="Thread.CurrentUICulture"/> of the current thread are set to the culture specified by the request's RouteData <see cref="PageBuilderConstants.CULTURE_ROUTE_KEY" /> parameter.
        /// </summary>
        /// <param name="requestContext">Request context.</param>
        /// <returns>HTTP handler.</returns>
        protected override IHttpHandler GetHttpHandler(RequestContext requestContext)
        {
            return GetHttpHandlerInternal(requestContext);
        }


        internal IHttpHandler GetHttpHandlerInternal(RequestContext requestContext)
        {
            if (requestContext.RouteData.Values.ContainsKey(PageBuilderConstants.CULTURE_ROUTE_KEY))
            {
                var cultureName = requestContext.RouteData.Values[PageBuilderConstants.CULTURE_ROUTE_KEY].ToString();

                var culture = new CultureInfo(cultureName);

                Thread.CurrentThread.CurrentUICulture = culture;
                Thread.CurrentThread.CurrentCulture = culture;
            }

            return GetBaseHttpHandler(requestContext);
        }


        internal virtual IHttpHandler GetBaseHttpHandler(RequestContext requestContext)
        {
            return base.GetHttpHandler(requestContext);
        }
    }
}
