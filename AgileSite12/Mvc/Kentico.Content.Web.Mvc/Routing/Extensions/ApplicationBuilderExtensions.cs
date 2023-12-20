using Kentico.Web.Mvc;
using Kentico.Web.Mvc.Internal;

namespace Kentico.Content.Web.Mvc.Routing
{
    /// <summary>
    /// Provides extension methods related to Kentico ASP.NET MVC integration features.
    /// </summary>
    public static class ApplicationBuilderExtensions
    {
        /// <summary>
        /// Enables features used with URL routing.
        /// </summary>
        /// <param name="builder">The application builder.</param>
        /// <param name="options">Routing options.</param>
        public static void UsePageRouting(this IApplicationBuilder builder, PageRoutingOptions options = null)
        {
            if (options == null)
            {
                return;
            }

            if (options.EnableAlternativeUrls)
            {
                RouteRegistration.Instance.Add(routes => routes.Kentico().MapAlternativeUrlsRoutes());
            }
        }
    }
}
