using CMS.Core;

namespace CMS.OnlineMarketing
{
    /// <summary>
    /// Represents the A/B tests module metadata.
    /// </summary>
    internal class ABTestModuleMetadata : ModuleMetadata
    {
        /// <summary>
        /// Initializes a new instance of the ABTestModuleMetadata class.
        /// </summary>
        public ABTestModuleMetadata() : base(ModuleName.ABTEST)
        {
            RootPath = "~/CMSModules/OnlineMarketing/";
        }
    }
}