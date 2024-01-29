using CMS;
using CMS.DataEngine;
using CMS.Protection.Web.UI;

[assembly: RegisterModule(typeof(ProtectionWebUIModule))]

namespace CMS.Protection.Web.UI
{
    /// <summary>
    /// Represents the Protection Web UI module.
    /// </summary>
    public class ProtectionWebUIModule : Module
    {
        internal const string PROTECTION_WEB_UI = "CMS.Protection.Web.UI";

        /// <summary>
        /// Default constructor
        /// </summary>
        public ProtectionWebUIModule()
            : base(new ProtectionWebUIModuleMetadata())
        {
        }

        /// <summary>
        /// Module pre-initialization
        /// </summary>
        protected override void OnPreInit()
        {
            base.OnPreInit();

            CsrfProtection.PreInit();
        }
    }
}