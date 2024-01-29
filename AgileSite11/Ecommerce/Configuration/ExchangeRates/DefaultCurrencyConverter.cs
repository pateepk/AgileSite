using System;
using System.Linq;
using System.Text;

namespace CMS.Ecommerce.ExchangeRates
{
    internal class DefaultCurrencyConverter : ICurrencyConverter
    {
        private readonly Func<string, CurrencyInfo> GetCurrency;
        private readonly Func<CurrencyInfo, decimal> GetRate;
        private readonly Func<decimal> GetRateFromGlobalMainToSiteMain; 
        

        /// <summary>
        /// Creates new instance of DefaultCurrencyConverter.
        /// </summary>
        /// <param name="codeToCurrencyConverter">Function returning CurrencyInfo object for currency code (e.g. USD).</param>
        /// <param name="getRate">Function returning exchange rate from main currency to given currency.</param>
        /// <param name="getExchangeRateFromGlobal">Function returning exchange rate from global main currency to site main currency.</param>
        public DefaultCurrencyConverter(Func<string, CurrencyInfo> codeToCurrencyConverter, Func<CurrencyInfo, decimal> getRate, Func<decimal> getExchangeRateFromGlobal)
        {
            GetCurrency = codeToCurrencyConverter;
            GetRateFromGlobalMainToSiteMain = getExchangeRateFromGlobal;
            GetRate = getRate;
        }


        /// <summary>
        /// Converts monetary value from one currency to other currency.
        /// </summary>
        /// <param name="amount">Monetary amount to be converted.</param>
        /// <param name="inCurrencyCode">Code of currency in which amount is expressed.</param>
        /// <param name="toCurrencyCode">Code of currency to convert amount to.</param>
        /// <returns>Amount converted to currency specified by toCurrencyCode parameter.</returns>
        /// <exception cref="System.ArgumentException">Thrown when currency with given code does not exist in the system.</exception>
        /// <exception cref="CMS.Ecommerce.InvalidCurrencyConversionException">Thrown when conversion fails (e.g. exchange rate not found).</exception> 
        public decimal Convert(decimal amount, string inCurrencyCode, string toCurrencyCode)
        {
            var rate = GetExchangeRate(inCurrencyCode, toCurrencyCode);

            return CurrencyConverter.ApplyExchangeRate(amount, rate);
        }


        /// <summary>
        /// Returns exchange rate for conversion from one currency to another. 
        /// Example: if 1 USD = 16 CZK then this method returns 1/16 = 0.0625 for conversion from USD to CZK.
        /// </summary>
        /// <param name="fromCurrencyCode">Code of currency in which amount is expressed, e.g. USD, EUR.</param>
        /// <param name="toCurrencyCode">Code of currency to convert amount to, e.g. USD, EUR.</param>
        /// <exception cref="System.ArgumentException">Thrown when currency with given code does not exist in the system.</exception>
        /// <exception cref="CMS.Ecommerce.InvalidCurrencyConversionException">Thrown when conversion fails (e.g. exchange rate not found).</exception>
        public decimal GetExchangeRate(string fromCurrencyCode, string toCurrencyCode)
        {
            var fromCurrency = GetCurrency(fromCurrencyCode);
            if (fromCurrency == null)
            {
                throw new ArgumentException(string.Format("Currency with code '{0}' not found.", fromCurrencyCode), nameof(fromCurrencyCode));
            }

            var toCurrency = GetCurrency(toCurrencyCode);
            if (toCurrency == null)
            {
                throw new ArgumentException(string.Format("Currency with code '{0}' not found.", toCurrencyCode), nameof(toCurrencyCode));
            }

            return GetExchangeRate(fromCurrency, toCurrency);
        }


        /// <summary>
        /// Returns exchange rate for conversion from one currency to another.
        /// </summary>
        /// <param name="fromCurrency">Source currency.</param>
        /// <param name="toCurrency">Target currency.</param>
        private decimal GetExchangeRate(CurrencyInfo fromCurrency, CurrencyInfo toCurrency)
        {
            if (fromCurrency == toCurrency)
            {
                return 1;
            }

            // Handle conversions within one site
            if (fromCurrency.CurrencySiteID == toCurrency.CurrencySiteID)
            {
                if (fromCurrency.CurrencyIsMain || toCurrency.CurrencyIsMain)
                {
                    return fromCurrency.CurrencyIsMain ? GetRateFromMain(toCurrency) : GetRateToMain(fromCurrency);
                }

                return GetRateToMain(fromCurrency) * GetRateFromMain(toCurrency);
            }

            // Conversion within different sites is not supported
            if (!fromCurrency.IsGlobal && !toCurrency.IsGlobal)
            {
                throw new InvalidCurrencyConversionException("Can not convert currencies between different sites.");
            }

            // One of currencies is global and another one is site-specific here

            // Calculate inverted rate if target currency is global
            if (!fromCurrency.IsGlobal)
            {
                return 1 / GetExchangeRate(toCurrency, fromCurrency);
            }

            // Source currency is global and target currency is site-specific here

            // Get rate to global main currency if not already main one
            var rate = 1m;
            if (!fromCurrency.CurrencyIsMain)
            {
                rate = GetRateToMain(fromCurrency);
            }

            // Add rate from global main to site main and rate from site main to target currency
            return rate * GetRateFromGlobalMainToSiteMain() * GetRateFromMain(toCurrency);
        }


        #region "Partial conversions"

        private decimal GetRateToMain(CurrencyInfo fromCurrency)
        {
            if (fromCurrency.CurrencyIsMain)
            {
                return 1;
            }

            return 1 / GetRate(fromCurrency);
        }


        private decimal GetRateFromMain(CurrencyInfo toCurrency)
        {
            if (toCurrency.CurrencyIsMain)
            {
                return 1;
            }

            return GetRate(toCurrency);
        }

        #endregion
    }
}
