using CMS;
using CMS.Ecommerce;

[assembly: RegisterImplementation(typeof(ISettingServiceFactory), typeof(DefaultSettingServiceFactory), Priority = CMS.Core.RegistrationPriority.Fallback)]

namespace CMS.Ecommerce
{
    /// <summary>
    /// Interface for factories providing site-specific setting services.
    /// </summary>
    /// <seealso cref="ISettingService"/>
    public interface ISettingServiceFactory
    {
        /// <summary>
        /// Returns setting service specific for given site.
        /// </summary>
        /// <param name="siteId">ID of the site.</param>
        ISettingService GetSettingService(int siteId);
    }
}