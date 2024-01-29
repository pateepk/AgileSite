using CMS.Core;

namespace CMS.Base
{
    /// <summary>
    /// Represents the Settings Provider module metadata.
    /// </summary>
    public class BaseModuleMetadata : ModuleMetadata
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public BaseModuleMetadata()
            : base(ModuleName.BASE)
        {
        }
    }
}