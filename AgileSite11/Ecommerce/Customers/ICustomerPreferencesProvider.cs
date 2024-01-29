using CMS;
using CMS.DataEngine;
using CMS.Ecommerce;

[assembly: RegisterImplementation(typeof(ICustomerPreferencesProvider), typeof(DefaultCustomerPreferencesProvider), Priority = CMS.Core.RegistrationPriority.Fallback)]

namespace CMS.Ecommerce
{
    /// <summary>
    /// Interface for provider of customer's preferences.
    /// </summary>
    public interface ICustomerPreferencesProvider
    {
        /// <summary>
        /// Gets customer's preferences on the site specified by <paramref name="site"/> parameter.
        /// </summary>
        /// <param name="customer">Customer for who the preferences are obtained.</param>
        /// <param name="site">Identifier of the site for which the preferences are obtained.</param>
        /// <returns>Customer's preferences on given site. 
        /// Returns <see cref="CustomerPreferences.Unknown"/> when customer or preferences not found.</returns>
        CustomerPreferences GetPreferences(CustomerInfo customer, SiteInfoIdentifier site);
    }
}
