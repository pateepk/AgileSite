using System;

using CMS.Base;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Represents a validation error resulting from shipping option being disabled.
    /// </summary>
    /// <seealso cref="ShippingOptionValidator"/>
    public class ShippingOptionDisabledValidationError : IValidationError
    {
        /// <summary>
        /// Gets a key which can be used to retrieve a localized error message.
        /// </summary>
        public string MessageKey => "ecommerce.validation.shippingoptiondisabled";


        /// <summary>
        /// Returns an empty array.
        /// </summary>
        public object[] MessageParameters => Array.Empty<object>();
    }
}
