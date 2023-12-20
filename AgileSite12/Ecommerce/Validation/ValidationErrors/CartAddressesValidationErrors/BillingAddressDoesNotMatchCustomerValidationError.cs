using System;

using CMS.Base;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Represents a validation error caused by setting billing address from different customer.
    /// </summary>
    /// <seealso cref="CartAddressesValidator"/>
    public class BillingAddressDoesNotMatchCustomerValidationError : IValidationError
    {
        /// <summary>
        /// Gets a key which can be used to retrieve a localized error message.
        /// </summary>
        string IValidationError.MessageKey => "ecommerce.validation.billingaddressdoesnotmatchcustomer";


        /// <summary>
        /// Returns an empty array.
        /// </summary>
        object[] IValidationError.MessageParameters => Array.Empty<object>();
    }
}
