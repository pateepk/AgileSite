using System.Linq;

using CMS.DataEngine;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Class providing <see cref="MultiBuyDiscountCollectionInfo"/> management.
    /// </summary>
    public class MultiBuyDiscountCollectionInfoProvider : AbstractInfoProvider<MultiBuyDiscountCollectionInfo, MultiBuyDiscountCollectionInfoProvider>
    {
        /// <summary>
        /// Returns all <see cref="MultiBuyDiscountCollectionInfo"/> bindings.
        /// </summary>
        public static ObjectQuery<MultiBuyDiscountCollectionInfo> GetMultiBuyDiscountCollections()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns <see cref="MultiBuyDiscountCollectionInfo"/> binding structure.
        /// </summary>
        /// <param name="multiBuyDiscountId">Multi buy discount ID.</param>
        /// <param name="collectionId">Collection ID.</param>  
        public static MultiBuyDiscountCollectionInfo GetMultiBuyDiscountCollectionInfo(int multiBuyDiscountId, int collectionId)
        {
            return ProviderObject.GetObjectQuery().TopN(1)
                                 .WhereEquals("MultiBuyDiscountID", multiBuyDiscountId)
                                 .WhereEquals("CollectionID", collectionId)
                                 .FirstOrDefault();
        }


        /// <summary>
        /// Sets specified <see cref="MultiBuyDiscountCollectionInfo"/>.
        /// </summary>
        /// <remarks>
        /// Setting the <see cref="MultiBuyDiscountCollectionInfo"/> does not affect other existing info objects like <see cref="MultiBuyDiscountSKUInfo"/> or <see cref="MultiBuyDiscountDepartmentInfo"/> or <see cref="MultiBuyDiscountBrandInfo"/> or <see cref="MultiBuyDiscountTreeInfo"/>
        /// This could lead to extended discount application than needed.
        /// </remarks>
        /// <param name="infoObj"><see cref="MultiBuyDiscountCollectionInfo"/> to set.</param>
        public static void SetMultiBuyDiscountCollectionInfo(MultiBuyDiscountCollectionInfo infoObj)
        {
            ProviderObject.SetInfo(infoObj);
        }


        /// <summary>
        /// Deletes specified <see cref="MultiBuyDiscountCollectionInfo"/> binding.
        /// </summary>
        /// <remarks>
        /// Deleting the <see cref="MultiBuyDiscountCollectionInfo"/> does not affect other existing info objects like <see cref="MultiBuyDiscountSKUInfo"/> or <see cref="MultiBuyDiscountDepartmentInfo"/> or <see cref="MultiBuyDiscountBrandInfo"/> or <see cref="MultiBuyDiscountTreeInfo"/>
        /// This could lead to extended discount application than needed.
        /// </remarks>
        /// <param name="infoObj"><see cref="MultiBuyDiscountCollectionInfo"/> object.</param>
        public static void DeleteMultiBuyDiscountCollectionInfo(MultiBuyDiscountCollectionInfo infoObj)
        {
            ProviderObject.DeleteInfo(infoObj);
        }


        /// <summary>
        /// Deletes <see cref="MultiBuyDiscountCollectionInfo"/> binding.
        /// </summary>
        /// <remarks>
        /// Adding the <see cref="MultiBuyDiscountCollectionInfo"/> does not affect other existing info objects like <see cref="MultiBuyDiscountSKUInfo"/> or <see cref="MultiBuyDiscountDepartmentInfo"/> or <see cref="MultiBuyDiscountBrandInfo"/> or <see cref="MultiBuyDiscountTreeInfo"/>
        /// This could lead to extended discount application than needed.
        /// </remarks>
        /// <param name="multiBuyDiscountId">Multi buy discount ID.</param>
        /// <param name="collectionId">Collection ID.</param>  
        public static void RemoveMultiBuyDiscountFromCollection(int multiBuyDiscountId, int collectionId)
        {
            var infoObj = GetMultiBuyDiscountCollectionInfo(multiBuyDiscountId, collectionId);
            if (infoObj != null)
            {
                DeleteMultiBuyDiscountCollectionInfo(infoObj);
            }
        }


        /// <summary>
        /// Creates <see cref="MultiBuyDiscountCollectionInfo"/> binding.
        /// </summary>
        /// <remarks>
        /// Adding the <see cref="MultiBuyDiscountCollectionInfo"/> does not affect other existing info objects like <see cref="MultiBuyDiscountSKUInfo"/> or <see cref="MultiBuyDiscountDepartmentInfo"/> or <see cref="MultiBuyDiscountBrandInfo"/> or <see cref="MultiBuyDiscountTreeInfo"/>
        /// This could lead to extended discount application than needed.
        /// </remarks>
        /// <param name="multiBuyDiscountId">Multi buy discount ID.</param>
        /// <param name="collectionId">Collection ID.</param>
        /// <param name="included">Inlude flag.</param>
        public static void AddMultiBuyDiscountToCollection(int multiBuyDiscountId, int collectionId, bool included)
        {
            var infoObj = new MultiBuyDiscountCollectionInfo
            {
                MultiBuyDiscountID = multiBuyDiscountId,
                CollectionID = collectionId,
                CollectionIncluded = included
            };

            SetMultiBuyDiscountCollectionInfo(infoObj);
        }
    }
}