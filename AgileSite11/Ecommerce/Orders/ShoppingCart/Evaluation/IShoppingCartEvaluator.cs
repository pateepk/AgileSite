using CMS;
using CMS.Ecommerce;

[assembly: RegisterImplementation(typeof(IShoppingCartEvaluator), typeof(DefaultShoppingCartEvaluator), Priority = CMS.Core.RegistrationPriority.Fallback)]

namespace CMS.Ecommerce
{
    /// <summary>
    /// Interface for shopping cart evaluation.
    /// </summary>
    public interface IShoppingCartEvaluator
    {
        /// <summary>
        /// Evaluates discounts and calculates the price, shipping, taxes information on the given <paramref name="cart"/>.
        /// </summary>
        void Evaluate(ShoppingCartInfo cart);
    }
}