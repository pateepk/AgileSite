using System;
using System.Linq;
using System.Text;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Defines methods that convert money between currencies.
    /// </summary>
    public interface ICurrencyConverter
    {
        /// <summary>
        /// Converts monetary value from one currency to other currency.
        /// </summary>
        /// <param name="amount">Monetary amount to be converted.</param>
        /// <param name="inCurrencyCode">Code of currency in which amount is expressed, e.g. USD, EUR.</param>
        /// <param name="toCurrencyCode">Code of currency to convert amount to, e.g. USD, EUR.</param>
        /// <returns>Amount converted to currency specified by toCurrencyCode parameter.</returns>
        /// <exception cref="System.ArgumentException">Thrown when currency with given code does not exist in the system.</exception>
        /// <exception cref="CMS.Ecommerce.InvalidCurrencyConversionException">Thrown when conversion fails (e.g. exchange rate not found).</exception>
        decimal Convert(decimal amount, string inCurrencyCode, string toCurrencyCode);


        /// <summary>
        /// Returns exchange rate for conversion from one currency to another. 
        /// Example: if 1 USD = 16 CZK then this method returns 1/16 = 0.0625 for conversion from USD to CZK.
        /// </summary>
        /// <param name="fromCurrencyCode">Code of currency in which amount is expressed, e.g. USD, EUR.</param>
        /// <param name="toCurrencyCode">Code of currency to convert amount to, e.g. USD, EUR.</param>
        /// <exception cref="System.ArgumentException">Thrown when currency with given code does not exist in the system.</exception>
        /// <exception cref="CMS.Ecommerce.InvalidCurrencyConversionException">Thrown when conversion fails (e.g. exchange rate not found).</exception>
        decimal GetExchangeRate(string fromCurrencyCode, string toCurrencyCode);
    }
}
