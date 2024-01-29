using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CMS.Membership
{
    /// <summary>
    /// Authentication request event arguments
    /// </summary>
    public class AuthenticationRequestEventArgs : EventArgs
    {
        /// <summary>
        /// Url that was requested when the event was raised
        /// </summary>
        public string RequestedUrl
        {
            get;
            set;
        }
    }
}