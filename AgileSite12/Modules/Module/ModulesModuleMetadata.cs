using CMS.Core;

namespace CMS.Modules
{
    /// <summary>
    /// Represents the Modules module metadata.
    /// </summary>
    public class ModulesModuleMetadata : ModuleMetadata
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public ModulesModuleMetadata()
            : base(ModuleName.MODULES)
        {
        }
    }
}