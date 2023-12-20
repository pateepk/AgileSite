using System;
using System.Collections.Generic;

using CMS.Base;
using CMS.DataEngine;
using CMS.Membership;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Input data necessary to perform calculation.
    /// </summary>
    public class CalculationRequest
    {
        /// <summary>
        /// Items to be calculated.
        /// </summary>
        public IEnumerable<CalculationRequestItem> Items
        {
            get;
            set;
        }


        /// <summary>
        /// Currency in what the calculation is to be performed.
        /// </summary>
        public CurrencyInfo Currency
        {
            get;
            set;
        }


        /// <summary>
        /// The method of the payment.
        /// </summary>
        public PaymentOptionInfo PaymentOption
        {
            get;
            set;
        }


        /// <summary>
        /// Site on which the calculation occurs.
        /// </summary>
        public SiteInfoIdentifier Site
        {
            get;
            set;
        }


        /// <summary>
        /// User for whom the calculation happens.
        /// </summary>
        public UserInfo User
        {
            get;
            set;
        }


        /// <summary>
        /// Customer for whom the calculation happens.
        /// </summary>
        public CustomerInfo Customer
        {
            get;
            set;
        }


        /// <summary>
        /// Collection of coupon codes affecting the calculation.
        /// </summary>
        public IReadOnlyCouponCodeCollection CouponCodes
        {
            get;
            set;
        } = new CouponCodeCollection();


        /// <summary>
        /// Billing address for the resulting order.
        /// </summary>
        public AddressInfo BillingAddress
        {
            get;
            set;
        }


        /// <summary>
        /// Shipping address for the resulting order.
        /// </summary>
        public AddressInfo ShippingAddress
        {
            get;
            set;
        }


        /// <summary>
        /// Shipping method for the resulting order.
        /// </summary>
        public ShippingOptionInfo ShippingOption
        {
            get;
            set;
        }


        /// <summary>
        /// Sum of all items' weight.
        /// </summary>
        public double TotalItemsWeight
        {
            get;
            set;
        }

        
        /// <summary>
        /// Calculation request custom data.
        /// </summary>
        public IDataContainer RequestCustomData
        {
            get;
            set;
        }


        /// <summary>
        /// Date and time when discounts and taxes are to be valid.
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


        /// <summary>
        /// Gets the order ID in case that calculation runs for already created order.
        /// </summary>
        /// <remarks>
        /// Returns 0 value when calculation runs prior order creation.
        /// </remarks>
        public int OrderId
        {
            get;
            internal set;
        }
    }
}
