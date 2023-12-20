using System;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;

using Kentico.Web.Mvc;

namespace Kentico.PageBuilder.Web.Mvc.PageTemplates
{
    internal static class RouteCollectionExtensions
    {
        /// <summary>
        /// Adds routes for Page templates system services.
        /// </summary>
        /// <param name="extensionPoint">The object that provides methods to add Page templates routes.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="extensionPoint"/> is null.</exception>
        public static void MapPageTemplateRoutes(this ExtensionPoint<RouteCollection> extensionPoint)
        {
            if (extensionPoint == null)
            {
                throw new ArgumentNullException(nameof(extensionPoint));
            }

            var routes = extensionPoint.Target;
            routes.MapHttpRoute(
                name: PageTemplateRoutes.TEMPLATE_ROUTE_NAME,
                routeTemplate: PageTemplateRoutes.TEMPLATE_METADATA_ROUTE,
                defaults: new
                {
                    controller = PageTemplateRoutes.TEMPLATE_METADATA_CONTROLLER_NAME,
                    action = "GetFiltered",
                },
                constraints: new { }
            );

            routes.MapRoute(
                name: PageTemplateRoutes.TEMPLATE_SELECTOR_DIALOG_ROUTE_NAME,
                url: PageTemplateRoutes.TEMPLATE_SELECTOR_DIALOG_ROUTE,
                defaults: new
                {
                    controller = PageTemplateRoutes.TEMPLATE_SELECTOR_DIALOG_CONTROLLER_NAME,
                    action = PageBuilderRoutes.DEFAULT_ACTION_NAME
                },
                constraints: new { }
            );
        }
    }
}
