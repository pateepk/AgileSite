namespace Kentico.Web.Mvc
{
    /// <summary>
    /// Interface for <see cref="CorsRequestContext"/>.
    /// </summary>
    internal interface ICorsRequestContext
    {
        /// <summary>
        /// Gets the Origin header value.
        /// </summary>
        string Origin { get; }


        /// <summary>
        /// Gets the Access-Control-Request-Method header value.
        /// </summary>
        string AccessControlRequestMethod { get; }


        /// <summary>
        /// Gets the request HTTP method.
        /// </summary>
        string HttpMethod { get; }


        /// <summary>
        /// Gets a value indicating whether this is a preflight request.
        /// </summary>
        bool IsPreflight { get; }


        /// <summary>
        /// Gets a value indicating if the requestHeaders origin domain is one of current site's domains. 
        /// </summary>
        bool IsCurrentSiteOrigin { get; }
    }
}