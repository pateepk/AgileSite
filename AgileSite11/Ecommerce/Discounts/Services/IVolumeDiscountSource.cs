using CMS;
using CMS.Ecommerce;

[assembly: RegisterImplementation(typeof(IVolumeDiscountSource), typeof(VolumeDiscountSource), Priority = CMS.Core.RegistrationPriority.Fallback)]

namespace CMS.Ecommerce
{
    /// <summary>
    /// Represents a source of volume discounts.
    /// </summary>
    public interface IVolumeDiscountSource
    {
        /// <summary>
        /// Returns product volume discount applicable to the specified <paramref name="sku"/> when purchased in given <paramref name="quantity"/>.
        /// </summary>
        /// <param name="sku">The product to get volume discount for.</param>
        /// <param name="quantity">The amount of the <paramref name="sku"/>.</param>
        VolumeDiscountInfo GetDiscount(SKUInfo sku, decimal quantity);
    }
}