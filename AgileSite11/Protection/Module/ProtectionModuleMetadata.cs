using CMS.Core;

namespace CMS.Protection
{
    /// <summary>
    /// Represents the Protection module metadata.
    /// </summary>
    public class ProtectionModuleMetadata : ModuleMetadata
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public ProtectionModuleMetadata()
            : base(ModuleName.PROTECTION)
        {
        }
    }
}