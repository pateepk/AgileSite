using CMS.Core;

namespace CMS.PortalEngine
{
    /// <summary>
    /// Represents the Portal module metadata.
    /// </summary>
    public class PortalModuleMetadata : ModuleMetadata
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public PortalModuleMetadata()
            : base(ModuleName.PORTALENGINE)
        {
            RootPath = "~/CMSModules/PortalEngine/";
        }
    }
}