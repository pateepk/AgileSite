using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CMS.Base;

namespace CMS.Membership
{
    /// <summary>
    /// SignOut event arguments
    /// </summary>
    public class SignOutEventArgs : CMSEventArgs
    {
        /// <summary>
        /// Authenticated user
        /// </summary>
        public UserInfo User
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the SignOut URL
        /// </summary>
        public string SignOutUrl
        {
            get;
            set;
        }
    }
}
