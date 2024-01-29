using System.Collections.Generic;

using CMS;
using CMS.Ecommerce;

[assembly: RegisterImplementation(typeof(IShippingDiscountSource), typeof(ShippingDiscountSource), Priority = CMS.Core.RegistrationPriority.Fallback)]

namespace CMS.Ecommerce
{
    /// <summary>
    /// Represents a source of shipping discounts.
    /// </summary>
    public interface IShippingDiscountSource
    {
        /// <summary>
        /// Returns the shipping discounts for the specified <paramref name="data"/>.
        /// Applied shipping discounts must be running, applicable for the given <see cref="CalculationRequest.User"/> and satisfy the discount conditions.
        /// Only shipping discounts satisfying the minimum order amount are returned.
        /// </summary>
        /// <param name="data">Calculation data.</param>
        /// <param name="orderAmount">Order amount which is used to filter applicable shipping discounts. (specified in the calculation currency)</param>
        IEnumerable<IDiscount> GetDiscounts(CalculatorData data, decimal orderAmount);


        /// <summary>
        /// Returns remaining amount for free shipping. 
        /// Method checks applicable free shipping offers which <see cref="DiscountInfo.DiscountOrderAmount"/> 
        /// is larger than <paramref name="orderAmount"/> and returns additional amount to reach free shipping offer.
        /// Method returns 0 if there is no valid discount or if free shipping is already applied.
        /// </summary>
        /// <param name="data">Calculation data.</param>
        /// <param name="orderAmount">Current order amount. (specified in the calculation currency)</param>
        decimal GetRemainingAmountForFreeShipping(CalculatorData data, decimal orderAmount);
    }
}