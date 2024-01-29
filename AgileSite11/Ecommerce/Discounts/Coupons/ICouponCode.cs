namespace CMS.Ecommerce
{
    /// <summary>
    /// Represents the coupon code.
    /// </summary>
    public interface ICouponCode : ICouponCodeApplication
    {
        /// <summary>
        /// Gets the coupon code.
        /// </summary>
        string Code
        {
            get;
        }


        /// <summary>
        /// The provided discount value in main currency.
        /// </summary>
        decimal? ValueInMainCurrency
        {
            get;
            set;
        }


        /// <summary>
        /// Gets the information about coupon application.
        /// </summary>
        CouponCodeApplicationStatusEnum ApplicationStatus
        {
            get;
            set;
        }
    }
}