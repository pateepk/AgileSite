using System.Collections.Generic;
using System.Linq;
using CMS.Helpers;
using CMS.MacroEngine;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Base class for discount filters.
    /// </summary>
    public class DiscountsFilterBase
    {
        private MacroResolver mResolver;


        /// <summary>
        /// Macro resolver to be used for resolving discount conditions.
        /// </summary>
        protected MacroResolver Resolver
        {
            get
            {
                return mResolver ?? (mResolver = MacroResolver.GetInstance());
            }
        }


        /// <summary>
        /// Returns discounts for which method DiscountIsUsable returns true. 
        /// Filtering stops when the first discount without ApplyFurtherDiscounts flag found.
        /// </summary>
        /// <param name="discounts">Discounts to be filtered.</param>
        protected virtual IEnumerable<IConditionalDiscount> FilterDiscounts(IEnumerable<IConditionalDiscount> discounts)
        {
            bool stopProcessing = false;
            double stoppedOrder = -1;

            foreach (var discount in discounts.Where(DiscountIsUsable))
            {
                // Mark stop processing flags if already not set
                if (!discount.ApplyFurtherDiscounts && !stopProcessing)
                {
                    stopProcessing = true;
                    stoppedOrder = discount.DiscountItemOrder;
                }

                // Stop processing further discounts if some of discounts with lower order had ApplyFurtherDiscounts set to false
                if (stopProcessing && (stoppedOrder < discount.DiscountItemOrder))
                {
                    break;
                }

                yield return discount;
            }
        }


        /// <summary>
        /// Returns true if condition in given discount is satisfied using Resolver object.
        /// </summary>
        /// <param name="discount">Discount to be evaluated.</param>
        protected virtual bool DiscountIsUsable(IConditionalDiscount discount)
        {
            var condition = GetCondition(discount);

            // Discount conditions are not set, so this discount is used for every product
            if (string.IsNullOrEmpty(condition))
            {
                return true;
            }

            // EvaluationException condition
            return ValidationHelper.GetBoolean(Resolver.ResolveMacros(condition), false);
        }


        /// <summary>
        /// Returns condition for given discount. 
        /// </summary>
        /// <param name="discount">Discount to get discount for.</param>
        protected virtual string GetCondition(IConditionalDiscount discount)
        {
            return discount.DiscountProductCondition;
        }
    }
}