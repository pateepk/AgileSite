using System;

using CMS.DataEngine;
using CMS.Membership;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Represents set of discounts parameters used e.g. for discounts filtering and querying.
    /// </summary>
    public class DiscountsParameters
    {
        /// <summary>
        /// ID or code name of the site to which discounts belong.
        /// </summary>
        public SiteInfoIdentifier SiteID
        {
            get;
            set;
        }


        /// <summary>
        /// Specifies if discounts are enabled. This flag is irrelevant when <c>null</c>.
        /// </summary>
        public bool? Enabled
        {
            get;
            set;
        }


        /// <summary>
        /// Date and time when discounts are to be valid. Date is irrelevant when <c>null</c>.
        /// </summary>
        public DateTime? DueDate
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the user for who the discounts are to be valid.
        /// </summary>
        public UserInfo User
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the currency in which the discounts are expressed.
        /// </summary>
        public CurrencyInfo Currency
        {
            get;
            set;
        }


        /// <summary>
        /// Collection of coupon codes.
        /// Empty collection means that discounts do not need coupons (only discount that doesn't use coupons).
        /// </summary>
        public IReadOnlyCouponCodeCollection CouponCodes
        {
            get;
            set;
        } = new CouponCodeCollection();
    }
}
