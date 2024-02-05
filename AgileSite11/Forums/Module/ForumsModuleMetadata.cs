using CMS.Core;

namespace CMS.Forums
{
    /// <summary>
    /// Represents the Forums module metadata.
    /// </summary>
    public class ForumsModuleMetadata : ModuleMetadata
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public ForumsModuleMetadata()
            : base(ModuleName.FORUMS)
        {
            RootPath = "~/CMSModules/Forums/";
        }
    }
}