using System;
using System.Collections.Generic;
using System.Linq;

using CMS.Base;


namespace CMS.Ecommerce
{
    /// <summary>
    /// Performs validation of a payment option to be assigned to a shopping cart.
    /// </summary>
    public class PaymentOptionValidator : IValidator
    {
        private List<IValidationError> mErrors;

        /// <summary>
        /// Gets an enumeration of validation errors associated with this validator. An empty enumeration is returned
        /// if validation succeeded.
        /// </summary>
        public IEnumerable<IValidationError> Errors => mErrors ?? Enumerable.Empty<IValidationError>();


        /// <summary>
        /// Gets a value indicating whether validation succeeded.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when <see cref="Validate"/> has not been called yet.</exception>
        public bool IsValid => mErrors != null ? mErrors.Count == 0 : throw new InvalidOperationException($"The {nameof(Validate)}() method must be called prior to accessing the {nameof(IsValid)} property.");


        /// <summary>
        /// Gets payment option this validator was initialized for.
        /// </summary>
        public PaymentOptionInfo PaymentOption
        {
            get;
        }


        /// <summary>
        /// Gets shopping cart this validator was initialized for.
        /// </summary>
        public ShoppingCartInfo Cart
        {
            get;
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="PaymentOptionValidator"/> class.
        /// </summary>
        /// <param name="paymentOption">Payment option for which to perform the validation.</param>
        /// <param name="shoppingCart">Shopping cart for which to perform the validation.</param>
        public PaymentOptionValidator(PaymentOptionInfo paymentOption, ShoppingCartInfo shoppingCart)
        {
            PaymentOption = paymentOption;
            Cart = shoppingCart;
        }


        /// <summary>
        /// Validates whether a payment option can be associated with the shopping cart.
        /// </summary>
        /// <returns>Returns true if validation passed, otherwise returns false.</returns>
        public virtual bool Validate()
        {
            mErrors = new List<IValidationError>();

            ValidatePaymentOption();

            return IsValid;
        }


        /// <summary>
        /// Validates that  payment option is:
        /// not null,
        /// assigned to the same site as the shopping cart or payment option is a global object,
        /// applicable to the shopping cart.
        /// </summary>
        /// <seealso cref="PaymentOptionNotSetValidationError"/>
        /// <seealso cref="PaymentOptionNotApplicableValidationError"/>
        /// <seealso cref="PaymentOptionFromDifferentSiteValidationError"/>
        protected virtual void ValidatePaymentOption()
        {
            if (PaymentOption == null)
            {
                mErrors.Add(new PaymentOptionNotSetValidationError());
                return;
            }

            if (!PaymentOptionInfoProvider.IsPaymentOptionApplicable(Cart, PaymentOption))
            {
                mErrors.Add(new PaymentOptionNotApplicableValidationError());
            }

            if (!PaymentOption.IsGlobal && (Cart.ShoppingCartSiteID != PaymentOption.PaymentOptionSiteID))
            {
                mErrors.Add(new PaymentOptionFromDifferentSiteValidationError());
            }
        }
    }
}
