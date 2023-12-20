using System;
using System.Linq;
using System.Text;

using CMS.Base;

namespace CMS.Membership
{
    /// <summary>
    /// SignOut handler
    /// </summary>
    public class SignOutHandler : AdvancedHandler<SignOutHandler, SignOutEventArgs>
    {
        /// <summary>
        /// Initiates the event handling
        /// </summary>
        /// <param name="userInfo">Current info of the authenticated user</param>
        /// <param name="signOutUrl">Sign out URL</param>
        public SignOutHandler StartEvent(UserInfo userInfo, ref string signOutUrl)
        {
            var e = new SignOutEventArgs()
            {
                User = userInfo,
                SignOutUrl = signOutUrl
            };

            var h = StartEvent(e);
            signOutUrl = e.SignOutUrl;

            return h;
        }
    }
}
