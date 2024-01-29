using CMS.Core;

namespace CMS.CustomTables
{
    /// <summary>
    /// Represents the Site module metadata.
    /// </summary>
    public class CustomTableModuleMetadata : ModuleMetadata
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public CustomTableModuleMetadata()
            : base(ModuleName.CUSTOMTABLES)
        {
        }
    }
}