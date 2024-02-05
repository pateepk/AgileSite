using System.Linq;

using CMS.DataEngine;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Class providing VolumeDiscountInfo management.
    /// </summary>
    public class VolumeDiscountInfoProvider : AbstractInfoProvider<VolumeDiscountInfo, VolumeDiscountInfoProvider>
    {
        /// <summary>
        /// Minimum amount of items to get volume discount applicable
        /// </summary>
        internal const int MINIMUM_APPLICABLE_AMOUNT = 1;


        #region "Constructors"

        /// <summary>
        /// Constructor.
        /// </summary>
        public VolumeDiscountInfoProvider()
            : base(VolumeDiscountInfo.TYPEINFO, new HashtableSettings
				{
					ID = true
				})
        {
        }

        #endregion


        #region "Public methods - Basic"

        /// <summary>
        /// Returns the query for all volume discounts.
        /// </summary>
        public static ObjectQuery<VolumeDiscountInfo> GetVolumeDiscounts()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns volume discount with specified ID.
        /// </summary>
        /// <param name="discountId">Volume discount ID</param>        
        public static VolumeDiscountInfo GetVolumeDiscountInfo(int discountId)
        {
            return ProviderObject.GetInfoById(discountId);
        }


        /// <summary>
        /// Sets (updates or inserts) specified volume discount.
        /// </summary>
        /// <param name="discountObj">Volume discount to be set</param>
        public static void SetVolumeDiscountInfo(VolumeDiscountInfo discountObj)
        {
            ProviderObject.SetInfo(discountObj);
        }


        /// <summary>
        /// Deletes specified volume discount.
        /// </summary>
        /// <param name="discountObj">Volume discount to be deleted</param>
        public static void DeleteVolumeDiscountInfo(VolumeDiscountInfo discountObj)
        {
            ProviderObject.DeleteInfo(discountObj);
        }


        /// <summary>
        /// Deletes volume discount with specified ID.
        /// </summary>
        /// <param name="discountId">Volume discount ID</param>
        public static void DeleteVolumeDiscountInfo(int discountId)
        {
            var discountObj = GetVolumeDiscountInfo(discountId);
            DeleteVolumeDiscountInfo(discountObj);
        }

        #endregion


        #region "Public methods - Advanced"

        /// <summary>
        /// Returns query for volume discounts related to specified product(SKU).
        /// </summary>
        /// <param name="skuId">SKU id</param>
        public static ObjectQuery<VolumeDiscountInfo> GetVolumeDiscounts(int skuId)
        {
            return ProviderObject.GetVolumeDiscountsInternal(skuId);
        }


        /// <summary>
        /// Returns corresponding VolumeDiscountInfo structure for the specified SKU and SKU units, if no volume discount found returns null.
        /// </summary>        
        /// <param name="skuId">SKU id</param>
        /// <param name="units">SKU units</param>
        public static VolumeDiscountInfo GetVolumeDiscountInfo(int skuId, int units)
        {
            return ProviderObject.GetVolumeDiscountInfoInternal(skuId, units);
        }


        /// <summary>
        /// Returns True if volume discount is valid and can be applied to the specified shopping cart item, otherwise returns False.
        /// </summary>
        /// <param name="item">Shopping cart item to which the volume discount should be applied</param>
        /// <param name="discount">Volume discount data</param>        
        public static bool ValidateVolumeDiscount(ShoppingCartItemInfo item, VolumeDiscountInfo discount)
        {
            return ProviderObject.ValidateVolumeDiscountInternal(item, discount);
        }

        #endregion


        #region "Internal methods - Advanced"

        /// <summary>
        /// Returns query for volume discounts related to specified product(SKU). Records are ordered by VolumeDiscountMinCount field.
        /// </summary>
        /// <param name="skuId">SKU id</param>
        protected virtual ObjectQuery<VolumeDiscountInfo> GetVolumeDiscountsInternal(int skuId)
        {
            return GetVolumeDiscounts()
                       .WhereEquals("VolumeDiscountSKUID", skuId)
                       .OrderBy("VolumeDiscountMinCount");
        }


        /// <summary>
        /// Returns corresponding VolumeDiscountInfo structure for the specified SKU and SKU units, if no volume discount found returns null.
        /// </summary>        
        /// <param name="skuId">SKU id</param>
        /// <param name="units">SKU units</param>
        protected virtual VolumeDiscountInfo GetVolumeDiscountInfoInternal(int skuId, int units)
        {
            // Do not search for volume discount
            if ((skuId <= 0) || (units <= MINIMUM_APPLICABLE_AMOUNT))
            {
                return null;
            }

            return GetObjectQuery().TopN(1)
                .WhereEquals("VolumeDiscountSKUID", skuId)
                .WhereLessOrEquals("VolumeDiscountMinCount", units)
                .OrderByDescending("VolumeDiscountMinCount")
                .FirstOrDefault();
        }


        /// <summary>
        /// Returns True if volume discount is valid and can be applied to the specified shopping cart item, otherwise returns False.
        /// </summary>
        /// <param name="item">Shopping cart item to which the volume discount should be applied</param>
        /// <param name="discount">Volume discount data</param>        
        protected virtual bool ValidateVolumeDiscountInternal(ShoppingCartItemInfo item, VolumeDiscountInfo discount)
        {
            return (item != null) && (discount != null) && (discount.VolumeDiscountValue > 0);
        }

        #endregion
    }
}