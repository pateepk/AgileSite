using System;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;

using Kentico.Web.Mvc;

namespace Kentico.Components.Web.Mvc
{
    /// <summary>
    /// Extends a <see cref="RouteCollection"/> object for MVC routing.
    /// </summary>
    internal static class RouteCollectionExtensions
    {
        /// <summary>
        /// Adds routes for selector dialogs.
        /// </summary>
        /// <param name="extensionPoint">The object that provides methods to add selectors routes.</param>
        /// <exception cref="ArgumentNullException"><paramref name="extensionPoint"/> is null.</exception>
        public static void MapSelectorDialogsRoutes(this ExtensionPoint<RouteCollection> extensionPoint)
        {
            if (extensionPoint == null)
            {
                throw new ArgumentNullException(nameof(extensionPoint));
            }

            var routes = extensionPoint.Target;
            routes.MapRoute(
                name: SelectorRoutes.SELECTORS_ROUTE_NAME,
                url: SelectorRoutes.SELECTORS_ROUTE,
                defaults: new { action = SelectorRoutes.DEFAULT_ACTION_NAME },
                constraints: new { controller = $"({SelectorRoutes.MEDIA_FILES_SELECTOR_CONTROLLER_NAME}|{SelectorRoutes.PAGE_SELECTOR_CONTROLLER_NAME})" }
            );

            routes.MapHttpRoute(
                name: SelectorRoutes.MEDIA_FILES_UPLOADER_ROUTE_NAME,
                routeTemplate: SelectorRoutes.MEDIA_FILES_UPLOADER_ROUTE,
                defaults: new { action = "Upload", controller = SelectorRoutes.MEDIA_FILES_UPLOADER_CONTROLLER_NAME }                
            );
        }
    }
}
