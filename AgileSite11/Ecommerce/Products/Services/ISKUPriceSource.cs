namespace CMS.Ecommerce
{
    /// <summary>
    /// Defines the members for a source of SKU price.
    /// </summary>
    public interface ISKUPriceSource
    {
        /// <summary>
        /// Returns a rounded price of the <paramref name="sku"/> in the specified <paramref name="currency"/>.
        /// </summary>
        /// <param name="sku">An SKU object.</param>
        /// <param name="currency">A currency.</param>
        decimal GetPrice(SKUInfo sku, CurrencyInfo currency);
    }
}
