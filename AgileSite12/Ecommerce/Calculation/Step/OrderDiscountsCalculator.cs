using System;
using System.Linq;

using CMS.Core;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Calculation step providing calculation of order discounts. 
    /// </summary>
    public class OrderDiscountsCalculator : IShoppingCartCalculator
    {
        /// <summary>
        /// Calculates the values of order discounts and stores them to <see cref="CalculationResult.OrderDiscount"/> property.
        /// </summary>
        /// <param name="calculationData">All calculation related data.</param>
        public void Calculate(CalculatorData calculationData)
        {
            var result = calculationData.Result;

            var discountGroups = Service.Resolve<IOrderDiscountSource>()
                .GetDiscounts(calculationData, result.Subtotal)
                .ToList();

            var applications = Service.Resolve<IDiscountApplicator>()
                .ApplyDiscounts(result.Subtotal, discountGroups);

            result.OrderDiscount = Math.Min(applications.Sum(application => application.Value), result.Subtotal);
            result.OrderDiscountSummary.Merge(applications);

            foreach (var discount in discountGroups
                .SelectMany(discount => discount)
                .Where(discount => !String.IsNullOrEmpty(discount.AppliedCouponCode)))
            {
                result.AppliedCouponCodes.Add(new CouponCode(discount.AppliedCouponCode, CouponCodeApplicationStatusEnum.AppliedInCart, discount));
            }
        }
    }
}