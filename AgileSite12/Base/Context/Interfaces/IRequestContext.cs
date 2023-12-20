namespace CMS.Base
{
    /// <summary>
    /// Defines implementation encapsulating information about an HTTP request that matches a defined route.
    /// </summary>
    public interface IRequestContext
    {
        /// <summary>
        /// Gets <see cref="IRouteData"/> object holding information about the requested route.
        /// </summary>
        IRouteData RouteData { get; }
    }
}
