using System.Collections.Generic;

using CMS;
using CMS.Ecommerce;

[assembly: RegisterImplementation(typeof(IProductCouponSource), typeof(ProductCouponSource), Priority = CMS.Core.RegistrationPriority.Fallback)]

namespace CMS.Ecommerce
{
    /// <summary>
    /// Represents a source of product coupon discounts.
    /// </summary>
    public interface IProductCouponSource
    {
        /// <summary>
        /// Returns product coupon discounts for the specified <paramref name="parameters"/>.
        /// </summary>
        /// <param name="parameters">Parameters used to filter discounts.</param>
        IEnumerable<IMultiBuyDiscount> GetDiscounts(DiscountsParameters parameters);
    }
}