using System;

using CMS.Base;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Represents a validation error resulting from payment option not being applicable.
    /// </summary>
    /// <seealso cref="PaymentOptionValidator"/>
    public class PaymentOptionNotApplicableValidationError : IValidationError
    {
        /// <summary>
        /// Gets a key which can be used to retrieve a localized error message.
        /// </summary>
        public string MessageKey => "com.checkout.paymentoptionnotapplicable";


        /// <summary>
        /// Returns an empty array.
        /// </summary>
        public object[] MessageParameters => Array.Empty<object>();
    }
}
