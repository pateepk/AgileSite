using System;
using System.Collections.Generic;
using System.Linq;

using CMS.Base;
using CMS.Core;
using CMS.Protection;
using CMS.SiteProvider;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Class representing validator for order finalization.
    /// </summary>
    /// <seealso cref="ShoppingCartValidator"/>
    /// <seealso cref="CMSPaymentAuthorizationValidator"/>
    /// <seealso cref="ShippingAndPaymentValidator"/>
    /// <seealso cref="ShoppingService"/>
    public class CreateOrderValidator : IValidator
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
        /// Initializes a new instance of the <see cref="CreateOrderValidator"/> class for given <paramref name="cart"/>.
        /// </summary>
        /// <param name="cart">Shopping cart for which to perform the validation.</param>
        public CreateOrderValidator(ShoppingCartInfo cart)
        {
            Cart = cart;
        }


        /// <summary>
        /// Validates shopping cart before creating order.
        /// Validation includes: all required properties must be set, cart must contain at least one item, user must not be disabled,
        /// shipping and payment options, cart addresses and user must not be banned if site requires
        /// registration.
        /// </summary>
        /// <returns>Returns true if validation passes, otherwise returns false.</returns>
        public bool Validate()
        {
            errors = new List<IValidationError>();

            if (Cart.Customer == null)
            {
                errors.Add(new CustomerNotSetValidationError());
            }

            if (Cart.Currency == null)
            {
                errors.Add(new CurrencyNotSetValidationError());
            }

            if (Cart.ShoppingCartBillingAddress == null)
            {
                errors.Add(new BillingAddressNotSetValidationError());
            }

            if (Cart.User != null && !Cart.User.Enabled)
            {
                errors.Add(new DisabledUserValidationError());
            }

            if (!Cart.CartItems.Any())
            {
                errors.Add(new ShoppingCartEmptyValidationError());
            }

            ValidateShoppingCart();

            ValidateShippingAndPaymentOptions();

            ValidateCartAddresses();

            ValidateBannedUser();

            return IsValid;
        }


        /// <summary>
        /// Validates shopping cart and its items.
        /// For more details on what is validated refer to <see cref="ShoppingCartValidator"/> and <see cref="ShoppingCartItemValidator"/>.
        /// </summary>
        protected virtual void ValidateShoppingCart()
        {
            var validator = new ShoppingCartValidator(Cart);

            if (!validator.Validate())
            {
                errors.AddRange(validator.Errors);
            }
        }


        /// <summary>
        /// Validates that user is authorized for selected payment option.
        /// </summary>
        /// <seealso cref="CMSPaymentAuthorizationValidator"/>
        protected virtual void ValidatePaymentOption()
        {
            var validator = new CMSPaymentAuthorizationValidator(Cart);

            if (!validator.Validate())
            {
                errors.AddRange(validator.Errors);
            }
        }


        /// <summary>
        /// Validates applicability of shipping and payment options on their own and with respect to each other.
        /// </summary>
        /// <seealso cref="ShippingAndPaymentValidator"/>
        protected virtual void ValidateShippingAndPaymentOptions()
        {
            var validator = new ShippingAndPaymentValidator(Cart);

            if (!validator.Validate())
            {
                errors.AddRange(validator.Errors);
            }
        }


        /// <summary>
        /// Validates shipping and billing addresses match customer and that all cart addresses are valid.
        /// </summary>
        /// <seealso cref="CartAddressesValidator"/>
        protected virtual void ValidateCartAddresses()
        {
            var validator = new CartAddressesValidator(Cart);

            if (!validator.Validate())
            {
                errors.AddRange(validator.Errors);
            }
        }


        /// <summary>
        /// When site requires registration after checkout validates that user is not banned.
        /// </summary>
        /// <seealso cref="BannedUserValidationError"/>
        protected virtual void ValidateBannedUser()
        {
            var customer = Cart.Customer;
            if ((customer == null) || customer.CustomerIsRegistered)
            {
                return;
            }

            var repository = Service.Resolve<ICustomerRegistrationRepositoryFactory>().GetRepository(SiteContext.CurrentSiteID);

            if (repository.IsCustomerRegisteredAfterCheckout)
            {
                // Ban IP addresses which are blocked for registration
                var registrationBan = !BannedIPInfoProvider.IsAllowed(Cart.SiteName, BanControlEnum.Registration);
                var allUserActionBan = !BannedIPInfoProvider.IsAllowed(Cart.SiteName, BanControlEnum.AllNonComplete);

                if (registrationBan || allUserActionBan)
                {
                    errors.Add(new BannedUserValidationError());
                }
            }
        }
    }
}
