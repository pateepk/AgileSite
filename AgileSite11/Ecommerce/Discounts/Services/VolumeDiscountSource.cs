using CMS.Helpers;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Represents a source of volume discounts.
    /// </summary>
    internal class VolumeDiscountSource : IVolumeDiscountSource
    { 
        /// <summary>
        /// Returns product volume discount applicable to the specified <paramref name="sku"/> when purchased in given <paramref name="quantity"/>.
        /// </summary>
        /// <param name="sku">The product to get volume discount for.</param>
        /// <param name="quantity">The amount of the <paramref name="sku"/>.</param>
        public VolumeDiscountInfo GetDiscount(SKUInfo sku, decimal quantity)
        {
            if (quantity <= VolumeDiscountInfoProvider.MINIMUM_APPLICABLE_AMOUNT || !VolumeDiscountExists())
            {
                return null;
            }

            var volumeDiscountsSKUID = sku.IsProductVariant ? sku.SKUParentSKUID : sku.SKUID;

            return VolumeDiscountInfoProvider.GetVolumeDiscountInfo(volumeDiscountsSKUID, decimal.ToInt32(quantity));
        }

       
        private bool VolumeDiscountExists()
        {
            return CacheHelper.Cache(() =>
                 VolumeDiscountInfoProvider.GetVolumeDiscounts().Column("VolumeDiscountID").TopN(1).FirstObject != null,
                new CacheSettings(ECommerceSettings.ProvidersCacheMinutes, "VolumeDiscountExists")
                {
                    GetCacheDependency = () => CacheHelper.GetCacheDependency(new[] { VolumeDiscountInfo.OBJECT_TYPE + "|all" })
                });
        }
    }
}