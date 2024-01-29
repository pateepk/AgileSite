using System.Collections.Concurrent;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Represents a factory providing site-specific tax calculation service.
    /// </summary>
    internal class DefaultSettingServiceFactory : ISettingServiceFactory
    {
        private readonly ConcurrentDictionary<int, ISettingService> mServices = new ConcurrentDictionary<int, ISettingService>();


        /// <summary>
        /// Returns setting service specific for given site.
        /// </summary>
        /// <param name="siteId">ID of the site.</param>
        public ISettingService GetSettingService(int siteId)
        {
            return mServices.GetOrAdd(siteId, CreateService);
        }


        private static ISettingService CreateService(int siteId)
        {
            return new DefaultSettingService(siteId);
        }
    }
}