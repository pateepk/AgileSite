using System;
using System.Linq;
using System.Text;

using CMS.Core;
using CMS.EventLog;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Static wrapper for currency conversion service.
    /// </summary>
    public static class CurrencyConverter
    {
        /// <summary>
        /// Converts given monetary amount expressed in one currency to another currency using site specific conversion.
        /// </summary>
        /// <param name="amount">Amount to be converted.</param>
        /// <param name="inCurrencyCode">Code of the currency in which the amount is expressed (e.g. USD).</param>
        /// <param name="toCurrencyCode">Code of the currency to convert amount to (e.g. EUR).</param>
        /// <param name="siteID">ID of the site on which the conversion is done.</param>
        /// <returns>Amount converted to target currency on specific site.</returns>
        /// <exception cref="System.ArgumentException">Thrown when currency with given code does not exist in the system.</exception>
        /// <exception cref="CMS.Ecommerce.InvalidCurrencyConversionException">Thrown when conversion fails (e.g. exchange rate not found).</exception>
        public static decimal Convert(decimal amount, string inCurrencyCode, string toCurrencyCode, int siteID)
        {
            return GetConverter(siteID).Convert(amount, inCurrencyCode, toCurrencyCode);
        }


        /// <summary>
        /// Converts given monetary amount expressed main currency to different currency using site specific conversion.
        /// </summary>
        /// <param name="amountInMain">Amount in.</param>
        /// <param name="inGlobalMain">Indicates if amount is expressed in global main currency. Amount is in site main currency when set to false.</param>
        /// <param name="toCurrencyCode">Code of the currency to convert amount to (e.g. EUR).</param>
        /// <param name="siteID">ID of the site on which the conversion is done.</param>
        /// <returns>Amount converted to target currency on specific site.</returns>
        /// <exception cref="System.ArgumentException">Thrown when currency with given code does not exist in the system.</exception>
        /// <exception cref="CMS.Ecommerce.InvalidCurrencyConversionException">Thrown when conversion fails (e.g. exchange rate not found).</exception>
        public static decimal Convert(decimal amountInMain, bool inGlobalMain, string toCurrencyCode, int siteID)
        {
            var fromCurrencyCode = CurrencyInfoProvider.GetMainCurrencyCode(inGlobalMain ? 0 : siteID);

            return Convert(amountInMain, fromCurrencyCode, toCurrencyCode, siteID);
        }


        /// <summary>
        /// Calculates exchange rate for conversion from one currency to another. 
        /// Example: if 1 USD = 16 CZK then this method returns 1/16 = 0.0625 for conversion from USD to CZK.
        /// </summary>
        /// <param name="fromCurrencyCode">Code of currency in which amount is expressed, e.g. USD, EUR.</param>
        /// <param name="toCurrencyCode">Code of currency to convert amount to, e.g. USD, EUR.</param>
        /// <param name="siteID">ID of the site for which the rate is calculated.</param>
        /// <returns>Exchange rate from one currency to another currency on specific site.</returns>
        /// <exception cref="System.ArgumentException">Thrown when currency with given code does not exist in the system.</exception>
        /// <exception cref="CMS.Ecommerce.InvalidCurrencyConversionException">Thrown when conversion fails (e.g. exchange rate not found).</exception>
        public static decimal GetExchangeRate(string fromCurrencyCode, string toCurrencyCode, int siteID)
        {
            return GetConverter(siteID).GetExchangeRate(fromCurrencyCode, toCurrencyCode);
        }


        /// <summary>
        /// Calculates exchange rate for conversion from one currency to another. 
        /// Example: if 1 USD = 16 CZK then this method returns 1/16 = 0.0625 for conversion from USD to CZK.
        /// All exceptions are logged into event log.
        /// </summary>
        /// <param name="fromCurrencyCode">Code of currency in which amount is expressed, e.g. USD, EUR.</param>
        /// <param name="toCurrencyCode">Code of currency to convert amount to, e.g. USD, EUR.</param>
        /// <param name="siteID">ID of the site for which the rate is calculated.</param>
        /// <param name="rate">When this method returns, contains exchange rate from one currency to another currency on specific site.</param>
        /// <returns>True if exchange rate was obtained successfully; otherwise, false.</returns>
        public static bool TryGetExchangeRate(string fromCurrencyCode, string toCurrencyCode, int siteID, ref decimal rate)
        {
            try
            {
                rate = GetExchangeRate(fromCurrencyCode, toCurrencyCode, siteID);
                return true;
            }
            catch (Exception ex)
            {
                EventLogProvider.LogException("CurrencyConversion", "GETEXCHANGERATE", ex);
            }
            
            return false;
        }


        /// <summary>
        /// Calculates exchange rate for conversion from main currency to different currency. 
        /// Example: if 1 USD = 16 CZK then this method returns 1/16 = 0.0625 for conversion from USD to CZK.
        /// </summary>
        /// <param name="fromGlobalMain">Indicates if conversion is done from global main currency or from site main currency.</param>
        /// <param name="toCurrencyCode">Code of currency to convert amount to, e.g. USD, EUR.</param>
        /// <param name="siteID">ID of the site for which the rate is calculated.</param>
        /// <returns>Exchange rate from main currency to another currency on specific site.</returns>
        /// <exception cref="System.ArgumentException">Thrown when currency with given code does not exist in the system.</exception>
        /// <exception cref="CMS.Ecommerce.InvalidCurrencyConversionException">Thrown when conversion fails (e.g. exchange rate not found).</exception>
        public static decimal GetExchangeRate(bool fromGlobalMain, string toCurrencyCode, int siteID)
        {
            var fromCurrencyCode = CurrencyInfoProvider.GetMainCurrencyCode(fromGlobalMain ? 0 : siteID);

            return GetExchangeRate(fromCurrencyCode, toCurrencyCode, siteID);
        }


        /// <summary>
        /// Calculates exchange rate for conversion from main currency to different currency. 
        /// Example: if 1 USD = 16 CZK then this method returns 1/16 = 0.0625 for conversion from USD to CZK.
        /// All exceptions are logged into event log.
        /// </summary>
        /// <param name="fromGlobalMain">Indicates if conversion is done from global main currency or from site main currency.</param>
        /// <param name="toCurrencyCode">Code of currency to convert amount to, e.g. USD, EUR.</param>
        /// <param name="siteID">ID of the site for which the rate is calculated.</param>
        /// <param name="rate">When this method returns, contains exchange rate from main currency to different currency on specific site.</param>
        /// <returns>True if exchange rate was obtained successfully; otherwise, false.</returns>
        public static bool TryGetExchangeRate(bool fromGlobalMain, string toCurrencyCode, int siteID, ref decimal rate)
        {
            try
            {
                rate = GetExchangeRate(fromGlobalMain, toCurrencyCode, siteID);
                return true;
            }
            catch (Exception ex)
            {
                EventLogProvider.LogException("CurrencyConversion", "GETEXCHANGERATE", ex);
            }

            return false;
        }


        /// <summary>
        /// Applies given exchange rate to monetary amount. If rate is negative or zero, amount is returned with no change.
        /// </summary>
        /// <param name="amount">Monetary amount.</param>
        /// <param name="rate">Exchange rate to be applied.</param>
        /// <returns>Amount converted using given rate.</returns>
        public static decimal ApplyExchangeRate(decimal amount, decimal rate)
        {
            if(rate <= 0)
            {
                return amount;
            }

            return amount / rate;
        }


        /// <summary>
        /// Returns currency converter used on given site.
        /// </summary>
        /// <param name="siteID">ID of the site to get currency converter for.</param>
        private static ICurrencyConverter GetConverter(int siteID)
        {
            return Service.Resolve<ICurrencyConverterFactory>().GetCurrencyConverter(siteID);
        }
    }
}
