using System;
using System.Diagnostics;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Detailed information of an item included in the calculation.
    /// </summary>
    [DebuggerDisplay("{" + nameof(DebuggerDisplay) + "}")]
    public class CalculationResultItem
    {
        /// <summary>
        /// Identifier of the corresponding cart item.
        /// </summary>
        public Guid ItemGuid
        {
            get;
            set;
        }


        /// <summary>
        /// Value of the discount applied on each unit of the item.
        /// </summary>
        /// <remarks>
        /// This discount is included in <see cref="ItemUnitPrice"/>. Summary of item discounts can be found in <see cref="UnitDiscountSummary"/>.
        /// </remarks>
        public decimal UnitDiscount
        {
            get;
            set;
        }


        /// <summary>
        /// The summary of discounts applied on each unit of the item.
        /// </summary>
        public ValuesSummary UnitDiscountSummary
        {
            get;
        } = new ValuesSummary();


        /// <summary>
        /// Unit price of the item. This value includes <see cref="UnitDiscount"/>.
        /// </summary>
        public decimal ItemUnitPrice
        {
            get;
            set;
        }


        /// <summary>
        /// Value of the item discount.
        /// </summary>
        /// <remarks>
        /// This discount is included in <see cref="LineSubtotal"/>. Summary of item discounts can be found in <see cref="ItemDiscountSummary"/>.
        /// </remarks>
        public decimal ItemDiscount
        {
            get;
            set;
        }


        /// <summary>
        /// The summary of discounts applied on this item.
        /// </summary>
        public ValuesSummary ItemDiscountSummary
        {
            get;
        } = new ValuesSummary();


        /// <summary>
        /// Total price of the item.
        /// </summary>
        public decimal LineSubtotal
        {
            get;
            set;
        }


        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string DebuggerDisplay => $"Unit price: {ItemUnitPrice} Discount: {ItemDiscount} Subtotal: {LineSubtotal}";
    }
}