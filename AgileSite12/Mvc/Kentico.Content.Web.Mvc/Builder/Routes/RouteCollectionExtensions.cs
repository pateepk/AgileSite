using System;
using System.Web.Mvc;
using System.Web.Routing;

using Kentico.Web.Mvc;

namespace Kentico.Builder.Web.Mvc
{
    /// <summary>
    /// Extends a <see cref="RouteCollection"/> object for MVC routing.
    /// </summary>
    internal static class RouteCollectionExtensions
    {
        /// <summary>
        /// Adds localization route for script to serve translations for resource strings.
        /// </summary>
        /// <param name="extensionPoint">The object that provides methods to add builders routes.</param>
        /// <exception cref="ArgumentNullException"><paramref name="extensionPoint"/> is null.</exception>
        public static void MapLocalizationScriptRoute(this ExtensionPoint<RouteCollection> extensionPoint)
        {
            if (extensionPoint == null)
            {
                throw new ArgumentNullException(nameof(extensionPoint));
            }

            var routes = extensionPoint.Target;

            if (routes[BuilderRoutes.LOCALIZATION_SCRIPT_ROUTE_NAME] != null)
            {
                return;
            }

            routes.MapRoute(
                name: BuilderRoutes.LOCALIZATION_SCRIPT_ROUTE_NAME,
                url: BuilderRoutes.LOCALIZATION_SCRIPT_ROUTE,
                defaults: new
                {
                    controller = BuilderRoutes.LOCALIZATION_SCRIPT_CONTROLLER_NAME,
                    action = "Script"
                }
            );
        }
    }
}
