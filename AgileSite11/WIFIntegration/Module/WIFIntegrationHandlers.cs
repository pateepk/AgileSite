using System;
using System.Linq;
using System.Text;

using CMS.Base;
using CMS.Helpers;
using CMS.Membership;

namespace CMS.WIFIntegration
{
    /// <summary>
    /// Event handlers for WIF integration
    /// </summary>
    internal class WIFIntegrationHandlers
    {
        /// <summary>
        /// Initializes the handlers
        /// </summary>
        public static void Init()
        {
            if (SystemContext.IsCMSRunningAsMainApplication)
            {
                SecurityEvents.AuthenticationRequested.Execute += RequestSignIn;
                SecurityEvents.SignOut.After += RequestSignOut;

                RequestEvents.Begin.Execute += ProcessRequest;
            }
        }


        private static void RequestSignOut(object sender, SignOutEventArgs e)
        {
            new SignOut().RequestSignOut();
        }


        private static void RequestSignIn(object sender, AuthenticationRequestEventArgs eventArgs)
        {
            new SignIn().RequestSignIn(eventArgs.RequestedUrl);
        }


        private static void ProcessRequest(object sender, EventArgs e)
        {
            new SignIn().ProcessSignInRequest();
            new SignOut().ProcessSignOutRequest();
        }
    }
}
