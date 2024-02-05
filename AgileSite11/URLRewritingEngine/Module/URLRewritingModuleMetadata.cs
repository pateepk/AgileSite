using CMS.Core;

namespace CMS.URLRewritingEngine
{
    /// <summary>
    /// Represents the URL Rewriting module metadata.
    /// </summary>
    public class URLRewritingModuleMetadata : ModuleMetadata
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public URLRewritingModuleMetadata()
            : base(ModuleName.URLREWRITING)
        {
        }
    }
}