using CMS.Helpers;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Represents the status of coupon code application.
    /// </summary>
    public enum CouponCodeApplicationStatusEnum
    {
        /// <summary>
        /// Default state. Coupon code hasn't been applied yet.
        /// </summary>
        [EnumStringRepresentation("Invalid")]
        Invalid = 1,

        /// <summary>
        /// Coupon code is in cart, but it is not applied.
        /// </summary>
        [EnumStringRepresentation("NotAppliedInCart")]
        NotAppliedInCart = 2,

        /// <summary>
        /// Coupon code is applied in shopping cart, but the order is not created yet.
        /// </summary>
        [EnumStringRepresentation("AppliedInCart")]
        AppliedInCart = 3,

        /// <summary>
        /// Coupon code is applied in order.
        /// </summary>
        [EnumStringRepresentation("AppliedInOrder")]
        AppliedInOrder = 4,

        /// <summary>
        /// Coupon code is applied temporarily as a gift card correction.
        /// </summary>
        [EnumStringRepresentation("GiftCardCorrection")]
        GiftCardCorrection = 5
    }
}