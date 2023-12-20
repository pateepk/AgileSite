namespace Kentico.PageBuilder.Web.Mvc
{
    /// <summary>
    /// Provides an interface for retrieving data for Page builder from route data.
    /// </summary>
    internal interface IPageBuilderRouteDataRetriever
    {
        /// <summary>
        /// Retrieves data for Page builder from route data.
        /// </summary>
        /// <typeparam name="TPropertiesType">Type of the widget properties.</typeparam>
        TPropertiesType Retrieve<TPropertiesType>()
            where TPropertiesType : IComponentProperties;
    }
}
