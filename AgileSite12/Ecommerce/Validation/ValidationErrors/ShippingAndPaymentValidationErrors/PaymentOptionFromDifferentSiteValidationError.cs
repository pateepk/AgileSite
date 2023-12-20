using System;

using CMS.Base;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Represents a validation error caused by setting payment option from different site.
    /// </summary>
    /// <seealso cref="CreateOrderValidator"/>
    public class PaymentOptionFromDifferentSiteValidationError : IValidationError
    {
        /// <summary>
        /// Gets a key which can be used to retrieve a localized error message.
        /// </summary>
        string IValidationError.MessageKey => "ecommerce.validation.paymentoptionfromdifferentsite";


        /// <summary>
        /// Returns an empty array.
        /// </summary>
        object[] IValidationError.MessageParameters => Array.Empty<object>();
    }
}
