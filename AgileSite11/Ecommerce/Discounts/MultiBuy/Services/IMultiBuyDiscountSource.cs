using System.Collections.Generic;

using CMS;
using CMS.Ecommerce;

[assembly: RegisterImplementation(typeof(IMultiBuyDiscountSource), typeof(MultiBuyDiscountSource), Priority = CMS.Core.RegistrationPriority.Fallback)]

namespace CMS.Ecommerce
{
    /// <summary>
    /// Represents a source of multibuy discounts.
    /// </summary>
    public interface IMultiBuyDiscountSource
    {
        /// <summary>
        /// Returns multibuy discounts for the specified <paramref name="parameters"/>.
        /// </summary>
        /// <param name="parameters">Other parameters used to filter discounts.</param>
        IEnumerable<IMultiBuyDiscount> GetDiscounts(DiscountsParameters parameters);
    }
}
