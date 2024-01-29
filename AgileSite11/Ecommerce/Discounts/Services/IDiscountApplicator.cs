using System.Collections.Generic;

using CMS;
using CMS.Ecommerce;

[assembly: RegisterImplementation(typeof(IDiscountApplicator), typeof(DiscountApplicator), Priority = CMS.Core.RegistrationPriority.Fallback)]

namespace CMS.Ecommerce
{
    /// <summary>
    /// Service that applies multiple discounts on the price.
    /// </summary>
    public interface IDiscountApplicator
    {
        /// <summary>
        /// Returns the application summary of the <paramref name="discountGroups"/> when applied on the specified <paramref name="price"/>.
        /// </summary>
        /// <remarks>
        /// Implementation handles the strategy of applying the multiple discounts.
        /// </remarks>
        /// <param name="price">Base price to apply discounts on.</param>
        /// <param name="discountGroups">Groups of discounts.</param>
        ValuesSummary ApplyDiscounts(decimal price, IEnumerable<DiscountCollection> discountGroups);
    }
}