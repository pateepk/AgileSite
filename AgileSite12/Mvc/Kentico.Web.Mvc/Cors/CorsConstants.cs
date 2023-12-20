namespace Kentico.Web.Mvc
{
    /// <summary>
    /// CORS-related constants.
    /// </summary>
    internal static class CorsConstants
    {
        /// <summary>
        /// The HTTP method for the CORS preflight request.
        /// </summary>
        public const string PREFLIGHT_HTTP_METHOD = "OPTIONS";


        /// <summary>
        /// The Origin request header.
        /// </summary>
        public const string ORIGIN = "Origin";


        /// <summary>
        /// The Access-Control-Request-Method request header.
        /// </summary>
        public const string ACCESS_CONTROL_REQUEST_METHOD = "Access-Control-Request-Method";


        /// <summary>
        /// The Access-Control-Allow-Origin response header.
        /// </summary>
        public const string ACCESS_CONTROL_ALLOW_ORIGIN = "Access-Control-Allow-Origin";


        /// <summary>
        /// The Access-Control-Allow-Headers response header.
        /// </summary>
        public const string ACCESS_CONTROL_ALLOW_HEADERS = "Access-Control-Allow-Headers";

 
        /// <summary>
        /// The Access-Control-Allow-Methods response header.
        /// </summary>
        public const string ACCESS_CONTROL_ALLOW_METHODS = "Access-Control-Allow-Methods";


        /// <summary>
        /// The Access-Control-Allow-Credentials response header.
        /// </summary>
        public const string ACCESS_CONTROL_ALLOW_CREDENTIALS = "Access-Control-Allow-Credentials";


        /// <summary>
        /// The Access-Control-Max-Age response header.
        /// </summary>
        public const string ACCESS_CONTROL_MAX_AGE = "Access-Control-Max-Age";
    }
}