using System;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Represents a source of sku price in different currencies.
    /// </summary>
    /// <remarks>
    /// This is the default implementation of <see cref="ISKUPriceSource"/> using currency conversion.
    /// </remarks>
    internal class SKUPriceSource : ISKUPriceSource
    {
        private readonly ISiteMainCurrencySource mMainCurrencySource;
        private readonly ICurrencyConverter mCurrencyConverter;
        private readonly IRoundingService mRoundingService;
        private readonly Func<SKUInfo, decimal> mPriceExtractor;


        /// <summary>
        /// Initializes a new instance of the <see cref="SKUPriceSource"/> class with the specified main currency source and currency converter.
        /// </summary>
        /// <param name="mainCurrencySource">A source of main currencies.</param>
        /// <param name="currencyConverter">A currency converter.</param>
        /// <param name="roundingService">Service for price rounding.</param>
        /// <param name="priceExtractor">A function extracting a price from the sku. The <see cref="SKUInfo.SKUPrice"/> is used when extractor is <c>null</c>.</param>
        public SKUPriceSource(ISiteMainCurrencySource mainCurrencySource, ICurrencyConverter currencyConverter, IRoundingService roundingService, Func<SKUInfo, decimal> priceExtractor = null)
        {
            mMainCurrencySource = mainCurrencySource;
            mCurrencyConverter = currencyConverter;
            mRoundingService = roundingService;
            mPriceExtractor = priceExtractor;
        }


        /// <summary>
        /// Returns a rounded price of the <paramref name="sku"/> in the specified <paramref name="currency"/>.
        /// </summary>
        /// <param name="sku">An SKU object.</param>
        /// <param name="currency">A currency.</param>
        public decimal GetPrice(SKUInfo sku, CurrencyInfo currency)
        {
            var skuCurrency = mMainCurrencySource.GetSiteMainCurrencyCode(sku.SKUSiteID);

            var price = ExtractPrice(sku);
            var convertedPrice = mCurrencyConverter.Convert(price, skuCurrency, currency.CurrencyCode);

            return mRoundingService.Round(convertedPrice, currency);
        }


        private decimal ExtractPrice(SKUInfo sku)
        {
            return mPriceExtractor?.Invoke(sku) ?? sku.SKUPrice;
        }
    }
}