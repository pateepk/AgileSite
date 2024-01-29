using System;

using CMS.DataEngine;
using CMS.Membership;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Parameters used in the product price calculation.
    /// <seealso cref="IProductPricingService"/>
    /// <seealso cref="ProductPricingService"/>
    /// </summary>
    public class PriceParameters
    {
        /// <summary>
        /// Site on which prices are calculated.
        /// </summary>
        public SiteInfoIdentifier SiteID
        {
            get;
            set;
        }


        /// <summary>
        /// Currency of the calculation.
        /// </summary>
        public CurrencyInfo Currency
        {
            get;
            set;
        }


        /// <summary>
        /// User for whom the prices are calculated.
        /// </summary>
        public UserInfo User
        {
            get;
            set;
        }


        /// <summary>
        /// Customer who is buying goods or services.
        /// <remarks>
        /// Can be used in custom code to customize the price and discount calculation.
        /// </remarks>
        /// </summary>
        public CustomerInfo Customer
        {
            get;
            set;
        }


        /// <summary>
        /// Product quantity - used to identify the volume discounts.
        /// </summary>
        public decimal Quantity
        {
            get;
            set;
        } = 1;


        /// <summary>
        /// Date and time when catalog discounts are to be valid.
        /// <remarks>
        /// For most price calculations, the value is equal to the current time. 
        /// When calculating prices for existing orders, the values is equal to the date and time when the order was created.
        /// </remarks>
        /// </summary>
        public DateTime CalculationDate
        {
            get;
            set;
        } = DateTime.Now;
    }
}
