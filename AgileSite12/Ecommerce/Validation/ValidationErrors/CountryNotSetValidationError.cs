using System;

using CMS.Base;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Represents a validation error resulting from missing country specification.
    /// </summary>
    /// <seealso cref="ShoppingService"/>
    public class CountryNotSetValidationError : IValidationError
    {
        /// <summary>
        /// Gets a key which can be used to retrieve a localized error message.
        /// </summary>
        public string MessageKey
        {
            get
            {
                return "ecommerce.validation.countrynotset";
            }
        }


        /// <summary>
        /// Returns an empty array.
        /// </summary>
        public object[] MessageParameters
        {
            get
            {
                return Array.Empty<object>();
            }
        }
    }
}