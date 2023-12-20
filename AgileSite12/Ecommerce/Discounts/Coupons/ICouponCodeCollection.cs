using System.Collections.Generic;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Represents the collection of coupon codes.
    /// </summary>
    public interface ICouponCodeCollection : IReadOnlyCouponCodeCollection, ICouponCodeApplication
    {
        /// <summary>
        /// Adds coupon with given <paramref name="couponCode"/> and <paramref name="status"/> to the collection.
        /// </summary>
        void Add(string couponCode, CouponCodeApplicationStatusEnum status);


        /// <summary>
        /// Removes the coupon with given <paramref name="couponCode"/> from collection.
        /// Does nothing when no such coupon exists.
        /// </summary>
        void Remove(string couponCode);


        /// <summary>
        /// Merge existing coupon codes with given ones.
        /// </summary>
        void Merge(IEnumerable<ICouponCode> couponCodes);


        /// <summary>
        /// Returns true when given codes is present in cart, but it is not applied.
        /// </summary>
        bool IsNotAppliedInCart(string couponCode);


        /// <summary>
        /// Returns true when given codes is present and is applied in cart.
        /// </summary>
        bool IsAppliedInCart(string couponCode);


        /// <summary>
        /// Returns true when given codes is present and was applied in order.
        /// </summary>
        bool IsAppliedInOrder(string couponCode);


        /// <summary>
        /// Serializes the collection into string.
        /// </summary>
        string Serialize();
    }
}
