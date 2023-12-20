using System;
using System.Collections.Generic;
using System.Linq;

using CMS.Base;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Class representing validator of addresses belonging to a cart.
    /// </summary>
    public class CartAddressesValidator : IValidator
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
        public virtual IEnumerable<IValidationError> Errors => errors ?? Enumerable.Empty<IValidationError>();


        /// <summary>
        /// Gets a value indicating whether validation succeeded.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when <see cref="Validate"/> has not been called yet.</exception>
        public virtual bool IsValid => errors != null ? errors.Count == 0 : throw new InvalidOperationException($"The {nameof(Validate)}() method must be called prior to accessing the {nameof(IsValid)} property.");


        /// <summary>
        /// Initializes a new instance of the <see cref="CartAddressesValidator"/> class for given <paramref name="cart"/>.
        /// </summary>
        /// <param name="cart">Shopping cart for which to perform the validation.</param>
        public CartAddressesValidator(ShoppingCartInfo cart)
        {
            Cart = cart;
        }


        /// <summary>
        /// Validates cart addresses.
        /// </summary>
        /// <returns>Result of validation.</returns>
        public bool Validate()
        {
            errors = new List<IValidationError>();

            ValidateAddresses();

            return IsValid;
        }


        /// <summary>
        /// Validates shipping and billing addresses.
        /// </summary>
        protected virtual void ValidateAddresses()
        {
            // Customer was not stored into the database yet
            if ((Cart.Customer == null) || (Cart.Customer.CustomerID <= 0))
            {
                return;
            }

            var billingAddress = Cart.ShoppingCartBillingAddress;
            var shippingAddress = Cart.ShoppingCartShippingAddress;

            if (billingAddress?.AddressID > 0)
            {
                if (billingAddress.AddressCustomerID != Cart.Customer.CustomerID)
                {
                    errors.Add(new BillingAddressDoesNotMatchCustomerValidationError());
                }
            }

            if (shippingAddress?.AddressID > 0)
            {
                if (shippingAddress.AddressCustomerID != Cart.Customer.CustomerID)
                {
                    errors.Add(new ShippingAddressDoesNotMatchCustomerValidationError());
                }
            }

            ValidateCustomerAddress(Cart.ShoppingCartShippingAddress);
            ValidateCustomerAddress(Cart.ShoppingCartBillingAddress);
            ValidateCustomerAddress(Cart.ShoppingCartCompanyAddress);
        }


        private void ValidateCustomerAddress(AddressInfo address)
        {
            if (address == null)
            {
                return;
            }

            var validator = new CustomerAddressValidator(address);
            if (!validator.Validate())
            {
                errors.AddRange(validator.Errors);
            }
        }
    }
}
