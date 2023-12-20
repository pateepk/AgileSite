using System;
using System.Web.Mvc;
using System.Web.Routing;

using CMS.Base;
using CMS.Core;

using Kentico.Web.Mvc;

namespace Kentico.Content.Web.Mvc.Routing
{
    internal static class RouteCollectionExtensions
    {
        /// <summary>
        /// Adds routes for Alternative URLs feature.
        /// </summary>
        /// <param name="extensionPoint">The object that provides methods to add routes.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="extensionPoint"/> is null.</exception>
        public static void MapAlternativeUrlsRoutes(this ExtensionPoint<RouteCollection> extensionPoint)
        {
            if (extensionPoint == null)
            {
                throw new ArgumentNullException(nameof(extensionPoint));
            }

            var routes = extensionPoint.Target;

            var route = routes.MapRoute(
                name: AlternativeUrlsRouteConstants.CATCH_ALL_ROUTE_NAME,
                url: AlternativeUrlsRouteConstants.CATCH_ALL_ROUTE,
                defaults: null,
                constraints: new RouteValueDictionary { { AlternativeUrlsRouteConstants.CATCH_ALL_ROUTE_DATA_KEY, new AlternativeUrlConstraint(Service.Resolve<IAlternativeUrlsService>(), Service.Resolve<ISiteService>()) } },
                namespaces: new[] { typeof(RouteCollectionExtensions).Namespace }
            );

            route.RouteHandler = new RouteHandlerWrapper<AlternativeUrlRedirectHttpHandler>();
        }
    }
}
