using Kentico.Web.Mvc;
using Kentico.Web.Mvc.Internal;

namespace Kentico.Activities.Web.Mvc
{
    /// <summary>
    /// Provides extension methods related to Kentico ASP.NET MVC integration features.
    /// </summary>
    public static class ApplicationBuilderExtensions
    {
        /// <summary>
        /// Enables the activity tracking feature.
        /// </summary>
        /// <param name="builder">The application builder.</param>
        /// <remarks>
        /// Enabling the activity tracking feature prepares routes to be mapped once <see cref="RouteCollectionAddRoutesMethods.MapRoutes"/> is called.
        /// Inherently this method must be called prior to system routes mapping.
        /// </remarks>
        public static void UseActivityTracking(this IApplicationBuilder builder)
        {
#pragma warning disable CS0618 // Type or member is obsolete
            RouteRegistration.Instance.Add(routes => routes.Kentico().MapActivitiesRoutes());
#pragma warning restore CS0618 // Type or member is obsolete
        }
    }
}
