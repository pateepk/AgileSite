using CMS;
using CMS.Ecommerce;

[assembly: RegisterImplementation(typeof(ICartItemChecker), typeof(DefaultItemChecker), Priority = CMS.Core.RegistrationPriority.Fallback)]

namespace CMS.Ecommerce
{
    /// <summary>
    /// Service for consistency checking of added products.
    /// </summary>
    public interface ICartItemChecker
    {
        /// <summary>
        /// Checks if the <paramref name="newItemParams"/> are valid and product configuration can be added to the given <paramref name="cart"/>.
        /// </summary>
        /// <param name="newItemParams">New item parameters (product with options)</param>
        /// <param name="cart"><see cref="ShoppingCartInfo"/> where the new item will be placed.</param>
        bool CheckNewItem(ShoppingCartItemParameters newItemParams, ShoppingCartInfo cart);
    }
}
