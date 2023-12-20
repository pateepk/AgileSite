using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

using Kentico.Content.Web.Mvc;

namespace Kentico.PageBuilder.Web.Mvc
{
    /// <summary>
    /// Retrieves URL to retrieve the component properties form markup.
    /// </summary>
    internal sealed class ComponentPropertiesFormMarkupUrlRetriever : AbstractComponentUrlRetriever<IPropertiesComponentDefinition>
    {
        /// <summary>
        /// Creates an instance of <see cref="ComponentPropertiesFormMarkupUrlRetriever"/> class.
        /// </summary>
        /// <param name="context">HTTP context in which the URL is retrieved.</param>
        /// <param name="decorator">Decorates URL with additional information.</param>
        public ComponentPropertiesFormMarkupUrlRetriever(HttpContextBase context, IPathDecorator decorator) : base(context, decorator)
        {
        }


        /// <summary>
        /// Generates URL to retrieve the properties HTML markup.
        /// </summary>
        /// <param name="component">Component with markup definition.</param>
        /// <returns>URL to retrieve the properties HTML markup.</returns>
        protected override string GenerateUrl(IPropertiesComponentDefinition component)
        {
            return UrlHelper.GenerateUrl(PageBuilderRoutes.FORM_ROUTE_NAME, PageBuilderRoutes.DEFAULT_ACTION_NAME, component.PropertiesFormMarkupControllerName, new RouteValueDictionary {
                { PageBuilderConstants.TYPE_IDENTIFIER_ROUTE_KEY, component.Identifier },
            }, RouteTable.Routes, context.Request.RequestContext, false);
        }
    }
}
