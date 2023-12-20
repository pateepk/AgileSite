using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CMS.Base;

namespace CMS.Membership
{
    /// <summary>
    /// Authentication request handler.
    /// </summary>
    public class AuthenticationRequestHandler : SimpleHandler<AuthenticationRequestHandler, AuthenticationRequestEventArgs>
    {
        /// <summary>
        /// Initiates the event handling
        /// </summary>
        /// <param name="requestedUrl">Url that was requested when the event was raised</param>
        public AuthenticationRequestEventArgs StartEvent(string requestedUrl)
        {
            var e = new AuthenticationRequestEventArgs
            {
                RequestedUrl = requestedUrl
            };

            return StartEvent(e);
        }
    }
}
