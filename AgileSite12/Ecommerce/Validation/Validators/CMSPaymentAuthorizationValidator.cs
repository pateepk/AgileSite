using System;
using System.Collections.Generic;
using System.Linq;

using CMS.Base;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Class represents validator for payment authorization.
    /// </summary>
    public class CMSPaymentAuthorizationValidator : IValidator
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
        /// Initializes a new instance of the <see cref="CMSPaymentAuthorizationValidator"/> class for given <paramref name="cart"/>.
        /// </summary>
        /// <param name="cart">Shopping cart for which to perform the validation.</param>
        public CMSPaymentAuthorizationValidator(ShoppingCartInfo cart)
        {
            Cart = cart;
        }


        /// <summary>
        /// Validates whether user is authorized for used payment method.
        /// </summary>
        /// <returns>Result of validation.</returns>
        public bool Validate()
        {
            errors = new List<IValidationError>();

            var provider = CMSPaymentGatewayProvider.GetPaymentGatewayProvider<CMSPaymentGatewayProvider>(Cart.ShoppingCartPaymentOptionID);

            if (provider != null && !provider.IsUserAuthorizedToFinishPayment(Cart.User, Cart))
            {
                errors.Add(new PaymentNotAuthorizedValidationError(provider.ErrorMessage));
            }

            return IsValid;
        }
    }
}

