using System;

using CMS.Core;
using CMS.DataEngine;
using CMS.Membership;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Static wrapper for <see cref="IShoppingCartFactory"/> service providing <see cref="ShoppingCartInfo"/> objects creation.
    /// </summary>
    /// <remarks>
    /// The new instance of <see cref="ShoppingCartInfo"/> is created using current implementation of <see cref="IShoppingCartFactory"/> service.
    /// </remarks>
    public static class ShoppingCartFactory
    {
        /// <summary>
        /// Creates a new instance of <see cref="ShoppingCartInfo"/> assigned to the user on given site.
        /// </summary>
        /// <remarks>
        /// Created <see cref="ShoppingCartInfo"/> has <see cref="ShoppingCartInfo.ShoppingCartSiteID"/> initialized to value specified by <paramref name="site"/>.
        /// </remarks>
        /// <param name="site">ID or site codename where the cart is created.</param>
        /// <param name="user">User for who the cart is created.</param>
        /// <returns>New instance of <see cref="ShoppingCartInfo"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="site"/> is null.</exception>
        public static ShoppingCartInfo CreateCart(SiteInfoIdentifier site, UserInfo user = null)
        {
            return Service.Resolve<IShoppingCartFactory>().CreateCart(site, user);
        }
    }
}
