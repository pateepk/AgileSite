using CMS;
using CMS.Ecommerce;

[assembly: RegisterImplementation(typeof(ICatalogTaxCalculatorFactory), typeof(CatalogTaxCalculatorFactory), Priority = CMS.Core.RegistrationPriority.Fallback)]

namespace CMS.Ecommerce
{
    /// <summary>
    /// Represents a factory creating tax calculators used in a product catalog.
    /// </summary>
    public interface ICatalogTaxCalculatorFactory
    {
        /// <summary>
        /// Returns a catalog tax calculator usable on the specified site.
        /// </summary>
        /// <param name="siteId">An ID of a site.</param>
        ICatalogTaxCalculator GetCalculator(int siteId);
    }
}
