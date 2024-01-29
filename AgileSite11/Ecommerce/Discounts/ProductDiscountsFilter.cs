using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Allows to filter list of conditional product discounts according to its conditions and stop processing flag.
    /// </summary>
    public class ProductDiscountsFilter : DiscountsFilterBase
    {
        /// <summary>
        /// Filters given discounts and leaves only those which conditions are met.
        /// </summary>
        /// <param name="product">Object to be used for condition evaluation.</param>
        /// <param name="discounts">Conditional discounts to be filtered.</param>
        public virtual IEnumerable<IConditionalDiscount> Filter(SKUInfo product, IEnumerable<IConditionalDiscount> discounts)
        {
            if (product == null)
            {
                throw new ArgumentNullException(nameof(product));
            }

            Resolver.SetNamedSourceData("SKU", product);

            return FilterDiscounts(discounts);
        }


        /// <summary>
        /// Returns DiscountProductCondition from given discount object.
        /// </summary>
        /// <param name="discount">Discount to get product condition from.</param>
        protected override string GetCondition(IConditionalDiscount discount)
        {
            return discount.DiscountProductCondition;
        }
    }
}
