using System.Collections.Concurrent;

namespace CMS.Ecommerce.ExchangeRates
{
    /// <summary>
    /// Default implementation of factory providing site specific currency converters. 
    /// Created converters uses exchange rates obtained using ExchangeRateInfoProvider and ExchangeTableInfoProvider.
    /// </summary>
    internal class DefaultCurrencyConverterFactory : ICurrencyConverterFactory
    {
        private readonly ConcurrentDictionary<int, ICurrencyConverter> mConvertersBySiteID = new ConcurrentDictionary<int, ICurrencyConverter>();


        /// <summary>
        /// Returns currency converter able to convert currencies used on site specified by its ID.
        /// </summary>
        /// <param name="siteID">ID of the site to get currency converter for.</param>
        public ICurrencyConverter GetCurrencyConverter(int siteID)
        {
            return mConvertersBySiteID.GetOrAdd(siteID, CreateConverter(siteID));
        }


        /// <summary>
        /// Returns new currency converter initialized to convert currencies on site specified by site ID.
        /// </summary>
        /// <param name="siteID">ID of the site to create converter for.</param>
        private static ICurrencyConverter CreateConverter(int siteID)
        {
            var converter = new DefaultCurrencyConverter(
                code => GetCurrency(code, siteID),
                toCurrency => GetRate(toCurrency, siteID),
                () => ExchangeTableInfoProvider.GetLastExchangeRateFromGlobalMainCurrency(siteID));

            return converter;
        }


        /// <summary>
        /// Returns rate from main currency to given currency using exchange table used on given site.
        /// If looking for rate for global currency, but given site does not use global currencies, global exchange table is used.
        /// </summary>
        /// <param name="toCurrency">Target currency.</param>
        /// <param name="siteID">ID of the site which exchange table should be used.</param>
        private static decimal GetRate(CurrencyInfo toCurrency, int siteID)
        {
            if ((siteID > 0) && toCurrency.IsGlobal && !ECommerceSettings.UseGlobalCurrencies(siteID))
            {
                siteID = 0;
            }

            return ExchangeRateInfoProvider.GetLastExchangeRate(toCurrency.CurrencyID, siteID);
        }


        /// <summary>
        /// Returns CurrencyInfo object with given currency code. Currency is taken primarily from site specified by id (0 for global).
        /// When site currency is not found, global currencies are searched.
        /// </summary>
        /// <param name="currencyCode"></param>
        /// <param name="siteID"></param>
        private static CurrencyInfo GetCurrency(string currencyCode, int siteID)
        {
            // Look for currency with given code
            var currencies = CurrencyInfoProvider.GetCurrenciesByCode(siteID);
            if(currencies.ContainsKey(currencyCode))
            {
                return currencies[currencyCode];
            }

            // Look for global currency when no site-specific currency with such a code found.
            return siteID > 0 ? GetCurrency(currencyCode, 0) : null;
        }
    }
}
