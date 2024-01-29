using System.Collections.Generic;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Represents the read only collection of coupon codes.
    /// </summary>
    public interface IReadOnlyCouponCodeCollection
    {
        /// <summary>
        /// Gets all coupon codes.
        /// </summary>
        IEnumerable<ICouponCode> Codes
        {
            get;
        }


        /// <summary>
        /// Gets coupon codes, that are in cart, but not applied.
        /// </summary>
        IEnumerable<ICouponCode> NotAppliedInCartCodes
        {
            get;
        }


        /// <summary>
        /// Gets coupon codes that are already applied in cart.
        /// </summary>
        IEnumerable<ICouponCode> CartAppliedCodes
        {
            get;
        }


        /// <summary>
        /// Gets coupon codes that are already applied in order.
        /// </summary>
        IEnumerable<ICouponCode> OrderAppliedCodes
        {
            get;
        }


        /// <summary>
        /// Gets coupon codes that are already applied in cart or order.
        /// </summary>
        IEnumerable<ICouponCode> AllAppliedCodes
        {
            get;
        }
    }
}