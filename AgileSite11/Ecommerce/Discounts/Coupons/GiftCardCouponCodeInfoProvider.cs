using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

using CMS.DataEngine;
using CMS.Helpers;
using CMS.SiteProvider;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Class providing GiftCardCouponCodeInfo management.
    /// </summary>
    public class GiftCardCouponCodeInfoProvider : AbstractInfoProvider<GiftCardCouponCodeInfo, GiftCardCouponCodeInfoProvider>
    {
        internal const string TOTAL_REMAINING_VALUE_ALIAS = "TotalRemainingValue";
        internal const string COUPONS_COUNT_ALIAS = "CouponsCount";


        #region "Constructors"

        /// <summary>
        /// Constructor.
        /// </summary>
        public GiftCardCouponCodeInfoProvider()
            : base(GiftCardCouponCodeInfo.TYPEINFO)
        { }

        #endregion


        #region "Public methods - Basic"

        /// <summary>
        /// Returns a query for all the GiftCardCouponCodeInfo objects.
        /// </summary>
        public static ObjectQuery<GiftCardCouponCodeInfo> GetGiftCardCouponCodes()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns a query for all the GiftCardCouponCodeInfo objects on specified site.
        /// </summary>
        public static ObjectQuery<GiftCardCouponCodeInfo> GetGiftCardCouponCodes(SiteInfoIdentifier siteIdentifier)
        {
            return ProviderObject.GetGiftCardCouponCodesInternal(siteIdentifier);
        }


        /// <summary>
        /// Returns GiftCardCouponCodeInfo with specified ID.
        /// </summary>
        /// <param name="id">GiftCardCouponCodeInfo ID</param>
        public static GiftCardCouponCodeInfo GetGiftCardCouponCodeInfo(int id)
        {
            return ProviderObject.GetInfoById(id);
        }


        /// <summary>
        /// Returns GiftCardCouponCodeInfo with specified name.
        /// </summary>
        /// <param name="name">GiftCardCouponCodeInfo name</param>
        /// <param name="siteIdentifier">The site identifier</param>
        public static GiftCardCouponCodeInfo GetGiftCardCouponCodeInfo(string name, SiteInfoIdentifier siteIdentifier)
        {
            return ProviderObject.GetGiftCardCouponCodeInfoInternal(name, siteIdentifier);
        }


        /// <summary>
        /// Returns GiftCardCouponCodeInfo with specified GUID.
        /// </summary>
        /// <param name="guid">GiftCardCouponCodeInfo GUID</param>
        public static GiftCardCouponCodeInfo GetGiftCardCouponCodeInfo(Guid guid)
        {
            return ProviderObject.GetInfoByGuid(guid);
        }


        /// <summary>
        /// Sets (updates or inserts) specified GiftCardCouponCodeInfo.
        /// </summary>
        /// <param name="infoObj">GiftCardCouponCodeInfo to be set</param>
        public static void SetGiftCardCouponCodeInfo(GiftCardCouponCodeInfo infoObj)
        {
            if (infoObj.GiftCardCouponCodeID <= 0 && infoObj.GiftCardCouponCodeRemainingValue <= 0)
            {
                infoObj.GiftCardCouponCodeRemainingValue = infoObj.GiftCardCouponCodeGiftCardValue;
            }

            ProviderObject.SetInfo(infoObj);
        }


        /// <summary>
        /// Deletes specified GiftCardCouponCodeInfo.
        /// </summary>
        /// <param name="infoObj">GiftCardCouponCodeInfo to be deleted</param>
        public static void DeleteGiftCardCouponCodeInfo(GiftCardCouponCodeInfo infoObj)
        {
            ProviderObject.DeleteInfo(infoObj);
        }


        /// <summary>
        /// Deletes GiftCardCouponCodeInfo with specified ID.
        /// </summary>
        /// <param name="id">GiftCardCouponCodeInfo ID</param>
        public static void DeleteGiftCardCouponCodeInfo(int id)
        {
            GiftCardCouponCodeInfo infoObj = GetGiftCardCouponCodeInfo(id);
            DeleteGiftCardCouponCodeInfo(infoObj);
        }

        #endregion


        #region "Public methods - Advanced"

        /// <summary>
        /// Gets information about the total number of coupon codes available for the gift cards with specified IDs and the total remaining value of these coupon codes.
        /// Returns <see cref="ObjectQuery"/> with one table containing GiftCardCouponCodeGiftCardID, GiftCardValue, TotalRemainingValue and CouponCount.
        /// </summary>
        /// <param name="giftCardIDs">IDs of gift cards to get coupon counts for. Use null for all relevant gift cards.</param>
        public static ObjectQuery GetGiftCardCouponCodeUsage(IEnumerable<int> giftCardIDs)
        {
            return ProviderObject.GetGiftCardCouponCodeUsageInternal(giftCardIDs);
        }


        /// <summary>
        /// Returns formatted message about codes.
        /// </summary>
        /// <param name="parent">Parent offer gift card of coupon codes</param>
        /// <param name="dataOnly">If <c>true</c>, only statistics (used / limit) are returned.</param>
        public static string GetGiftCardCouponUsageInfoMessage(GiftCardInfo parent, bool dataOnly = false)
        {
            // Parent is null
            if (parent == null)
            {
                return string.Empty;
            }

            // Get info
            var codes = GetGiftCardCouponCodeUsage(new[] { parent.GiftCardID });
            return GetCouponUsageInfoMessage(codes, dataOnly);
        }


        /// <summary>
        /// Creates and saves new coupon for specified gift card.
        /// NOTE: The uniqueness is not checked.
        /// </summary>
        /// <param name="giftCard">The gift card</param>
        /// <param name="code">The code</param>
        public static GiftCardCouponCodeInfo CreateGiftCardCoupon(GiftCardInfo giftCard, string code)
        {
            return ProviderObject.CreateGiftCardCouponInternal(giftCard, code);
        }

        #endregion


        #region "Internal methods - Basic"

        /// <summary>
        /// Returns a query for all the GiftCardCouponCodeInfo objects on specified site.
        /// </summary>
        protected virtual ObjectQuery<GiftCardCouponCodeInfo> GetGiftCardCouponCodesInternal(SiteInfoIdentifier siteIdentifier)
        {
            return GetObjectQuery()
                .Source(s => s.Join<GiftCardInfo>("GiftCardCouponCodeGiftCardID", "GiftCardID", JoinTypeEnum.Inner,
                    new WhereCondition("GiftCardSiteID", QueryOperator.Equals, siteIdentifier.ObjectID)));
        }


        /// <summary>
        /// Returns GiftCardCouponCodeInfo with specified name on specified site.
        /// </summary>
        /// <param name="name">GiftCardCouponCodeInfo name</param>
        /// <param name="siteIdentifier">The site identifier</param>
        protected virtual GiftCardCouponCodeInfo GetGiftCardCouponCodeInfoInternal(string name, SiteInfoIdentifier siteIdentifier)
        {
            var query = GetGiftCardCouponCodesInternal(siteIdentifier)
                .Where("GiftCardCouponCodeCode", QueryOperator.Equals, name);

            return query.FirstOrDefault();
        }

        #endregion


        #region "Internal methods - Advanced"

        /// <summary>
        /// Returns formatted message about codes.
        /// </summary>
        /// <param name="codes">Coupon codes.</param>
        /// <param name="dataOnly">If <c>true</c>, only statistics (used / limit) are returned.</param>
        protected static string GetCouponUsageInfoMessage(ObjectQuery codes, bool dataOnly)
        {
            if (DataHelper.DataSourceIsEmpty(codes))
            {
                return String.Empty;
            }

            DataRow dr = codes.Tables[0].Rows[0];

            // Get data
            var totalRemaining = ValidationHelper.GetDecimal(dr[TOTAL_REMAINING_VALUE_ALIAS], 0m);
            var couponCount = ValidationHelper.GetInteger(dr[COUPONS_COUNT_ALIAS], 0);
            var giftCardValue = ValidationHelper.GetDecimal(dr["GiftCardValue"], 0m);

            var potentialValue = couponCount * giftCardValue;
            var potentialValueString = CurrencyInfoProvider.GetFormattedPrice(potentialValue, SiteContext.CurrentSiteID);
            var currentUsageString = CurrencyInfoProvider.GetFormattedPrice(potentialValue - totalRemaining, SiteContext.CurrentSiteID);

            var message = dataOnly ? "" : $"{ResHelper.GetString("com.giftcardcouponcode.amountredeemed")}: ";

            // Format message
            return $"{message}{currentUsageString} / {potentialValueString}";
        }


        /// <summary>
        /// Gets information about the total number of coupon codes available for the gift cards with specified IDs and the total remaining value of these coupon codes.
        /// Returns <see cref="ObjectQuery"/> with one table containing GiftCardCouponCodeGiftCardID, GiftCardValue, TotalRemainingValue and CouponCount.
        /// </summary>
        /// <param name="giftCardIDs">IDs of gift cards to get coupon counts for. Use null for all relevant gift cards.</param>
        protected virtual ObjectQuery GetGiftCardCouponCodeUsageInternal(IEnumerable<int> giftCardIDs)
        {
            var parentColumnName = TypeInfo.ParentIDColumn;

            var query = new ObjectQuery(TypeInfo.ObjectType)
                .Source(s => s.Join<GiftCardInfo>(TypeInfo.ParentIDColumn, TypeInfo.ParentTypeInfo.IDColumn));
            query.Columns(
                new AggregatedColumn(AggregationType.Sum, "GiftCardCouponCodeRemainingValue").As(TOTAL_REMAINING_VALUE_ALIAS),
                new CountColumn("1").As(COUPONS_COUNT_ALIAS),
                new QueryColumn(parentColumnName),
                new QueryColumn("GiftCardValue")
            );

            if (giftCardIDs != null)
            {
                query = query.WhereIn(parentColumnName, giftCardIDs.ToArray());
            }

            query = query.GroupBy(parentColumnName, "GiftCardValue");

            return query;
        }


        /// <summary>
        /// Creates and saves new coupon for specified gift card.
        /// NOTE: The uniqueness is not checked.
        /// </summary>
        /// <param name="giftCard">The gift card</param>
        /// <param name="code">The code</param>
        protected virtual GiftCardCouponCodeInfo CreateGiftCardCouponInternal(GiftCardInfo giftCard, string code)
        {
            var coupon = new GiftCardCouponCodeInfo
            {                
                GiftCardCouponCodeGiftCardID = giftCard.GiftCardID,
                GiftCardCouponCodeCode = code,
                GiftCardCouponCodeRemainingValue = giftCard.GiftCardValue
            };

            SetGiftCardCouponCodeInfo(coupon);

            return coupon;
        }


        /// <summary>
        /// Returns a query for <see cref="GiftCardCouponCodeInfo"/> objects for the given <paramref name="giftCardID"/>.
        /// </summary>
        /// <param name="giftCardID">GiftCardInfo identifier</param>
        internal static ObjectQuery<GiftCardCouponCodeInfo> GetGiftCardCouponCodes(int giftCardID)
        {
            return GetGiftCardCouponCodes().WhereEquals("GiftCardCouponCodeGiftCardID", giftCardID);
        }


        /// <summary>
        /// Returns <see cref="GiftCardCouponCodeInfo"/> for the gift card with specified <paramref name="couponCode"/>.
        /// Returns <c>null</c> if no code found.
        /// </summary>
        /// <param name="giftCardID">GiftCardInfo identifier</param>
        /// <param name="couponCode">CouponCode code</param>
        internal static GiftCardCouponCodeInfo GetGiftCardCouponCode(int giftCardID, string couponCode)
        {
            return GetGiftCardCouponCodes()
                .WhereEquals("GiftCardCouponCodeGiftCardID", giftCardID)
                .WhereEquals("GiftCardCouponCodeCode", couponCode)
                .TopN(1)
                .FirstObject;
        }

        #endregion
    }
}