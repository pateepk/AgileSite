using System.Collections.Generic;

namespace CMS.Base
{
    /// <summary>
    /// Defines interface encapsulating information about a route.
    /// </summary>
    public interface IRouteData
    {
        /// <summary>
        /// Gets a collection of custom values that are passed to the route handler but are not used when routing engine determines whether the route matches a request.
        /// </summary>
        IDictionary<string, object> DataTokens { get; }


        /// <summary>
        /// Gets a collection of URL parameter values and default values for the route.
        /// </summary>
        IDictionary<string, object> Values { get; }
    }
}
