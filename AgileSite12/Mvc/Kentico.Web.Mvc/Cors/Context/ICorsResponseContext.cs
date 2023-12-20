using System.Collections.Generic;

namespace Kentico.Web.Mvc
{
    /// <summary>
    /// Interface for <see cref="CorsResponseContext"/>.
    /// </summary>
    internal interface ICorsResponseContext
    {
        /// <summary>
        /// Gets or sets the Access-Control-Allow-Origin response header. 
        /// </summary>
        string AllowOrigin { get; set; }

        /// <summary>
        /// Gets or sets the Access-Control-Allow-Methods response header values.
        /// </summary>
        IEnumerable<string> AllowMethods { get; set; }


        /// <summary>
        /// Gets or sets the Access-Control-Allow-Headers response header values.
        /// </summary>
        IEnumerable<string> AllowHeaders { get; set; }


        /// <summary>
        /// Gets or sets the Access-Control-Allow-Credentials response header.
        /// </summary>
        bool AllowCredentials { get; set; }


        /// <summary>
        /// Gets or sets the Access-Control-Max-Age response header.
        /// </summary>
        int MaxAge { get; set; }


        /// <summary>
        /// Sets OK (200) status code, stops execution of the request
        /// and sends all currently buffered output to the client.
        /// </summary>
        void EndResponse();
    }
}