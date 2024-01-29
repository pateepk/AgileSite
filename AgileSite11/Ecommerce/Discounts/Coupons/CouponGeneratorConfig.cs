namespace CMS.Ecommerce
{
    /// <summary>
    /// Represents configuration object for discount coupon creation with redemption limit.
    /// </summary>
    public class CouponGeneratorConfig
    {
        /// <summary>
        /// Generated coupon code.
        /// </summary>
        public string CouponCode
        {
            get;
        }


        /// <summary>
        /// Specifies number of uses of coupon code.
        /// </summary>
        public int NumberOfUses
        {
            get;
        }


        /// <summary>
        /// Creates a new instance of <see cref="CouponGeneratorConfig"/>
        /// </summary>
        public CouponGeneratorConfig(string couponCode, int numberOfUses)
        {
            CouponCode = couponCode;
            NumberOfUses = numberOfUses;
        }


        /// <summary>
        /// Creates a new instance of <see cref="CouponGeneratorConfig"/>
        /// </summary>
        public CouponGeneratorConfig(string couponCode)
            : this(couponCode, 0)
        {
        }
    }
}
