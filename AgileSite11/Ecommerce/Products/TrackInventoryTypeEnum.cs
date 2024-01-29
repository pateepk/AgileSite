using System;
using System.Linq;
using System.Text;

using CMS.Helpers;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Track Inventory Methods
    /// </summary>
    public enum TrackInventoryTypeEnum
    {
        /// <summary>
        /// Inventory is disabled
        /// </summary>        
        [EnumStringRepresentation("Disabled")]
        Disabled = 0,

        /// <summary>
        /// Track by product
        /// </summary>
        [EnumDefaultValue]
        [EnumStringRepresentation("ByProduct")]
        ByProduct = 1,

        /// <summary>
        /// Track by variants
        /// </summary>
        [EnumStringRepresentation("ByVariants")]
        ByVariants = 2
    }
}
