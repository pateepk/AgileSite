using CMS;
using CMS.Ecommerce;

[assembly: RegisterImplementation(typeof(IProductDiscountService), typeof(ProductDiscountService), Priority = CMS.Core.RegistrationPriority.Fallback)]

namespace CMS.Ecommerce
{
    /// <summary>
    /// Service providing calculation of discounts related to product units.
    /// </summary>
    /// <remarks>
    /// The calculation processes Catalog discounts and Volume discounts.
    /// </remarks>
    public interface IProductDiscountService
    {
        /// <summary>
        /// Returns the discount application summary for the given <paramref name="sku"/> product.
        /// </summary>
        /// <param name="sku">Product or product option</param>
        /// <param name="standardPrice">Input price for calculation.</param>
        /// <param name="priceParams"><see cref="PriceParameters"/> calculation parameters.</param>
        ValuesSummary GetProductDiscounts(SKUInfo sku, decimal standardPrice, PriceParameters priceParams);
    }
}