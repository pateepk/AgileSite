namespace CMS.Ecommerce
{
    /// <summary>
    /// Provides tax rates based on the configuration stored using <see cref="TaxClassCountryInfoProvider"/> and <see cref="TaxClassStateInfoProvider"/>.
    /// </summary>
    internal class CountryStateTaxRateSource : ICountryStateTaxRateSource
    {
        /// <summary>
        /// Returns a tax rate (0..1) for the specified tax class, country and state.
        /// </summary>
        /// <param name="taxClassId">An ID of the tax class.</param>
        /// <param name="countryId">An ID of the country.</param>
        /// <param name="stateId">An ID of the state. Use 0 when unknown or not applicable.</param>
        public decimal GetRate(int taxClassId, int countryId, int stateId)
        {
            var taxValue = 0m;
            TaxClassStateInfo stateTax = null;

            if (stateId > 0)
            {
                // try to get value from the state first
                stateTax = TaxClassStateInfoProvider.GetTaxClassStateInfo(taxClassId, stateId);
                taxValue = stateTax?.TaxValue ?? 0m;
            }

            if ((countryId > 0) && (stateTax == null))
            {
                var countryTax = TaxClassCountryInfoProvider.GetTaxClassCountryInfo(countryId, taxClassId);
                taxValue = countryTax?.TaxValue ?? 0m;
            }

            return taxValue / 100m;
        }
    }
}