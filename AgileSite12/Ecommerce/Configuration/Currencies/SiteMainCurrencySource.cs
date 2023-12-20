namespace CMS.Ecommerce
{
    /// <summary>
    /// Represents a source of site's main currencies.
    /// </summary>
    internal class SiteMainCurrencySource : ISiteMainCurrencySource
    {
        /// <summary>
        /// Returns a main currency of the specified site.
        /// </summary>
        /// <param name="siteId">An ID of the site. Use <c>0</c> for global main currency.</param>
        public CurrencyInfo GetSiteMainCurrency(int siteId)
        {
            return CurrencyInfoProvider.GetMainCurrency(siteId);
        }


        /// <summary>
        /// Returns a code of main currency for given site. Returns an empty string when not found.
        /// </summary>
        /// <param name="siteId">An ID of the site. Use <c>0</c> for global main currency.</param>
        public string GetSiteMainCurrencyCode(int siteId)
        {
            return CurrencyInfoProvider.GetMainCurrencyCode(siteId);
        }
    }
}