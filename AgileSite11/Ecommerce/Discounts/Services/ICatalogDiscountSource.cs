using System.Collections.Generic;

using CMS;
using CMS.Ecommerce;

[assembly: RegisterImplementation(typeof(ICatalogDiscountSource), typeof(CatalogDiscountSource), Priority = CMS.Core.RegistrationPriority.Fallback)]

namespace CMS.Ecommerce
{
    /// <summary>
    /// Represents a source of catalog discounts.
    /// </summary>
    public interface ICatalogDiscountSource
    {
        /// <summary>
        /// Returns the catalog discounts collection which should be applied for the specified <paramref name="sku"/>.
        /// Applied discounts must be running due to <see cref="PriceParameters.CalculationDate"/>, applicable for the given <see cref="PriceParameters.User"/> and satisfy the discount conditions.
        /// </summary>
        /// <param name="sku">The SKU to get discounts for.</param>
        /// <param name="priceParams">Product price calculation parameters</param>
        IEnumerable<DiscountInfo> GetDiscounts(SKUInfo sku, PriceParameters priceParams);
    }
}