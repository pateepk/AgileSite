using System;
using System.Linq;
using System.Text;

using CMS.Helpers;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Discount application enumeration.
    /// </summary>
    public enum DiscountApplicationEnum
    {
        /// <summary>
        /// Entire order.
        /// </summary>
        [EnumStringRepresentation("Order")]
        [EnumDefaultValue]
        Order = 0,


        /// <summary>
        /// Products.
        /// </summary>
        [EnumStringRepresentation("Products")]
        Products = 1,


        /// <summary>
        /// Shipping.
        /// </summary>
        [EnumStringRepresentation("Shipping")]
        Shipping = 2
    }
}
