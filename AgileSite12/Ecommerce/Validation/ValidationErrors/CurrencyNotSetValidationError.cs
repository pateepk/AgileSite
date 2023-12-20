using System;

using CMS.Base;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Represents a validation error resulting from shopping cart missing a currency.
    /// </summary>
    /// <seealso cref="ShoppingService"/>
    public class CurrencyNotSetValidationError : IValidationError
    {
        /// <summary>
        /// Gets a key which can be used to retrieve a localized error message.
        /// </summary>
        public string MessageKey => "ecommerce.validation.currencynotset";


        /// <summary>
        /// Returns an empty array.
        /// </summary>
        public object[] MessageParameters => Array.Empty<object>();
    }
}
