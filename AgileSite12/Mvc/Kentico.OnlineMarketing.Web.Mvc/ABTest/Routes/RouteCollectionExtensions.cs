using System;
using System.Web.Mvc;
using System.Web.Routing;

using Kentico.Web.Mvc;
using Kentico.Web.Mvc.Internal;

namespace Kentico.OnlineMarketing.Web.Mvc
{
    internal static class RouteCollectionExtensions
    {
        /// <summary>
        /// Adds routes for A/B test conversion logging services.
        /// </summary>
        /// <param name="extensionPoint">The object that provides methods to add routes.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="extensionPoint"/> is null.</exception>
        public static void MapABTestConversionLoggerRoutes(this ExtensionPoint<RouteCollection> extensionPoint)
        {
            if (extensionPoint == null)
            {
                throw new ArgumentNullException(nameof(extensionPoint));
            }

            var routes = extensionPoint.Target;

            routes.MapRoute(
                name: ABTestRouteConstants.PAGE_VISIT_CONVERSION_LOGGER_ROUTE_NAME,
                url: ABTestRouteConstants.PAGE_VISIT_CONVERSION_LOGGER_ROUTE,
                defaults: new { controller = ABTestRouteConstants.ABTEST_CONVERSION_LOGGER_CONTROLLER_NAME, action = nameof(KenticoABTestLoggerController.LogPageVisit) },
                namespaces: new[] { typeof(RouteCollectionExtensions).Namespace }
            );

            routes.MapRoute(
                name: ABTestRouteConstants.ABTEST_LOGGER_SCRIPT_ROUTE_NAME,
                url: ABTestRouteConstants.ABTEST_LOGGER_SCRIPT_ROUTE,
                defaults: new { controller = ABTestRouteConstants.ABTEST_CONVERSION_LOGGER_CONTROLLER_NAME, action = nameof(KenticoABTestLoggerController.Script) },
                constraints: new RouteValueDictionary { { ABTestRouteConstants.CULTURE_ROUTE_DATA_KEY, new SiteCultureConstraint() }},
                namespaces: new[] { typeof(RouteCollectionExtensions).Namespace }
            );
        }
    }
}
