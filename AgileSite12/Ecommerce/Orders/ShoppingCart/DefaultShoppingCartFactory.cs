using System;

using CMS.DataEngine;
using CMS.Membership;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Represents a factory able to create and initialize new instances of <see cref="ShoppingCartInfo"/>.
    /// </summary>
    /// <remarks>
    /// This is the default implementation of <see cref="IShoppingCartFactory"/>.
    /// </remarks>
    internal class DefaultShoppingCartFactory : IShoppingCartFactory
    {
        /// <summary>
        /// Creates a new instance of <see cref="ShoppingCartInfo"/> assigned to the user on given site.
        /// </summary>
        /// <remarks>
        /// Created <see cref="ShoppingCartInfo"/> has <see cref="ShoppingCartInfo.ShoppingCartSiteID"/> initialized to value specified by <paramref name="site"/>.
        /// <see cref="ShoppingCartInfo.ShoppingCartCurrencyID"/> is initialized with user's preferred currency. Main currency is used when user's preference is unknown.
        /// </remarks>
        /// <param name="site">ID or site codename where the cart is created.</param>
        /// <param name="user">User for who the cart is created.</param>
        /// <returns>New instance of <see cref="ShoppingCartInfo"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="site"/> is <c>null</c>.</exception>
        public ShoppingCartInfo CreateCart(SiteInfoIdentifier site, UserInfo user)
        {
            if (site == null)
            {
                throw new ArgumentNullException(nameof(site));
            }

            var cart = new ShoppingCartInfo
            {
                ShoppingCartSiteID = site.ObjectID
            };

            if (user != null)
            {
                InitCartWithUser(cart, user);
            }

            PreselectCurrency(cart);
            PreselectPaymentOption(cart);
            PreselectShippingOption(cart);

            return cart;
        }


        /// <summary>
        /// Assigns the shopping cart to the user. Override this method to add extra user-related initialization.
        /// </summary>
        /// <param name="cart">The shopping cart to be assigned to the <paramref name="user"/></param>
        /// <param name="user">The user who is assigned with the <paramref name="cart"/></param>
        private static void InitCartWithUser(ShoppingCartInfo cart, UserInfo user)
        {
            cart.User = user;
        }


        /// <summary>
        /// Selects the preferred currency for the given cart. 
        /// </summary>
        /// <remarks>
        /// No currency is set if the site does not have main currency defined.
        /// </remarks>
        /// <param name="cart">The shopping cart object.</param>
        private static void PreselectCurrency(ShoppingCartInfo cart)
        {
            var siteId = cart.ShoppingCartSiteID;
            var mainCurrency = CurrencyInfoProvider.GetMainCurrency(siteId);
            if (mainCurrency == null)
            {
                return;
            }

            // Get user currency
            if (cart.Customer != null)
            {
                // Get customer currency
                var customerCurrencyID = cart.Customer.GetPreferences(siteId).CurrencyID ?? 0;
                var customerCurrency = CurrencyInfoProvider.GetCurrencyInfo(customerCurrencyID);

                // Set preferred currency if valid 
                if ((customerCurrency != null) &&
                    (customerCurrency.CurrencyID != mainCurrency.CurrencyID) &&
                    (customerCurrency.CurrencySiteID == mainCurrency.CurrencySiteID) &&
                    customerCurrency.CurrencyEnabled &&
                    CurrencyInfoProvider.IsCurrencyWithExchangeRate(customerCurrencyID, siteId))
                {
                    // Set customer currency 
                    cart.ShoppingCartCurrencyID = customerCurrency.CurrencyID;
                    return;
                }
            }

            // Use main currency
            cart.ShoppingCartCurrencyID = mainCurrency.CurrencyID;
        }


        /// <summary>
        /// Assigns payment option in given <paramref name="cart"/> based on <see cref="CustomerInfo"/> preferences.
        /// </summary>
        private static void PreselectPaymentOption(ShoppingCartInfo cart)
        {
            var paymentOptionId = cart.Customer?.GetPreferences(cart.ShoppingCartSiteID).PaymentOptionID;
            if (paymentOptionId != null)
            {
                cart.ShoppingCartPaymentOptionID = paymentOptionId.Value;
            }
        }


        /// <summary>
        /// Assigns shipping option in given <paramref name="cart"/> based on <see cref="CustomerInfo"/> preferences.
        /// </summary>
        private static void PreselectShippingOption(ShoppingCartInfo cart)
        {
            var customer = cart.Customer;
            if (cart.ShoppingCartShippingOptionID <= 0 && customer != null)
            {
                var shippingOptionId = customer.GetPreferences(cart.ShoppingCartSiteID).ShippingOptionID;
                if (shippingOptionId.HasValue)
                {
                    cart.ShoppingCartShippingOptionID = shippingOptionId.Value;
                }
            }
        }
    }
}