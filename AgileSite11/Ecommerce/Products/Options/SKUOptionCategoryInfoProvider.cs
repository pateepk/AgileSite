using System.Data;
using System.Linq;

using CMS.DataEngine;
using CMS.Helpers;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Class providing SKUOptionCategoryInfo management.
    /// </summary>
    public class SKUOptionCategoryInfoProvider : AbstractInfoProvider<SKUOptionCategoryInfo, SKUOptionCategoryInfoProvider>
    {
        #region "Public methods - Basic"

        /// <summary>
        /// Returns the query for all relationships between categories and SKUs.
        /// </summary>        
        public static ObjectQuery<SKUOptionCategoryInfo> GetSKUOptionCategories()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns specified relationship between the given category and SKU.
        /// </summary>
        /// <param name="relationshipId">Category-SKU relationship ID.</param>        
        public static SKUOptionCategoryInfo GetSKUOptionCategoryInfo(int relationshipId)
        {
            return ProviderObject.GetInfoById(relationshipId);
        }


        /// <summary>
        /// Returns relationship between specified category and SKU.
        /// </summary>
        /// <param name="categoryId">Category ID</param>
        /// <param name="skuId">SKU ID</param>
        public static SKUOptionCategoryInfo GetSKUOptionCategoryInfo(int categoryId, int skuId)
        {
            return ProviderObject.GetSKUOptionCategoryInfoInternal(categoryId, skuId);
        }


        /// <summary>
        /// Sets relationship between specified category and SKU.
        /// </summary>
        /// <param name="infoObj">Category-SKU relationship to be set</param>
        public static void SetSKUOptionCategoryInfo(SKUOptionCategoryInfo infoObj)
        {
            ProviderObject.SetInfo(infoObj);
        }


        /// <summary>
        /// Deletes relationship between specified category and SKU.
        /// </summary>
        /// <param name="infoObj">Category-SKU relationship to be deleted</param>
        public static void DeleteSKUOptionCategoryInfo(SKUOptionCategoryInfo infoObj)
        {
            ProviderObject.DeleteInfo(infoObj);
        }


        /// <summary>
        /// Sets relationship between specified category and SKU.
        /// </summary>  
        /// <param name="categoryId">Category ID</param>
        /// <param name="skuId">SKU ID</param>
        public static void AddOptionCategoryToSKU(int categoryId, int skuId)
        {
            var infoObj = ProviderObject.CreateInfo();

            infoObj.SKUID = skuId;
            infoObj.CategoryID = categoryId;

            SetSKUOptionCategoryInfo(infoObj);
        }


        /// <summary>
        /// Deletes relationship between specified category and SKU.
        /// </summary>
        /// <param name="categoryId">Category ID</param>
        /// <param name="skuId">SKU ID</param>
        public static void RemoveOptionCategoryFromSKU(int categoryId, int skuId)
        {
            var infoObj = GetSKUOptionCategoryInfo(categoryId, skuId);
            DeleteSKUOptionCategoryInfo(infoObj);
        }

        #endregion


        #region "Internal methods - Basic"

        /// <summary>
        /// Returns relationship between specified category and SKU.
        /// </summary>
        /// <param name="categoryId">Category ID</param>
        /// <param name="skuId">SKU ID</param>
        protected virtual SKUOptionCategoryInfo GetSKUOptionCategoryInfoInternal(int categoryId, int skuId)
        {
            return GetObjectQuery().TopN(1)
                       .WhereEquals("SKUID", skuId)
                       .WhereEquals("CategoryID", categoryId)
                       .FirstOrDefault();
        }


        /// <summary>
        /// Inserts or Updates the object to the database.
        /// </summary>
        /// <param name="info">Object to insert / update</param>
        protected override void SetInfo(SKUOptionCategoryInfo info)
        {
            if ((info != null) && (info.SKUCategoryID == 0) && (info.SKUCategoryOrder <= 0))
            {
                // Set SKU category order at the end
                info.SKUCategoryOrder = GetLastStatusOrderInternal(info.SKUID) + 1;
            }

            base.SetInfo(info);

            if (info != null)
            {
                // Clear cache
                CacheHelper.TouchKey("ecommerce.sku|byid|" + info.SKUID);
            }
        }


        /// <summary>
        /// Deletes the object to the database.
        /// </summary>
        /// <param name="info">Object to delete</param>
        protected override void DeleteInfo(SKUOptionCategoryInfo info)
        {
            base.DeleteInfo(info);

            if (info != null)
            {
                // Clear cache
                CacheHelper.TouchKey("ecommerce.sku|byid|" + info.SKUID);
            }
        }

        #endregion


        #region "Internal methods - Advanced"

        /// <summary>
        /// Returns SKU category of last category item for skuCategoryId.
        /// </summary>
        /// <param name="skuId">SKU ID</param>
        protected virtual int GetLastStatusOrderInternal(int skuId)
        {
            DataSet ds = GetSKUOptionCategories()
                             .Column("SKUCategoryOrder")
                             .TopN(1)
                             .WhereEquals("SKUID", skuId)
                             .OrderByDescending("SKUCategoryOrder");

            if (!DataHelper.DataSourceIsEmpty(ds))
            {
                return ValidationHelper.GetInteger(ds.Tables[0].Rows[0][0], 0);
            }

            return 0;
        }

        #endregion
    }
}