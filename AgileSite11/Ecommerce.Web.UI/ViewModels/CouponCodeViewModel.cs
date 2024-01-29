using CMS.Base;

namespace CMS.Ecommerce.Web.UI
{
    /// <summary>
    /// View model class allows to display simple string values in ASCX and text transformations
    /// </summary>
    public sealed class CouponCodeViewModel : INameIndexable<string>
    {
        internal CouponCodeViewModel(ICouponCode couponCode)
        {
            Code = couponCode.Code;
            IsApplied = couponCode.ApplicationStatus == CouponCodeApplicationStatusEnum.AppliedInCart ||
                        couponCode.ApplicationStatus == CouponCodeApplicationStatusEnum.AppliedInOrder;
        }


        object INameIndexable.this[string name] => this[name];


        /// <summary>
        /// String indexer, gets the value with the specified name
        /// </summary>
        /// <param name="name">Name</param>
        public string this[string name]
        {
            get
            {
                switch (name.ToLowerInvariant())
                {
                    case "code":
                        return Code;

                    case "isapplied":
                        return IsApplied.ToString();

                    default: return null;
                }
            }
        }


        /// <summary>
        /// Gets the coupon code.
        /// </summary>
        public string Code
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates that the coupon code is applied.
        /// </summary>
        public bool IsApplied
        {
            get;
        }
    }
}
