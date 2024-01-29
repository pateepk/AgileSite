using CMS.DataEngine;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Represents info object providing any discounts.
    /// </summary>
    public interface IDiscountInfo
    {
        /// <summary>
        /// Gets discount ID.
        /// </summary>
        int DiscountID
        {
            get;
        }


        /// <summary>
        /// Gets discount site ID.
        /// </summary>
        int DiscountSiteID
        {
            get;
        }


        /// <summary>
        /// Gets discount display name.
        /// </summary>
        string DiscountDisplayName
        {
            get;
        }


        /// <summary>
        /// Gets discount enabled.
        /// </summary>
        bool DiscountEnabled
        {
            get;
        }


        /// <summary>
        /// Indicates if this discount is applicable only with discount coupon.
        /// </summary>
        bool DiscountUsesCoupons
        {
            get;
        }


        /// <summary>
        /// Gets discount status.
        /// </summary>
        DiscountStatusEnum DiscountStatus
        {
            get;
        }


        /// <summary>
        /// Determines whether discount is currently running. ValidFrom and ValidTo properties are compared to current date/time.
        /// </summary>
        bool IsRunning
        {
            get;
        }


        /// <summary>
        /// Gets the type of the discount.
        /// </summary>
        DiscountTypeEnum DiscountType
        {
            get;
        }


        /// <summary>
        /// Creates and saves new coupon for this discount.
        /// </summary>        
        /// <param name="config">Configuration for coupon code creation</param>
        BaseInfo CreateCoupon(CouponGeneratorConfig config);
    }
}