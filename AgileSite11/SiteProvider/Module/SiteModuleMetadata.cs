using CMS.Core;

namespace CMS.SiteProvider
{
    /// <summary>
    /// Represents the Site module metadata.
    /// </summary>
    public class SiteModuleMetadata : ModuleMetadata
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public SiteModuleMetadata()
            : base(ModuleName.SITE)
        {
        }
    }
}