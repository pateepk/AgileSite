using System;

using CMS.Helpers;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Options saying what the taxes are to be based on.
    /// </summary>
    public enum ApplyTaxBasedOnEnum
    {
        /// <summary>
        /// Taxes are based on billing address.
        /// </summary>
        [EnumDefaultValue]
        [EnumStringRepresentation("BillingAddress")]
        BillingAddress = 0,

        /// <summary>
        /// Taxes are based on shipping address.
        /// </summary>
        [EnumStringRepresentation("ShippingAddress")]
        ShippingAddress = 1
    }
}