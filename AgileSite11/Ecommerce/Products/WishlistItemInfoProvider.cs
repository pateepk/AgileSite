using System.Linq;

using CMS.DataEngine;
using CMS.Membership;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Class providing WishlistItemInfo management.
    /// </summary>
    public class WishlistItemInfoProvider : AbstractInfoProvider<WishlistItemInfo, WishlistItemInfoProvider>
    {
        #region "Public methods - Basic"

        /// <summary>
        /// Returns the query for all wishlist items.
        /// </summary>        
        public static ObjectQuery<WishlistItemInfo> GetWishlistItems()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns specified wishlist item. 
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="skuId">Product ID</param>
        /// <param name="siteId">Site ID</param>
        public static WishlistItemInfo GetWishlistItemInfo(int userId, int skuId, int siteId)
        {
            return ProviderObject.GetWishlistItemInfoInternal(userId, skuId, siteId);
        }


        /// <summary>
        /// Sets specified wishlist item.
        /// </summary>
        /// <param name="infoObj">Wishlist item to be set</param>
        public static void SetWishlistItemInfo(WishlistItemInfo infoObj)
        {
            ProviderObject.SetInfo(infoObj);
        }


        /// <summary>
        /// Deletes specified wishlist item.
        /// </summary>
        /// <param name="infoObj">Wishlist item to be deleted</param>
        public static void DeleteWishlistItemInfo(WishlistItemInfo infoObj)
        {
            ProviderObject.DeleteInfo(infoObj);
        }


        /// <summary>
        /// Adds product to specified wishlist.
        /// </summary>  
        /// <param name="userId">User ID</param>
        /// <param name="skuId">Product ID</param>
        /// <param name="siteId">Site ID</param>
        public static void AddSKUToWishlist(int userId, int skuId, int siteId)
        {
            var infoObj = ProviderObject.CreateInfo();

            infoObj.UserID = userId;
            infoObj.SKUID = skuId;
            infoObj.SiteID = siteId;

            SetWishlistItemInfo(infoObj);
        }


        /// <summary>
        /// Removes specified wishlist item.
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="skuId">Product ID</param>
        /// <param name="siteId">Site ID</param>
        public static void RemoveSKUFromWishlist(int userId, int skuId, int siteId)
        {
            var infoObj = GetWishlistItemInfo(userId, skuId, siteId);
            DeleteWishlistItemInfo(infoObj);
        }

        #endregion


        #region "Internal methods - Basic"

        /// <summary>
        /// Returns specified wishlist item. 
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="skuId">Product ID</param>
        /// <param name="siteId">Site ID</param>
        protected virtual WishlistItemInfo GetWishlistItemInfoInternal(int userId, int skuId, int siteId)
        {
            return GetObjectQuery().TopN(1)
                      .WhereEquals("UserID", userId)
                      .WhereEquals("SKUID", skuId)
                      .WhereEquals("SiteID", siteId)
                      .FirstOrDefault();
        }


        /// <summary>
        /// Inserts or Updates the object to the database.
        /// </summary>
        /// <param name="info">Object to insert / update</param>
        protected override void SetInfo(WishlistItemInfo info)
        {
            base.SetInfo(info);

            // Invalidate user info          
            UserInfoProvider.InvalidateUser(info.UserID);
        }


        /// <summary>
        /// Deletes the object to the database.
        /// </summary>
        /// <param name="info">Object to delete</param>
        protected override void DeleteInfo(WishlistItemInfo info)
        {
            // Nothing to delete
            if (info == null)
            {
                return;
            }

            // Invalidate user info          
            UserInfoProvider.InvalidateUser(info.UserID);

            base.DeleteInfo(info);
        }

        #endregion
    }
}