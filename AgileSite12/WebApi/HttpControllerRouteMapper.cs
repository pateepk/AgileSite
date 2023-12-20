using System;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using System.Web.Routing;

namespace CMS.WebApi
{
    /// <summary>
    /// Maps routes served by Kentico CMS API controllers.
    /// </summary>
    internal static class HttpControllerRouteMapper
    {
        /// <summary>
        /// Maps a route served by CMS API controller into <paramref name="routes"/>.
        /// </summary>
        /// <param name="routes">Route collection to map route in.</param>
        /// <param name="controllerConfiguration">Controller configuration.</param>
        public static void MapRoute(RouteCollection routes, CMSApiControllerConfiguration controllerConfiguration)
        {
            string controllerName = GetControllerName(controllerConfiguration.ControllerType.Name);

            if (String.IsNullOrEmpty(controllerName) || IsRouteMapped(routes, controllerName))
            {
                return;
            }

            var defaults = new { controller = controllerName, action = RouteParameter.Optional };
            var messageHandler = controllerConfiguration.RequiresSessionState ? GetSessionHttpMessageHandler() : null;

            var route = routes.MapHttpRoute(controllerName, $"cmsapi/{controllerName}/{{action}}/", defaults, null, messageHandler);
            route.RouteHandler = controllerConfiguration.RequiresSessionState ? new RequiredSessionStateRouteHandler() : route.RouteHandler;
        }


        /// <summary>
        /// Gets name of Controller without <c>"Controller"</c> suffix.
        /// </summary>
        /// <param name="controllerTypeName">Controller type name.</param>
        /// <returns>Controller type name without <c>"Controller"</c> suffix or empty string, if <paramref name="controllerTypeName"/> does not ends with <c>"Controller"</c>.</returns>
        private static string GetControllerName(string controllerTypeName)
        {
            const string controllerSuffix = "Controller";

            if (!controllerTypeName.EndsWith(controllerSuffix, StringComparison.OrdinalIgnoreCase))
            {
                return String.Empty;
            }

            var controllerNameIdex = controllerTypeName.LastIndexOf(controllerSuffix, StringComparison.OrdinalIgnoreCase);

            return controllerTypeName.Remove(controllerNameIdex);
        }


        /// <summary>
        /// Returns <c>true</c> if a route served by <paramref name="controllerName"/> is mapped in <paramref name="routes"/>.
        /// </summary>
        /// <param name="routes">Routes.</param>
        /// <param name="controllerName">Controller name.</param>
        private static bool IsRouteMapped(RouteCollection routes, string controllerName)
        {
            return routes[controllerName] != null;
        }


        /// <summary>
        /// Gets instance of <see cref="HttpMessageHandler"/> that ensures the session is available for the Web API requests.
        /// </summary>
        /// <returns><see cref="HttpMessageHandler"/> ensuring the session is available.</returns>
        private static HttpMessageHandler GetSessionHttpMessageHandler()
        {
            return HttpClientFactory.CreatePipeline(
                new HttpControllerDispatcher(GlobalConfiguration.Configuration),
                new DelegatingHandler[]
                {
                    new EnsureSessionMessageHandler()
                });
        }
    }
}