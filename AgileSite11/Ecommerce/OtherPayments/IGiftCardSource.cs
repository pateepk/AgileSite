using System.Collections.Generic;

using CMS;
using CMS.Ecommerce;

[assembly: RegisterImplementation(typeof(IGiftCardSource), typeof(GiftCardSource), Priority = CMS.Core.RegistrationPriority.Fallback)]

namespace CMS.Ecommerce
{
    /// <summary>
    /// Represents a source of gift cards.
    /// </summary>
    public interface IGiftCardSource
    {
        /// <summary>
        /// Returns the gift card collection for the specified <paramref name="data"/>.
        /// Applied gift cards must be running, applicable for the given <see cref="CalculationRequest.User"/> and satisfy the gift card conditions.
        /// Only gift cards satisfying the minimum order amount are returned.
        /// </summary>
        /// <param name="data">Calculation data</param>
        /// <param name="orderAmount">Order amount which is used to filter applicable gift cards. (specified in the calculation currency)</param>
        IEnumerable<GiftCardApplication> GetGiftCards(CalculatorData data, decimal orderAmount);
    }
}