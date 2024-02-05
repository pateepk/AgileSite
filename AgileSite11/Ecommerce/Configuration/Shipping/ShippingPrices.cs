using System.Collections.Generic;
using System.Diagnostics;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Container for the shipping prices and applied discounts.
    /// </summary>
    [DebuggerDisplay("Price = {" + nameof(Price) + "}")]
    public class ShippingPrices
    {
        /// <summary>
        /// Standard shipping price. Price does not contain any discounts.
        /// </summary>
        public decimal StandardPrice
        {
            get;
            set;
        }


        /// <summary>
        /// Calculated shipping price. All discounts are included.
        /// </summary>
        public decimal Price
        {
            get;
            set;
        }


        /// <summary>
        /// Applied discounts summary. Sum of the discount is already applied in the <see cref="Price"/> property.
        /// </summary>
        public ValuesSummary ShippingDiscountSummary
        {
            get;
            set;
        }


        /// <summary>
        /// Collection of the applied <see cref="IDiscount"/> discounts.
        /// </summary>
        public IEnumerable<IDiscount> AppliedDiscounts
        {
            get;
            set;
        }
    }
}
