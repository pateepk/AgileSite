namespace CMS.Ecommerce
{
    /// <summary>
    /// Represents a factory creating site specific calculators of product catalog prices.
    /// </summary>
    internal class CatalogPriceCalculatorFactory : ICatalogPriceCalculatorFactory
    {
        private readonly IProductPricingService mPricingService;
        private readonly ICatalogTaxCalculatorFactory mTaxCalculatorFactory;


        /// <summary>
        /// Initializes a new instance of the <see cref="CatalogPriceCalculatorFactory"/> class 
        /// with the specified pricing service and tax calculator factory.
        /// </summary>
        /// <param name="pricingService">A pricing service.</param>
        /// <param name="taxCalculatorFactory">A factory supplying tax calculators.</param>
        public CatalogPriceCalculatorFactory(IProductPricingService pricingService, ICatalogTaxCalculatorFactory taxCalculatorFactory)
        {
            mPricingService = pricingService;
            mTaxCalculatorFactory = taxCalculatorFactory;
        }


        /// <summary>
        /// Returns a catalog price calculator usable on the specified site.
        /// </summary>
        /// <param name="siteId">An ID of a site.</param>
        public ICatalogPriceCalculator GetCalculator(int siteId)
        {
            var taxCalculator = mTaxCalculatorFactory.GetCalculator(siteId);

            return new CatalogPriceCalculator(mPricingService, taxCalculator);
        }
    }
}