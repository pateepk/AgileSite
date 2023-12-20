using System.Collections.Generic;
using System.Linq;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Extension methods for <see cref="CouponCodeCollection"/> class.
    /// </summary>
    internal static class CouponCodeCollectionExtensions
    {
        /// <summary>
        /// Returns all unique coupon codes from the collection. Uses case-insensitive ordinal string comparison.
        /// </summary>
        public static List<string> GetAllDistinctCouponCodes(this IReadOnlyCouponCodeCollection couponCodeCollection)
        {
            return couponCodeCollection.Codes.Select(x => x.Code).Distinct(ECommerceHelper.CouponCodeComparer).ToList();
        }


        /// <summary>
        /// Returns true if the collection contains the specified <paramref name="couponCode"/>. Uses case-insensitive ordinal string comparison.
        /// </summary>
        public static bool Contains(this IReadOnlyCouponCodeCollection couponCodeCollection, string couponCode)
        {
            return couponCodeCollection.Codes.Any(x => ECommerceHelper.CouponCodeComparer.Equals(x.Code, couponCode));
        }
    }
}
