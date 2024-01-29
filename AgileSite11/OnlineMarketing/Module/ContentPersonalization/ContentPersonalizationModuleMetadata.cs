using CMS.Core;

namespace CMS.OnlineMarketing
{
    /// <summary>
    /// Represents the Content personalization module metadata.
    /// </summary>
    internal class ContentPersonalizationModuleMetadata : ModuleMetadata
    {
        /// <summary>
        /// Initializes a new instance of the ContentPersonalizationModuleMetadata class.
        /// </summary>
        public ContentPersonalizationModuleMetadata() : base(ModuleName.CONTENTPERSONALIZATION)
        {
            RootPath = "~/CMSModules/OnlineMarketing/";
        }
    }
}