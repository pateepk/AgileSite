using System;
using System.Collections.Generic;
using System.Linq;

using CMS.DataEngine;
using CMS.Helpers;
using CMS.Base;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Class providing CurrencyInfo management.
    /// </summary>
    public class CurrencyInfoProvider : AbstractInfoProvider<CurrencyInfo, CurrencyInfoProvider>
    {
        #region "Constructors"

        /// <summary>
        /// Constructor.
        /// </summary>
        public CurrencyInfoProvider()
            : base(CurrencyInfo.TYPEINFO, new HashtableSettings
            {
                ID = true,
                Name = true,
                Load = LoadHashtableEnum.All
            })
        {
        }

        #endregion


        #region "Variables & Properties"

        /// <summary>
        /// Main currencies indexed by the site ID
        /// </summary>
        private static readonly CMSStatic<ProviderInfoDictionary<int>> mMainCurrencies = new CMSStatic<ProviderInfoDictionary<int>>();

        /// <summary>
        /// Table lock for loading.
        /// </summary>
        private static readonly object tableLock = new object();


        /// <summary>
        /// Main currencies indexed by the site ID
        /// </summary>
        private static ProviderInfoDictionary<int> MainCurrencies
        {
            get
            {
                return mMainCurrencies;
            }
            set
            {
                mMainCurrencies.Value = value;
            }
        }

        #endregion


        #region "Public methods - Basic"

        /// <summary>
        /// Returns the query for all currencies.
        /// </summary>
        public static ObjectQuery<CurrencyInfo> GetCurrencies()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns currency with specified ID.
        /// </summary>
        /// <param name="currencyId">Currency ID</param>        
        public static CurrencyInfo GetCurrencyInfo(int currencyId)
        {
            return ProviderObject.GetInfoById(currencyId);
        }


        /// <summary>
        /// Returns currency with specified name.
        /// </summary>
        /// <param name="currencyName">Currency name</param>                
        /// <param name="siteName">Site name</param>                
        public static CurrencyInfo GetCurrencyInfo(string currencyName, string siteName)
        {
            return ProviderObject.GetCurrencyInfoInternal(currencyName, siteName);
        }


        /// <summary>
        /// Sets (updates or inserts) specified currency.
        /// </summary>
        /// <param name="currencyObj">Currency to be set</param>
        public static void SetCurrencyInfo(CurrencyInfo currencyObj)
        {
            ProviderObject.SetInfo(currencyObj);
        }


        /// <summary>
        /// Deletes specified currency.
        /// </summary>
        /// <param name="currencyObj">Currency to be deleted</param>        
        public static void DeleteCurrencyInfo(CurrencyInfo currencyObj)
        {
            ProviderObject.DeleteInfo(currencyObj);
        }


        /// <summary>
        /// Deletes currency with specified ID.
        /// </summary>
        /// <param name="currencyId">Currency ID</param>
        public static void DeleteCurrencyInfo(int currencyId)
        {
            var currencyObj = GetCurrencyInfo(currencyId);
            DeleteCurrencyInfo(currencyObj);
        }

        #endregion


        #region "Public methods - Advanced"

        /// <summary>
        /// Returns dataset of all currencies matching the specified parameters.
        /// </summary>
        /// <param name="siteId">ID of the site the currencies should be retrieved from. If set to 0, global currencies are retrieved</param>
        /// <param name="onlyEnabled">If true, only enabled currencies are returned</param>
        public static ObjectQuery<CurrencyInfo> GetCurrencies(int siteId, bool onlyEnabled = false)
        {
            return ProviderObject.GetCurrenciesInternal(siteId, onlyEnabled);
        }


        /// <summary>
        /// Returns true if site specified by its ID has defined exchange rate to given currency. Only enabled currencies are searched.
        /// </summary>
        /// <param name="currencyId">ID of currency to be looked for</param>
        /// <param name="siteId">ID of the site to get exchange table for</param>
        public static bool IsCurrencyWithExchangeRate(int currencyId, int siteId)
        {
            return ProviderObject.IsCurrencyWithExchangeRateInternal(currencyId, siteId);
        }


        /// <summary>
        /// Returns main currency of the specified site.
        /// </summary>
        /// <param name="siteId">Site ID for site main currency or 0 for global main currency</param>
        public static CurrencyInfo GetMainCurrency(int siteId)
        {
            return ProviderObject.GetMainCurrencyInternal(siteId);
        }


        /// <summary>
        /// Returns code of main currency for given site. Returns empty string when not found.
        /// </summary>
        /// <param name="siteId">Site ID for site main currency or 0 for global main currency</param>
        public static string GetMainCurrencyCode(int siteId)
        {
            return ProviderObject.GetMainCurrencyCodeInternal(siteId);
        }


        /// <summary>
        /// Returns CurrencyInfo objects present on given site in form of dictionary mapping currency code to CurrencyInfo object.
        /// </summary>
        /// <param name="siteId">ID of the site to get currencies for. 'Use global currencies' setting is considered.</param>
        public static IDictionary<string, CurrencyInfo> GetCurrenciesByCode(int siteId)
        {
            return ProviderObject.GetCurrenciesByCodeInternal(siteId);
        }


        /// <summary>
        /// Returns HTML encoded (depends on <paramref name="encode"/> parameter) and relatively formatted price according to <see cref="CurrencyInfo.CurrencyFormatString"/> defined for <paramref name="currencyObj"/> parameter.
        /// </summary>
        /// <param name="price">Price to be formatted</param>
        /// <param name="currencyObj">Currency which supplies formatting string</param>
        /// <param name="encode">Encode output</param>
        public static string GetRelativelyFormattedPrice(decimal price, CurrencyInfo currencyObj, bool encode = true)
        {
            return ProviderObject.GetRelativelyFormattedPriceInternal(price, currencyObj, encode);
        }


        /// <summary>
        /// Returns HTML encoded (depends on <paramref name="encode"/> parameter) and relatively formatted price according to <see cref="CurrencyInfo.CurrencyFormatString"/> defined for specified site main currency.
        /// </summary>
        /// <param name="price">Price to be formatted</param>
        /// <param name="siteId">ID of the site which main currency has to be used for formatting</param>
        /// <param name="encode">Encode output</param>
        public static string GetRelativelyFormattedPrice(decimal price, int siteId, bool encode = true)
        {
            return ProviderObject.GetRelativelyFormattedPriceInternal(price, siteId, encode);
        }


        /// <summary>
        /// Returns HTML encoded (depends on <paramref name="encode"/> parameter) and formatted price according to <see cref="CurrencyInfo.CurrencyFormatString"/> defined for <paramref name="currencyObj"/> parameter. 
        /// </summary>
        /// <param name="price">Price to be formatted</param>
        /// <param name="currencyObj">Currency which supplies formatting string</param>
        /// <param name="encode">Encode output</param>
        public static string GetFormattedPrice(decimal price, CurrencyInfo currencyObj, bool encode = true)
        {
            return ProviderObject.GetFormattedPriceInternal(price, currencyObj, encode);
        }


        /// <summary>
        /// Returns HTML encoded (depends on <paramref name="encode"/> parameter) and formatted price according to <see cref="CurrencyInfo.CurrencyFormatString"/> defined for specified site main currency.
        /// </summary>
        /// <param name="price">Price to be formatted</param>
        /// <param name="siteId">ID of the site which main currency has to be used for formatting</param>
        /// <param name="encode">Encode output</param>
        public static string GetFormattedPrice(decimal price, int siteId, bool encode = true)
        {
            return ProviderObject.GetFormattedPriceInternal(price, siteId, encode);
        }


        /// <summary>
        /// Returns formatted price according to <see cref="CurrencyInfo.CurrencyRoundTo"/>. 
        /// </summary>
        /// <param name="price">Price to be formatted</param>
        /// <param name="currencyObj">
        /// Currency which can supply formatting string or 'round to' parameter - number of digits the price should be rounded to. 
        /// By default currency has no effect to the result and formatting string "{0:0.00}" is used.
        /// </param>
        public static string GetFormattedValue(decimal price, CurrencyInfo currencyObj)
        {
            return ProviderObject.GetFormattedValueInternal(price, currencyObj);
        }


        /// <summary>
        /// Changes main currency.
        /// </summary>
        /// <param name="siteId">ID of the site, which main currency is to be changed</param>
        /// <param name="newCurrencyId">ID of the new main currency</param>
        public static void ChangeMainCurrency(int siteId, int newCurrencyId)
        {
            ProviderObject.ChangeMainCurrencyInternal(siteId, newCurrencyId);
        }

        #endregion


        #region "Internal methods - Basic"

        /// <summary>
        /// Clear hashtables.
        /// </summary>
        /// <param name="logTasks">If true, web farm tasks are logged</param>
        protected override void ClearHashtables(bool logTasks)
        {
            base.ClearHashtables(logTasks);

            // Clear main currencies
            lock (tableLock)
            {
                MainCurrencies?.Clear(logTasks);
            }
        }


        /// <summary>
        /// Returns currency with specified name from specified site.
        /// </summary>
        /// <param name="currencyName">Currency name</param>                
        /// <param name="siteName">Site name. Use null for global currency</param>         
        protected virtual CurrencyInfo GetCurrencyInfoInternal(string currencyName, string siteName)
        {
            // Ensure site ID
            int siteId = ECommerceHelper.GetSiteID(siteName, ECommerceSettings.USE_GLOBAL_CURRENCIES);

            return GetInfoByCodeName(currencyName, siteId, true);
        }


        /// <summary>
        /// Inserts or Updates the object to the database.
        /// </summary>
        /// <param name="info">Object to insert / update</param>
        protected override void SetInfo(CurrencyInfo info)
        {
            LoadMainCurrencies();

            if (!CheckUniqueValues(info, "CurrencyCode", "CurrencySiteID"))
            {
                throw new Exception($"CurrencyInfoProvider.SetCurrencyInfoInternal: A different currency object with the same currency code ({info.CurrencyCode}) already exists.");
            }

            using (var tr = BeginTransaction())
            {
                // Check if updating/inserting main currency
                if (info.CurrencyIsMain)
                {
                    // Get all other currencies marked as main for updated site
                    var oldMains = GetCurrencies()
                                     .WhereTrue("CurrencyIsMain")
                                     .Where("CurrencyID", QueryOperator.NotEquals, info.CurrencyID)
                                     .OnSite(info.CurrencySiteID);

                    // Clear all other IsMain flags
                    foreach (var oldMain in oldMains)
                    {
                        oldMain.CurrencyIsMain = false;
                        SetInfo(oldMain);
                    }
                }

                // Set currency
                base.SetInfo(info);

                // Update site to main currency hashtable
                if (info.CurrencyIsMain)
                {
                    MainCurrencies?.Update(info.CurrencySiteID, info);
                }

                // Commit a transaction
                tr.Commit();
            }
        }


        /// <summary>
        /// Deletes the object to the database.
        /// </summary>
        /// <param name="info">Object to delete</param>
        protected override void DeleteInfo(CurrencyInfo info)
        {
            LoadMainCurrencies();

            // Delete currency
            base.DeleteInfo(info);

            // Delete main currency            
            if (info.CurrencyIsMain)
            {
                MainCurrencies?.Delete(info.CurrencySiteID);
            }
        }

        #endregion


        #region "Internal methods - Advanced"

        /// <summary>
        /// Returns dataset of all currencies matching the specified parameters.
        /// </summary>
        /// <param name="siteId">ID of the site the currencies should be retrieved from. If set to 0, global currencies are retrieved</param>
        /// <param name="onlyEnabled">True - only enabled currencies from the specified site are returned. False - all site currencies are returned</param>
        protected virtual ObjectQuery<CurrencyInfo> GetCurrenciesInternal(int siteId, bool onlyEnabled)
        {
            // Check if site uses site or global currencies
            siteId = ECommerceHelper.GetSiteID(siteId, ECommerceSettings.USE_GLOBAL_CURRENCIES);

            // Get currencies on requested site
            var query = GetCurrencies().OnSite(siteId);

            if (onlyEnabled)
            {
                query.WhereTrue("CurrencyEnabled");
            }

            return query;
        }


        /// <summary>
        /// Returns true if site specified by its ID has defined exchange rate to given currency. Only enabled currencies are searched.
        /// </summary>
        /// <param name="currencyId">ID of currency to be looked for</param>
        /// <param name="siteId">ID of the site to get exchange table for</param>
        protected virtual bool IsCurrencyWithExchangeRateInternal(int currencyId, int siteId)
        {
            var currency = GetCurrencyInfo(currencyId);

            if ((currency == null) || !currency.CurrencyEnabled)
            {
                return false;
            }

            decimal rate = 1;
            return CurrencyConverter.TryGetExchangeRate(currency.IsGlobal, currency.CurrencyCode, siteId, ref rate);
        }


        /// <summary>
        /// Returns main currency of specified site.
        /// </summary>
        /// <param name="siteId">Site ID for site main currency or 0 for global main currency</param>
        protected virtual CurrencyInfo GetMainCurrencyInternal(int siteId)
        {
            LoadMainCurrencies();

            CurrencyInfo result;
            BaseInfo existing;

            // Get correct site ID
            siteId = ECommerceHelper.GetSiteID(siteId, ECommerceSettings.USE_GLOBAL_CURRENCIES);

            if (MainCurrencies.TryGetValue(siteId, out existing))
            {
                // Get main currency from hashtable
                result = (CurrencyInfo)existing;
            }
            else
            {
                // Filter site main currency
                var siteWhere = siteId != ProviderHelper.ALL_SITES
                                    ? TypeInfo.GetSiteWhereCondition(siteId, false) 
                                    : new WhereCondition();

                // Get main currency from DB
                result = ProviderObject.GetObjectQuery().TopN(1).WhereTrue("CurrencyIsMain").Where(siteWhere).FirstOrDefault();

                MainCurrencies[siteId] = result;
            }

            return result;
        }


        /// <summary>
        /// Returns main currency code for given site or empty string when not found.
        /// </summary>
        /// <param name="siteId">Site ID for site main currency or 0 for global main currency code</param>
        protected virtual string GetMainCurrencyCodeInternal(int siteId)
        {
            // Get main currency
            var currency = GetMainCurrency(siteId);

            // Get code or empty string
            return (currency != null) ? currency.CurrencyCode : "";
        }


        /// <summary>
        /// Returns CurrencyInfo objects present on given site in form of dictionary mapping currency code to CurrencyInfo object.
        /// </summary>
        /// <param name="siteId">ID of the site to get currencies for. 'Use global currencies' setting is considered.</param>
        protected virtual IDictionary<string, CurrencyInfo> GetCurrenciesByCodeInternal(int siteId)
        {
            var currencies = CacheHelper.Cache(
                () => GetCurrencies(siteId).ToDictionary(currency => currency.CurrencyCode),
                new CacheSettings(ECommerceSettings.ProvidersCacheMinutes, "CurrencyInfoProvider", "CurrenciesByCodeForSite", siteId)
                {
                    GetCacheDependency = () => CacheHelper.GetCacheDependency(new[] { CurrencyInfo.OBJECT_TYPE + "|all" })
                });

            return currencies;
        }


        /// <summary>
        /// Returns relatively formatted price according to formatting string of the specified site main currency.
        /// </summary>
        /// <param name="price">Price to be formatted</param>
        /// <param name="siteId">Id of the site which main currency has to be used for formatting</param>
        /// <param name="encode">Encode output</param>
        protected virtual string GetRelativelyFormattedPriceInternal(decimal price, int siteId, bool encode)
        {
            // Get site main currency
            CurrencyInfo currency = GetMainCurrency(siteId);

            // Apply main currency formatting string
            return GetRelativelyFormattedPriceInternal(price, currency, encode);
        }


        /// <summary>
        /// Returns relatively formatted price according to formatting string of the specified currency.
        /// </summary>
        /// <param name="price">Price to be formatted</param>
        /// <param name="currencyObj">Currency which supplies formatting string</param>
        /// <param name="encode">Encode output</param>
        protected virtual string GetRelativelyFormattedPriceInternal(decimal price, CurrencyInfo currencyObj, bool encode)
        {
            if (!String.IsNullOrEmpty(currencyObj?.CurrencyFormatString))
            {
                string prefix = (price >= 0) ? "+ " : "- ";
                price = Math.Abs(price);

                // Add prefix to currency formatting string
                string fullPrice = prefix + string.Format(currencyObj.CurrencyFormatString, price);
                return encode ? HTMLHelper.HTMLEncode(fullPrice) : fullPrice;
            }

            // Do not format the price
            return Convert.ToString(price);
        }


        /// <summary>
        /// Returns formatted price according to formatting string of the specified site main currency.
        /// </summary>
        /// <param name="price">Price to be formatted</param>
        /// <param name="siteId">Id of the site which main currency has to be used for formatting</param>
        /// <param name="encode">Encode output</param>
        protected virtual string GetFormattedPriceInternal(decimal price, int siteId, bool encode)
        {
            // Get site main currency
            CurrencyInfo currency = GetMainCurrency(siteId);

            // Apply main currency formatting string
            return GetFormattedPriceInternal(price, currency, encode);
        }


        /// <summary>
        /// Returns formatted price according to formatting string of the specified currency.
        /// </summary>
        /// <param name="price">Price to be formatted</param>
        /// <param name="currencyObj">Currency which supplies formatting string</param>
        /// <param name="encode">Encode output</param>
        protected virtual string GetFormattedPriceInternal(decimal price, CurrencyInfo currencyObj, bool encode)
        {
            if (!String.IsNullOrEmpty(currencyObj?.CurrencyFormatString))
            {
                // Apply currency formatting string
                string text = string.Format(currencyObj.CurrencyFormatString, price);

                // Remove scripts
                return encode ? HTMLHelper.HTMLEncode(text) : text;
            }

            // Do not format the price
            return Convert.ToString(price);
        }


        /// <summary>
        /// Returns formatted price, by default formatting string "{0:0.00}" is used.
        /// </summary>
        /// <param name="price">Price to be formatted</param>
        /// <param name="currencyObj">
        /// Currency which can supply formatting string or 'round to' parameter - number of digits the price should be rounded to. 
        /// By default currency has no effect to the result and formatting string "{0:0.00}" is used.
        /// </param>
        protected virtual string GetFormattedValueInternal(decimal price, CurrencyInfo currencyObj)
        {
            if (currencyObj != null && currencyObj.CurrencyRoundTo > 0)
            {
                string format = $"{{0:F{currencyObj.CurrencyRoundTo}}}";
                return string.Format(format, price);
            }

            return $"{price:0.00}";
        }


        /// <summary>
        /// Changes main currency.
        /// </summary>
        /// <param name="siteId">ID of the site, which main currency is to be changed</param>
        /// <param name="newCurrencyId">ID of the new main currency</param>
        protected virtual void ChangeMainCurrencyInternal(int siteId, int newCurrencyId)
        {
            // Get currencies
            var oldMainCurrency = GetMainCurrency(siteId);
            var newMainCurrency = GetCurrencyInfo(newCurrencyId);

            // Test some assumptions
            if (oldMainCurrency == null)
            {
                throw new Exception("CurrencyInfoProvider.ChangeMainCurrencyInternal: Old main currency was not found.");
            }

            if (newMainCurrency == null)
            {
                throw new Exception("CurrencyInfoProvider.ChangeMainCurrencyInternal: New main currency was not found.");
            }

            if (oldMainCurrency.CurrencySiteID != newMainCurrency.CurrencySiteID)
            {
                throw new Exception("CurrencyInfoProvider.ChangeMainCurrencyInternal: New main currency site ID must be the same as old one's.");
            }

            using (var tr = BeginTransaction())
            {
                // Toggle 'is main' flag
                oldMainCurrency.CurrencyIsMain = false;
                newMainCurrency.CurrencyIsMain = true;

                // Save new flags
                oldMainCurrency.Update();
                newMainCurrency.Update();

                tr.Commit();
            }
        }

        #endregion


        #region "Private methods"

        /// <summary>
        /// Loads all main currencies to hashtable.
        /// </summary>
        private static void LoadMainCurrencies()
        {
            if (ProviderHelper.LoadTables(MainCurrencies))
            {
                lock (tableLock)
                {
                    if (ProviderHelper.LoadTables(MainCurrencies))
                    {
                        // Prepare the table
                        var tempCurrencies = new ProviderInfoDictionary<int>(CurrencyInfo.OBJECT_TYPE, "CurrencySiteID");

                        // Load the data
                        if (ProviderHelper.LoadHashTables(CurrencyInfo.OBJECT_TYPE, LoadHashtableEnum.All) != LoadHashtableEnum.None)
                        {
                            var mainCurrencies = GetCurrencies().WhereTrue("CurrencyIsMain");

                            // Add currencies to hashtable
                            foreach (var currency in mainCurrencies)
                            {
                                tempCurrencies[currency.CurrencySiteID] = currency;
                            }
                        }

                        MainCurrencies = tempCurrencies;
                    }
                }
            }
        }

        #endregion
    }
}