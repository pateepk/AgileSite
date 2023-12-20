using System.Threading;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

using Kentico.Content.Web.Mvc;

namespace Kentico.PageBuilder.Web.Mvc
{
    /// <summary>
    /// Retrieves URL to retrieve the component markup.
    /// </summary>
    internal sealed class ComponentMarkupUrlRetriever : AbstractComponentUrlRetriever<IMarkupComponentDefinition>
    {
        /// <summary>
        /// Creates an instance of <see cref="ComponentMarkupUrlRetriever"/> class.
        /// </summary>
        /// <param name="context">HTTP context in which the URL is retrieved.</param>
        /// <param name="decorator">Decorates markup URL with additional information.</param>
        public ComponentMarkupUrlRetriever(HttpContextBase context, IPathDecorator decorator) : base(context, decorator)
        {
        }


        /// <summary>
        /// Generates URL for component markup.
        /// </summary>
        /// <param name="component">Component with markup definition.</param>
        /// <returns>URL for component markup.</returns>
        protected override string GenerateUrl(IMarkupComponentDefinition component)
        {
            return UrlHelper.GenerateUrl(component.RouteName, PageBuilderRoutes.DEFAULT_ACTION_NAME, component.ControllerName, new RouteValueDictionary {
                { PageBuilderConstants.TYPE_IDENTIFIER_ROUTE_KEY, component.Identifier },
                { PageBuilderConstants.CULTURE_ROUTE_KEY, Thread.CurrentThread.CurrentCulture.Name }
            }, RouteTable.Routes, context.Request.RequestContext, false);
        }
    }
}
