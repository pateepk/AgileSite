using System;
using System.Web.Mvc;
using System.Web.Routing;

using Kentico.Web.Mvc;

namespace Kentico.Content.Web.Mvc
{
    /// <summary>
    /// <see cref="RouteCollection"/> extension methods.
    /// </summary>
    internal static class RouteCollectionExtensions
    {
        /// <summary>
        /// Register the route of the end-point that enables the administration to detect 3rd party cookie policy.
        /// </summary>
        /// <param name="extensionPoint">Extension point.</param>
        public static void MapCookiePolicyDetectionRoute(this ExtensionPoint<RouteCollection> extensionPoint)
        {
            if (extensionPoint == null)
            {
                throw new ArgumentNullException(nameof(extensionPoint));
            }

            extensionPoint.Target.MapRoute(
                name: "CookiePolicyDetectionRoute",
                url: "KenticoCookiePolicyCheck",
                defaults: new
                {
                    controller = "CookiePolicyDetection",
                    action = "Index",
                    id = UrlParameter.Optional,
                },
                constraints: new { }
            );
        }
    }
}
