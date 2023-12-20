using System;
using System.Web.Mvc;
using System.Web.Routing;

using Kentico.Web.Mvc;

namespace Kentico.Activities.Web.Mvc
{
    /// <summary>
    /// Extends a <see cref="RouteCollection"/> object for MVC routing.
    /// </summary>
    public static class RouteCollectionExtensions
    {
        private static bool routesMapped;


        /// <summary>
        /// Adds Activities routes.
        /// </summary>
        /// <param name="extensionPoint">The object that provides methods to add Kentico Activities routes.</param>
        /// <remarks>Routes are fixed with namespace prefix.</remarks>
        /// <exception cref="ArgumentNullException"><paramref name="extensionPoint"/> is null.</exception>
        [Obsolete("Use Kentico.Activities.Web.Mvc.ApplicationBuilderExtensions.UseActivityTracking() to enable the activity tracking feature. The routes are registered automatically as part of the feature.")]
        // When resolving this obsolete member, do not remove it completely. Make the class internal instead
        public static void MapActivitiesRoutes(this ExtensionPoint<RouteCollection> extensionPoint)
        {
            if (extensionPoint == null)
            {
                throw new ArgumentNullException(nameof(extensionPoint));
            }

            if (routesMapped)
            {
                // Prevent duplicate routes mapping in case the UseActivityTracking method is called in App_Start while direct call to this obsolete member is not removed from RouteConfig
                // Can be removed when resolving this obsolete member
                return;
            }

            var routes = extensionPoint.Target;

            routes.MapRoute(
                name: "KenticoLogActivity",
                url: "Kentico.Activities/KenticoActivityLogger/Log",
                defaults: new { controller = "KenticoActivityLogger", action = "Log" },
                namespaces: new[] { typeof(RouteCollectionExtensions).Namespace }
                );

            routes.MapRoute(
                name: "KenticoLogActivityScript",
                url: "Kentico.Resource/Activities/KenticoActivityLogger/Logger.js",
                defaults: new { controller = "KenticoActivityLogger", action = "Script" },
                namespaces: new[] { typeof(RouteCollectionExtensions).Namespace }
                );

            routesMapped = true;
        }
    }
}
