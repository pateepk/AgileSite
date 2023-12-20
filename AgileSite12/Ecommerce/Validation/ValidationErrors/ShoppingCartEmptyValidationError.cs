using System;

using CMS.Base;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Represents a validation error resulting from shopping cart being empty.
    /// </summary>
    /// <seealso cref="ShoppingCartValidator"/>
    public class ShoppingCartEmptyValidationError : IValidationError
    {
        /// <summary>
        /// Gets a key which can be used to retrieve a localized error message.
        /// </summary>
        public string MessageKey => "com.checkout.cartisempty";


        /// <summary>
        /// Returns an empty array.
        /// </summary>
        public object[] MessageParameters => Array.Empty<object>();
    }
}
