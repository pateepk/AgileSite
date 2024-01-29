using System;

using CMS.DataEngine;
using CMS.Membership;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Default implementation of <see cref="IShoppingCartRepository"/> encapsulating <see cref="ShoppingCartInfoProvider"/>.
    /// </summary>
    /// <seealso cref="ShoppingCartInfoProvider"/>
    /// <seealso cref="ShoppingCartItemInfoProvider"/>
    internal sealed class DefaultShoppingCartRepository : IShoppingCartRepository
    {
        /// <summary>
        /// Finds the most recent shopping cart stored for the user on the specified site.
        /// </summary>
        /// <remarks>
        /// Returned shopping cart is populated with cart items.
        /// </remarks>
        /// <param name="user">The user to get the shopping cart for.</param>
        /// <param name="site">ID or code name of the site to look for cart.</param>
        /// <returns>User's most recent shopping cart or <c>null</c> when not found.</returns>
        public ShoppingCartInfo GetUsersCart(UserInfo user, SiteInfoIdentifier site)
        {
            if (user.IsPublic())
            {
                return null;
            }

            return ShoppingCartInfoProvider.GetShoppingCartInfo(user.UserID, site.ObjectCodeName);
        }


        /// <summary>
        /// Deletes user's shopping cart stored in the system for given site.
        /// </summary>
        /// <param name="user">The user to delete the shopping cart for.</param>
        /// <param name="site">ID or code name of the site to look for cart.</param>
        public void DeleteUsersCart(UserInfo user, SiteInfoIdentifier site)
        {
            if (!user.IsPublic())
            {
                ShoppingCartInfoProvider.DeleteShoppingCartInfo(user.UserID, site.ObjectCodeName);
            }
        }


        /// <summary>
        /// Gets the <see cref="ShoppingCartInfo"/> by its ID.
        /// </summary>
        /// <remarks>
        /// Returned shopping cart is populated with cart items.
        /// </remarks>
        /// <param name="cartId">ID of the <see cref="ShoppingCartInfo"/>.</param>
        /// <returns>Shopping cart with requested ID or null when not found.</returns>
        public ShoppingCartInfo GetCart(int cartId)
        {
            return ShoppingCartInfoProvider.GetShoppingCartInfo(cartId);
        }


        /// <summary>
        /// Gets <see cref="ShoppingCartInfo"/> by its GUID.
        /// </summary>
        /// <remarks>
        /// Returned shopping cart is populated with cart items.
        /// </remarks>
        /// <param name="cartGuid">ID of the <see cref="ShoppingCartInfo"/>.</param>
        /// <returns>Shopping cart with requested GUID or null when not found.</returns>
        public ShoppingCartInfo GetCart(Guid cartGuid)
        {
            return ShoppingCartInfoProvider.GetShoppingCartInfo(cartGuid);
        }


        /// <summary>
        /// Sets (inserts or updates) the <see cref="ShoppingCartInfo"/>.
        /// </summary>
        /// <param name="cart">The shopping cart to be set.</param>
        public void SetCart(ShoppingCartInfo cart)
        {
            ShoppingCartInfoProvider.SetShoppingCartInfo(cart);
        }


        /// <summary>
        /// Removes given <see cref="ShoppingCartInfo"/> from the system.
        /// </summary>
        /// <param name="cart">The shopping cart to be removed.</param>
        public void DeleteCart(ShoppingCartInfo cart)
        {
            ShoppingCartInfoProvider.DeleteShoppingCartInfo(cart);
        }
    }
}