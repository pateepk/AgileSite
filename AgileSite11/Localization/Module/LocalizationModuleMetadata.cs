using CMS.Core;

namespace CMS.Localization
{
    /// <summary>
    /// Represents the Localization module metadata.
    /// </summary>
    public class LocalizationModuleMetadata : ModuleMetadata
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public LocalizationModuleMetadata()
            : base(ModuleName.LOCALIZATION)
        {
        }
    }
}