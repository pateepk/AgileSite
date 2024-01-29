using System;

using CMS.Helpers;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Type of the product option category.
    /// </summary>
    public enum OptionCategoryTypeEnum
    {
        /// <summary>
        /// Category of this type represents specific optional accessories of the product which is selected by the customer from the predefined list of products.
        /// </summary>
        [EnumDefaultValue]
        [EnumStringRepresentation("PRODUCTS")]
        Products = 0,


        /// <summary>
        /// Category of this type represents specific attribute of the product which is configured by the customer from the predefined list of values, e.g. color.
        /// </summary>
        [EnumStringRepresentation("ATTRIBUTE")]
        Attribute = 1,


        /// <summary>
        /// Category of this type represents text attribute of the product which is entered by the customer, e.g.: label for the T-shirt
        /// </summary>
        [EnumStringRepresentation("TEXT")]
        Text = 2
    }
}