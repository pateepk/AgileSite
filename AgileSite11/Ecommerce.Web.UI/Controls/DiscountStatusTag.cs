using System;

using CMS.Base.Web.UI;
using CMS.Helpers;
using CMS.UIControls;

namespace CMS.Ecommerce.Web.UI
{
    /// <summary>
    /// Control for displaying discount status.
    /// </summary>
    public sealed class DiscountStatusTag : Tag
    {
        /// <summary>
        /// Data provider for coupons usage info.
        /// </summary>
        private ObjectTransformationDataProvider DataProvider
        {
            get;
        }


        /// <summary>
        /// Discount object.
        /// </summary>
        private IDiscountInfo Discount
        {
            get;
        }


        /// <summary>
        /// Initializes a new instance of the DiscountStatusTag class.
        /// </summary>
        /// <param name="dataProvider">The data provider.</param>
        /// <param name="discount">The discount.</param>
        public DiscountStatusTag(ObjectTransformationDataProvider dataProvider, IDiscountInfo discount)
        {
            DataProvider = dataProvider;
            Discount = discount;

            if ((DataProvider != null) && (Discount != null) && Discount.DiscountUsesCoupons)
            {
                DataProvider.RequireObject("CouponsCounts", Discount.DiscountID);
            }
        }


        /// <summary>
        /// Require coupons usage info if discount uses coupons.
        /// </summary>
        protected override void OnPreRender(EventArgs e)
        {
            var status = GetStatus();

            Text = status.ToLocalizedString("com.discountstatus");

            CssClass = "tag ";

            // Set tag color according discount type
            switch (status)
            {
                case DiscountStatusEnum.Active:
                    CssClass += "tag-active";
                    break;

                case DiscountStatusEnum.NotStarted:
                    CssClass += "tag-scheduled";
                    break;

                case DiscountStatusEnum.Incomplete:
                    CssClass += "tag-incomplete";
                    break;

                default:
                    CssClass += "tag-default";
                    break;
            }

            base.OnPreRender(e);
        }


        /// <summary>
        /// Returns discount status.
        /// </summary>
        private DiscountStatusEnum GetStatus()
        {
            return Discount?.DiscountStatus ?? DiscountStatusEnum.Incomplete;
        }
    }
}
