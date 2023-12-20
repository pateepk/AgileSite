using System;

using CMS.Base;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Represents a validation error caused by user being disabled.
    /// </summary>
    /// <seealso cref="CreateOrderValidator"/>
    public class DisabledUserValidationError : IValidationError
    {
        /// <summary>
        /// Gets a key which can be used to retrieve a localized error message.
        /// </summary>
        string IValidationError.MessageKey => "ecommerce.validation.disableduser";


        /// <summary>
        /// Returns an empty array.
        /// </summary>
        object[] IValidationError.MessageParameters => Array.Empty<object>();
    }
}
