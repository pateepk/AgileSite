using System;

using CMS.Helpers;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Options specifying how will the products be organized in product management UI.
    /// </summary>
    public enum ProductsTreeModeEnum
    {
        /// <summary>
        /// No products tree.
        /// </summary>
        [EnumDefaultValue]
        [EnumStringRepresentation("None")]
        None = 0,

        /// <summary>
        /// Products tree based on documents.
        /// </summary>
        [EnumStringRepresentation("Sections")]
        Sections = 1
    }
}
