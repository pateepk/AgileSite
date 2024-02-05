using System;

using CMS.Helpers;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Inventory removal options for bundle products.
    /// </summary>
    public enum BundleInventoryTypeEnum
    {
        /// <summary>
        /// Remove only bundle itself from inventory.
        /// </summary>
        [EnumStringRepresentation("REMOVEBUNDLE")]
        [EnumDefaultValue]
        RemoveBundle = 0,

        /// <summary>
        /// Remove only bundle items from inventory.
        /// </summary>
        [EnumStringRepresentation("REMOVEPRODUCTS")]
        RemoveProducts = 1,

        /// <summary>
        /// Remove both bundle and bundle items from inventory.
        /// </summary>
        [EnumStringRepresentation("REMOVEBUNDLEANDPRODUCTS")]
        RemoveBundleAndProducts = 2
    }
}