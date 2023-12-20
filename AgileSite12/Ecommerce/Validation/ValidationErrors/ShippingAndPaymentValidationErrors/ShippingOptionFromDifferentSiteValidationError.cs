using System;

using CMS.Base;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Represents a validation error resulting from shipping option belonging to a different site.
    /// </summary>
    /// <seealso cref="ShippingOptionValidator"/>
    public class ShippingOptionFromDifferentSiteValidationError : IValidationError
    {
        /// <summary>
        /// Gets a key which can be used to retrieve a localized error message.
        /// </summary>
        public string MessageKey => "ecommerce.validation.shippingoptionfromdifferentsite";


        /// <summary>
        /// Returns an empty array.
        /// </summary>
        public object[] MessageParameters => Array.Empty<object>();
    }
}
