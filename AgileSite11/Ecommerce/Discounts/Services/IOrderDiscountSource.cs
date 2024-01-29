using System.Collections.Generic;

using CMS;
using CMS.Ecommerce;

[assembly: RegisterImplementation(typeof(IOrderDiscountSource), typeof(OrderDiscountSource), Priority = CMS.Core.RegistrationPriority.Fallback)]

namespace CMS.Ecommerce
{
    /// <summary>
    /// Represents a source of order discounts.
    /// </summary>
    public interface IOrderDiscountSource
    {
        /// <summary>
        /// Returns the order discounts for the specified <paramref name="data"/> grouped by their priority.
        /// Applied order discounts must be running, applicable for the given <see cref="CalculationRequest.User"/> and satisfy the discount conditions.
        /// Only order discounts satisfying the minimum order amount are returned.
        /// </summary>
        /// <param name="data">Calculation data.</param>
        /// <param name="orderPrice">Order price which is used to filter applicable order discounts. (specified in the calculation currency)</param>
        IEnumerable<DiscountCollection> GetDiscounts(CalculatorData data, decimal orderPrice);
    }
}