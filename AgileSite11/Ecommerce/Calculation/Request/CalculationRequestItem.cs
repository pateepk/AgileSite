using System;
using System.Collections.Generic;
using System.Diagnostics;

using CMS.Helpers;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Item to be processed in a calculation.
    /// </summary>
    [DebuggerDisplay("{" + nameof(DebuggerDisplay) + ",nq}")]
    public class CalculationRequestItem
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
        /// Item's SKU
        /// </summary>
        public SKUInfo SKU
        {
            get;
            set;
        }


        /// <summary>
        /// Item quantity
        /// </summary>
        public decimal Quantity
        {
            get;
            set;
        }


        /// <summary>
        /// The quantity of the item which was automatically added.
        /// </summary>
        public decimal AutoAddedQuantity
        {
            get;
            set;
        }


        /// <summary>
        /// Options that modify or customize the item.
        /// </summary>
        public IEnumerable<CalculationRequestItemOption> Options
        {
            get;
            set;
        }


        /// <summary>
        /// Request item custom data.
        /// </summary>
        public ContainerCustomData ItemCustomData
        {
            get;
            set;
        }


        /// <summary>
        /// Prices which are used in the calculation instead of the calculation from the <see cref="SKU"/> with the <see cref="IProductPricingService"/> implementation.
        /// <see cref="ProductPrices.Price"/> represents unit price with applied catalog-level discounts.
        /// </summary>
        public ProductPrices PriceOverride
        {
            get;
            set;
        }


        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string DebuggerDisplay => $"{Quantity} x {SKU?.SKUName ?? ItemGuid.ToString()}";
    }
}