using System;
using System.Collections.Generic;
using System.Linq;

using CMS.Base;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Performs validation of an item to be added to a shopping cart.
    /// </summary>
    /// <seealso cref="ShoppingService"/>
    public class AddItemToCartValidator : IValidator
    {
        private List<IValidationError> errors;


        /// <summary>
        /// Gets shopping cart this validator was initialized for.
        /// </summary>
        public ShoppingCartInfo Cart
        {
            get;
        }


        /// <summary>
        /// Gets item parameters this validator was initialized for.
        /// </summary>
        public ShoppingCartItemParameters ItemParameters
        {
            get;
        }


        /// <summary>
        /// Gets an enumeration of validation errors associated with this validator. An empty enumeration is returned
        /// if validation succeeded.
        /// </summary>
        /// <seealso cref="SKUFromDifferentSiteValidationError"/>
        public IEnumerable<IValidationError> Errors => errors ?? Enumerable.Empty<IValidationError>();


        /// <summary>
        /// Gets a value indicating whether validation succeeded.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when <see cref="Validate"/> has not been called yet.</exception>
        public bool IsValid => errors != null ? errors.Count == 0 : throw new InvalidOperationException($"The {nameof(Validate)}() method must be called prior to accessing the {nameof(IsValid)} property.");


        /// <summary>
        /// Initializes a new instance of the <see cref="AddItemToCartValidator"/> class for given <paramref name="cart"/> and <paramref name="itemParameters"/>.
        /// </summary>
        /// <param name="cart">Shopping cart for which to perform the validation.</param>
        /// <param name="itemParameters">Parameters representing an item to be validated against.</param>
        public AddItemToCartValidator(ShoppingCartInfo cart, ShoppingCartItemParameters itemParameters)
        {
            Cart = cart;
            ItemParameters = itemParameters;
        }


        /// <summary>
        /// Validates the <see cref="ItemParameters"/> specify an SKU from the same site as <see cref="Cart"/> is.
        /// </summary>
        /// <returns>Returns a value indicating whether validation succeeded.</returns>
        public bool Validate()
        {
            errors = new List<IValidationError>();

            var sku = SKUInfoProvider.GetSKUInfo(ItemParameters.SKUID);
            if (sku?.SKUSiteID > 0 && Cart.ShoppingCartSiteID != sku.SKUSiteID)
            {
                errors.Add(new SKUFromDifferentSiteValidationError());
            }

            return IsValid;
        }
    }
}
