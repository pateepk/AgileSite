namespace CMS.Ecommerce
{
    /// <summary>
    /// Represents a factory creating site specific calculators of product catalog prices.
    /// </summary>
    internal class CatalogPriceCalculatorFactory : ICatalogPriceCalculatorFactory
    {
        private readonly IProductPricingService mPricingService;
        private readonly ICatalogTaxCalculatorFactory mTaxCalculatorFactory;
        private readonly ISKUPriceSourceFactory mSKUPriceFactory;


        /// <summary>
        /// Initializes a new instance of the <see cref="CatalogPriceCalculatorFactory"/> class 
        /// with the specified pricing service and tax calculator factory.
        /// </summary>
        /// <param name="pricingService">A pricing service.</param>
        /// <param name="taxCalculatorFactory">A factory supplying tax calculators.</param>
        /// <param name="skuPriceFactory">Site specific price source factory.</param>
        public CatalogPriceCalculatorFactory(IProductPricingService pricingService, ICatalogTaxCalculatorFactory taxCalculatorFactory, ISKUPriceSourceFactory skuPriceFactory)
        {
            mPricingService = pricingService;
            mTaxCalculatorFactory = taxCalculatorFactory;
            mSKUPriceFactory = skuPriceFactory;
        }


        /// <summary>
        /// Returns a catalog price calculator usable on the specified site.
        /// </summary>
        /// <param name="siteId">An ID of a site.</param>
        public ICatalogPriceCalculator GetCalculator(int siteId)
        {
            var taxCalculator = mTaxCalculatorFactory.GetCalculator(siteId);

            return new CatalogPriceCalculator(mPricingService, taxCalculator, mSKUPriceFactory);
        }
    }
}