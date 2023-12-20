namespace CMS.Ecommerce
{
    /// <summary>
    /// Represents action for coupon code application.
    /// </summary>
    public interface ICouponCodeApplication
    {
        /// <summary>
        /// Applies coupon code during order creation.
        /// </summary>
        void Apply();
    }
}