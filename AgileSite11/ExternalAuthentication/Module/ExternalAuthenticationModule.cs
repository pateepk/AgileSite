using CMS;
using CMS.Core;
using CMS.ExternalAuthentication;
using CMS.DataEngine;
using CMS.Membership;

[assembly: RegisterModule(typeof(ExternalAuthenticationModule))]


namespace CMS.ExternalAuthentication
{
    /// <summary>
    /// Represents the External authentication module.
    /// </summary>
    internal class ExternalAuthenticationModule : Module
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public ExternalAuthenticationModule() 
            : base(new ModuleMetadata("CMS.ExternalAuthentication"))
        {
        }
        

        /// <summary>
        /// Module init
        /// </summary>
        protected override void OnInit()
        {
            base.OnInit();

            // Handle sign-out event
            SecurityEvents.SignOut.Before += SignOut_Before;
        }


        /// <summary>
        /// SignOut event handler
        /// </summary>
        void SignOut_Before(object sender, SignOutEventArgs e)
        {
            WindowsLiveLogin.SignOut(e);
        }
    }
}
