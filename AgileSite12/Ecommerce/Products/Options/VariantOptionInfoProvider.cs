using System;
using System.Linq;

using CMS.DataEngine;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Class providing VariantOptionInfo management.
    /// </summary>
    public class VariantOptionInfoProvider : AbstractInfoProvider<VariantOptionInfo, VariantOptionInfoProvider>
    {
        #region "Public methods - Basic"

        /// <summary>
        /// Returns the query for all relationships between product variants and options.
        /// </summary>        
        public static ObjectQuery<VariantOptionInfo> GetVariantOptions()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns relationship between specified product variant and option.
        /// </summary>
        /// <param name="variantId">Product variant ID (SKUID).</param>
        /// <param name="optionId">Product option ID (SKUID).</param>
        public static VariantOptionInfo GetVariantOptionInfo(int variantId, int optionId)
        {
            return ProviderObject.GetVariantOptionInfoInternal(variantId, optionId);
        }


        /// <summary>
        /// Sets relationship between specified product variant and option.
        /// </summary>
        /// <param name="infoObj">Product-option relationship to be set.</param>
        public static void SetVariantOptionInfo(VariantOptionInfo infoObj)
        {
            ProviderObject.SetInfo(infoObj);
        }


        /// <summary>
        /// Deletes relationship between specified product variant and option.
        /// </summary>
        /// <param name="infoObj">Product-option relationship to be deleted.</param>
        public static void DeleteVariantOptionInfo(VariantOptionInfo infoObj)
        {
            ProviderObject.DeleteInfo(infoObj);
        }


        /// <summary>
        /// Sets relationship between specified product variant and option.
        /// </summary>	
        /// <param name="variantId">Product variant ID (SKUID).</param>
        /// <param name="optionId">Product option ID (SKUID).</param>
        public static void AddOptionToVariant(int variantId, int optionId)
        {
            var infoObj = ProviderObject.CreateInfo();

            infoObj.VariantSKUID = variantId;
            infoObj.OptionSKUID = optionId;

            SetVariantOptionInfo(infoObj);
        }


        /// <summary>
        /// Deletes relationship between specified product variant and option.
        /// </summary>
        /// <param name="variantId">Product variant ID (SKUID).</param>
        /// <param name="optionId">Product option ID (SKUID).</param>
        public static void RemoveOptionFromVariant(int variantId, int optionId)
        {
            var infoObj = GetVariantOptionInfo(variantId, optionId);
            DeleteVariantOptionInfo(infoObj);
        }

        #endregion


        #region "Internal methods - Basic"

        /// <summary>
        /// Returns relationship between specified product variant and option.
        /// </summary>
        /// <param name="variantId">Product variant ID (SKUID).</param>
        /// <param name="optionId">Product option ID (SKUID).</param>
        protected virtual VariantOptionInfo GetVariantOptionInfoInternal(int variantId, int optionId)
        {
            return GetObjectQuery().TopN(1)
                       .WhereEquals("VariantSKUID", variantId)
                       .WhereEquals("OptionSKUID", optionId)
                       .FirstOrDefault();
        }

        #endregion
    }
}
