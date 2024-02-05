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

[assembly: RegisterCustomClass("GiftCardListExtender", typeof(CMS.Ecommerce.Web.UI.GiftCardListExtender))]

namespace CMS.Ecommerce.Web.UI
{
    /// <summary>
    /// Extender for list of gift cards.
    /// </summary>
    public class GiftCardListExtender : ControlExtender<UniGrid>
    {
        private ObjectTransformationDataProvider mCouponCountsDataProvider;

        private readonly Hashtable giftCards = new Hashtable();

        /// <summary>
        /// Initializes the extender.
        /// </summary>
        public override void OnInit()
        {
            mCouponCountsDataProvider = new ObjectTransformationDataProvider();
            mCouponCountsDataProvider.SetDefaultDataHandler(GetCountsDataHandler);

            if (Control != null)
            {
                Control.OnExternalDataBound += OnExternalDataBound;
            }
        }


        private object OnExternalDataBound(object sender, string sourceName, object parameter)
        {
            var giftCardRow = parameter as DataRowView;
            var giftCardInfo = GetGiftCardInfo(giftCardRow?.Row);

            if (giftCardInfo == null)
            {
                return String.Empty;
            }

            switch (sourceName.ToLowerInvariant())
            {
                case "value":
                    return CurrencyInfoProvider.GetFormattedPrice(giftCardInfo.GiftCardValue, giftCardInfo.GiftCardSiteID);
                    
                case "status":
                    if (sender == null)
                    {
                        return giftCardInfo.Status.ToLocalizedString("com.discountstatus");
                    }

                    return new DiscountStatusTag(mCouponCountsDataProvider, giftCardInfo);

                case "application":
                    return giftCardInfo.HasCoupons ? GiftCardCouponCodeInfoProvider.GetGiftCardCouponUsageInfoMessage(giftCardInfo, true) : ResHelper.GetString("com.discount.notcreated");
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
            DataSet counts = GiftCardCouponCodeInfoProvider.GetGiftCardCouponCodeUsage(discountIDs);

            return counts.ToDictionaryById("GiftCardCouponCodeGiftCardID");
        }


        private GiftCardInfo GetGiftCardInfo(DataRow row)
        {
            if (row == null)
            {
                return null;
            }

            var giftCardId = DataHelper.GetIntValue(row, GiftCardInfo.TYPEINFO.IDColumn);

            var giftCard = giftCards[giftCardId] as GiftCardInfo;
            if (giftCard == null)
            {
                giftCard = new GiftCardInfo(row);
                giftCards[giftCard.GiftCardID] = giftCard;
            }

            return giftCard;
        }
    }
}
