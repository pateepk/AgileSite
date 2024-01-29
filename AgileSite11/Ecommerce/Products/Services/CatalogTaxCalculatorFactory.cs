using System.Collections.Concurrent;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Represents a factory creating tax calculators used in a product catalog.
    /// </summary>
    internal class CatalogTaxCalculatorFactory : ICatalogTaxCalculatorFactory
    {
        private readonly ConcurrentDictionary<int, ICatalogTaxCalculator> mCatalogTaxCalculatorsBySiteId = new ConcurrentDictionary<int, ICatalogTaxCalculator>();

        private readonly ITaxAddressServiceFactory mAddressServiceFactory;
        private readonly ITaxEstimationService mEstimator;
        private readonly ITaxClassService mTaxClassService;
        private readonly ISettingServiceFactory mSettingServiceFactory;


        /// <summary>
        /// Initializes a new instance of the <see cref="CatalogTaxCalculatorFactory"/> class 
        /// with the specified tax estimation service, tax class service, tax address service factory and setting service factory.
        /// </summary>
        /// <param name="estimator">A tax estimation service.</param>
        /// <param name="taxClassService">A tax class service.</param>
        /// <param name="addressServiceFactory">A factory supplying tax address services.</param>
        /// <param name="settingServiceFactory">Factory used to get instances of setting services.</param>
        public CatalogTaxCalculatorFactory(ITaxEstimationService estimator, ITaxClassService taxClassService, 
            ITaxAddressServiceFactory addressServiceFactory, ISettingServiceFactory settingServiceFactory)
        {
            mEstimator = estimator;
            mTaxClassService = taxClassService;
            mAddressServiceFactory = addressServiceFactory;
            mSettingServiceFactory = settingServiceFactory;
        }


        /// <summary>
        /// Returns a catalog tax calculator usable on the site specified by <paramref name="siteId"/>.
        /// </summary>
        /// <param name="siteId">An ID of a site.</param>
        public ICatalogTaxCalculator GetCalculator(int siteId)
        {
            // Reuse previously created instances
            return mCatalogTaxCalculatorsBySiteId.GetOrAdd(siteId, CreateCalculator);
        }


        private ICatalogTaxCalculator CreateCalculator(int siteId)
        {
            var addressService = mAddressServiceFactory.GetTaxAddressService(siteId);
            var settingService = mSettingServiceFactory.GetSettingService(siteId);

            return new CatalogTaxCalculator(mEstimator, mTaxClassService, addressService, settingService);
        }
    }
}