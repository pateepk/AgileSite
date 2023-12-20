using System;
using System.Diagnostics;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Represents a fixed value discount application.
    /// </summary>
    [DebuggerDisplay("{" + nameof(DebuggerDisplay) + ",nq}")]
    [Serializable]
    internal class FixedDiscountApplication : IDiscount
    {
        private readonly decimal mValue;
        private readonly Action<string> mLogCoupon;


        /// <summary>
        /// Discount name used for summaries.
        /// </summary>
        public string DiscountName
        {
            get;
        }


        /// <summary>
        /// Coupon code which triggered this application.
        /// <c>null</c> if the discount is not triggered by coupon code
        /// </summary>
        public string AppliedCouponCode
        {
            get;
            set;
        }


        /// <summary>
        /// Creates a new instance of the <see cref="FixedDiscountApplication"/>.
        /// </summary>
        /// <param name="displayName">Discount display name</param>
        /// <param name="fixedValue">Fixed discount value</param>
        /// <param name="code">Applied coupon code</param>
        /// <param name="logCoupon">Action which represents coupon code log</param>
        public FixedDiscountApplication(string displayName, decimal fixedValue, string code, Action<string> logCoupon = null)
        {
            mValue = fixedValue;
            DiscountName = displayName;
            AppliedCouponCode = code;
            mLogCoupon = logCoupon;
        }


        /// <summary>
        /// Calculates the discount value from the specified <paramref name="basePrice"/>.
        /// </summary>
        /// <param name="basePrice">Base price in the calculation currency.</param>
        public decimal CalculateDiscount(decimal basePrice)
        {
            return (Math.Abs(mValue) > Math.Abs(basePrice)) ? basePrice : mValue;
        }


        /// <summary>
        /// Increments the discount coupon code usage count.
        /// </summary>
        public void Apply()
        {
            if (!string.IsNullOrEmpty(AppliedCouponCode))
            {
                mLogCoupon?.Invoke(AppliedCouponCode);
            }
        }


        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string DebuggerDisplay => $"{DiscountName} ({mValue})";
    }
}