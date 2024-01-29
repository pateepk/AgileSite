using System.Collections.Concurrent;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Default implementation of<see cref="IRoundingServiceFactory"/> interface.
    /// </summary>
    internal class DefaultRoundingServiceFactory : IRoundingServiceFactory
    {
        private readonly ConcurrentDictionary<int, IRoundingService> mServices = new ConcurrentDictionary<int, IRoundingService>();

        private readonly ISettingServiceFactory mSettingServiceFactory;


        /// <summary>
        /// Creates a new instance of <see cref="DefaultRoundingServiceFactory"/>.
        /// with the specified setting service factory.
        /// </summary>
        /// <param name="settingServiceFactory">Factory used to get instances of setting services.</param>
        public DefaultRoundingServiceFactory(ISettingServiceFactory settingServiceFactory)
        {
            mSettingServiceFactory = settingServiceFactory;
        }


        /// <summary>
        /// Returns rounding service specific for given site.
        /// </summary>
        /// <param name="siteId">ID of the site.</param>
        public IRoundingService GetRoundingService(int siteId)
        {
            return mServices.GetOrAdd(siteId, CreateService);
        }


        private IRoundingService CreateService(int siteId)
        {
            var settingService = mSettingServiceFactory.GetSettingService(siteId);

            return new DefaultRoundingService(settingService);
        }
    }
}