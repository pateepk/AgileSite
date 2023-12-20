using System.Collections.Generic;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Represents the tax calculation request. Contains all the parameters needed to evaluate taxes applied to the purchase.
    /// </summary>
    public class TaxCalculationRequest
    {
        /// <summary>
        /// Items to calculate taxes for.
        /// </summary>
        public readonly ICollection<TaxItem> Items = new List<TaxItem>();


        /// <summary>
        /// The price of the shipping.
        /// </summary>
        public decimal ShippingPrice
        {
            get;
            set;
        }


        /// <summary>
        /// Shipping option used to deliver items being calculated.
        /// </summary>
        public ShippingOptionInfo Shipping
        {
            get;
            set;
        }


        /// <summary>
        /// Other parameters of tax calculation.
        /// </summary>
        public TaxCalculationParameters TaxParameters
        {
            get;
            set;
        }


        /// <summary>
        /// The discount amout to apply to the purchase.
        /// </summary>
        /// <remarks>
        /// By default, this is the amount of applied order discounts, not a percent.
        /// </remarks>
        public decimal Discount
        {
            get;
            set;
        }
    }
}