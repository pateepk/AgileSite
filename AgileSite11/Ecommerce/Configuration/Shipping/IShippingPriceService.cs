using CMS;
using CMS.Ecommerce;

[assembly: RegisterImplementation(typeof(IShippingPriceService), typeof(ShippingPriceService), Priority = CMS.Core.RegistrationPriority.Fallback)]

namespace CMS.Ecommerce
{
    /// <summary>
    /// Service providing shipping price calculation.
    /// </summary>
    public interface IShippingPriceService
    {
        /// <summary>
        /// Calculates the price of the <see cref="CalculationRequest.ShippingOption"/> in the <see cref="CalculationRequest.Currency"/> including all discounts.
        /// </summary>
        /// <param name="data">Calculation data.</param>
        /// <param name="orderAmount">Actual order amount for evaluation of the shipping discounts.</param>
        ShippingPrices GetShippingPrice(CalculatorData data, decimal orderAmount);
    }
}