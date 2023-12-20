using System;

using CMS.DataEngine;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Parameters of the tax calculation.
    /// </summary>
    public class TaxCalculationParameters
    {
        /// <summary>
        /// Customer who is buying goods or services.
        /// </summary>
        public CustomerInfo Customer
        {
            get;
            set;
        }


        /// <summary>
        /// Currency in which the purchase is made.
        /// </summary>
        public CurrencyInfo Currency
        {
            get;
            set;
        }


        /// <summary>
        /// Date of purchase.
        /// </summary>
        public DateTime CalculationDate
        {
            get;
            set;
        }


        /// <summary>
        /// Address where the goods are delivered.
        /// </summary>
        public AddressInfo ShippingAddress
        {
            get;
            set;
        }


        /// <summary>
        /// Billing address of taxed purchase.
        /// </summary>
        public AddressInfo BillingAddress
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the site on which taxes are calculated.
        /// </summary>
        public SiteInfoIdentifier SiteID
        {
            get;
            set;
        }
    }
}