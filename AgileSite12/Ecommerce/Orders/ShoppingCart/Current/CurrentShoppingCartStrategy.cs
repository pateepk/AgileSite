using System;

using CMS.ContactManagement;
using CMS.DataEngine;
using CMS.Membership;
using CMS.WebAnalytics;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Default implementation of <see cref="ICurrentShoppingCartStrategy"/>.
    /// </summary>
    /// <remarks>
    /// You can derive your own implementation from this class to customize the way the system finds shopping cart for visitor. 
    /// The following code illustrates how to register custom implementation.
    /// <code>
    /// [assembly:RegisterImplementation(typeof(ICurrentShoppingCartStrategy), typeof(MyCurrentShoppingCartStrategy))]
    /// public class MyCurrentShoppingCartStrategy : CurrentShoppingCartStrategy
    /// {
    ///     ...
    /// }
    /// </code>
    /// </remarks>
    /// <seealso cref="ShoppingCartInfo"/>
    /// <seealso cref="SiteInfoIdentifier"/>
    /// <seealso cref="UserInfo"/>
    public class CurrentShoppingCartStrategy : ICurrentShoppingCartStrategy
    {
        /// <summary>
        /// Checks if given shopping cart is assigned to the site.
        /// </summary>
        /// <param name="cart">Shopping cart to be checked.</param>
        /// <param name="site">ID or code name of the site to check.</param>
        /// <returns><c>True</c> if given <paramref name="cart"/> can be used on the site specified by <paramref name="site"/></returns>
        /// <exception cref="ArgumentNullException">Thrown when any of arguments is <c>null</c>.</exception>
        /// <seealso cref="IShoppingCartFactory"/>
        public virtual bool CartCanBeUsedOnSite(ShoppingCartInfo cart, SiteInfoIdentifier site)
        {
            if (cart == null)
            {
                throw new ArgumentNullException(nameof(cart));
            }

            if (site == null)
            {
                throw new ArgumentNullException(nameof(site));
            }

            return cart.ShoppingCartSiteID == site.ObjectID;
        }


        /// <summary>
        /// Checks if user can adopt shopping cart.
        /// </summary>
        /// <param name="cart">Candidate shopping cart to be checked.</param>
        /// <param name="user">Potential owner of the <paramref name="cart"/>.</param>
        /// <returns><c>True</c> if given <paramref name="cart"/> has no owner or the owner is anonymous.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="cart"/> is <c>null</c>.</exception>
        public virtual bool UserCanTakeOverCart(ShoppingCartInfo cart, UserInfo user)
        {
            if (cart == null)
            {
                throw new ArgumentNullException(nameof(cart));
            }

            var cartOwner = cart.User;

            return (cartOwner == null) || cartOwner.IsPublic();
        }


        /// <summary>
        /// Makes the <see cref="ShoppingCartInfo"/> owned by the user.
        /// </summary>
        /// <remarks>
        /// Updates the <see cref="ShoppingCartInfo.User"/>, <see cref="ShoppingCartInfo.Customer"/> and <see cref="ShoppingCartInfo.ShoppingCartContactID"/> properties.
        /// If the cart is empty, it's currency is set to user's preferred currency.
        /// </remarks>
        /// <param name="cart">The cart to be assigned to the <paramref name="user"/>.</param>
        /// <param name="user">User which is assigned to the <paramref name="cart"/>.</param>
        /// <exception cref="ArgumentNullException">Thrown when any of arguments is <c>null</c>.</exception>
        public virtual void TakeOverCart(ShoppingCartInfo cart, UserInfo user)
        {
            if (cart == null)
            {
                throw new ArgumentNullException(nameof(cart));
            }

            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            if (!user.IsPublic())
            {
                // Assign cart to user and contact
                cart.User = user;
                cart.ShoppingCartContactID = ContactMembershipInfoProvider.GetContactIDByMembership(user.UserID, MembershipType.CMS_USER);

                // Set customer if exists
                var customer = CustomerInfoProvider.GetCustomerInfoByUserID(user.UserID);
                if (customer != null)
                {
                    cart.Customer = customer;
                }
            }

            if (!cart.IsEmpty)
            {
                return;
            }

            var currencyID = cart.Customer?.GetPreferences(cart.ShoppingCartSiteID).CurrencyID;
            if (currencyID != null)
            {
                cart.ShoppingCartCurrencyID = currencyID.Value;
            }
        }


        /// <summary>
        /// Decides whether shopping cart is 'good enough' for user 
        /// or an older shopping cart stored in the system should be preferred.
        /// </summary>
        /// <param name="cart">Candidate shopping cart to be checked. <c>Null</c> if no candidate cart found.</param>
        /// <param name="user">The user whose preference is explored.</param>
        /// <returns><c>True</c> if <paramref name="cart"/> is <c>null</c> or empty.</returns>
        public virtual bool PreferStoredCart(ShoppingCartInfo cart, UserInfo user)
        {
            return (cart == null) || cart.IsEmpty;
        }


        /// <summary>
        /// Removes personal information from the shopping cart.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This method is used when the current shopping was not created specifically for current user. 
        /// </para>
        /// <para>
        /// This situation occurs in the following situations:
        /// <list type="bullet">
        /// <item>
        /// <description>User logs in. This means that cart was created for public user and now it is used for logged user.</description>
        /// </item>
        /// <item>
        /// <description>The shopping cart was found by identifier taken from the client (<see cref="IShoppingCartClientStorage"/>). 
        /// This happens when the cache (<see cref="IShoppingCartCache"/>) expires and it is not guaranteed that cart belongs to different anonymous customer.</description>
        /// </item> 
        /// </list>
        /// </para>
        /// </remarks>
        /// <param name="cart">Shopping cart which private data are cleared.</param>
        /// <seealso cref="ClearPrivateData"/>
        /// <seealso cref="ClearCustomer"/>
        /// <seealso cref="ClearShipping"/>
        /// <seealso cref="ClearPayment"/>
        /// <seealso cref="ClearAddresses"/>
        /// <seealso cref="ClearNote"/>
        public void AnonymizeShoppingCart(ShoppingCartInfo cart)
        {
            if (cart == null)
            {
                return;
            }

            ClearPrivateData(cart);

            cart.PrivateDataCleared = true;
        }


        /// <summary>
        /// Ensures that <see cref="ShoppingCartInfo"/> does not contain invalid data.
        /// </summary>
        /// <remarks>
        /// Invalidates cached discounts and checks selected currency.
        /// </remarks>
        /// <param name="cart">Shopping cart to refresh.</param>
        public virtual void RefreshCart(ShoppingCartInfo cart)
        {
            // Get current currency info
            var currency = cart.Currency;

            // Set main currency if current is disabled
            if ((currency == null) || !currency.CurrencyEnabled)
            {
                var mainCurrency = CurrencyInfoProvider.GetMainCurrency(cart.ShoppingCartSiteID);
                if (mainCurrency != null)
                {
                    cart.ShoppingCartCurrencyID = mainCurrency.CurrencyID;
                }
            }
        }


        /// <summary>
        /// Clears all data considered to be 'private' (i.e. customer, shipping option including addresses, payment option, discounts and note).
        /// </summary>
        /// <param name="cart">The shopping cart to clear private data.</param>
        /// <seealso cref="ClearCustomer"/>
        /// <seealso cref="ClearShipping"/>
        /// <seealso cref="ClearPayment"/>
        /// <seealso cref="ClearAddresses"/>
        /// <seealso cref="ClearNote"/>
        protected virtual void ClearPrivateData(ShoppingCartInfo cart)
        {
            ClearCustomer(cart);
            ClearShipping(cart);
            ClearPayment(cart);
            ClearAddresses(cart);
            ClearNote(cart);
        }


        /// <summary>
        /// Removes information about customer from the shopping cart.
        /// </summary>
        /// <param name="cart">The shopping cart from which customer is removed.</param>
        /// <seealso cref = "ClearAddresses" />
        protected virtual void ClearCustomer(ShoppingCartInfo cart)
        {
            cart.ShoppingCartCustomerID = 0;
        }


        /// <summary>
        /// Removes information about shipping from the shopping cart.
        /// </summary>
        /// <param name="cart">The shopping cart from which the shipping is removed.</param>
        protected virtual void ClearShipping(ShoppingCartInfo cart)
        {
            cart.ShoppingCartShippingOptionID = 0;
        }


        /// <summary>
        /// Removes information about payment from the shopping cart.
        /// </summary>
        /// <param name="cart">The shopping cart from which the payment data are removed.</param>
        protected virtual void ClearPayment(ShoppingCartInfo cart)
        {
            cart.ShoppingCartPaymentOptionID = 0;
            cart.PaymentGatewayCustomData.Clear();
        }


        /// <summary>
        /// Removes information about addresses from the shopping cart.
        /// </summary>
        /// <param name="cart">The shopping cart from which the addresses are removed.</param>
        /// <seealso cref="ClearCustomer"/>
        protected virtual void ClearAddresses(ShoppingCartInfo cart)
        {
            cart.ShoppingCartBillingAddress = null;
            cart.ShoppingCartCompanyAddress = null;
            cart.ShoppingCartShippingAddress = null;
        }


        /// <summary>
        /// Removes information about note from the shopping cart.
        /// </summary>
        /// <param name="cart">The shopping cart from which the note is removed.</param>
        protected virtual void ClearNote(ShoppingCartInfo cart)
        {
            cart.ShoppingCartNote = string.Empty;
        }
    }
}