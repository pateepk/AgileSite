using CMS.Core;

namespace CMS.TranslationServices
{
    /// <summary>
    /// Represents the Translation Services module metadata.
    /// </summary>
    public class TranslationServicesModuleMetadata : ModuleMetadata
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public TranslationServicesModuleMetadata()
            : base(ModuleName.TRANSLATIONSERVICES)
        {
            RootPath = "~/CMSModules/Translations/";
        }
    }
}