using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;

using CMS;
using CMS.Base;
using CMS.Base.Web.UI;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.UIControls;

[assembly: RegisterCustomClass("MultiBuyDiscountListExtender", typeof(CMS.Ecommerce.Web.UI.MultiBuyDiscountListExtender))]

namespace CMS.Ecommerce.Web.UI
{
    /// <summary>
    /// MultiBuy discount list extender
    /// </summary>
    public class MultiBuyDiscountListExtender : ControlExtender<UniGrid>
    {
        private ObjectTransformationDataProvider couponCountsDataProvider;

        private readonly Hashtable discounts = new Hashtable();


        /// <summary>
        /// Initializes the extender.
        /// </summary>
        public override void OnInit()
        {
            couponCountsDataProvider = new ObjectTransformationDataProvider();
            couponCountsDataProvider.SetDefaultDataHandler(GetCountsDataHandler);

            if (Control != null)
            {
                Control.OnExternalDataBound += OnExternalDataBound;
            }
        }


        /// <summary>
        /// External data bound handler.
        /// </summary>
        protected object OnExternalDataBound(object sender, string sourceName, object parameter)
        {
            var discountRow = parameter as DataRowView;
            var discountInfo = GetDiscountInfo(discountRow?.Row);

            if (discountInfo == null)
            {
                return String.Empty;
            }

            switch (sourceName.ToLowerInvariant())
            {
                case "status":
                    // Ensure correct values for unigrid export
                    if (sender == null)
                    {
                        return discountInfo.Status.ToLocalizedString("com.discountstatus");
                    }

                    return new DiscountStatusTag(couponCountsDataProvider, discountInfo);

                case "application":
                    // Display dash if discount don't use coupons 
                    if (!discountInfo.MultiBuyDiscountUsesCoupons)
                    {
                        return "&mdash;";
                    }

                    return discountInfo.HasCoupons ? MultiBuyCouponCodeInfoProvider.GetMultiBuyCouponUsageInfoMessage(discountInfo, true) : ResHelper.GetString("com.discount.notcreated");
            }

            return parameter;
        }


        /// <summary>
        /// Returns dictionary of discount coupon use count and limit. Key of the dictionary is the ID of discount.
        /// </summary>
        /// <param name="type">Object type (ignored).</param>
        /// <param name="discountIDs">IDs of discount which the dictionary is to be filled with.</param>
        private SafeDictionary<int, IDataContainer> GetCountsDataHandler(string type, IEnumerable<int> discountIDs)
        {
            DataSet counts = MultiBuyCouponCodeInfoProvider.GetCouponCodeUseCount(discountIDs);

            return counts.ToDictionaryById("MultiBuyCouponCodeMultiBuyDiscountID");
        }


        private MultiBuyDiscountInfo GetDiscountInfo(DataRow row)
        {
            if (row == null)
            {
                return null;
            }

            var discountId = DataHelper.GetIntValue(row, MultiBuyDiscountInfo.TYPEINFO.IDColumn);

            var discount = discounts[discountId] as MultiBuyDiscountInfo;
            if (discount == null)
            {
                discount = new MultiBuyDiscountInfo(row);
                discounts[discount.MultiBuyDiscountID] = discount;
            }

            return discount;
        }
    }
}