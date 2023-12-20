using Kentico.Web.Mvc;

namespace Kentico.Content.Web.Mvc.Routing
{
    /// <summary>
    /// Options configuring the URL routing features.
    /// </summary>
    public class PageRoutingOptions
    {
        /// <summary>
        /// Indicates whether the Alternative URLs feature is enabled.
        /// </summary>
        /// <remarks>
        /// Enabling the Alternative URLs feature prepares routes to be mapped once <see cref="RouteCollectionAddRoutesMethods.MapRoutes"/> is called.
        /// The property must be set prior to system routes mapping.
        /// </remarks>
        public bool EnableAlternativeUrls
        {
            get;
            set;
        }
    }
}
