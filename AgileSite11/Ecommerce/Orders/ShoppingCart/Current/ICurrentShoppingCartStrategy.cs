using System;

using CMS;
using CMS.DataEngine;
using CMS.Ecommerce;
using CMS.Membership;

[assembly: RegisterImplementation(typeof(ICurrentShoppingCartStrategy), typeof(CurrentShoppingCartStrategy), Priority = CMS.Core.RegistrationPriority.Fallback)]

namespace CMS.Ecommerce
{
    /// <summary>
    /// Represent a strategy used during obtaining visitor's current shopping cart.
    /// </summary>
    /// <seealso cref="ICurrentShoppingCartService"/>
    /// <seealso cref="CurrentShoppingCartStrategy"/>
    public interface ICurrentShoppingCartStrategy
    {
        /// <summary>
        /// Checks if given shopping cart can be used on given site.
        /// </summary>
        /// <param name="cart">Shopping cart to be checked.</param>
        /// <param name="site">ID or codename of the site to check.</param>
        /// <returns><c>True</c> if given <paramref name="cart"/> can be used on the site specified by <paramref name="site"/></returns>
        /// <exception cref="ArgumentNullException">Thrown when any of arguments is <c>null</c>.</exception>
        bool CartCanBeUsedOnSite(ShoppingCartInfo cart, SiteInfoIdentifier site);


        /// <summary>
        /// Checks if <paramref name="user"/> can adopt shopping cart specified by <paramref name="cart"/>.
        /// </summary>
        /// <param name="cart">Candidate shopping cart to be checked.</param>
        /// <param name="user">New potential owner of the <paramref name="cart"/></param>
        /// <returns><c>True</c> if given <paramref name="cart"/> can be taken over by user.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="cart"/> is <c>null</c>.</exception>
        bool UserCanTakeOverCart(ShoppingCartInfo cart, UserInfo user);


        /// <summary>
        /// Makes the <see cref="ShoppingCartInfo"/> owned by the user.
        /// </summary>
        /// <param name="cart">The cart to be assigned to the <paramref name="user"/>.</param>
        /// <param name="user">User which is assigned to the <paramref name="cart"/>.</param>
        /// <exception cref="ArgumentNullException">Thrown when any of arguments is <c>null</c>.</exception>
        void TakeOverCart(ShoppingCartInfo cart, UserInfo user);


        /// <summary>
        /// Decides whether shopping cart specified by <paramref name="cart"/> is 'good enough' for user 
        /// or an older shopping cart stored in the system should be preferred.
        /// </summary>
        /// <param name="cart">Candidate shopping cart to be checked. <c>Null</c> if no candidate cart found.</param>
        /// <param name="user">The user whose preference is explored.</param>
        /// <returns><c>True</c> if any user's shopping cart stored in the system is more preferred than <paramref name="cart"/>. 
        /// Returns <c>false</c> if <paramref name="cart"/> is good enough for user to be used for shopping.</returns>
        bool PreferStoredCart(ShoppingCartInfo cart, UserInfo user);


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
        /// <description>The shopping cart was found by identifier taken from the client (<see cref="IShoppingCartClientStorage"/>). 
        /// This happens when the cache (<see cref="IShoppingCartCache"/>) expires and it is not guaranteed that cart belongs to different anonymous customer.</description>
        /// </item> 
        /// </list>
        /// </para>
        /// <para>
        /// Implementation must set <see cref="ShoppingCartInfo.PrivateDataCleared"/> to <c>true</c>.
        /// </para>
        /// </remarks>
        /// <param name="cart">Shopping cart which private data are cleared.</param>
        /// <seealso cref="IShoppingCartClientStorage"/>
        /// <seealso cref="IShoppingCartCache"/>
        void AnonymizeShoppingCart(ShoppingCartInfo cart);


        /// <summary>
        /// Ensures that <see cref="ShoppingCartInfo"/> does not contain invalid data.
        /// </summary>
        /// <remarks>
        /// This method is used to remove possibly outdated values from shopping cart. 
        /// This can happen when the cart is fetched from the database or cache after longer period of time.
        /// </remarks>
        /// <param name="cart">Shopping cart to refresh.</param>
        void RefreshCart(ShoppingCartInfo cart);
    }
}