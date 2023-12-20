using System.Collections.Generic;

namespace Kentico.Web.Mvc
{
    /// <summary>
    /// Provides configuration for the <see cref="KenticoCorsModule" />.
    /// </summary>
    internal sealed class CorsConfiguration
    {
        /// <summary>
        /// Specifies the method or methods allowed when accessing the resource in response to a preflight request.
        /// </summary>
        /// <remarks>
        /// See <a href="https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/Access-Control-Allow-Methods">
        /// https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/Access-Control-Allow-Methods</a> for more information.
        /// </remarks>
        public IEnumerable<string> AllowMethods { get; set; }


        /// <summary>
        /// Indicate which HTTP headers can be used during the actual request after a preflight request.
        /// </summary>
        /// <remarks>
        /// See <a href="https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/Access-Control-Allow-Headers">
        /// https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/Access-Control-Allow-Headers</a> for more information.
        /// </remarks>
        public IEnumerable<string> AllowHeaders { get; set; }


        /// <summary>
        /// When set to <c>true</c> browsers will expose credentials (cookies, authorization headers or TLS client certificates) to frontend JavaScript code.
        /// </summary>
        /// <remarks>
        /// See <a href="https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/Access-Control-Allow-Credentials">
        /// https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/Access-Control-Allow-Credentials</a> for more information.
        /// </remarks>
        public bool AllowCredentials { get; set; }


        /// <summary>
        /// Indicates how long the results of a preflight request can be cached in seconds.
        /// </summary>
        /// <remarks>
        /// See <a href="https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/Access-Control-Max-Age">
        /// https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/Access-Control-Max-Age</a> for more information.
        /// </remarks>
        public int MaxAge { get; set; } = 86400;
    }
}
