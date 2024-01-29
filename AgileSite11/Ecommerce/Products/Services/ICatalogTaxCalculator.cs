namespace CMS.Ecommerce
{
    /// <summary>
    /// Represents a calculator of product taxes.
    /// </summary>
    /// <remarks>
    /// An implementation of this interface is used to obtain product taxes in product catalogs and product details.
    /// </remarks>
    public interface ICatalogTaxCalculator
    {
        /// <summary>
        /// Applies a tax on the specified <paramref name="price"/> using the <paramref name="parameters"/>.
        /// Applied tax value is returned in the <paramref name="tax"/> parameter.
        /// </summary>
        /// <param name="sku">An SKU object.</param>
        /// <param name="price">A taxed price.</param>
        /// <param name="parameters">A parameters of calculation.</param>
        /// <param name="tax">A tax</param>
        /// <returns>A price with the tax reflected.</returns>
        decimal ApplyTax(SKUInfo sku, decimal price, TaxCalculationParameters parameters, out decimal tax);
    }
}