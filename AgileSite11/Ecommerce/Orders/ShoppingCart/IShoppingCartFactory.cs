using System;

using CMS;
using CMS.DataEngine;
using CMS.Ecommerce;
using CMS.Membership;

[assembly: RegisterImplementation(typeof(IShoppingCartFactory), typeof(DefaultShoppingCartFactory), Priority = CMS.Core.RegistrationPriority.Fallback)]

namespace CMS.Ecommerce
{
    /// <summary>
    /// Represents a factory able to create and initialize new instances of <see cref="ShoppingCartInfo"/>.
    /// </summary>
    public interface IShoppingCartFactory
    {
        /// <summary>
        /// Creates a new instance of <see cref="ShoppingCartInfo"/> assigned to specified <paramref name="user"/> on given <paramref name="site"/>.
        /// </summary>
        /// <remarks>
        /// Created <see cref="ShoppingCartInfo"/> has <see cref="ShoppingCartInfo.ShoppingCartSiteID"/> initialized to value specified by <paramref name="site"/>. 
        /// Also <see cref="ShoppingCartInfo.ShoppingCartCurrencyID"/> property is initialized to currency which can be used on <paramref name="site"/>.
        /// </remarks>
        /// <param name="site">ID or site code name where the cart is created.</param>
        /// <param name="user">User for who the cart is created.</param>
        /// <returns>New instance of <see cref="ShoppingCartInfo"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="site"/> is null.</exception>
        ShoppingCartInfo CreateCart(SiteInfoIdentifier site, UserInfo user);
    }
}