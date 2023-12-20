using CMS.DataEngine;
using CMS.WebAnalytics;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Provides handlers for Ecommerce and conversion interaction.
    /// </summary>
    internal static class ConversionHandlers
    {
        /// <summary>
        /// Initializes the conversion handlers.
        /// </summary>
        public static void Init()
        {
            ConversionInfo.TYPEINFO.Events.Delete.After += DeleteConversionFromSKU;
        }


        /// <summary>
        /// Removes conversion information from SKU.
        /// </summary>
        private static void DeleteConversionFromSKU(object sender, ObjectEventArgs e)
        {
            var conversion = (ConversionInfo)e.Object;
            var skus = GetSKUsWithConversion(conversion);
            RemoveConversionDataFromSKUs(skus);
        }


        private static ObjectQuery<SKUInfo> GetSKUsWithConversion(ConversionInfo conversion)
        {
            return SKUInfoProvider.GetSKUs(conversion.ConversionSiteID)
                                  .WhereEquals("SKUConversionName", conversion.ConversionName);
        }


        private static void RemoveConversionDataFromSKUs(ObjectQuery<SKUInfo> SKUs)
        {
            foreach (var sku in SKUs)
            {
                sku.SKUConversionName = null;
                sku.SKUConversionValue = null;

                SKUInfoProvider.SetSKUInfo(sku);
            }
        }
    }
}