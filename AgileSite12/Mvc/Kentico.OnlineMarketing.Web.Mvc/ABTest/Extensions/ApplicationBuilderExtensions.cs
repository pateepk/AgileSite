using CMS.OnlineMarketing.Internal;

using Kentico.Web.Mvc;
using Kentico.Web.Mvc.Internal;

namespace Kentico.OnlineMarketing.Web.Mvc
{
    /// <summary>
    /// Provides extension methods related to Kentico ASP.NET MVC integration features.
    /// </summary>
    public static class ApplicationBuilderExtensions
    {
        /// <summary>
        /// Enables the A/B testing feature.
        /// </summary>
        /// <param name="builder">The application builder.</param>
        public static void UseABTesting(this IApplicationBuilder builder)
        {
            // Output cache specific settings
            DelayedABResponseCookieProvider.Init();
            ABVisitRequestHelper.ABVisitRequestEnabled = true;

            RouteRegistration.Instance.Add(routes => routes.Kentico().MapABTestConversionLoggerRoutes());
        }
    }
}
