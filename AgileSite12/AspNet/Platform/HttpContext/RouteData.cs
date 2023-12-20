using System.Collections.Generic;
using System.Web.Routing;

using CMS.Base;

namespace CMS.AspNet.Platform
{
    /// <summary>
    /// Wrapper over <see cref="RouteData"/> object implementing <see cref="IRouteData"/> interface.
    /// </summary>
    internal class RouteDataImpl : IRouteData
    {
        private readonly RouteData mRouteData;


        public RouteDataImpl(RouteData routeData)
        {
            mRouteData = routeData;
        }


        public IDictionary<string, object> DataTokens => mRouteData.DataTokens;

        public IDictionary<string, object> Values => mRouteData.Values;
    }
}
