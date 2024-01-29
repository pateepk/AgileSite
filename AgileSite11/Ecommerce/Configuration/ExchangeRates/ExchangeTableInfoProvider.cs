using System;
using System.Linq;

using CMS.DataEngine;
using CMS.Helpers;
using CMS.SiteProvider;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Class providing ExchangeTableInfo management.
    /// </summary>
    public class ExchangeTableInfoProvider : AbstractInfoProvider<ExchangeTableInfo, ExchangeTableInfoProvider>
    {
        #region "Constructors"

        /// <summary>
        /// Constructor.
        /// </summary>
        public ExchangeTableInfoProvider()
            : base(ExchangeTableInfo.TYPEINFO, new HashtableSettings
                {
                    ID = true
                })
        {
        }

        #endregion


        #region "Public methods - Basic"

        /// <summary>
        /// Returns the query for all exchange tables.
        /// </summary>
        public static ObjectQuery<ExchangeTableInfo> GetExchangeTables()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns exchange table with specified ID.
        /// </summary>
        /// <param name="tableId">Exchange table ID</param>        
        public static ExchangeTableInfo GetExchangeTableInfo(int tableId)
        {
            return ProviderObject.GetInfoById(tableId);
        }


        /// <summary>
        /// Sets (updates or inserts) specified exchange table.
        /// </summary>
        /// <param name="tableObj">Exchange table to be set</param>
        public static void SetExchangeTableInfo(ExchangeTableInfo tableObj)
        {
            ProviderObject.SetInfo(tableObj);
        }


        /// <summary>
        /// Deletes specified exchange table.
        /// </summary>
        /// <param name="tableObj">Exchange table to be deleted</param>
        public static void DeleteExchangeTableInfo(ExchangeTableInfo tableObj)
        {
            ProviderObject.DeleteInfo(tableObj);
        }


        /// <summary>
        /// Deletes exchange table with specified ID.
        /// </summary>
        /// <param name="tableId">Exchange table ID</param>
        public static void DeleteExchangeTableInfo(int tableId)
        {
            var tableObj = GetExchangeTableInfo(tableId);
            DeleteExchangeTableInfo(tableObj);
        }

        #endregion


        #region "Public methods - Advanced"

        /// <summary>
        /// Returns dataset of all exchange tables for specified site.
        /// </summary>
        /// <param name="siteId">Site ID</param>        
        public static ObjectQuery<ExchangeTableInfo> GetExchangeTables(int siteId)
        {
            return ProviderObject.GetExchangeTablesInternal(siteId);
        }


        /// <summary>
        /// Returns exchange table with specified display name.
        /// </summary>
        /// <param name="tableName">Exchange table display name</param>        
        /// <param name="siteName">Exchange table site name</param>        
        public static ExchangeTableInfo GetExchangeTableInfo(string tableName, string siteName)
        {
            return ProviderObject.GetExchangeTableInfoInternal(tableName, siteName);
        }


        /// <summary>
        /// For specified site returns last valid exchange table or last invalid exchange table if no valid table found or 
        /// null if no exchange table found.
        /// </summary>
        /// <param name="siteId">Site ID</param>
        public static ExchangeTableInfo GetLastExchangeTableInfo(int siteId)
        {
            return ProviderObject.GetLastExchangeTableInfoInternal(siteId);
        }


        /// <summary>
        /// For specified site returns last valid exchange table or null if no valid exchange table found.
        /// </summary>
        /// <param name="siteId">Site ID</param>
        public static ExchangeTableInfo GetLastValidExchangeTableInfo(int siteId)
        {
            return ProviderObject.GetLastValidExchangeTableInfoInternal(siteId);
        }


        /// <summary>
        /// Returns valid exchange rate from global main currency to the currency of the specified site
        /// or last invalid exchange rate if no valid rate found. Throws an exception when no rate found.
        /// If site uses global currencies or global currency code equals site currency code, 1 is returned automatically.
        /// </summary>
        /// <param name="siteId">ID of the site for which the exchange rate should be returned</param>  
        public static decimal GetLastExchangeRateFromGlobalMainCurrency(int siteId)
        {
            return ProviderObject.GetLastExchangeRateFromGlobalMainCurrencyInternal(siteId);
        }

        #endregion


        #region "Internal methods - Advanced"

        /// <summary>
        /// Returns dataset of all exchange tables for specified site.
        /// </summary>
        /// <param name="siteId">Site ID</param>        
        protected virtual ObjectQuery<ExchangeTableInfo> GetExchangeTablesInternal(int siteId)
        {
            siteId = ECommerceHelper.GetSiteID(siteId, ECommerceSettings.USE_GLOBAL_EXCHANGE_RATES);

            return GetExchangeTables().OnSite(siteId);
        }


        /// <summary>
        /// Returns exchange table with specified display name.
        /// </summary>
        /// <param name="tableName">Exchange table display name</param>        
        /// <param name="siteName">Exchange table site name</param>        
        protected virtual ExchangeTableInfo GetExchangeTableInfoInternal(string tableName, string siteName)
        {
            var siteId = SiteInfoProvider.GetSiteID(siteName);

            return GetExchangeTables(siteId)
                       .WhereEquals("ExchangeTableDisplayName", tableName)
                       .OrderByDescending("ExchangeTableSiteID")
                       .FirstOrDefault();
        }


        /// <summary>
        /// For specified site returns last valid exchange table or last invalid exchange table if no valid table found or 
        /// null if no exchange table found.
        /// </summary>
        /// <param name="siteId">Site ID</param>
        protected virtual ExchangeTableInfo GetLastExchangeTableInfoInternal(int siteId)
        {
            // Get last valid exchange table
            var et = GetLastValidExchangeTableInfoInternal(siteId);

            if (et == null)
            {
                return GetExchangeTables(siteId)
                         .TopN(1)
                         .Where("ExchangeTableValidTo", QueryOperator.LessThan, DateTime.Now)
                         .OrderByDescending("ExchangeTableValidTo")
                         .FirstOrDefault();
            }
            return et;
        }


        /// <summary>
        /// For specified site returns last valid exchange table or null if no valid exchange table found.
        /// </summary>
        /// <param name="siteId">Site ID</param>
        protected virtual ExchangeTableInfo GetLastValidExchangeTableInfoInternal(int siteId)
        {
            // Get correct site ID            
            siteId = ECommerceHelper.GetSiteID(siteId, ECommerceSettings.USE_GLOBAL_EXCHANGE_RATES);

            // Try get table from cache
            var table = CacheHelper.Cache(
                cacheSettings => GetLastValidTableInfo(siteId, cacheSettings),
                new CacheSettings(ECommerceSettings.ProvidersCacheMinutes, "ExchangeTableInfoProvider", "LastValidExchangeTableInfo", siteId)
                {
                    GetCacheDependency = () => CacheHelper.GetCacheDependency(new[] {
                        CurrencyInfo.OBJECT_TYPE + "|all",
                        ExchangeTableInfo.OBJECT_TYPE + "|all"
                    })
                });

            return table;
        }


        /// <summary>
        /// Returns valid exchange rate from global main currency to the currency of the specified site
        /// or last invalid exchange rate if no valid rate found. Throws an exception when no rate found.
        /// If site uses global currencies or global currency code equals site currency code, 1 is returned automatically.
        /// </summary>
        /// <param name="siteId">ID of the site for which the exchange rate should be returned</param>
        /// <exception cref="CMS.Ecommerce.InvalidCurrencyConversionException">Thrown when exchange rate from global main currency is not specified.</exception>
        protected virtual decimal GetLastExchangeRateFromGlobalMainCurrencyInternal(int siteId)
        {
            // Site main currency code is different from global main currency code
            if (CurrencyInfoProvider.GetMainCurrencyCode(0) == CurrencyInfoProvider.GetMainCurrencyCode(siteId))
            {
                return 1m;
            }

            // Site is already using global currencies
            if (ECommerceSettings.UseGlobalCurrencies(siteId))
            {
                return 1m;
            }

            // Get last valid exchange table
            decimal rate = -1m;
            var exTable = GetLastExchangeTableInfoInternal(siteId);
            if (exTable != null)
            {
                // From global main currency to site main currency
                rate = exTable.ExchangeTableRateFromGlobalCurrency;
            }

            if (rate <= 0m)
            {
                throw new InvalidCurrencyConversionException(string.Format("No suitable exchange rate from global main currency to site main currency found for site ID {0}", siteId));
            }

            return rate;
        }

        #endregion


        #region "Private methods"

        /// <summary>
        /// Returns last valid exchange table from database.
        /// </summary>
        /// <param name="siteId">Site ID</param>
        /// <param name="cacheSettings">Cache settings</param>
        private ExchangeTableInfo GetLastValidTableInfo(int siteId, CacheSettings cacheSettings)
        {
            // Prepare the parameters
            var parameters = new QueryDataParameters();
            parameters.Add("@CurrentDate", DateTime.Now);
            parameters.Add("@SiteID", siteId);

            // Get valid table
            var ds = ConnectionHelper.ExecuteQuery("ecommerce.exchangetable.SelectLastValid", parameters);
            if (!DataHelper.DataSourceIsEmpty(ds))
            {
                var table = CreateInfo(ds.Tables[0].Rows[0]);

                // Set cache expiration if exchange table expires before default cache expiration.
                if ((table != null) && (table.ExchangeTableValidTo != DateTimeHelper.ZERO_TIME))
                {
                    var minutes = table.ExchangeTableValidTo.Subtract(DateTime.Now).TotalMinutes;
                    cacheSettings.CacheMinutes = Math.Min(cacheSettings.CacheMinutes, Math.Max(minutes, 0));
                }

                return table;
            }

            return null;
        }

        #endregion
    }
}