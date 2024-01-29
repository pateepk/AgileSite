using System;

using CMS;
using CMS.DataEngine;
using CMS.Ecommerce;
using CMS.Membership;

[assembly: RegisterImplementation(typeof(IShoppingCartRepository), typeof(DefaultShoppingCartRepository), Priority = CMS.Core.RegistrationPriority.Fallback)]

namespace CMS.Ecommerce
{
    /// <summary>
    /// Interface for basic (CRUD) operations with shopping cart info.
    /// </summary>
    public interface IShoppingCartRepository
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
        ShoppingCartInfo GetUsersCart(UserInfo user, SiteInfoIdentifier site);


        /// <summary>
        /// Deletes user's shopping cart stored in the system for given site.
        /// </summary>
        /// <param name="user">The user to delete the shopping cart for.</param>
        /// <param name="site">ID or code name of the site to look for cart.</param>
        void DeleteUsersCart(UserInfo user, SiteInfoIdentifier site);


        /// <summary>
        /// Gets the <see cref="ShoppingCartInfo"/> by its ID.
        /// </summary>
        /// <remarks>
        /// Returned shopping cart is populated with cart items.
        /// </remarks>
        /// <param name="cartId">ID of the <see cref="ShoppingCartInfo"/>.</param>
        /// <returns>Shopping cart with requested ID or null when not found.</returns>
        ShoppingCartInfo GetCart(int cartId);


        /// <summary>
        /// Gets <see cref="ShoppingCartInfo"/> by its GUID.
        /// </summary>
        /// <remarks>
        /// Returned shopping cart is populated with cart items.
        /// </remarks>
        /// <param name="cartGuid">ID of the <see cref="ShoppingCartInfo"/>.</param>
        /// <returns>Shopping cart with requested GUID or null when not found.</returns>
        ShoppingCartInfo GetCart(Guid cartGuid);


        /// <summary>
        /// Sets (inserts or updates) the <see cref="ShoppingCartInfo"/>.
        /// </summary>
        /// <param name="cart">The shopping cart to be set.</param>
        void SetCart(ShoppingCartInfo cart);


        /// <summary>
        /// Removes given <see cref="ShoppingCartInfo"/> from the system.
        /// </summary>
        /// <param name="cart">The shopping cart to be removed.</param>
        void DeleteCart(ShoppingCartInfo cart);
    }
}