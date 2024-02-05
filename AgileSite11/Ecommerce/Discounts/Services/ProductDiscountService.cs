namespace CMS.Ecommerce
{
    /// <summary>
    /// Default implementation of the <see cref="IProductDiscountService"/> interface.
    /// </summary>
    /// <remarks>
    /// The calculation processes Catalog discounts and Volume discounts.
    /// </remarks>
    public class ProductDiscountService : IProductDiscountService
    {
        private readonly IDiscountApplicator mApplicator;
        private readonly IProductDiscountSource mDiscountSource;


        /// <summary>
        /// Creates a new instance of <see cref="ProductDiscountService"/> using the specified discount source and discount applicator.
        /// </summary>
        /// <param name="discountSource">The source of the discounts.</param>
        /// <param name="applicator"><see cref="IDiscountApplicator"/> item discount applicator service for discounts application.</param>
        public ProductDiscountService(IProductDiscountSource discountSource, IDiscountApplicator applicator)
        {
            mApplicator = applicator;
            mDiscountSource = discountSource;
        }


        /// <summary>
        /// Returns the discounts application summary for the given <paramref name="sku"/> product.
        /// </summary>
        /// <param name="sku">Product or product option.</param>
        /// <param name="standardPrice">Input price for calculation.</param>
        /// <param name="priceParams"><see cref="PriceParameters"/> calculation parameters.</param>
        /// <returns>Instance of <see cref="ProductPrices"/> with calculation summary.</returns>
        public virtual ValuesSummary GetProductDiscounts(SKUInfo sku, decimal standardPrice, PriceParameters priceParams)
        {
            var groups = mDiscountSource.GetDiscounts(sku, standardPrice, priceParams);
            
            return mApplicator.ApplyDiscounts(standardPrice, groups);
        }
    }
}
