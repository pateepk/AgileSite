using System;

namespace CMS.PortalEngine
{
    /// <summary>
    /// Web part layout exception.
    /// </summary>
    public class WebPartLayoutException : Exception
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="message">Message</param>
        public WebPartLayoutException(string message)
            : base(message)
        {
        }
    }
}