using CMS.Core;

namespace CMS.Synchronization
{
    /// <summary>
    /// Represents the Synchronization Engine module metadata.
    /// </summary>
    public class SynchronizationEngineModuleMetadata : ModuleMetadata
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public SynchronizationEngineModuleMetadata()
            : base(ModuleName.SYNCHRONIZATIONENGINE)
        {
            RootPath = "~/CMSModules/Integration/";
        }
    }
}