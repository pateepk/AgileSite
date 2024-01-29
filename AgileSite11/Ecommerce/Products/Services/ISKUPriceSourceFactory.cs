using CMS;
using CMS.Ecommerce;

[assembly: RegisterImplementation(typeof(ISKUPriceSourceFactory), typeof(SKUPriceSourceFactory), Priority = CMS.Core.RegistrationPriority.Fallback)]

namespace CMS.Ecommerce
{
    /// <summary>
    /// Defines the members for a factory creating creating site specific price sources.
    /// </summary>
    public interface ISKUPriceSourceFactory
    {
        /// <summary>
        /// Returns a source of SKU prices usable on the site specified by <paramref name="siteId"/>.
        /// </summary>
        /// <param name="siteId">An ID of a site.</param>
        ISKUPriceSource GetSKUPriceSource(int siteId);


        /// <summary>
        /// Returns a source of SKU list prices usable on the site specified by <paramref name="siteId"/>.
        /// </summary>
        /// <param name="siteId">An ID of a site.</param>
        ISKUPriceSource GetSKUListPriceSource(int siteId);
    }
}
