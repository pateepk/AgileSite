using CMS.Core;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Represents the E-commerce module metadata.
    /// </summary>
    internal class ECommerceModuleMetadata : ModuleMetadata
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public ECommerceModuleMetadata()
            : base(ModuleName.ECOMMERCE)
        {
            RootPath = "~/CMSModules/Ecommerce/";
        }
    }
}