using CMS;
using CMS.Ecommerce;

[assembly: RegisterImplementation(typeof(IRoundingService), typeof(DefaultRoundingService), Priority = CMS.Core.RegistrationPriority.Fallback)]

namespace CMS.Ecommerce
{
    /// <summary>
    /// Defines interface to implement price rounding service.
    /// </summary>
    public interface IRoundingService
    {
        /// <summary>
        /// Returns rounded price according to the specified currency.
        /// </summary>
        /// <param name="price">Price to be rounded</param>
        /// <param name="currency">Currency which supplies number of decimal places</param>
        decimal Round(decimal price, CurrencyInfo currency);
    }
}