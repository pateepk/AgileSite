using System;

namespace CMS.Base
{
    /// <summary>
    /// Defines implementation to create and manipulate individual HTTP cookies.
    /// </summary>
    public interface IHttpCookie
    {
        /// <summary>
        /// Gets or sets the name of a cookie.
        /// </summary>
        string Name { get; set; }


        /// <summary>
        /// Gets or sets the value of a cookie.
        /// </summary>
        string Value { get; set; }


        /// <summary>
        /// Gets or sets the expiration date and time for the cookie.
        /// </summary>
        DateTime Expires { get; set; }


        /// <summary>
        /// Gets or sets a value that specifies whether a cookie is accessible by client-side script.
        /// </summary>
        bool HttpOnly { get; set; }


        /// <summary>
        /// Gets or sets the domain to associate the cookie with.
        /// </summary>
        string Domain { get; set; }


        /// <summary>
        /// Gets or sets the virtual path to transmit with the current cookie.
        /// </summary>
        string Path { get; set; }
    }
}
