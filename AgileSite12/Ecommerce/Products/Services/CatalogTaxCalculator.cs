namespace CMS.Ecommerce
{
    /// <summary>
    /// Represents a calculator of product taxes.
    /// </summary>
    internal class CatalogTaxCalculator : ICatalogTaxCalculator
    {
        private readonly ITaxEstimationService mEstimator;
        private readonly ITaxClassService mTaxClassService;
        private readonly ITaxAddressService mAddressService;
        private readonly ISettingService mSettingService;


        /// <summary>
        /// Gets whether product price already include tax.
        /// </summary>
        private bool IsTaxIncludedInPrice
        {
            get
            {
                return mSettingService.GetBooleanValue(ECommerceSettings.INCLUDE_TAX_IN_PRICES);
            }
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="CatalogTaxCalculator"/> class with the specified tax estimator, tax class service, and address service.
        /// </summary>
        /// <param name="estimator">A tax estimation service.</param>
        /// <param name="taxClassService">A tax class service.</param>
        /// <param name="addressService">A tax address service.</param>
        /// <param name="settingService">A setting key service.</param>
        public CatalogTaxCalculator(ITaxEstimationService estimator, ITaxClassService taxClassService, ITaxAddressService addressService, ISettingService settingService)
        {
            mEstimator = estimator;
            mTaxClassService = taxClassService;
            mAddressService = addressService;
            mSettingService = settingService;
        }


        /// <summary>
        /// Applies a tax on the specified <paramref name="price"/> using the <paramref name="parameters"/>.
        /// Applied tax value is returned in the <paramref name="tax"/> parameter.
        /// </summary>
        /// <param name="sku">An SKU object.</param>
        /// <param name="price">A taxed price.</param>
        /// <param name="parameters">A parameters of calculation.</param>
        /// <param name="tax">A resulting tax.</param>
        /// <returns>A price with the tax reflected.</returns>
        public decimal ApplyTax(SKUInfo sku, decimal price, TaxCalculationParameters parameters, out decimal tax)
        {
            tax = 0m;

            var taxClass = mTaxClassService.GetTaxClass(sku);

            if (taxClass != null)
            {
                var address = mAddressService.GetTaxAddress(parameters.BillingAddress, parameters.ShippingAddress, taxClass, parameters.Customer);
                var estimationParams = new TaxEstimationParameters
                {
                    Currency = parameters.Currency,
                    Address = address,
                    SiteID = parameters.SiteID
                };

                tax = IsTaxIncludedInPrice 
                    ? mEstimator.ExtractTax(price, taxClass, estimationParams) 
                    : mEstimator.GetTax(price, taxClass, estimationParams);
            }

            return price;
        }
    }
}