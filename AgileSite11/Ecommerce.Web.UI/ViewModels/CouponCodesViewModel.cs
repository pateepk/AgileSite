using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace CMS.Ecommerce.Web.UI
{
    /// <summary>
    /// View model class allows to display multiple coupons in ASCX and text transformations
    /// </summary>
    public sealed class CouponCodesViewModel : IEnumerable<CouponCodeViewModel>
    {
        private readonly IEnumerable<CouponCodeViewModel> mCoupons;


        /// <summary>
        /// Creates a new instance of <see cref="CouponCodesViewModel"/>.
        /// </summary>
        public CouponCodesViewModel(ICouponCodeCollection coupons)
        {
            if (coupons == null)
            {
                throw new ArgumentNullException(nameof(coupons));
            }

            mCoupons = coupons.AllAppliedCodes.Select(i => new CouponCodeViewModel(i));
            mCoupons = mCoupons.Concat(coupons.NotAppliedInCartCodes.Select(i => new CouponCodeViewModel(i)));
        }


        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }


        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        public IEnumerator<CouponCodeViewModel> GetEnumerator()
        {
            return mCoupons.GetEnumerator();
        }
    }
}
