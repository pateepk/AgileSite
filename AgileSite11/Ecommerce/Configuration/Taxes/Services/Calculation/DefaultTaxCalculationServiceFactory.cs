using System.Collections.Concurrent;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Represents a factory providing site-specific tax calculation service.
    /// </summary>
    internal class DefaultTaxCalculationServiceFactory : ITaxCalculationServiceFactory
    {
        private readonly ConcurrentDictionary<int, ITaxCalculationService> mCalculationServicesBySiteID = new ConcurrentDictionary<int, ITaxCalculationService>();

        private readonly ITaxEstimationService mEstimationService;
        private readonly ITaxClassService mTaxService;
        private readonly ICustomerTaxClassService mCustomerTaxService;
        private readonly ITaxAddressServiceFactory mAddressServiceFactory;
        private readonly ISettingServiceFactory mSettingServiceFactory;


        /// <summary>
        /// Constructor. Creates a new instance of <see cref="DefaultTaxCalculationServiceFactory"/>.
        /// </summary>
        /// <param name="estimationService">Service used to get tax estimates.</param>
        /// <param name="taxService">Service used to get tax classes for items and shipping.</param>
        /// <param name="customerTaxService">Service used to get customer tax class.</param>
        /// <param name="addressServiceFactory">Factory used to get instances of address services.</param>
        /// <param name="settingServiceFactory">Factory used to get instances of setting services.</param>
        public DefaultTaxCalculationServiceFactory(ITaxEstimationService estimationService, ITaxClassService taxService, 
            ICustomerTaxClassService customerTaxService, ITaxAddressServiceFactory addressServiceFactory, ISettingServiceFactory settingServiceFactory)
        {
            mEstimationService = estimationService;
            mTaxService = taxService;
            mCustomerTaxService = customerTaxService;
            mAddressServiceFactory = addressServiceFactory;
            mSettingServiceFactory = settingServiceFactory;
        }


        /// <summary>
        /// Gets Tax calculation service specific for given site.
        /// </summary>
        /// <remarks>
        /// Note that only one instance of service is created per site. 
        /// This means that all calls with the same value of <paramref name="siteId"/> will return the same instance of the service.</remarks>
        /// <param name="siteId">ID of the site.</param>
        public ITaxCalculationService GetTaxCalculationService(int siteId)
        {
            // Reuse already created calculation services
            return mCalculationServicesBySiteID.GetOrAdd(siteId, CreateTaxCalculationService);
        }


        /// <summary>
        /// Creates a new tax calculation service for site specified by <paramref name="siteId"/>.
        /// </summary>
        /// <param name="siteId">ID of the site to create service for.</param>
        private ITaxCalculationService CreateTaxCalculationService(int siteId)
        {
            var addressService = mAddressServiceFactory.GetTaxAddressService(siteId);
            var settingService = mSettingServiceFactory.GetSettingService(siteId);

            return new DefaultTaxCalculationService(mEstimationService, mTaxService, mCustomerTaxService, addressService, settingService);
        }
    }
}