using System.Linq;

using CMS.DataEngine;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Class providing <see cref="MultiBuyDiscountSKUInfo"/> management.
    /// </summary>
    public class MultiBuyDiscountSKUInfoProvider : AbstractInfoProvider<MultiBuyDiscountSKUInfo, MultiBuyDiscountSKUInfoProvider>
    {
        #region "Public methods"

        /// <summary>
        /// Returns all MultiBuyDiscountSKUInfo bindings.
        /// </summary>
        public static ObjectQuery<MultiBuyDiscountSKUInfo> GetMultiBuyDiscountSKUs()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns MultiBuyDiscountSKUInfo binding structure.
        /// </summary>
        /// <param name="multiBuyDiscountId">Multi buy discount ID</param>
        /// <param name="skuId">Product ID</param>  
        public static MultiBuyDiscountSKUInfo GetMultiBuyDiscountSKUInfo(int multiBuyDiscountId, int skuId)
        {
            return ProviderObject.GetMultiBuyDiscountSKUInfoInternal(multiBuyDiscountId, skuId);
        }


        /// <summary>
        /// Sets specified MultiBuyDiscountSKUInfo.
        /// </summary>
        /// <remarks>
        /// Seting the <see cref="MultiBuyDiscountSKUInfo"/> does not affect other existing info objects like <see cref="MultiBuyDiscountDepartmentInfo"/> or <see cref="MultiBuyDiscountBrandInfo"/> or <see cref="MultiBuyDiscountCollectionInfo"/>
        /// This could lead to extended discount application than needed.
        /// </remarks>
        /// <param name="infoObj">MultiBuyDiscountSKUInfo to set</param>
        public static void SetMultiBuyDiscountSKUInfo(MultiBuyDiscountSKUInfo infoObj)
        {
            ProviderObject.SetInfo(infoObj);
        }


        /// <summary>
        /// Deletes specified MultiBuyDiscountSKUInfo binding.
        /// </summary>
        /// <remarks>
        /// Deleting the <see cref="MultiBuyDiscountSKUInfo"/> does not affect other existing info objects like <see cref="MultiBuyDiscountDepartmentInfo"/> or <see cref="MultiBuyDiscountBrandInfo"/> or <see cref="MultiBuyDiscountCollectionInfo"/>
        /// This could lead to extended discount application than needed.
        /// </remarks>
        /// <param name="infoObj">MultiBuyDiscountSKUInfo object</param>
        public static void DeleteMultiBuyDiscountSKUInfo(MultiBuyDiscountSKUInfo infoObj)
        {
            ProviderObject.DeleteInfo(infoObj);
        }


        /// <summary>
        /// Deletes MultiBuyDiscountSKUInfo binding.
        /// </summary>
        /// <remarks>
        /// Removing the <see cref="MultiBuyDiscountSKUInfo"/> does not affect other existing info objects like <see cref="MultiBuyDiscountDepartmentInfo"/> or <see cref="MultiBuyDiscountBrandInfo"/> or <see cref="MultiBuyDiscountCollectionInfo"/>
        /// This could lead to extended discount application than needed.
        /// </remarks>
        /// <param name="multiBuyDiscountId">Multi buy discount ID</param>
        /// <param name="skuId">Product ID</param>  
        public static void RemoveMultiBuyDiscountFromProduct(int multiBuyDiscountId, int skuId)
        {
            ProviderObject.RemoveMultiBuyDiscountFromProductInternal(multiBuyDiscountId, skuId);
        }


        /// <summary>
        /// Creates MultiBuyDiscountSKUInfo binding. 
        /// </summary>
        /// <remarks>
        /// Adding the <see cref="MultiBuyDiscountSKUInfo"/> does not affect other existing info objects like <see cref="MultiBuyDiscountDepartmentInfo"/> or <see cref="MultiBuyDiscountBrandInfo"/> or <see cref="MultiBuyDiscountCollectionInfo"/>
        /// This could lead to extended discount application than needed.
        /// </remarks>
        /// <param name="multiBuyDiscountId">Multi buy discount ID</param>
        /// <param name="skuId">Product ID</param>   
        public static void AddMultiBuyDiscountToProduct(int multiBuyDiscountId, int skuId)
        {
            ProviderObject.AddMultiBuyDiscountToProductInternal(multiBuyDiscountId, skuId);
        }

        #endregion


        #region "Internal methods"

        /// <summary>
        /// Returns the MultiBuyDiscountSKUInfo structure.
        /// Null if binding doesn't exist.
        /// </summary>
        /// <param name="multiBuyDiscountId">Multi buy discount ID</param>
        /// <param name="skuId">Product ID</param>  
        protected virtual MultiBuyDiscountSKUInfo GetMultiBuyDiscountSKUInfoInternal(int multiBuyDiscountId, int skuId)
        {
            return ProviderObject.GetObjectQuery().TopN(1)
                .WhereEquals("MultiBuyDiscountID", multiBuyDiscountId)
                .WhereEquals("SKUID", skuId)
                .FirstOrDefault();
        }


        /// <summary>
        /// Deletes MultiBuyDiscountSKUInfo binding.
        /// </summary>
        /// <param name="multiBuyDiscountId">Multi buy discount ID</param>
        /// <param name="skuId">Product ID</param>  
        protected virtual void RemoveMultiBuyDiscountFromProductInternal(int multiBuyDiscountId, int skuId)
        {
            var infoObj = GetMultiBuyDiscountSKUInfo(multiBuyDiscountId, skuId);
            if (infoObj != null)
            {
                DeleteMultiBuyDiscountSKUInfo(infoObj);
            }
        }


        /// <summary>
        /// Creates MultiBuyDiscountSKUInfo binding. 
        /// </summary>
        /// <param name="multiBuyDiscountId">Multi buy discount ID</param>
        /// <param name="skuId">Product ID</param>   
        protected virtual void AddMultiBuyDiscountToProductInternal(int multiBuyDiscountId, int skuId)
        {
            var infoObj = new MultiBuyDiscountSKUInfo
            {
                MultiBuyDiscountID = multiBuyDiscountId,
                SKUID = skuId
            };

            SetMultiBuyDiscountSKUInfo(infoObj);
        }

        #endregion
    }
}