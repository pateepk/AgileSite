using System;
using System.Diagnostics;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Represents a percentage discount application.
    /// </summary>
    [DebuggerDisplay("{" + nameof(DebuggerDisplay) + ",nq}")]
    [Serializable]
    internal class PercentageDiscountApplication : IDiscount
    {
        private readonly decimal mRate;
        private readonly CurrencyInfo mCurrency;
        [NonSerialized]
        private readonly IRoundingService mRoundingService;
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
        /// Creates a new instance of the <see cref="PercentageDiscountApplication"/>.
        /// </summary>
        /// <param name="displayName">Discount display name</param>
        /// <param name="rate">Percentage rate (0..1)</param>
        /// <param name="currency">Currency used to determine rounding</param>
        /// <param name="code">Applied coupon code</param>
        /// <param name="roundingService">Service used to round discount calculation</param>
        /// <param name="logCoupon">Action which represents coupon code log</param>
        public PercentageDiscountApplication(string displayName, decimal rate, CurrencyInfo currency, string code, IRoundingService roundingService, Action<string> logCoupon = null)
        {
            mRate = rate;
            mCurrency = currency;
            mRoundingService = roundingService;
            DiscountName = displayName;
            AppliedCouponCode = code;
            mLogCoupon = logCoupon;
        }


        /// <summary>
        /// Calculates percentage discount from the <paramref name="basePrice"/>.
        /// </summary>
        /// <param name="basePrice">Base price in the calculation currency.</param>
        public decimal CalculateDiscount(decimal basePrice)
        {
            var value = mRoundingService.Round(basePrice * mRate, mCurrency);

            return (Math.Abs(value) > Math.Abs(basePrice)) ? basePrice : value;
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
        private string DebuggerDisplay => $"{DiscountName} ({mRate*100}%)";
    }

}