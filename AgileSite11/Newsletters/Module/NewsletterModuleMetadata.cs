using CMS.Core;

namespace CMS.Newsletters
{
    /// <summary>
    /// Represents the Newsletter module metadata.
    /// </summary>
    public class NewsletterModuleMetadata : ModuleMetadata
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public NewsletterModuleMetadata()
            : base(ModuleName.NEWSLETTER)
        {
            RootPath = "~/CMSModules/Newsletters/";
        }
    }
}