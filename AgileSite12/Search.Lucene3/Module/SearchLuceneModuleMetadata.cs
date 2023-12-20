using CMS.Core;

namespace CMS.Search.Lucene3
{
    /// <summary>
    /// Represents the Search Lucene module metadata.
    /// </summary>
    public class SearchLuceneModuleMetadata : ModuleMetadata
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public SearchLuceneModuleMetadata()
            :base(ModuleName.SEARCHLUCENE3)
        {
        }
    }
}