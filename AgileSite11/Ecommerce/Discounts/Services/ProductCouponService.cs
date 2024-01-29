using System.Collections.Generic;
using System.Linq;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Default implementation of <see cref="IProductCouponService"/>.
    /// </summary>
    internal class ProductCouponService : IProductCouponService
    {
        private readonly IProductCouponSource mCouponSource;


        public ProductCouponService(IProductCouponSource couponSource)
        {
            mCouponSource = couponSource;
        }


        /// <summary>
        /// Evaluates product coupon discounts specified by the <paramref name="parameters"/> on the given <paramref name="items"/> collection.
        /// </summary>
        /// <param name="items">Items to be evaluated.</param>
        /// <param name="parameters">Parameter of the discounts.</param>
        /// <param name="applicator">Applicator to be used to apply results of the evaluation.</param>
        public void EvaluateDiscounts(IEnumerable<MultiBuyItem> items, DiscountsParameters parameters, IMultiBuyDiscountsApplicator applicator)
        {
            // Return if no coupons were applied
            if (!parameters.CouponCodes.Codes.Any())
            {
                return;
            }

            // Get product coupons
            var discounts = mCouponSource.GetDiscounts(parameters);

            var itemsElems = items.ToList();
            foreach (var discount in discounts)
            {
                var applicableProducts = itemsElems.Where(i => discount.IsBasedOn(i)).ToList();

                applicableProducts.ForEach(i => applicator.ApplyDiscount(discount, i));

                if (!discount.ApplyFurtherDiscounts)
                {
                    return;
                }
            }
        }
    }
}
