using System;
using System.Collections.Generic;

namespace CMS.Ecommerce
{
    internal class DiscountApplicator : IDiscountApplicator
    {
        /// <summary>
        /// Returns the application summary of discount in the <paramref name="discountGroups"/>.
        /// </summary>
        /// <remarks>
        /// Implementation handles the strategy of applying the multiple discounts.
        /// </remarks>
        /// <param name="price">Base price to apply discounts on.</param>
        /// <param name="discountGroups">Groups of discounts</param>
        public ValuesSummary ApplyDiscounts(decimal price, IEnumerable<DiscountCollection> discountGroups)
        {
            if (discountGroups == null)
            {
                throw new ArgumentNullException(nameof(discountGroups));
            }

            var totalDiscount = 0m;
            var summary = new ValuesSummary();

            foreach (var group in discountGroups)
            {
                // Prepare a new discount base for the current discount group
                var discountBase = price - totalDiscount;

                foreach (var discount in group)
                {
                    // Get resulting discount value
                    var discountValue = discount.CalculateDiscount(discountBase);

                    summary.Sum(discount.DiscountName, discountValue);

                    totalDiscount += discountValue;
                }
            }

            return summary;
        }
    }
}
