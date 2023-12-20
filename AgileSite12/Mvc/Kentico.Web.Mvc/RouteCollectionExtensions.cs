using System;
using System.Web.Routing;

namespace Kentico.Web.Mvc
{
    /// <summary>
    /// Provides methods to add routes to Kentico HTTP handlers.
    /// </summary>
    public static class RouteCollectionExtensions
    {
        /// <summary>
        /// Returns an object that provides methods to add routes to Kentico HTTP handlers.
        /// </summary>
        /// <param name="target">An instance of the <see cref="RouteCollection"/> class.</param>
        /// <returns>An object that provides methods to add routes to Kentico HTTP handlers.</returns>
        public static ExtensionPoint<RouteCollection> Kentico(this RouteCollection target)
        {
            return new ExtensionPoint<RouteCollection>(target);
        }
    }
}
