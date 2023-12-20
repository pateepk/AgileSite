using System;

using CMS.Base;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Represents a validation error caused by setting shipping address from different customer.
    /// </summary>
    /// <seealso cref="CartAddressesValidator"/>
    public class ShippingAddressDoesNotMatchCustomerValidationError : IValidationError
    {
        /// <summary>
        /// Gets a key which can be used to retrieve a localized error message.
        /// </summary>
        string IValidationError.MessageKey => "ecommerce.validation.shippingaddressdoesnotmatchcustomer";


        /// <summary>
        /// Returns an empty array.
        /// </summary>
        object[] IValidationError.MessageParameters => Array.Empty<object>();
    }
}
