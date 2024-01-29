using System;

using CMS.Base;
using CMS.Ecommerce;
using CMS.SiteProvider;

namespace APIExamples
{
    /// <summary>
    /// Holds currency and exchange rate API examples.
    /// </summary>
    /// <pageTitle>Currencies and exchange rates</pageTitle>
    internal class CurrenciesAndExchangeRates
    {
        /// <summary>
        /// Holds currency API examples.
        /// </summary>
        /// <groupHeading>Currencies</groupHeading>
        private class Currencies
        {
            /// <heading>Creating a new currency</heading>
            private void CreateCurrency()
            {
                // Creates a new currency object
                CurrencyInfo newCurrency = new CurrencyInfo();

                // Sets the currency properties
                newCurrency.CurrencyDisplayName = "New currency";
                newCurrency.CurrencyName = "NewCurrency";
                newCurrency.CurrencyCode = "NC";
                newCurrency.CurrencySiteID = SiteContext.CurrentSiteID;
                newCurrency.CurrencyEnabled = true;
                newCurrency.CurrencyFormatString = "{0:F} NC";
                newCurrency.CurrencyIsMain = false;

                // Saves the currency to the database
                CurrencyInfoProvider.SetCurrencyInfo(newCurrency);
            }


            /// <heading>Updating a currency</heading>
            private void GetAndUpdateCurrency()
            {
                // Gets the currency
                CurrencyInfo updateCurrency = CurrencyInfoProvider.GetCurrencyInfo("NewCurrency", SiteContext.CurrentSiteName);
                if (updateCurrency != null)
                {
                    // Updates the currency properties
                    updateCurrency.CurrencyDisplayName = updateCurrency.CurrencyDisplayName.ToLowerCSafe();

                    // Saves the changes to the database
                    CurrencyInfoProvider.SetCurrencyInfo(updateCurrency);
                }
            }


            /// <heading>Updating multiple currencies</heading>
            private void GetAndBulkUpdateCurrencies()
            {
                // Gets all currencies whose code name starts with 'NewCurrency'
                var currencies = CurrencyInfoProvider.GetCurrencies().WhereStartsWith("CurrencyName", "NewCurrency");

                // Loops through the currency objects
                foreach (CurrencyInfo modifyCurrency in currencies)
                {
                    // Updates the currency properties
                    modifyCurrency.CurrencyDisplayName = modifyCurrency.CurrencyDisplayName.ToUpperCSafe();

                    // Saves the changes to the database
                    CurrencyInfoProvider.SetCurrencyInfo(modifyCurrency);
                }
            }


            /// <heading>Deleting a currency</heading>
            private void DeleteCurrency()
            {
                // Gets the currency
                CurrencyInfo deleteCurrency = CurrencyInfoProvider.GetCurrencyInfo("NewCurrency", SiteContext.CurrentSiteName);
                if (deleteCurrency != null)
                {
                    // Deletes the currency
                    CurrencyInfoProvider.DeleteCurrencyInfo(deleteCurrency);
                }
            }
        }

        /// <summary>
        /// Holds exchange table API examples.
        /// </summary>
        /// <groupHeading>Exchange tables</groupHeading>
        private class ExchangeTables
        {
            /// <heading>Creating an exchange table</heading>
            private void CreateExchangeTable()
            {
                // Creates a new exchange table object
                ExchangeTableInfo newTable = new ExchangeTableInfo();

                // Sets the exchange table properties
                newTable.ExchangeTableDisplayName = "New table";
                newTable.ExchangeTableSiteID = SiteContext.CurrentSiteID;
                newTable.ExchangeTableValidFrom = DateTime.Now;
                newTable.ExchangeTableValidTo = DateTime.Now;

                // Saves the exchange table to the database
                ExchangeTableInfoProvider.SetExchangeTableInfo(newTable);
            }


            /// <heading>Updating an exchange table</heading>
            private void GetAndUpdateExchangeTable()
            {
                // Gets the exchange table
                ExchangeTableInfo updateTable = ExchangeTableInfoProvider.GetExchangeTableInfo("New table", SiteContext.CurrentSiteName);
                if (updateTable != null)
                {
                    // Sets the desired time
                    TimeSpan time = TimeSpan.FromDays(7);

                    // Updates the time validity properties
                    updateTable.ExchangeTableValidFrom = updateTable.ExchangeTableValidFrom.Add(time);
                    updateTable.ExchangeTableValidTo = updateTable.ExchangeTableValidTo.Add(time);

                    // Saves the changes to the database
                    ExchangeTableInfoProvider.SetExchangeTableInfo(updateTable);
                }
            }


            /// <heading>Updating multiple exchange tables</heading>
            private void GetAndBulkUpdateExchangeTables()
            {
                // Gets all exchange tables assigned to the current site whose display name starts with 'New table'
                var tables = ExchangeTableInfoProvider.GetExchangeTables()
                                                        .OnSite(SiteContext.CurrentSiteID)
                                                        .WhereStartsWith("ExchangeTableDisplayName", "New table");

                // Prepares a 14 day time span for the exchange table validity
                TimeSpan time = TimeSpan.FromDays(14);

                // Loops through the exchange tables
                foreach (ExchangeTableInfo modifyTable in tables)
                {
                    // Updates the time validity properties
                    modifyTable.ExchangeTableValidFrom = modifyTable.ExchangeTableValidFrom.Add(time);
                    modifyTable.ExchangeTableValidTo = modifyTable.ExchangeTableValidTo.Add(time);

                    // Saves the changes to the database
                    ExchangeTableInfoProvider.SetExchangeTableInfo(modifyTable);
                }
            }


            /// <heading>Deleting an exchange table</heading>
            private void DeleteExchangeTable()
            {
                // Gets the exchange table
                ExchangeTableInfo deleteTable = ExchangeTableInfoProvider.GetExchangeTableInfo("New table", SiteContext.CurrentSiteName);

                if (deleteTable != null)
                {
                    // Deletes the exchange table
                    ExchangeTableInfoProvider.DeleteExchangeTableInfo(deleteTable);
                }
            }
        }
    }
}
