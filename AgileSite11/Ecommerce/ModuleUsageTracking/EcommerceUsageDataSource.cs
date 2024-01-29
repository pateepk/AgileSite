using System;
using System.Linq;
using System.Text;

using CMS.Base;
using CMS.Core;
using CMS.DataEngine;
using CMS.DocumentEngine;
using CMS.Ecommerce.ModuleUsageTracking;

[assembly: RegisterModuleUsageDataSource(typeof(EcommerceUsageDataSource))]

namespace CMS.Ecommerce.ModuleUsageTracking
{
    internal class EcommerceUsageDataSource : IModuleUsageDataSource
    {
        /// <summary>
        /// Get the data source name.
        /// </summary>
        public string Name
        {
            get
            {
                return ModuleName.ECOMMERCE;
            }
        }


        /// <summary>
        /// Get Ecommerce module statistical data.
        /// </summary>
        public IModuleUsageDataCollection GetData()
        {
            var result = ObjectFactory<IModuleUsageDataCollection>.New();

            result.Add("ParentProductsCount", GetParentProductsCount());
            result.Add("VariantsCount", GetVariantsCount());
            result.Add("SitesWithOrdersCount", GetSitesWithOrdersCount());
            result.Add("ProductDocumentsCount", GetProductDocumentsCount());
            result.Add("ExchangeRatesFromGlobalMainCurrencyCount", GetExRateFromGlobalMainCurrencyCount());

            return result;
        }


        /// <summary>
        /// Returns the number of products (variants and product options excluded).
        /// </summary>
        private int GetParentProductsCount()
        {
            return SKUInfoProvider.GetSKUs()
                                  .WhereNull("SKUOptionCategoryID")
                                  .WhereNull("SKUParentSKUID")
                                  .Count;
        }


        /// <summary>
        /// Returns the number of variants (options excluded).
        /// </summary>
        private int GetVariantsCount()
        {
            return SKUInfoProvider.GetSKUs()
                                  .WhereNull("SKUOptionCategoryID")
                                  .WhereNotNull("SKUParentSKUID")
                                  .Count;
        }


        /// <summary>
        /// Returns the number of sites having at least one order.
        /// </summary>
        private int GetSitesWithOrdersCount()
        {
            return OrderInfoProvider.GetOrders()
                                    .Distinct()
                                    .Columns("OrderSiteID")
                                    .Count();
        }


        /// <summary>
        /// Returns the number of product documents (nodes bound to SKU).
        /// </summary>
        private int GetProductDocumentsCount()
        {
            return DocumentHelper.GetDocuments()
                                 .Columns("NodeID")
                                 .WhereNotNull("NodeSKUID")
                                 .Count;
        }


        /// <summary>
        /// Returns the number of exchange tables with exchange rate from global main currency filled.
        /// </summary>
        /// <returns></returns>
        private int GetExRateFromGlobalMainCurrencyCount()
        {
            return ExchangeTableInfoProvider.GetExchangeTables()
                                            .WhereNotNull("ExchangeTableRateFromGlobalCurrency")
                                            .Where("ExchangeTableRateFromGlobalCurrency", QueryOperator.GreaterThan, 0)
                                            .Count;
        }
    }
}
