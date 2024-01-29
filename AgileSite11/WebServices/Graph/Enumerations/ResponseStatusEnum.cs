using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CMS.WebServices
{
    /// <summary>
    /// Service response codes
    /// </summary>
    public enum ResponseStatusEnum
    {
        /// <summary>
        /// Default value
        /// </summary>
        None = 0,


        /// <summary>
        /// OK
        /// </summary>
        OK = 200,


        /// <summary>
        /// Bad request
        /// </summary>
        BadRequest = 400,


        /// <summary>
        /// Unauthorized
        /// </summary>
        Unauthorized = 401,


        /// <summary>
        /// Not found
        /// </summary>
        NotFound = 404,


        /// <summary>
        /// Internal error
        /// </summary>
        InternalError = 500
    }
}
