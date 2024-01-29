using System.Collections.Concurrent;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Represents a factory creating price sources supplying prices of SKUs.
    /// </summary>
    /// <seealso cref="ISKUPriceSource"/>
    internal class SKUPriceSourceFactory : ISKUPriceSourceFactory
    {
        private readonly ConcurrentDictionary<int, ISKUPriceSource> mPriceSourcesBySiteId = new ConcurrentDictionary<int, ISKUPriceSource>();
        private readonly ConcurrentDictionary<int, ISKUPriceSource> mListPriceSourcesBySiteId = new ConcurrentDictionary<int, ISKUPriceSource>();

        private readonly ISiteMainCurrencySource mMainCurrencySource;
        private readonly ICurrencyConverterFactory mConverterFactory;
        private readonly IRoundingServiceFactory mRoundingServiceFactory;


        /// <summary>
        /// Initializes a new instance of the <see cref="SKUPriceSourceFactory"/> class with the specified source of main currencies and currency converter factory.
        /// </summary>
        /// <param name="mainCurrencySource">A source of main currencies for sites.</param>
        /// <param name="converterFactory">A factory used for obtaining site-specific currency converters.</param>
        /// <param name="roundingServiceFactory">The factory obtaining service for price rounding.</param>
        public SKUPriceSourceFactory(ISiteMainCurrencySource mainCurrencySource, ICurrencyConverterFactory converterFactory, IRoundingServiceFactory roundingServiceFactory)
        {
            mMainCurrencySource = mainCurrencySource;
            mConverterFactory = converterFactory;
            mRoundingServiceFactory = roundingServiceFactory;
        }


        /// <summary>
        /// Returns a source of SKU prices usable on the site specified by <paramref name="siteId"/>.
        /// </summary>
        /// <param name="siteId">An ID of a site.</param>
        public ISKUPriceSource GetSKUPriceSource(int siteId)
        {
            // Reuse previously created instances
            return mPriceSourcesBySiteId.GetOrAdd(siteId, CreatePriceSource);
        }


        /// <summary>
        /// Returns a source of SKU list prices usable on the site specified by <paramref name="siteId"/>.
        /// </summary>
        /// <param name="siteId">An ID of a site.</param>
        public ISKUPriceSource GetSKUListPriceSource(int siteId)
        {
            // Reuse previously created instances
            return mListPriceSourcesBySiteId.GetOrAdd(siteId, CreateListPriceSource);
        }


        private ISKUPriceSource CreatePriceSource(int siteId)
        {
            var currencyConverter = mConverterFactory.GetCurrencyConverter(siteId);
            var roundingService = mRoundingServiceFactory.GetRoundingService(siteId);

            return new SKUPriceSource(mMainCurrencySource, currencyConverter, roundingService);
        }


        private ISKUPriceSource CreateListPriceSource(int siteId)
        {
            var currencyConverter = mConverterFactory.GetCurrencyConverter(siteId);
            var roundingService = mRoundingServiceFactory.GetRoundingService(siteId);

            return new SKUPriceSource(mMainCurrencySource, currencyConverter, roundingService, sku => sku.SKURetailPrice);
        }
    }
}