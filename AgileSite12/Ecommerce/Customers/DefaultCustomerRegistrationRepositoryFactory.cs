using System.Collections.Concurrent;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Default implementation of <see cref="ICustomerRegistrationRepositoryFactory"/>.
    /// </summary>
    internal class DefaultCustomerRegistrationRepositoryFactory : ICustomerRegistrationRepositoryFactory
    {
        private readonly ConcurrentDictionary<int, ICustomerRegistrationRepository> mRepositories = new ConcurrentDictionary<int, ICustomerRegistrationRepository>();
        private readonly ISettingServiceFactory mSettingServiceFactory;


        /// <summary>
        /// Create a new instance of <see cref="DefaultCustomerRegistrationRepositoryFactory"/>.
        /// </summary>
        public DefaultCustomerRegistrationRepositoryFactory(ISettingServiceFactory settingServiceFactory)
        {
            mSettingServiceFactory = settingServiceFactory;
        }


        /// <summary>
        /// Returns customer registration repository for given site.
        /// </summary>
        /// <param name="siteId">ID of the site.</param>
        public ICustomerRegistrationRepository GetRepository(int siteId)
        {
            return mRepositories.GetOrAdd(siteId, CreateRepository);
        }


        private ICustomerRegistrationRepository CreateRepository(int siteId)
        {
            return new DefaultCustomerRegistrationRepository(mSettingServiceFactory, siteId);
        }
    }
}