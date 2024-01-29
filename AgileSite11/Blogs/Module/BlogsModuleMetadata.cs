using CMS.Core;

namespace CMS.Blogs
{
    /// <summary>
    /// Represents the Blogs module metadata.
    /// </summary>
    public class BlogsModuleMetadata : ModuleMetadata
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public BlogsModuleMetadata()
            : base(ModuleName.BLOGS)
        {
            RootPath = "~/CMSModules/Blogs/";
        }
    }
}