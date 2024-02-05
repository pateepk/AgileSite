using System.Collections.Generic;
using System.Linq;

using CMS.Core;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Provides calculation of shipping price.
    /// </summary>
    public class ShippingCalculator : IShoppingCartCalculator
    {
        /// <summary>
        /// Runs shipping calculation based on given calculation related data.
        /// </summary>
        /// <param name="calculationData">All calculation related data.</param>
        /// <remarks>
        /// Calculator uses the default implementation of the <see cref="IShippingPriceService"/> to determine the shipping price.
        /// Given <see cref="CalculatorData.Result"/> is modified during calculation process.
        /// Calculates <see cref="CalculationResult.Shipping"/> price.
        /// </remarks>
        public void Calculate(CalculatorData calculationData)
        {
            var result = calculationData.Result;

            if (!ShippingNeeded(calculationData.Request))
            {
                result.Shipping = 0.0m;
                return;
            }
            
            var shippingService = Service.Resolve<IShippingPriceService>();
            var shippingPrices = shippingService.GetShippingPrice(calculationData, calculationData.Result.Subtotal);

            MarkCouponsUsage(result, shippingPrices.AppliedDiscounts);

            result.Shipping = shippingPrices.Price;
        }


        private void MarkCouponsUsage(CalculationResult result, IEnumerable<IDiscount> discounts)
        {
            foreach (var discount in discounts)
            {
                if (!string.IsNullOrEmpty(discount.AppliedCouponCode))
                {
                    result.AppliedCouponCodes.Add(new CouponCode(discount.AppliedCouponCode, CouponCodeApplicationStatusEnum.AppliedInCart, discount));
                }
            }
        }


        private bool ShippingNeeded(CalculationRequest request)
        {
            return request.Items.Any(i => i.SKU.SKUNeedsShipping || (i.Options != null && i.Options.Any(o => o.SKU.SKUNeedsShipping)));
        }
    }
}
