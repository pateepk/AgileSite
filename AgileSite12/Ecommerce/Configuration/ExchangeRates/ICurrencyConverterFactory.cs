using CMS;
using CMS.Ecommerce;
using CMS.Ecommerce.ExchangeRates;

[assembly: RegisterImplementation(typeof(ICurrencyConverterFactory), typeof(DefaultCurrencyConverterFactory), Priority = CMS.Core.RegistrationPriority.Fallback)]

namespace CMS.Ecommerce
{
    /// <summary>
    /// Defines the contract that class factories must implement to create new ICurrencyConverter objects.
    /// </summary>
    public interface ICurrencyConverterFactory
    {
        /// <summary>
        /// Returns currency converter able to convert currencies used on site specified by its ID.
        /// </summary>
        /// <param name="siteID">ID of the site to get currency converter for.</param>
        ICurrencyConverter GetCurrencyConverter(int siteID);
    }
}
