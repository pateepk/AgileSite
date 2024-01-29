using CMS.Core;

namespace CMS.LicenseProvider
{
    /// <summary>
    /// Represents the License module metadata.
    /// </summary>
    public class LicenseModuleMetadata : ModuleMetadata
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public LicenseModuleMetadata()
            : base(ModuleName.LICENSE)
        {
            RootPath = "~/CMSModules/Licenses/";
        }
    }
}