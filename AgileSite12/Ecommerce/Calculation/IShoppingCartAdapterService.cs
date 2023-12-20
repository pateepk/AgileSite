using CMS;
using CMS.Ecommerce;

[assembly: RegisterImplementation(typeof(IShoppingCartAdapterService), typeof(ShoppingCartAdapterService), Priority = CMS.Core.RegistrationPriority.Fallback)]

namespace CMS.Ecommerce
{
    /// <summary>
    /// Service providing an interface between <see cref="ShoppingCartInfo"/> and the calculation pipeline.
    /// </summary>
    public interface IShoppingCartAdapterService
    {
        /// <summary>
        /// Collects information necessary for the calculation from given <see cref="ShoppingCartInfo"/> and stores it in <see cref="CalculationRequest"/>.
        /// </summary>
        /// <param name="cartInfo">Shopping cart to be calculated</param>
        /// <returns><see cref="CalculationRequest"/> filled with relevant information from given <paramref name="cartInfo"/>.</returns>
        CalculationRequest GetCalculationRequest(ShoppingCartInfo cartInfo);


        /// <summary>
        /// Creates the <see cref="CalculationResult"/> object used to store calculation results (subtotals, price summaries etc.).
        /// </summary>
        /// <param name="cartInfo">Shopping cart to be calculated.</param>
        /// <returns>Instance of the <see cref="CalculationResult"/>.</returns>
        CalculationResult GetCalculationResult(ShoppingCartInfo cartInfo);


        /// <summary>
        /// Applies result values to given <see cref="ShoppingCartInfo"/>.
        /// </summary>
        /// <param name="cartInfo">Shopping cart where the result values should be applied</param>
        /// <param name="result">Container with calculation result values</param>
        void ApplyCalculationResult(ShoppingCartInfo cartInfo, CalculationResult result);
    }
}
