using System;

using CMS.Base;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Represents a validation error caused by customer being banned from regsitration.
    /// </summary>
    /// <seealso cref="CreateOrderValidator"/>
    public class BannedUserValidationError : IValidationError
    {
        /// <summary>
        /// Gets a key which can be used to retrieve a localized error message.
        /// </summary>
        public string MessageKey => "banip.ipisbannedregistration";


        /// <summary>
        /// Returns an empty array.
        /// </summary>
        public object[] MessageParameters => Array.Empty<object>();
    }
}
