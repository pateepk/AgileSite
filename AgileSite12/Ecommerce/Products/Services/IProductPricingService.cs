using System;
using System.Collections.Generic;

using CMS;
using CMS.Ecommerce;

[assembly: RegisterImplementation(typeof(IProductPricingService), typeof(ProductPricingService), Priority = CMS.Core.RegistrationPriority.Fallback)]

namespace CMS.Ecommerce
{
    /// <summary>
    /// Service providing product prices retrieval.
    /// </summary>
    /// <remarks>
    /// Prices are calculated using the <see cref="PriceParameters"/> parameters.
    /// </remarks>
    public interface IProductPricingService
    {
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
        ProductPrices GetPrices(SKUInfo sku, IEnumerable<SKUInfo> options, PriceParameters priceParams);
    }
}