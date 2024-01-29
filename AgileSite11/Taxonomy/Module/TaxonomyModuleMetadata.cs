using CMS.Core;

namespace CMS.Taxonomy
{
    /// <summary>
    /// Represents the Taxonomy module metadata.
    /// </summary>
    public class TaxonomyModuleMetadata : ModuleMetadata
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public TaxonomyModuleMetadata()
            : base(ModuleName.TAXONOMY)
        {
        }
    }
}