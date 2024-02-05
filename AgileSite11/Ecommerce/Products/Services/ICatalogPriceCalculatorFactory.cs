using CMS;
using CMS.Ecommerce;

[assembly: RegisterImplementation(typeof(ICatalogPriceCalculatorFactory), typeof(CatalogPriceCalculatorFactory), Priority = CMS.Core.RegistrationPriority.Fallback)]

namespace CMS.Ecommerce
{
    /// <summary>
    /// Represents a factory creating site specific calculators of product catalog prices.
    /// </summary>
    public interface ICatalogPriceCalculatorFactory
    {
        /// <summary>
        /// Returns a catalog price calculator usable on the specified site.
        /// </summary>
        /// <param name="siteId">An ID of a site.</param>
        ICatalogPriceCalculator GetCalculator(int siteId);
    }
}