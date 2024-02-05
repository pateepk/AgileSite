using System.Collections.Generic;

using CMS;
using CMS.Ecommerce;

[assembly: RegisterImplementation(typeof(IProductCouponService), typeof(ProductCouponService), Priority = CMS.Core.RegistrationPriority.Fallback)]

namespace CMS.Ecommerce
{
    /// <summary>
    /// Defines the contract for classes providing product coupon discount evaluation.
    /// </summary>
    public interface IProductCouponService
    {
        /// <summary>
        /// Evaluates product coupon discounts specified by the <paramref name="parameters"/> on the given <paramref name="items"/> collection.
        /// </summary>
        /// <param name="items">Items to be evaluated.</param>
        /// <param name="parameters">Parameter of the discounts.</param>
        /// <param name="applicator">Applicator to be used to apply results of the evaluation.</param>
        void EvaluateDiscounts(IEnumerable<MultiBuyItem> items, DiscountsParameters parameters, IMultiBuyDiscountsApplicator applicator);
    }
}
