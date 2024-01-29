using CMS;
using CMS.Ecommerce;

[assembly: RegisterImplementation(typeof(IShoppingCartCache), typeof(ShoppingCartCache), Priority = CMS.Core.RegistrationPriority.Fallback)]

namespace CMS.Ecommerce
{
    /// <summary>
    /// Interface representing general cache able to store shopping cart info object for current visitor.
    /// </summary>
    /// <remarks>
    /// The cache is expected to expire after some time.
    /// </remarks>
    public interface IShoppingCartCache
    {
        /// <summary>
        /// Returns cached shopping cart of the current visitor.
        /// </summary>
        ShoppingCartInfo GetCart();


        /// <summary>
        /// Stores current visitor's <see cref="ShoppingCartInfo"/> to the cache.
        /// </summary>
        /// <param name="cart">Shopping cart to be stored.</param>
        void StoreCart(ShoppingCartInfo cart);
    }
}