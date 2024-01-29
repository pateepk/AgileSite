using System;

using CMS.Helpers;

namespace CMS.Ecommerce
{
    /// <summary>
    /// SKU product type options.
    /// </summary>
    public enum SKUProductTypeEnum
    {
        /// <summary>
        /// Standard product type.
        /// </summary>
        [EnumDefaultValue]
        [EnumStringRepresentation("PRODUCT")]
        Product = 0,

        /// <summary>
        /// Membership product type.
        /// </summary>
        [EnumStringRepresentation("MEMBERSHIP")]
        Membership = 1,

        /// <summary>
        /// E-product product type.
        /// </summary>
        [EnumStringRepresentation("EPRODUCT")]
        EProduct = 2,

        /// <summary>
        /// Bundle product type.
        /// </summary>
        [EnumStringRepresentation("BUNDLE")]
        Bundle = 4,

        /// <summary>
        /// Text product type. Used for product options to which customer is able to define his custom text, e.g. custom label for a T-shirt.
        /// </summary>
        [EnumStringRepresentation("TEXT")]
        Text = 5
    }
}