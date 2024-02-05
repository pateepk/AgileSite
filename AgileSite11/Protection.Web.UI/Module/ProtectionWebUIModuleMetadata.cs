using CMS.Core;

namespace CMS.Protection.Web.UI
{
    /// <summary>
    /// Represents the Protection Web UI module metadata.
    /// </summary>
    public class ProtectionWebUIModuleMetadata : ModuleMetadata
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public ProtectionWebUIModuleMetadata()
            : base(ProtectionWebUIModule.PROTECTION_WEB_UI)
        {
        }
    }
}