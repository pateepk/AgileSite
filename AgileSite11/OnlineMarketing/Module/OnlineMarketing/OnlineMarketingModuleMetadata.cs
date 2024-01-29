using CMS.Core;

namespace CMS.OnlineMarketing
{
    /// <summary>
    /// Represents the Online Marketing module metadata.
    /// </summary>
    internal class OnlineMarketingModuleMetadata : ModuleMetadata
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public OnlineMarketingModuleMetadata()
            : base(ModuleName.ONLINEMARKETING)
        {
            RootPath = "~/CMSModules/OnlineMarketing/";
        }
    }
}