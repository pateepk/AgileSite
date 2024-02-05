using CMS;
using CMS.Ecommerce;

[assembly: RegisterImplementation(typeof(ICountryStateTaxRateSource), typeof(CountryStateTaxRateSource), Priority = CMS.Core.RegistrationPriority.Fallback)]

namespace CMS.Ecommerce
{
    /// <summary>
    /// Defines the contract for classes providing tax rate for country (and state).
    /// </summary>
    public interface ICountryStateTaxRateSource
    {
        /// <summary>
        /// Returns a tax rate (0..1) for the specified tax class, country and state.
        /// </summary>
        /// <param name="taxClassId">An ID of the tax class.</param>
        /// <param name="countryId">An ID of the country.</param>
        /// <param name="stateId">An ID of the state. Use 0 when unknown or not applicable.</param>
        decimal GetRate(int taxClassId, int countryId, int stateId);
    }
}