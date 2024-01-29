using CMS;
using CMS.Ecommerce;

[assembly: RegisterImplementation(typeof(ICustomerRegistrationRepositoryFactory), typeof(DefaultCustomerRegistrationRepositoryFactory), Priority = CMS.Core.RegistrationPriority.Fallback)]

namespace CMS.Ecommerce
{
    /// <summary>
    /// Interface for factories providing customer registration repositories.
    /// </summary>
    /// <seealso cref="ICustomerRegistrationRepository"/>
    public interface ICustomerRegistrationRepositoryFactory
    {
        /// <summary>
        /// Returns customer registration repository for given site.
        /// </summary>
        /// <param name="siteId">ID of the site.</param>
        ICustomerRegistrationRepository GetRepository(int siteId);
    }
}