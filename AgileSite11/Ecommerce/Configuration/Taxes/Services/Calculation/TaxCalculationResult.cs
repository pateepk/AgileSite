using System.Diagnostics;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Represents the result of a tax calculation process.
    /// </summary>
    [DebuggerDisplay("{" + nameof(TotalTax) + "}")]
    public class TaxCalculationResult
    {
        /// <summary>
        /// Summary of tax classes and their values.
        /// </summary>
        public ValuesSummary Summary
        {
            get;
        } = new ValuesSummary();


        /// <summary>
        /// Gets or sets calculated tax for items.
        /// </summary>
        public decimal ItemsTax
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets calculated tax for shipping.
        /// </summary>
        public decimal ShippingTax
        {
            get;
            set;
        }


        /// <summary>
        /// Gets total calculated tax.
        /// </summary>
        public decimal TotalTax => ItemsTax + ShippingTax;
    }
}