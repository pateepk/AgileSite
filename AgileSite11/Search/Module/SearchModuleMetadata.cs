using CMS.Core;

namespace CMS.Search
{
    /// <summary>
    /// Represents the Search module metadata.
    /// </summary>
    public class SearchModuleMetadata : ModuleMetadata
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public SearchModuleMetadata()
            :base(ModuleName.SEARCH)
        {
        }
    }
}