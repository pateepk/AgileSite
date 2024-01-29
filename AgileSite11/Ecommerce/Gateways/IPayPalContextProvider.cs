using CMS;
using CMS.Ecommerce;

using PayPal.Api;

[assembly: RegisterImplementation(typeof(IPayPalContextProvider), typeof(PayPalContextProvider), Priority = CMS.Core.RegistrationPriority.Fallback)]

namespace CMS.Ecommerce
{
    /// <summary>
    /// Describes provider of PayPal context.
    /// </summary>
    public interface IPayPalContextProvider
    {
        /// <summary>
        /// Returns complete <see cref="APIContext"/> used in PayPal SDK for payment processing.
        /// </summary>
        /// <param name="siteId">ID of the site.</param>
        /// <returns></returns>
        APIContext GetApiContext(int siteId);
    }
}