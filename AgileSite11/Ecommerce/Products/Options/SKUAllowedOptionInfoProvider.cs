using System;
using System.Linq;

using CMS.DataEngine;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Class providing SKUAllowedOptionInfo management.
    /// </summary>
    public class SKUAllowedOptionInfoProvider : AbstractInfoProvider<SKUAllowedOptionInfo, SKUAllowedOptionInfoProvider>
    {
        #region "Public methods - Basic"

        /// <summary>
        /// Returns the query for all relationships between products and options.
        /// </summary>        
        public static ObjectQuery<SKUAllowedOptionInfo> GetSKUOptions()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns relationship between specified product and option.
        /// </summary>
        /// <param name="productId">Product ID (SKUID).</param>
        /// <param name="optionId">Product option ID (SKUID).</param>
        public static SKUAllowedOptionInfo GetSKUOptionInfo(int productId, int optionId)
        {
            return ProviderObject.GetSKUOptionInfoInternal(productId, optionId);
        }


        /// <summary>
        /// Sets relationship between specified product and option.
        /// </summary>
        /// <param name="infoObj">Product-option relationship to be set.</param>
        public static void SetSKUOptionInfo(SKUAllowedOptionInfo infoObj)
        {
            ProviderObject.SetInfo(infoObj);
        }


        /// <summary>
        /// Deletes relationship between specified product and option.
        /// </summary>
        /// <param name="infoObj">Product-option relationship to be deleted.</param>
        public static void DeleteSKUOptionInfo(SKUAllowedOptionInfo infoObj)
        {
            ProviderObject.DeleteInfo(infoObj);
        }


        /// <summary>
        /// Sets relationship between specified product and option.
        /// </summary>	
        /// <param name="productId">Product ID (SKUID).</param>
        /// <param name="optionId">Product option ID (SKUID).</param>
        public static void AddOptionToProduct(int productId, int optionId)
        {
            var infoObj = ProviderObject.CreateInfo();

            infoObj.OptionSKUID = optionId;
            infoObj.SKUID = productId;

            SetSKUOptionInfo(infoObj);
        }


        /// <summary>
        /// Deletes relationship between specified product and option.
        /// </summary>
        /// <param name="productId">Product ID (SKUID).</param>
        /// <param name="optionId">Product option ID (SKUID).</param>
        public static void RemoveOptionFromProduct(int productId, int optionId)
        {
            var infoObj = GetSKUOptionInfo(productId, optionId);
            DeleteSKUOptionInfo(infoObj);
        }

        #endregion


        #region "Internal methods - Basic"

        /// <summary>
        /// Returns relationship between specified product and option.
        /// </summary>
        /// <param name="productId">Product ID (SKUID).</param>
        /// <param name="optionId">Product option ID (SKUID).</param>
        protected virtual SKUAllowedOptionInfo GetSKUOptionInfoInternal(int productId, int optionId)
        {
            return GetObjectQuery().TopN(1)
                       .WhereEquals("SKUID", productId)
                       .WhereEquals("OptionSKUID", optionId)
                       .FirstOrDefault();
        }

        #endregion
    }
}
