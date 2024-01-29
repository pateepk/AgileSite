using System;
using System.Linq;

using CMS.DataEngine;
using CMS.Helpers;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Class providing ExchangeRateInfo management.
    /// </summary>
    public class ExchangeRateInfoProvider : AbstractInfoProvider<ExchangeRateInfo, ExchangeRateInfoProvider>
    {
        #region "Constructors"

        /// <summary>
        /// Constructor.
        /// </summary>
        public ExchangeRateInfoProvider()
            : base(ExchangeRateInfo.TYPEINFO, new HashtableSettings
                {
                    ID = true
                })
        {
        }

        #endregion


        #region "Public methods - Basic"

        /// <summary>
        /// Returns the query for all exchange rates between currencies.
        /// </summary>
        public static ObjectQuery<ExchangeRateInfo> GetExchangeRates()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns exchange rate with specified ID.
        /// </summary>
        /// <param name="rateId">Exchange rate ID</param>        
        public static ExchangeRateInfo GetExchangeRateInfo(int rateId)
        {
            return ProviderObject.GetInfoById(rateId);
        }


        /// <summary>
        /// Sets (updates or inserts) specified exchange rate.
        /// </summary>
        /// <param name="rateObj">Exchange rate to be set</param>
        public static void SetExchangeRateInfo(ExchangeRateInfo rateObj)
        {
            ProviderObject.SetInfo(rateObj);
        }


        /// <summary>
        /// Deletes specified exchange rate.
        /// </summary>
        /// <param name="rateObj">Exchange rate to be deleted</param>
        public static void DeleteExchangeRateInfo(ExchangeRateInfo rateObj)
        {
            ProviderObject.DeleteInfo(rateObj);
        }


        /// <summary>
        /// Deletes exchange rate with specified ID.
        /// </summary>
        /// <param name="rateId">Exchange rate ID</param>
        public static void DeleteExchangeRateInfo(int rateId)
        {
            var rateObj = GetExchangeRateInfo(rateId);
            DeleteExchangeRateInfo(rateObj);
        }

        #endregion


        #region "Public methods - Advanced"

        /// <summary>
        /// Returns query with all exchange rates of the specified exchange table.
        /// </summary>
        /// <param name="tableId">Exchange table ID</param>        
        public static ObjectQuery<ExchangeRateInfo> GetExchangeRates(int tableId)
        {
            return ProviderObject.GetExchangeRatesInternal(tableId);
        }


        /// <summary>
        /// Returns exchange rate from specified exchange table which is applied to specified currency.
        /// </summary>
        /// <param name="currencyId">Currency ID</param>        
        /// <param name="tableId">Exchange table ID</param>  
        public static ExchangeRateInfo GetExchangeRateInfo(int currencyId, int tableId)
        {
            return ProviderObject.GetExchangeRateInfoInternal(currencyId, tableId);
        }


        /// <summary>
        /// Returns valid exchange rate between main currency of the specified site and currency specified by currencyID
        /// or last invalid exchange rate if no valid rate found. Throws an exception when currency or exchange rate not found.
        /// </summary>
        /// <param name="currencyId">ID of the currency to which we want exchange</param>
        /// <param name="siteId">ID of the site for which the exchange rate should be returned</param>
        /// <param name="fromGlobal">If set to true, exchange rate between global main currency and specified site currency is calculated and returned. Has effect only when converting to site currency</param>
        public static decimal GetLastExchangeRate(int currencyId, int siteId, bool fromGlobal = false)
        {
            return ProviderObject.GetLastExchangeRateInternal(currencyId, siteId, fromGlobal);
        }

        #endregion


        #region "Internal methods - Advanced"

        /// <summary>
        /// Returns query containing all exchange rates of the specified exchange table.
        /// </summary>
        /// <param name="tableId">Exchange table ID</param>        
        protected virtual ObjectQuery<ExchangeRateInfo> GetExchangeRatesInternal(int tableId)
        {
            return GetExchangeRates().Where("ExchangeTableID", QueryOperator.Equals, tableId);
        }


        /// <summary>
        /// Returns exchange rate from specified exchange table which is applied to specified currency.
        /// </summary>
        /// <param name="currencyId">Currency ID</param>        
        /// <param name="tableId">Exchange table ID</param>  
        protected virtual ExchangeRateInfo GetExchangeRateInfoInternal(int currencyId, int tableId)
        {
            return GetExchangeRates(tableId)
                       .Where("ExchangeRateToCurrencyID", QueryOperator.Equals, currencyId)
                       .FirstOrDefault();
        }


        /// <summary>
        /// Returns valid exchange rate between main currency of the specified site and currency specified by currencyID
        /// or last invalid exchange rate if no valid rate found. Throws an exception when currency or exchange rate not found.
        /// </summary>
        /// <param name="currencyId">ID of the currency to which we want exchange</param>
        /// <param name="siteId">ID of the site for which the exchange rate should be returned</param>
        /// <param name="fromGlobal">If set to true, exchange rate between global main currency and specified site currency is calculated and returned. Has effect only when converting to site currency</param>
        /// <exception cref="System.ArgumentException">Thrown when currency with given code does not exist in the system.</exception>
        /// <exception cref="CMS.Ecommerce.InvalidCurrencyConversionException">Thrown when trying to convert currencies among different sites.</exception>
        protected virtual decimal GetLastExchangeRateInternal(int currencyId, int siteId, bool fromGlobal)
        {
            // Get currency site ID
            var currency = CurrencyInfoProvider.GetCurrencyInfo(currencyId);
            if (currency == null)
            {
                throw new ArgumentException(string.Format("Currency for ID {0} not found.", currencyId), nameof(currencyId));
            }

            int currencySiteId = currency.CurrencySiteID;

            // Can not convert from site currency to currency from another site
            if ((currencySiteId != 0) && (siteId != 0) && (currencySiteId != siteId))
            {
                throw new InvalidCurrencyConversionException("[ExchangeRateInfoProvider.GetLastExchangeRateInternal]: It is not possible to convert from site currency to currency from another site.");
            }

            decimal rate = 1m;

            if (!currency.CurrencyIsMain)
            {
                // From site main currency to site currency OR 
                // From global main currency to global currency
                rate = GetLastExchangeRateForSite(currencyId, siteId);
            }

            // From global main currency to site currency
            if (fromGlobal && (currencySiteId > 0))
            {
                decimal mainRate = ExchangeTableInfoProvider.GetLastExchangeRateFromGlobalMainCurrency(siteId);

                if (currency.CurrencyIsMain)
                {
                    // From global main currency to site main currency
                    return mainRate;
                }

                // Global main currency to site currency
                return mainRate * rate;
            }

            return rate;
        }

        #endregion


        #region "Private methods"

        /// <summary>
        /// Returns cached valid exchange rate between main currency of the specified site and currency specified by ID
        /// or last invalid exchange rate if no valid rate found or 1 if no exchange rate found.
        /// </summary>
        /// <param name="currencyId">ID of the currency to which we want exchange</param>
        /// <param name="siteId">ID of the site for which the exchange rate should be returned</param>
        private decimal GetLastExchangeRateForSite(int currencyId, int siteId)
        {
            siteId = ECommerceHelper.GetSiteID(siteId, ECommerceSettings.USE_GLOBAL_EXCHANGE_RATES);

            var rate = CacheHelper.Cache(
                () => GetLastExchangeRateFromDB(currencyId, siteId),
                new CacheSettings(ECommerceSettings.ProvidersCacheMinutes, "ExchangeRateInfoProvider", "LastExchangeRateForSite", currencyId, siteId)
                {
                    GetCacheDependency = () => CacheHelper.GetCacheDependency(new[] {
                        ExchangeTableInfo.OBJECT_TYPE + "|all",
                        ExchangeRateInfo.OBJECT_TYPE + "|all",
                        CurrencyInfo.OBJECT_TYPE + "|all"
                    })
                });

            return rate;
        }


        /// <summary>
        /// Returns valid exchange rate between main currency of the specified site and currency specified by ID
        /// or last invalid exchange rate if no valid rate found. Throws an exception when exchange rate not found. Takes data from database.
        /// </summary>
        /// <param name="currencyId">ID of the currency to which we want exchange</param>
        /// <param name="siteId">ID of the site for which the exchange rate should be returned</param>
        /// <exception cref="CMS.Ecommerce.InvalidCurrencyConversionException">Thrown when no suitable exchange rate found.</exception>
        private decimal GetLastExchangeRateFromDB(int currencyId, int siteId)
        {
            decimal result = -1m;

            // Prepare the parameters
            var parameters = new QueryDataParameters();
            parameters.Add("@CurrencyID", currencyId);
            parameters.Add("@CurrentDate", DateTime.Now);
            parameters.Add("@SiteID", siteId);

            // Get last valid exchange rate
            var ds = ConnectionHelper.ExecuteQuery("ecommerce.exchangerate.selectlastvalidrate", parameters);
            if (DataHelper.DataSourceIsEmpty(ds))
            {
                // Get last invalid exchange rate
                ds = ConnectionHelper.ExecuteQuery("ecommerce.exchangerate.selectlastinvalidrate", parameters);
            }

            // Get the exchange rate from the data
            if (!DataHelper.DataSourceIsEmpty(ds))
            {
                result = ValidationHelper.GetDecimal(ds.Tables[0].Rows[0]["ExchangeRateValue"], -1m);
            }

            if(result < 0m)
            {
                throw new InvalidCurrencyConversionException(string.Format("No suitable exchange rate found for currency ID {0} on site with ID {1}", currencyId, siteId));
            }

            return result;
        }

        #endregion
    }
}