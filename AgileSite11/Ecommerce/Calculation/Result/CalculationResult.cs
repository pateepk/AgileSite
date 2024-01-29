using System.Collections.Generic;
using System.Diagnostics;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Result data of a calculation
    /// </summary>
    [DebuggerDisplay("{" + nameof(GrandTotal) + "}")]
    public class CalculationResult
    {
        /// <summary>
        /// The final value to be paid.
        /// </summary>
        public decimal GrandTotal
        {
            get;
            set;
        }


        /// <summary>
        /// The total price of all items, shipping and taxes.
        /// </summary>
        public decimal Total
        {
            get;
            set;
        }


        /// <summary>
        /// The sum of prices of all items.
        /// </summary>
        public decimal Subtotal
        {
            get;
            set;
        }


        /// <summary>
        /// The price for shipping.
        /// </summary>
        public decimal Shipping
        {
            get;
            set;
        }


        /// <summary>
        /// Total tax value.
        /// </summary>
        public decimal Tax
        {
            get;
            set;
        }


        /// <summary>
        /// The summary of taxes applied during the calculation.
        /// </summary>
        public ValuesSummary TaxSummary
        {
            get;
        } = new ValuesSummary();


        /// <summary>
        /// Collection of detailed information for each cart item.
        /// </summary>
        public IEnumerable<CalculationResultItem> Items
        {
            get;
            set;
        }


        /// <summary>
        /// Discount which is applied to the items total price.
        /// </summary>
        public decimal OrderDiscount
        {
            get;
            set;
        }


        /// <summary>
        /// The summary of discounts applied to the items total price.
        /// </summary>
        public ValuesSummary OrderDiscountSummary
        {
            get;
        } = new ValuesSummary();


        /// <summary>
        /// Sum of all side payments used. E.g. gift card
        /// </summary>
        public decimal OtherPayments
        {
            get;
            set;
        }


        /// <summary>
        /// Other payment application summary.
        /// </summary>
        public ValuesSummary OtherPaymentsApplications
        {
            get;
        } = new ValuesSummary();


        /// <summary>
        /// List of the applied coupon codes.
        /// </summary>
        public IList<ICouponCode> AppliedCouponCodes
        {
            get;
        } = new List<ICouponCode>();
    }
}