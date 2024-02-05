using CMS.Core;

namespace CMS.OnlineMarketing
{
    /// <summary>
    /// Represents the MVT tests module metadata.
    /// </summary>
    internal class MVTestModuleMetadata : ModuleMetadata
    {
        /// <summary>
        /// Initializes a new instance of the MVTestModuleMetadata class.
        /// </summary>
        public MVTestModuleMetadata() : base(ModuleName.MVTEST)
        {
            RootPath = "~/CMSModules/OnlineMarketing/";
        }
    }
}