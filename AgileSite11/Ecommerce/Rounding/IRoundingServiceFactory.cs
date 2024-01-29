using CMS;
using CMS.Ecommerce;

[assembly: RegisterImplementation(typeof(IRoundingServiceFactory), typeof(DefaultRoundingServiceFactory), Priority = CMS.Core.RegistrationPriority.Fallback)]

namespace CMS.Ecommerce
{
    /// <summary>
    /// Interface for factories providing site-specific rounding services.
    /// </summary>
    public interface IRoundingServiceFactory
    {
        /// <summary>
        /// Returns rounding service specific for given site.
        /// </summary>
        /// <param name="siteId">ID of the site.</param>
        IRoundingService GetRoundingService(int siteId);
    }
}