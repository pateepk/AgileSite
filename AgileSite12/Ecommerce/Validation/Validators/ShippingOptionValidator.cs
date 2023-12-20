using System;
using System.Collections.Generic;
using System.Linq;

using CMS.Base;


namespace CMS.Ecommerce
{
    /// <summary>
    /// Performs validation of a shipping option to be assigned to a shopping cart.
    /// </summary>
    public class ShippingOptionValidator : IValidator
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
        /// Gets shipping option this validator was initialized for.
        /// </summary>
        public ShippingOptionInfo ShippingOptionInfo
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
        /// Initializes a new instance of the <see cref="ShippingOptionValidator"/> class.
        /// </summary>
        /// <param name="shippingOption">Shipping option for which to perform the validation.</param>
        /// <param name="shoppingCart">Shopping cart for which to perform the validation.</param>
        public ShippingOptionValidator(ShippingOptionInfo shippingOption, ShoppingCartInfo shoppingCart)
        {
            ShippingOptionInfo = shippingOption;
            Cart = shoppingCart;
        }


        /// <summary>
        /// Validates whether a shipping option can be associated with the shopping cart.
        /// </summary>
        /// <returns>Returns true if validation passed, otherwise returns false.</returns>
        public virtual bool Validate()
        {
            mErrors = new List<IValidationError>();

            ValidateShippingOption();

            return IsValid;
        }


        /// <summary>
        /// Validates that shipping option is:
        /// not null,
        /// enabled,
        /// applicable to <see cref="Cart"/>,
        /// assigned to the same site as the <see cref="Cart"/>.
        /// </summary>
        /// <seealso cref="ShippingOptionNotSetValidationError"/>
        /// <seealso cref="ShippingOptionDisabledValidationError"/>
        /// <seealso cref="ShippingOptionNotApplicableValidationError"/>
        /// <seealso cref="ShippingOptionFromDifferentSiteValidationError"/>
        protected virtual void ValidateShippingOption()
        {
            if (ShippingOptionInfo == null)
            {
                if (!Cart.IsShippingNeeded)
                {
                    return;
                }

                mErrors.Add(new ShippingOptionNotSetValidationError());
                return;
            }

            if (!ShippingOptionInfo.ShippingOptionEnabled)
            {
                mErrors.Add(new ShippingOptionDisabledValidationError());
            }

            if (!ShippingOptionInfoProvider.IsShippingOptionApplicable(Cart, ShippingOptionInfo))
            {
                mErrors.Add(new ShippingOptionNotApplicableValidationError());
            }

            if (ShippingOptionInfo.ShippingOptionSiteID != 0 && Cart.ShoppingCartSiteID != ShippingOptionInfo.ShippingOptionSiteID)
            {
                mErrors.Add(new ShippingOptionFromDifferentSiteValidationError());
            }
        }
    }
}
