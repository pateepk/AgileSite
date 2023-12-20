using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Web.Routing;

namespace CMS.WebApi
{
    /// <summary>
    /// Stores the URL routes served by Kentico CMS API controllers.
    /// </summary>
    internal sealed class HttpControllerRouteTable
    {
        private readonly ConcurrentDictionary<string, CMSApiControllerConfiguration> mControllerConfigurations = 
            new ConcurrentDictionary<string, CMSApiControllerConfiguration>(4, 4*16, StringComparer.OrdinalIgnoreCase);

        private static readonly Lazy<HttpControllerRouteTable> mInstance = new Lazy<HttpControllerRouteTable>(() => new HttpControllerRouteTable(), LazyThreadSafetyMode.PublicationOnly);


        /// <summary>
        /// Gets default instance of <see cref="HttpControllerRouteTable"/>.
        /// </summary>
        public static HttpControllerRouteTable Instance
        {
            get
            {
                return mInstance.Value;
            }
        }


        /// <summary>
        /// Constructor.
        /// </summary>
        private HttpControllerRouteTable()
        {
        }


        /// <summary>
        /// Registers a route served by Kentico CMS API controller.
        /// </summary>
        /// <param name="controllerType">Kentico CMS API controller type.</param>
        /// <param name="requiresSessionState">Indicates whether session state is required for given controller.</param>
        /// <exception cref="ArgumentNullException"><paramref name="controllerType"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">A controller of given name is already registered.</exception>
        public void Register(Type controllerType, bool requiresSessionState)
        {
            if (controllerType == null)
            {
                throw new ArgumentNullException(nameof(controllerType));
            }

            var controllerConfiguration = new CMSApiControllerConfiguration { ControllerType = controllerType, RequiresSessionState = requiresSessionState };

            if (!mControllerConfigurations.TryAdd(controllerType.Name, controllerConfiguration))
            {
                var alreadyRegisteredControllerType = mControllerConfigurations[controllerType.Name].ControllerType;

                throw new ArgumentException("A controller of given name is already registered. Controller names must be unique. " +
                    $"Already registered controller type FullName: '{alreadyRegisteredControllerType.FullName}', " +
                    $"Registering controller type FullName: '{controllerType.FullName}'.", nameof(controllerType));
            }
        }


        /// <summary>
        /// Maps HTTP routes served by all registered Kentico CMS API controllers.
        /// </summary>
        public void MapRoutes()
        {
            var routes = RouteTable.Routes;
            using (routes.GetWriteLock())
            {
                foreach (var controllerConfiguration in mControllerConfigurations.Values)
                {
                    HttpControllerRouteMapper.MapRoute(routes, controllerConfiguration);
                } 
            }
        }
    }
}