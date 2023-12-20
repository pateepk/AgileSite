using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

using Kentico.Content.Web.Mvc;

namespace Kentico.PageBuilder.Web.Mvc
{
    /// <summary>
    /// Retrieves URL to retrieve default component properties.
    /// </summary>
    internal sealed class ComponentDefaultPropertiesUrlRetriever : AbstractComponentUrlRetriever<IPropertiesComponentDefinition>
    {
        /// <summary>
        /// Creates an instance of <see cref="ComponentDefaultPropertiesUrlRetriever"/> class.
        /// </summary>
        /// <param name="context">HTTP context in which the URL is retrieved.</param>
        /// <param name="decorator">Decorates URL with additional information.</param>
        public ComponentDefaultPropertiesUrlRetriever(HttpContextBase context, IPathDecorator decorator) : base(context, decorator)
        {
        }


        /// <summary>
        /// Generates URL to retrieve default component properties.
        /// </summary>
        /// <param name="component">Component with markup definition.</param>
        /// <returns>URL to retrieve default component properties.</returns>
        protected override string GenerateUrl(IPropertiesComponentDefinition component)
        {
            return UrlHelper.GenerateUrl(PageBuilderRoutes.DEFAULT_PROPERTIES_ROUTE_NAME, component.DefaultPropertiesActionName, PageBuilderRoutes.CONFIGURATION_CONTROLLER_NAME, new RouteValueDictionary {
                { PageBuilderConstants.TYPE_IDENTIFIER_ROUTE_KEY, component.Identifier },
                { "httpRoute", true }
            }, RouteTable.Routes, context.Request.RequestContext, false);
        }
    }
}
