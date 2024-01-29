using System.Collections.Concurrent;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Represents a factory creating site-specific tax address services.
    /// </summary>
    internal class TaxAddressServiceFactory : ITaxAddressServiceFactory
    {
        private readonly ConcurrentDictionary<int, ITaxAddressService> mServicesBySiteId = new ConcurrentDictionary<int, ITaxAddressService>();


        /// <summary>
        /// Returns a site specific tax address service.
        /// </summary>
        /// <remarks>
        /// Only one instance is created for each site. The instance is reused on the next call with the same <paramref name="siteId"/>.
        /// </remarks> 
        /// <param name="siteId">ID of the site for which the service is created.</param>
        public ITaxAddressService GetTaxAddressService(int siteId)
        {
            return mServicesBySiteId.GetOrAdd(siteId, CreateNewTaxAddressService);
        }


        private ITaxAddressService CreateNewTaxAddressService(int siteId)
        {
            return new TaxAddressService(siteId);
        }
    }
}