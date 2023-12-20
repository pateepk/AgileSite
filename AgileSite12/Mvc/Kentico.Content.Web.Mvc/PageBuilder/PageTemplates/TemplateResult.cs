using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;

using Kentico.Web.Mvc;

namespace Kentico.PageBuilder.Web.Mvc.PageTemplates
{
    /// <summary>
    /// Represents a class that is used to render a page by using the appropriate page template.
    /// </summary>
    public class TemplateResult : ActionResult
    {
        private readonly int pageId;
        private readonly IComponentDefinitionProvider<PageTemplateDefinition> provider;
        private readonly IControllerFactory controllerFactory;


        /// <summary>
        /// Creates an instance of the <see cref="TemplateResult"/> class.
        /// </summary>
        /// <param name="pageId">Identifier of the page to render.</param>
        public TemplateResult(int pageId)
            : this(pageId, new ComponentDefinitionProvider<PageTemplateDefinition>(), ControllerBuilder.Current.GetControllerFactory())
        {
        }


        /// <summary>
        /// Creates an instance of the <see cref="TemplateResult"/> class.
        /// </summary>
        /// <param name="pageId">Identifier of the page to render.</param>
        /// <param name="provider">The page template definitions provider.</param>
        /// <param name="controllerFactory">Controller factory to instantiate page template controller.</param>
        internal TemplateResult(int pageId, IComponentDefinitionProvider<PageTemplateDefinition> provider, IControllerFactory controllerFactory)
        {
            if (pageId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(pageId), $"{nameof(pageId)} has to be an identifier of an existing page.");
            }

            this.pageId = pageId;
            this.provider = provider ?? throw new ArgumentNullException(nameof(provider));
            this.controllerFactory = controllerFactory;
        }


        /// <summary>
        /// Executes page rendering with the appropriate template page.
        /// </summary>
        /// <param name="context">The context that the result is executed in.</param>
        public override void ExecuteResult(ControllerContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var invoker = new ControllerActionInvoker();
            var configuration = InitPageBuilderAndGetTemplateConfiguration(context);
            ExecuteResultInternal(invoker, context, configuration);
        }


        internal void ExecuteResultInternal(IActionInvoker invoker, ControllerContext context, PageTemplateConfiguration configuration)
        {
            configuration = configuration 
                ?? throw new InvalidOperationException($"Page template not found for selected page.");

            var template = provider.GetAll().FirstOrDefault(definition => definition.Identifier == configuration.Identifier)
                ?? throw new InvalidOperationException($"Page template with identifier \"{configuration.Identifier}\" is not registered in the application.");

            var pageTemplateContext = GetTemplateControllerContext(context, template);
            invoker.InvokeAction(pageTemplateContext, PageBuilderRoutes.DEFAULT_ACTION_NAME);
        }


        internal PageTemplateConfiguration InitPageBuilderAndGetTemplateConfiguration(ControllerContext context)
        {
            var feature = GetInitializedPageBuilderFeature(context);
            var dataContext = feature.GetDataContext();

            return dataContext.Configuration.PageTemplate;
        }


        internal virtual IPageBuilderFeature GetInitializedPageBuilderFeature(ControllerContext context)
        {
            var feature = context.HttpContext.Kentico().PageBuilder();
            feature.Initialize(pageId);

            return feature;
        }


        internal ControllerContext GetTemplateControllerContext(ControllerContext context, PageTemplateDefinition template)
        {
            var controllerName = template.ControllerName;
            var templateController = controllerFactory.CreateController(context.RequestContext, controllerName) as Controller;
            var routeData = CreateRouteData(context.RouteData.Route, context.RouteData.Values, template);
            var controllerContext = new ControllerContext(context.HttpContext, routeData, templateController);
            templateController.ControllerContext = controllerContext;

            return controllerContext;
        }


        private static RouteData CreateRouteData(RouteBase route, RouteValueDictionary routeValues, PageTemplateDefinition template)
        {
            var routeData = new RouteData
            {
                Route = route
            };

            foreach (KeyValuePair<string, object> kvp in routeValues)
            {
                routeData.Values.Add(kvp.Key, kvp.Value);
            }

            routeData.Values[PageBuilderConstants.PAGE_TEMPLATE_DEFINITION_ROUTE_DATA_KEY] = template;

            return routeData;
        }
    }
}
