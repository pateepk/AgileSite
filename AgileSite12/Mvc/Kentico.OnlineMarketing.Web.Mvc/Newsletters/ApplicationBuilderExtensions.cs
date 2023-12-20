using Kentico.Web.Mvc;
using Kentico.Web.Mvc.Internal;

namespace Kentico.Newsletters.Web.Mvc
{
    /// <summary>
    /// Provides extension methods related to Kentico ASP.NET MVC integration features.
    /// </summary>
    public static class ApplicationBuilderExtensions
    {
        /// <summary>
        /// Enables the opened email and email link tracking feature.
        /// </summary>
        /// <param name="builder">The application builder.</param>
        /// <param name="options">Email tracking options.</param>
        /// <remarks>
        /// Enabling the opened email and email link tracking feature prepares routes to be mapped once <see cref="RouteCollectionAddRoutesMethods.MapRoutes"/> is called.
        /// Inherently this method must be called prior to system routes mapping.
        /// </remarks>
        public static void UseEmailTracking(this IApplicationBuilder builder, EmailTrackingOptions options)
        {
            RouteRegistration.Instance.Add(routes =>
            {
#pragma warning disable CS0618 // Type or member is obsolete
                routes.Kentico().MapOpenedEmailHandlerRoute(options.OpenedEmailHandlerRouteUrl);
                routes.Kentico().MapEmailLinkHandlerRoute(options.EmailLinkHandlerRouteUrl);
#pragma warning restore CS0618 // Type or member is obsolete
            });
        }
    }
}
