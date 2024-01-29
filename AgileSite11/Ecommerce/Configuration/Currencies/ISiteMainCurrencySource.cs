using CMS;
using CMS.Ecommerce;

[assembly: RegisterImplementation(typeof(ISiteMainCurrencySource), typeof(SiteMainCurrencySource), Priority = CMS.Core.RegistrationPriority.Fallback)]

namespace CMS.Ecommerce
{
    /// <summary>
    /// Defines the members for a service providing a main currency for sites.
    /// </summary>
    public interface ISiteMainCurrencySource
    {
        /// <summary>
        /// Returns a main currency of the specified site.
        /// </summary>
        /// <param name="siteId">An ID of the site. Use <c>0</c> for global main currency.</param>
        CurrencyInfo GetSiteMainCurrency(int siteId);


        /// <summary>
        /// Returns a code of main currency for given site. Returns an empty string when not found.
        /// </summary>
        /// <param name="siteId">An ID of the site. Use <c>0</c> for global main currency.</param>
        string GetSiteMainCurrencyCode(int siteId);
    }
}
