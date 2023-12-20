using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CMS.Base;
using CMS.Helpers;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Class responsible for validation of shopping cart and its items.
    /// </summary>
    /// <seealso cref="ShoppingCartItemValidator"/>
    /// <seealso cref="ShoppingCartInfo"/>
    /// <seealso cref="ShoppingCartItemInfo"/>
    public class ShoppingCartValidator : IValidator
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
        /// Gets an enumeration of validation errors associated with this validator. An empty enumeration is returned
        /// if validation succeeded.
        /// </summary>
        public IEnumerable<IValidationError> Errors => errors ?? Enumerable.Empty<IValidationError>();


        /// <summary>
        /// Gets a value indicating whether validation succeeded.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when <see cref="Validate"/> has not been called yet.</exception>
        public bool IsValid => errors != null ? errors.Count == 0 : throw new InvalidOperationException($"The {nameof(Validate)}() method must be called prior to accessing the {nameof(IsValid)} property.");


        /// <summary>
        /// Initializes a new instance of the <see cref="ShoppingCartValidator"/> class for given <paramref name="cart"/>.
        /// </summary>
        /// <param name="cart">Shopping cart for which to perform the validation.</param>
        public ShoppingCartValidator(ShoppingCartInfo cart)
        {
            Cart = cart;
        }


        /// <summary>
        /// Validates shopping cart and its items.
        /// </summary>
        /// <returns>Returns true if validation passed, otherwise returns false.</returns>
        public bool Validate()
        {
            errors = new List<IValidationError>();

            ValidateCart();

            return IsValid;
        }


        /// <summary>
        /// Validates shopping cart with all its items.
        /// If any validation error occurs, it is added into the <see cref="Errors"/> collection.
        /// </summary>
        /// <seealso cref="ShoppingCartItemValidator"/>
        protected virtual void ValidateCart()
        {
            foreach (var item in Cart.CartItems)
            {
                var itemValidator = new ShoppingCartItemValidator(item);
                if (!itemValidator.Validate())
                {
                    errors.AddRange(itemValidator.Errors);
                }
            }
        }
    }
}
