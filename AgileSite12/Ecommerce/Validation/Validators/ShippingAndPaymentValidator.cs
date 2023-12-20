using System;
using System.Collections.Generic;
using System.Linq;

using CMS.Base;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Validates that selected shipping option and payment method are valid for given cart.
    /// </summary>
    public class ShippingAndPaymentValidator : IValidator
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
        /// Initializes a new instance of the <see cref="ShippingAndPaymentValidator"/> class for given <paramref name="cart"/>.
        /// </summary>
        /// <param name="cart">Shopping cart for which to perform the validation.</param>
        public ShippingAndPaymentValidator(ShoppingCartInfo cart)
        {
            Cart = cart;
        }


        /// <summary>
        /// Validates shipping and payment options associated with shopping cart.
        /// </summary>
        /// <returns>Returns true if validation passed, otherwise returns false.</returns>
        public virtual bool Validate()
        {
            errors = new List<IValidationError>();

            ValidateShippingOption();
            ValidatePaymentOption();
            ValidatePaymentAndShipping();

            return IsValid;
        }


        /// <summary>
        /// Validates shipping and payment options associated with shopping cart.
        /// </summary>
        protected virtual void ValidatePaymentAndShipping()
        {
        }


        /// <summary>
        /// Validates payment option associated with shopping cart.
        /// </summary>
        /// <seealso cref="PaymentOptionNotSetValidationError"/>
        /// <seealso cref="PaymentOptionNotApplicableValidationError"/>
        /// <seealso cref="PaymentOptionFromDifferentSiteValidationError"/>
        protected virtual void ValidatePaymentOption()
        {
            var validator = new PaymentOptionValidator(Cart.PaymentOption, Cart);

            if (!validator.Validate())
            {
                errors.AddRange(validator.Errors);
            }
        }


        /// <summary>
        /// Validates shipping option associated with shopping cart.
        /// </summary>
        /// <seealso cref="ShippingOptionNotSetValidationError"/>
        /// <seealso cref="ShippingOptionDisabledValidationError"/>
        /// <seealso cref="ShippingOptionNotApplicableValidationError"/>
        /// <seealso cref="ShippingOptionFromDifferentSiteValidationError"/>
        protected virtual void ValidateShippingOption()
        {
            var validator = new ShippingOptionValidator(Cart.ShippingOption, Cart);

            if (!validator.Validate())
            {
                errors.AddRange(validator.Errors);
            }
        }
    }
}
