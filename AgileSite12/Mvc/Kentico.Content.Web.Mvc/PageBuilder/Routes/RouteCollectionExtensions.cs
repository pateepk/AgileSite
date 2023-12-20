using System;
using System.Web.Http;
using System.Web.Http.Routing.Constraints;
using System.Web.Mvc;
using System.Web.Routing;

using Kentico.Web.Mvc;
using Kentico.Web.Mvc.Internal;
using Kentico.Builder.Web.Mvc;
using Kentico.PageBuilder.Web.Mvc.Personalization;
using Kentico.PageBuilder.Web.Mvc.PageTemplates;

namespace Kentico.PageBuilder.Web.Mvc
{
    /// <summary>
    /// Extends a <see cref="RouteCollection"/> object for MVC routing.
    /// </summary>
    internal static class RouteCollectionExtensions
    {
        /// <summary>
        /// Adds routes for Page builder system services.
        /// </summary>
        /// <param name="extensionPoint">The object that provides methods to add Page builder routes.</param>
        /// <exception cref="ArgumentNullException"><paramref name="extensionPoint"/> is null.</exception>
        public static void MapPageBuilderRoutes(this ExtensionPoint<RouteCollection> extensionPoint)
        {
            if (extensionPoint == null)
            {
                throw new ArgumentNullException(nameof(extensionPoint));
            }

            extensionPoint.MapLocalizationScriptRoute();

            var routes = extensionPoint.Target;
            var route = routes.MapRoute(
                name: PageBuilderRoutes.WIDGETS_ROUTE_NAME,
                url: PageBuilderRoutes.WIDGETS_ROUTE,
                defaults: new { },
                constraints: new RouteValueDictionary {
                    { "controller", new ComponentControllerConstraint<WidgetDefinition>() },
                    { PageBuilderConstants.TYPE_IDENTIFIER_ROUTE_KEY, new ComponentIdentifierConstraint<WidgetDefinition>() },
                    { PageBuilderConstants.CULTURE_ROUTE_KEY, new SiteCultureConstraint() }
                }
            );
            route.RouteHandler = new PageBuilderCultureRouteHandler();

            route = routes.MapRoute(
                name: PageBuilderRoutes.SECTIONS_ROUTE_NAME,
                url: PageBuilderRoutes.SECTIONS_ROUTE,
                defaults: new { },
                constraints: new RouteValueDictionary {
                    { "controller", new ComponentControllerConstraint<SectionDefinition>() },
                    { PageBuilderConstants.TYPE_IDENTIFIER_ROUTE_KEY, new ComponentIdentifierConstraint<SectionDefinition>() },
                    { PageBuilderConstants.CULTURE_ROUTE_KEY, new SiteCultureConstraint() }
                }
            );
            route.RouteHandler = new PageBuilderCultureRouteHandler();

            routes.MapRoute(
                name: PageBuilderRoutes.PERSONALIZATION_CONDITION_TYPE_ROUTE_NAME,
                url: PageBuilderRoutes.PERSONALIZATION_CONDITION_TYPE_ROUTE,
                defaults: new { },
                constraints: new RouteValueDictionary {
                    { "controller", new ComponentControllerConstraint<ConditionTypeDefinition>() },
                    { PageBuilderConstants.TYPE_IDENTIFIER_ROUTE_KEY, new ComponentIdentifierConstraint<ConditionTypeDefinition>() }
                }
            );

            routes.MapHttpRoute(
                name: PageBuilderRoutes.CONFIGURATION_STORE_ROUTE_NAME,
                routeTemplate: PageBuilderRoutes.CONFIGURATION_STORE_ROUTE,
                defaults: new
                {
                    controller = PageBuilderRoutes.CONFIGURATION_CONTROLLER_NAME,
                    action = "Set"
                },
                constraints: new { }
            );

            routes.MapHttpRoute(
                name: PageBuilderRoutes.CONFIGURATION_CHANGE_TEMPLATE_ROUTE_NAME,
                routeTemplate: PageBuilderRoutes.CONFIGURATION_CHANGE_TEMPLATE_ROUTE,
                defaults: new
                {
                    controller = PageBuilderRoutes.CONFIGURATION_CONTROLLER_NAME,
                    action = "ChangeTemplate"
                },
                constraints: new { }
             );

            routes.MapHttpRoute(
                name: PageBuilderRoutes.CONFIGURATION_LOAD_ROUTE_NAME,
                routeTemplate: PageBuilderRoutes.CONFIGURATION_LOAD_ROUTE,
                defaults: new
                {
                    controller = PageBuilderRoutes.CONFIGURATION_CONTROLLER_NAME,
                    action = "Get"
                },
                constraints: new
                {
                    pageId = new IntRouteConstraint()
                }
            );

            routes.MapHttpRoute(
                name: PageBuilderRoutes.DEFAULT_PROPERTIES_ROUTE_NAME,
                routeTemplate: PageBuilderRoutes.DEFAULT_PROPERTIES_ROUTE,
                defaults: new
                {
                    controller = PageBuilderRoutes.CONFIGURATION_CONTROLLER_NAME,
                },
                constraints: new { action = $"({PageBuilderRoutes.DEFAULT_WIDGET_PROPERTIES_ACTION_NAME}|{PageBuilderRoutes.DEFAULT_SECTION_PROPERTIES_ACTION_NAME}|{PageTemplateRoutes.DEFAULT_TEMPLATE_PROPERTIES_ACTION_NAME})" }
            );

            routes.MapRoute(
                name: PageBuilderRoutes.FORM_ROUTE_NAME,
                url: PageBuilderRoutes.FORM_ROUTE,
                defaults: new { },
                constraints: new { }
            );

            routes.MapHttpRoute(
                name: PageBuilderRoutes.METADATA_ROUTE_NAME,
                routeTemplate: PageBuilderRoutes.METADATA_ROUTE,
                defaults: new
                {
                    controller = "KenticoComponentsMetadata",
                    action = "GetAll"
                },
                constraints: new { }
            );
        }
    }
}
