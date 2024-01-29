using CMS;
using CMS.Base;
using CMS.Core;
using CMS.DataEngine;
using CMS.WIFIntegration;

[assembly: RegisterModule(typeof(WIFIntegrationModule))]

namespace CMS.WIFIntegration
{
    /// <summary>
    /// Represents the WIF Integration module.
    /// </summary>
    public class WIFIntegrationModule : Module
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public WIFIntegrationModule()
            : base(new WIFIntegrationModuleMetadata())
        {
        }


        /// <summary>
        /// Register AuthenticationRequested event.
        /// </summary>
        protected override void OnInit()
        {
            base.OnInit();
            if (SystemContext.IsFullTrustLevel)
            {
                WIFIntegrationHandlers.Init();
            }
            else
            {
                CoreServices.EventLog.LogEvent("W", "WIF authentication", "NotEnabled", "WIF authentication was not enabled as it requires fully trusted application.");
            }
        }
    }
}
