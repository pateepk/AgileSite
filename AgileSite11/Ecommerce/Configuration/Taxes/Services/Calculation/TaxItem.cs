using System.Diagnostics;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Item of taxed purchase.
    /// </summary>
    [DebuggerDisplay("{" + nameof(DebuggerDisplay) + ",nq}")]
    public class TaxItem
    {
        /// <summary>
        /// Taxed SKUInfo object.
        /// </summary>
        public SKUInfo SKU
        {
            get;
            set;
        }


        /// <summary>
        ///  Amount of taxed SKUs.
        /// </summary>
        public decimal Quantity
        {
            get;
            set;
        }


        /// <summary>
        /// Price of all units of this item.
        /// </summary>
        public decimal Price
        {
            get;
            set;
        }


        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string DebuggerDisplay => $"{Quantity} x {SKU?.SKUName} = {Price}";
    }
}