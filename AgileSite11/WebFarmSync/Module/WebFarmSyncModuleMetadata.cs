using CMS.Core;

namespace CMS.WebFarmSync
{
    /// <summary>
    /// Represents the Web Farm Synchronization module metadata.
    /// </summary>
    public class WebFarmSyncModuleMetadata : ModuleMetadata
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public WebFarmSyncModuleMetadata()
            : base(ModuleName.WEBFARMSYNC)
        {
            RootPath = "~/CMSModules/WebFarms/";
        }
    }
}