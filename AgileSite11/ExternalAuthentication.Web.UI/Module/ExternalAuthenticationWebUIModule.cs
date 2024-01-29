using CMS;
using CMS.Core;
using CMS.DataEngine;
using CMS.ExternalAuthentication.Web.UI;

[assembly: RegisterModule(typeof(ExternalAuthenticationWebUIModule))]


namespace CMS.ExternalAuthentication.Web.UI
{
    /// <summary>
    /// Represents the External authentication module.
    /// </summary>
    internal class ExternalAuthenticationWebUIModule : Module
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public ExternalAuthenticationWebUIModule() 
            : base(new ModuleMetadata("CMS.ExternalAuthentication.Web.UI"))
        {
        }
        

        /// <summary>
        /// Module init
        /// </summary>
        protected override void OnInit()
        {
            base.OnInit();

            // Handle sign-out event
            FacebookConnectScriptHelper.InitializeFacebookConnect();
        }
    }
}
