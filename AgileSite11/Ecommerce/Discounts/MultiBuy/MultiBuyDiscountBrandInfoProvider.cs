using System.Linq;

using CMS.DataEngine;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Class providing <see cref="MultiBuyDiscountBrandInfo"/> management.
    /// </summary>
    public class MultiBuyDiscountBrandInfoProvider : AbstractInfoProvider<MultiBuyDiscountBrandInfo, MultiBuyDiscountBrandInfoProvider>
    {
        /// <summary>
        /// Returns all <see cref="MultiBuyDiscountBrandInfo"/> bindings.
        /// </summary>
        public static ObjectQuery<MultiBuyDiscountBrandInfo> GetMultiBuyDiscountBrands()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns <see cref="MultiBuyDiscountBrandInfo"/> binding structure.
        /// </summary>
        /// <param name="multiBuyDiscountId">Multi buy discount ID.</param>
        /// <param name="brandId">Brand ID.</param>  
        public static MultiBuyDiscountBrandInfo GetMultiBuyDiscountBrandInfo(int multiBuyDiscountId, int brandId)
        {
            return ProviderObject.GetObjectQuery().TopN(1)
                .WhereEquals("MultiBuyDiscountID", multiBuyDiscountId)
                .WhereEquals("BrandID", brandId)
                .FirstOrDefault();
        }


        /// <summary>
        /// Sets specified <see cref="MultiBuyDiscountBrandInfo"/>.
        /// </summary>
        /// <remarks>
        /// Seting the <see cref="MultiBuyDiscountBrandInfo"/> does not affect other existing info objects like <see cref="MultiBuyDiscountSKUInfo"/> or <see cref="MultiBuyDiscountDepartmentInfo"/> or <see cref="MultiBuyDiscountCollectionInfo"/> or <see cref="MultiBuyDiscountTreeInfo"/>
        /// This could lead to extended discount application than needed.
        /// </remarks>
        /// <param name="infoObj"><see cref="MultiBuyDiscountBrandInfo"/> to set.</param>
        public static void SetMultiBuyDiscountBrandInfo(MultiBuyDiscountBrandInfo infoObj)
        {
            ProviderObject.SetInfo(infoObj);
        }


        /// <summary>
        /// Deletes specified <see cref="MultiBuyDiscountBrandInfo"/> binding.
        /// </summary>
        /// <remarks>
        /// Deleting the <see cref="MultiBuyDiscountBrandInfo"/> does not affect other existing info objects like <see cref="MultiBuyDiscountSKUInfo"/> or <see cref="MultiBuyDiscountDepartmentInfo"/> or <see cref="MultiBuyDiscountCollectionInfo"/> or <see cref="MultiBuyDiscountTreeInfo"/>
        /// This could lead to extended discount application than needed.
        /// </remarks>
        /// <param name="infoObj"><see cref="MultiBuyDiscountBrandInfo"/> object.</param>
        public static void DeleteMultiBuyDiscountBrandInfo(MultiBuyDiscountBrandInfo infoObj)
        {
            ProviderObject.DeleteInfo(infoObj);
        }


        /// <summary>
        /// Deletes <see cref="MultiBuyDiscountBrandInfo"/> binding.
        /// </summary>
        /// <remarks>
        /// Removing the <see cref="MultiBuyDiscountBrandInfo"/> does not affect other existing info objects like <see cref="MultiBuyDiscountSKUInfo"/> or <see cref="MultiBuyDiscountDepartmentInfo"/> or <see cref="MultiBuyDiscountCollectionInfo"/> or <see cref="MultiBuyDiscountTreeInfo"/>
        /// This could lead to extended discount application than needed.
        /// </remarks>
        /// <param name="multiBuyDiscountId">Multi buy discount ID.</param>
        /// <param name="brandId">Brand ID.</param>  
        public static void RemoveMultiBuyDiscountFromBrand(int multiBuyDiscountId, int brandId)
        {
            var infoObj = GetMultiBuyDiscountBrandInfo(multiBuyDiscountId, brandId);
            if (infoObj != null)
            {
                DeleteMultiBuyDiscountBrandInfo(infoObj);
            }
        }


        /// <summary>
        /// Creates <see cref="MultiBuyDiscountBrandInfo"/> binding.
        /// </summary>
        /// <remarks>
        /// Adding the <see cref="MultiBuyDiscountBrandInfo"/> does not affect other existing info objects like <see cref="MultiBuyDiscountSKUInfo"/> or <see cref="MultiBuyDiscountDepartmentInfo"/> or <see cref="MultiBuyDiscountCollectionInfo"/> or <see cref="MultiBuyDiscountTreeInfo"/>
        /// This could lead to extended discount application than needed.
        /// </remarks>
        /// <param name="multiBuyDiscountId">Multi buy discount ID.</param>
        /// <param name="brandId">Brand ID.</param>
        /// <param name="included">Inlude flag.</param>
        public static void AddMultiBuyDiscountToBrand(int multiBuyDiscountId, int brandId, bool included)
        {
            // Create new binding
            var infoObj = new MultiBuyDiscountBrandInfo();
            infoObj.MultiBuyDiscountID = multiBuyDiscountId;
            infoObj.BrandID = brandId;
            infoObj.BrandIncluded = included;

            // Save to the database
            SetMultiBuyDiscountBrandInfo(infoObj);
        }
    }
}