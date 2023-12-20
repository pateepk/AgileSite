using System;
using System.Web.Routing;

namespace Kentico.PageBuilder.Web.Mvc
{
    /// <summary>
    /// Retrieves data from route data for Page builder.
    /// </summary>
    internal sealed class PageBuilderRouteDataRetriever : IPageBuilderRouteDataRetriever
    {
        private readonly RouteData routeData;


        /// <summary>
        /// Creates an instance of <see cref="PageBuilderRouteDataRetriever"/> class.
        /// </summary>
        /// <param name="routeData">Route data to be searched.</param>
        /// <exception cref="ArgumentNullException">Is thrown when <paramref name="routeData"/> is <c>null</c>.</exception>
        public PageBuilderRouteDataRetriever(RouteData routeData)
        {
            this.routeData = routeData ?? throw new ArgumentNullException(nameof(routeData));
        }


        /// <summary>
        /// Retrieves data for Page builder from route data.
        /// </summary>
        /// <typeparam name="TPropertiesType">Type of the widget properties.</typeparam>
        public TPropertiesType Retrieve<TPropertiesType>()
            where TPropertiesType : IComponentProperties
        {
            if (routeData.Values[PageBuilderConstants.COMPONENT_PROPERTIES_ROUTE_DATA_KEY] is TPropertiesType)
            {
                return (TPropertiesType)routeData.Values[PageBuilderConstants.COMPONENT_PROPERTIES_ROUTE_DATA_KEY];
            }

            return default(TPropertiesType);
        }
    }
}
