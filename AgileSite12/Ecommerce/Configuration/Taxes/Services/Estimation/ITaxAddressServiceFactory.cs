using CMS;
using CMS.Ecommerce;

[assembly: RegisterImplementation(typeof(ITaxAddressServiceFactory), typeof(TaxAddressServiceFactory), Priority = CMS.Core.RegistrationPriority.Fallback)]

namespace CMS.Ecommerce
{
    /// <summary>
    /// Defines the interface for factories providing a site-specific tax address selection.
    /// </summary>
    /// <seealso cref="TaxAddressService"/>
    public interface ITaxAddressServiceFactory
    {
        /// <summary>
        /// Returns a tax address service for the given site.
        /// </summary>
        /// <param name="siteId">An ID of the site.</param>
        ITaxAddressService GetTaxAddressService(int siteId);
    }
}
