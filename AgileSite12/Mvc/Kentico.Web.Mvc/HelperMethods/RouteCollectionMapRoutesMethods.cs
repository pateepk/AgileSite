using System;
using System.Web.Routing;

using CMS.Routing.Web;

using Kentico.Web.Mvc.Internal;

namespace Kentico.Web.Mvc
{
    /// <summary>
    /// Provides methods to add routes to Kentico HTTP handlers.
    /// </summary>
    public static class RouteCollectionAddRoutesMethods
    {
        /// <summary>
        /// Adds system routes such as Kentico HTTP handlers or feature specific routes. All required features must be enabled
        /// prior to calling this method as feature specific routes cannot be added later.
        /// </summary>
        /// <param name="instance">The object that provides methods to add routes to Kentico HTTP handlers.</param>
        /// <exception cref="ArgumentNullException"><paramref name="instance"/> is null.</exception>
        public static void MapRoutes(this ExtensionPoint<RouteCollection> instance)
        {
            if (instance == null)
            {
                throw new ArgumentNullException(nameof(instance));
            }

            var routes = instance.Target;
            using (routes.GetWriteLock())
            {
                foreach (var route in HttpHandlerRouteTable.Default.GetRoutes())
                {
                    routes.Add(route);
                }
            }

            RouteRegistration.Instance.Execute(routes);
        }
    }
}
