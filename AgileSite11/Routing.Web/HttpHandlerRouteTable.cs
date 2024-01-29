using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web.Routing;

namespace CMS.Routing.Web
{
    /// <summary>
    /// Stores the URL routes to Kentico HTTP handlers.
    /// </summary>
    /// <threadsafety static="true" instance="true"/>
    public sealed class HttpHandlerRouteTable
    {
        /// <summary>
        /// A collection of attributes used for a registration of Kentico HTTP handlers.
        /// </summary>
        private readonly ConcurrentDictionary<string, RegisterHttpHandlerAttribute> mDescriptors = new ConcurrentDictionary<string, RegisterHttpHandlerAttribute>(4, 4*16, StringComparer.OrdinalIgnoreCase);
        
        
        /// <summary>
        /// An object that builds routes to Kentico HTTP handlers from route templates.
        /// </summary>
        private readonly HttpHandlerRouteBuilder mRouteBuilder = new HttpHandlerRouteBuilder();


        /// <summary>
        /// The default instance of the <see cref="HttpHandlerRouteTable"/> class.
        /// </summary>
        private static readonly Lazy<HttpHandlerRouteTable> mInstance = new Lazy<HttpHandlerRouteTable>(LazyThreadSafetyMode.PublicationOnly);


        /// <summary>
        /// Gets the default instance of the <see cref="HttpHandlerRouteTable"/> class.
        /// </summary>
        public static HttpHandlerRouteTable Default
        {
            get
            {
                return mInstance.Value;
            }
        }


        /// <summary>
        /// Adds a route to Kentico HTTP handler.
        /// </summary>
        /// <param name="descriptor">The attribute used for a registration of Kentico HTTP handlers.</param>
        /// <exception cref="ArgumentNullException"><paramref name="descriptor"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">The route template is already registered.</exception>
        internal void Register(RegisterHttpHandlerAttribute descriptor)
        {
            if (descriptor == null)
            {
                throw new ArgumentNullException(nameof(descriptor));
            }

            if (!mDescriptors.TryAdd(descriptor.RouteTemplate, descriptor))
            {
                throw new ArgumentException($"The route template '{descriptor.RouteTemplate}' is already registered.", nameof(descriptor));
            }
        }


        /// <summary>
        /// Returns an enumerable collection of routes to Kentico HTTP handlers.
        /// </summary>
        /// <returns>An enumerable collection of routes to Kentico HTTP handlers.</returns>
        public IEnumerable<Route> GetRoutes()
        {
            return mDescriptors.Values.GroupBy(x => x.MarkedType).SelectMany(descriptors => descriptors.OrderBy(x => x.Order).Select(x => mRouteBuilder.Build(x.RouteTemplate, x)));
        }
    }
}
