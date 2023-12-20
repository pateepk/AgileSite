using CMS;
using CMS.DataEngine;
using CMS.Ecommerce;
using CMS.Membership;

[assembly: RegisterImplementation(typeof(ICurrentShoppingCartService), typeof(CurrentShoppingCartService), Priority = CMS.Core.RegistrationPriority.Fallback)]

namespace CMS.Ecommerce
{
    /// <summary>
    /// Interface for service providing current shopping cart.
    /// </summary>
    public interface ICurrentShoppingCartService
    {
        /// <summary>
        /// Finds the most suitable shopping cart for given user. Creates and initializes new one when no cart found.
        /// </summary>
        /// <param name="user">User to get current shopping cart for.</param>
        /// <param name="site">ID or the codename of the site.</param>
        /// <returns>Visitor's current shopping cart.</returns>
        ShoppingCartInfo GetCurrentShoppingCart(UserInfo user, SiteInfoIdentifier site);


        /// <summary>
        /// Sets current shopping cart for current visitor.
        /// </summary>
        /// <remarks>
        /// The default implementation of this service calls the <see cref="ECommerceContext.InvalidateCurrentShoppingCartCache"/> method.
        /// </remarks>
        void SetCurrentShoppingCart(ShoppingCartInfo cart);
    }
}