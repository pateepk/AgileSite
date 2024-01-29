using CMS;
using CMS.Ecommerce;

[assembly: RegisterImplementation(typeof(ITaxCalculationServiceFactory), typeof(DefaultTaxCalculationServiceFactory), Priority = CMS.Core.RegistrationPriority.Fallback)]

namespace CMS.Ecommerce
{
    /// <summary>
    /// Interface for factories providing site-specific tax calculation services.
    /// </summary>
    public interface ITaxCalculationServiceFactory
    {
        /// <summary>
        /// Gets Tax calculation service specific for given site.
        /// </summary>
        /// <param name="siteId">ID of the site.</param>
        ITaxCalculationService GetTaxCalculationService(int siteId);
    }
}