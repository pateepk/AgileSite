using CMS.Core;

namespace CMS.StrandsRecommender
{
    /// <summary>
    /// Represents the Strands Recommender module metadata.
    /// </summary>
    internal class StrandsRecommenderModuleMetadata : ModuleMetadata
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public StrandsRecommenderModuleMetadata()
            : base(ModuleName.STRANDSRECOMMENDER)
        {
            RootPath = "~/CMSModules/StrandsRecommender/";
        }
    }
}