using System.Collections.Generic;

using CMS;
using CMS.Ecommerce;

[assembly: RegisterImplementation(typeof(IProductDiscountSource), typeof(ProductDiscountSource), Priority = CMS.Core.RegistrationPriority.Fallback)]

namespace CMS.Ecommerce
{
    /// <summary>
    /// Represents a source of discounts related to product units.
    /// </summary>
    public interface IProductDiscountSource
    {
        /// <summary>
        /// Returns groups of discounts for specified <paramref name="sku"/>.
        /// </summary>
        /// <param name="sku">The SKU object.</param>
        /// <param name="standardPrice">The price of the <paramref name="sku"/></param>
        /// <param name="priceParams">Other parameters.</param>
        IEnumerable<DiscountCollection> GetDiscounts(SKUInfo sku, decimal standardPrice, PriceParameters priceParams);
    }
}