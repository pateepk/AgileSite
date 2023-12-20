using System.Linq;

using CMS.DataEngine;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Class providing BundleInfo management.
    /// </summary>
    public class BundleInfoProvider : AbstractInfoProvider<BundleInfo, BundleInfoProvider>
    {
        #region "Public methods - Basic"

        /// <summary>
        /// Returns the query for all relationships between bundles and SKUs.
        /// </summary>        
        public static ObjectQuery<BundleInfo> GetBundles()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns relationship between specified bundle and SKU.
        /// </summary>
        /// <param name="bundleId">Bundle ID.</param>
        /// <param name="skuId">SKU ID.</param>
        public static BundleInfo GetBundleInfo(int bundleId, int skuId)
        {
            return ProviderObject.GetBundleInfoInternal(bundleId, skuId);
        }


        /// <summary>
        /// Sets relationship between specified bundle and SKU.
        /// </summary>
        /// <param name="infoObj">Bundle-SKU relationship to be set.</param>
        public static void SetBundleInfo(BundleInfo infoObj)
        {
            ProviderObject.SetInfo(infoObj);
        }


        /// <summary>
        /// Deletes relationship between specified bundle and SKU.
        /// </summary>
        /// <param name="infoObj">Bundle-SKU relationship to be deleted.</param>
        public static void DeleteBundleInfo(BundleInfo infoObj)
        {
            ProviderObject.DeleteInfo(infoObj);
        }


        /// <summary>
        /// Sets relationship between specified bundle and SKU.
        /// </summary>	
        /// <param name="bundleId">Bundle ID.</param>
        /// <param name="skuId">SKU ID.</param>
        public static void AddSKUToBundle(int bundleId, int skuId)
        {
            BundleInfo infoObj = ProviderObject.CreateInfo();

            infoObj.BundleID = bundleId;
            infoObj.SKUID = skuId;

            SetBundleInfo(infoObj);
        }


        /// <summary>
        /// Deletes relationship between specified bundle and SKU.
        /// </summary>
        /// <param name="bundleId">Bundle ID.</param>
        /// <param name="skuId">SKU ID.</param>
        public static void RemoveSKUFromBundle(int bundleId, int skuId)
        {
            BundleInfo infoObj = GetBundleInfo(bundleId, skuId);
            DeleteBundleInfo(infoObj);
        }

        #endregion


        #region "Internal methods - Basic"

        /// <summary>
        /// Returns relationship between specified bundle and SKU.
        /// </summary>
        /// <param name="bundleId">Bundle ID.</param>
        /// <param name="skuId">SKU ID.</param>
        protected virtual BundleInfo GetBundleInfoInternal(int bundleId, int skuId)
        {
            var condition = new WhereCondition()
                .WhereEquals("BundleID", bundleId)
                .WhereEquals("SKUID", skuId);

            return GetObjectQuery().TopN(1).Where(condition).FirstOrDefault();
        }

        #endregion
    }
}