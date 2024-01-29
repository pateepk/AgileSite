using System;
using System.Collections.Generic;
using System.Linq;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Service providing product prices retrieval.
    /// </summary>
    /// <remarks>
    /// Prices are calculated using the <see cref="PriceParameters"/> parameters.
    /// </remarks>
    public class ProductPricingService : IProductPricingService
    {
        private readonly ISKUPriceSourceFactory mPriceSourceFactory;
        private readonly IProductDiscountService mProductDiscountService;


        /// <summary>
        /// Creates a new instance of the <see cref="ProductPricingService"/>.
        /// </summary>
        /// <param name="priceSourceFactory">A factory creating SKU price sources for site.</param>
        /// <param name="productDiscountService">A <see cref="IProductDiscountService"/> service for discounts calculation.</param>
        public ProductPricingService(ISKUPriceSourceFactory priceSourceFactory, IProductDiscountService productDiscountService)
        {
            mPriceSourceFactory = priceSourceFactory;
            mProductDiscountService = productDiscountService;
        }


        /// <summary>
        /// Calculates the price of the given <paramref name="sku"/> with the specified product <paramref name="options"/> configuration.
        /// Returned prices are in the specified <see cref="PriceParameters.Currency"/>.
        /// </summary>
        /// <remarks>
        /// <see cref="ProductPrices.StandardPrice"/> represents base price including <paramref name="options"/> configuration.
        /// <see cref="ProductPrices.Price"/> represents final price with applied discounts.
        /// <see cref="ProductPrices.AppliedDiscounts"/> collection contains applied discounts with respective discount values.
        /// </remarks>
        /// <param name="sku">Main product, accessory product or product variant.</param>
        /// <param name="options">Attribute and text product options configuration.</param>
        /// <param name="priceParams"><see cref="PriceParameters"/> parameters of the current calculation.</param>
        /// <exception cref="ArgumentNullException"><paramref name="sku"/> or <paramref name="priceParams"/> is <c>null</c>.</exception>
        /// <returns>Instance of <see cref="ProductPrices"/> with calculation summary.</returns>
        public virtual ProductPrices GetPrices(SKUInfo sku, IEnumerable<SKUInfo> options, PriceParameters priceParams)
        {
            if (sku == null)
            {
                throw new ArgumentNullException(nameof(sku));
            }

            if (priceParams == null)
            {
                throw new ArgumentNullException(nameof(priceParams));
            }
            
            // Get a source of prices
            var priceSource = mPriceSourceFactory.GetSKUPriceSource(priceParams.SiteID);

            // Calculate price with attribute options
            var standardPrice = priceSource.GetPrice(sku, priceParams.Currency);
            var attrOptionsPrice = options?.Sum(option => priceSource.GetPrice(option, priceParams.Currency)) ?? 0m;
            var priceWithOptions = standardPrice + attrOptionsPrice;

            // Ensure positive or zero price
            priceWithOptions = (priceWithOptions < 0) ? 0 : priceWithOptions;

            // Apply all product discounts
            var appliedDiscounts = mProductDiscountService.GetProductDiscounts(sku, priceWithOptions, priceParams);

            var price = priceWithOptions - SumDiscounts(appliedDiscounts, priceWithOptions);

            return new ProductPrices(price, priceWithOptions, appliedDiscounts);
        }


        private decimal SumDiscounts(ValuesSummary discounts, decimal maxDiscount)
        {
            var discountValueSum = discounts.Sum(d => d.Value);

            // Discount can not be greater (further from zero) than max
            return (Math.Abs(discountValueSum) > Math.Abs(maxDiscount)) ? maxDiscount : discountValueSum;
        }
    }
}
