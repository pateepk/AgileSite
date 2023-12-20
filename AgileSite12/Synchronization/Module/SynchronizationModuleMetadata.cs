using CMS.Core;

namespace CMS.Synchronization
{
    /// <summary>
    /// Represents the Synchronization module metadata.
    /// </summary>
    public class SynchronizationModuleMetadata : ModuleMetadata
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public SynchronizationModuleMetadata()
            : base(ModuleName.SYNCHRONIZATION)
        {
            RootPath = "~/CMSModules/Integration/";
        }
    }
}