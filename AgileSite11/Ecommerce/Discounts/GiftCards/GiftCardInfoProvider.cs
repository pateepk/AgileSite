using System;
using System.Linq;

using CMS.Base;
using CMS.DataEngine;
using CMS.SiteProvider;
using CMS.Helpers;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Class providing <see cref="GiftCardInfo"/> management.
    /// </summary>
    public class GiftCardInfoProvider : AbstractInfoProvider<GiftCardInfo, GiftCardInfoProvider>
    {
        #region "Constructors"

        /// <summary>
        /// Creates an instance of <see cref="GiftCardInfoProvider"/>.
        /// </summary>
        public GiftCardInfoProvider()
            : base(GiftCardInfo.TYPEINFO, new HashtableSettings() {ID = true, Name = true, Load = LoadHashtableEnum.All})
        {
        }

        #endregion


        #region "Public methods - Basic"

        /// <summary>
        /// Returns a query for all the <see cref="GiftCardInfo"/> objects.
        /// </summary>
        public static ObjectQuery<GiftCardInfo> GetGiftCards()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns <see cref="GiftCardInfo"/> with specified ID.
        /// </summary>
        /// <param name="id"><see cref="GiftCardInfo"/> ID</param>
        public static GiftCardInfo GetGiftCardInfo(int id)
        {
            return ProviderObject.GetInfoById(id);
        }


        /// <summary>
        /// Returns <see cref="GiftCardInfo"/> with specified name.
        /// </summary>
        /// <param name="name"><see cref="GiftCardInfo"/> name</param>
        /// <param name="siteName">Site name</param>
        public static GiftCardInfo GetGiftCardInfo(string name, string siteName)
        {
            return ProviderObject.GetInfoByCodeName(name, SiteInfoProvider.GetSiteID(siteName));
        }


        /// <summary>
        /// Sets (updates or inserts) specified <see cref="GiftCardInfo"/>.
        /// </summary>
        /// <param name="infoObj"><see cref="GiftCardInfo"/> to be set</param>
        public static void SetGiftCardInfo(GiftCardInfo infoObj)
        {
            ProviderObject.SetInfo(infoObj);
        }


        /// <summary>
        /// Deletes specified <see cref="GiftCardInfo"/>.
        /// </summary>
        /// <param name="infoObj"><see cref="GiftCardInfo"/> to be deleted</param>
        public static void DeleteGiftCardInfo(GiftCardInfo infoObj)
        {
            ProviderObject.DeleteInfo(infoObj);
        }


        /// <summary>
        /// Deletes <see cref="GiftCardInfo"/> with specified ID.
        /// </summary>
        /// <param name="id"><see cref="GiftCardInfo"/> ID</param>
        public static void DeleteGiftCardInfo(int id)
        {
            GiftCardInfo infoObj = GetGiftCardInfo(id);
            DeleteGiftCardInfo(infoObj);
        }

        #endregion


        #region "Public methods - Advanced"

        /// <summary>
        /// Returns dataset of all gift cards for specified conditions.
        /// </summary>
        /// <param name="parameters">Parameters to return gift cards for</param>
        /// <returns>Dataset of gift cards</returns>
        public static ObjectQuery<GiftCardInfo> GetGiftCards(DiscountsParameters parameters)
        {
            return ProviderObject.GetGiftCardsInternal(parameters);
        }


        /// <summary>
        /// Indicates if user is authorized to modify gift card.
        /// </summary>
        /// <param name="siteName">Site name</param>
        /// <param name="user">User to be checked</param>
        /// <param name="exceptionOnFailure">If <c>true</c>, PermissionCheckException is thrown whenever a permission check fails</param>
        public static bool IsUserAuthorizedToModifyGiftCard(string siteName, IUserInfo user, bool exceptionOnFailure = false)
        {
            return ECommerceHelper.IsUserAuthorizedForPermission(EcommercePermissions.DISCOUNTS_MODIFY, siteName, user, exceptionOnFailure);
        }


        /// <summary>
        /// Indicates if user is authorized to read gift card.
        /// </summary>
        /// <param name="site">Site identifier</param>
        /// <param name="user">User to be checked</param>
        public static bool IsUserAuthorizedToReadGiftCard(SiteInfoIdentifier site, IUserInfo user)
        {
            return ECommerceHelper.IsUserAuthorizedForPermission(EcommercePermissions.DISCOUNTS_READ, site, user);
        }


        /// <summary>
        /// Informs this gift card that it was applied.
        /// </summary>
        /// <param name="giftCard">The gift card</param>
        /// <param name="couponCode">The coupon code that was used for gift card application</param>
        /// <param name="paymentValue">The amount paid by gift card coupon.</param>
        public static void LogGiftCardUseOnce(GiftCardInfo giftCard, string couponCode, decimal paymentValue)
        {
            ProviderObject.LogGiftCardUseOnceInternal(giftCard, couponCode, paymentValue);
        }


        /// <summary>
        /// Gets running gift cards on given sites in defined time. If gift card uses coupons, only gift cards with usable coupons are returned.
        /// </summary>
        /// <param name="site">Site to get active gift cards for</param>
        /// <param name="date">Time when gift card should be active</param>
        public static ObjectQuery<GiftCardInfo> GetRunningGiftCards(SiteInfoIdentifier site, DateTime date)
        {
            return ProviderObject.GetRunningGiftCardsInternal(site, date);
        }

        #endregion


        #region "Internal methods - Basic"

        /// <summary>
        /// Inserts or Updates the object to the database.
        /// </summary>
        /// <param name="info">Object to insert / update</param>
        protected override void SetInfo(GiftCardInfo info)
        {
            // In case gift card is not restricted by roles, delete selected roles configuration
            if (info.GiftCardCustomerRestriction != DiscountCustomerEnum.SelectedRoles)
            {
                info.GiftCardRoles = "";
            }

            base.SetInfo(info);
        }

        #endregion


        #region "Internal methods - Advanced"

        /// <summary>
        /// Returns dataset of all gift cards for specified conditions.
        /// </summary>
        /// <param name="parameters">Parameters to return gift cards for</param>
        /// <returns>Dataset of gift cards</returns>
        protected virtual ObjectQuery<GiftCardInfo> GetGiftCardsInternal(DiscountsParameters parameters)
        {
            var query = GetGiftCards().OnSite(parameters.SiteID);

            // Filter enabled gift cards
            if (parameters.Enabled.HasValue)
            {
                query.WhereEquals("GiftCardEnabled", parameters.Enabled.Value);
            }

            // Add time restriction
            if (parameters.DueDate.HasValue)
            {
                var date = parameters.DueDate.Value;
                query.Where(new WhereCondition().Where("GiftCardValidFrom", QueryOperator.LessThan, date)
                                            .Or()
                                            .WhereNull("GiftCardValidFrom"));

                query.Where(new WhereCondition().Where("GiftCardValidTo", QueryOperator.LargerThan, date)
                                                .Or()
                                                .WhereNull("GiftCardValidTo"));
            }

            // Select public gift cards only
            if (parameters.User == null)
            {
                query.WhereEquals("GiftCardCustomerRestriction", DiscountCustomerEnum.All.ToStringRepresentation());
            }

            // Check gift card coupons
            var couponsWhere = new WhereCondition();

            var couponCodes = parameters.CouponCodes;
            var cartAppliedCodes = couponCodes.CartAppliedCodes.Select(x => x.Code).ToList();

            if (cartAppliedCodes.Any())
            {
                var codesWhere = new IDQuery<GiftCardCouponCodeInfo>("GiftCardCouponCodeGiftCardID");
                var orderCouponCodes = couponCodes.OrderAppliedCodes.Select(x => x.Code).ToList();

                if (!orderCouponCodes.Any())
                {
                    codesWhere.WhereIn("GiftCardCouponCodeCode", cartAppliedCodes);
                }
                else
                {
                    codesWhere.Where(new WhereCondition().WhereIn("CouponCodeCode", orderCouponCodes))
                              .Or()
                              .Where(new WhereCondition().WhereIn("CouponCodeCode", cartAppliedCodes)
                                                         .And()
                                                         .WhereGreaterThan("GiftCardCouponCodeRemainingValue", 0));
                }

                couponsWhere.WhereIn("GiftCardID", codesWhere);
            }

            query.Where(couponsWhere);
            return query;
        }


        /// <summary>
        /// Informs this gift card that it was applied.
        /// </summary>
        /// <param name="giftCard">The gift card</param>
        /// <param name="couponCode">The coupon code that was used for gift card application</param>
        /// <param name="paymentValue">The amount paid by gift card coupon.</param>      
        protected virtual void LogGiftCardUseOnceInternal(GiftCardInfo giftCard, string couponCode, decimal paymentValue)
        {
            if (giftCard == null)
            {
                return;
            }

            // Log coupon code use only if not exceeded
            var coupon = GiftCardCouponCodeInfoProvider.GetGiftCardCouponCode(giftCard.GiftCardID, couponCode);
            if ((coupon != null) && coupon.GiftCardCouponCodeRemainingValue >= paymentValue)
            {
                coupon.GiftCardCouponCodeRemainingValue -= paymentValue;
                coupon.Update();
            }
        }


        /// <summary>
        /// Gets running gift cards on given sites in defined time.
        /// </summary>
        /// <param name="site">Site to get active gift cards for</param>
        /// <param name="date">Time when gift card should be active</param>
        protected virtual ObjectQuery<GiftCardInfo> GetRunningGiftCardsInternal(SiteInfoIdentifier site, DateTime date)
        {
            // Gift cards with available coupon codes
            var codesWhere = new IDQuery(GiftCardCouponCodeInfo.OBJECT_TYPE, "GiftCardCouponCodeGiftCardID")
                                 .WhereGreaterThan("GiftCardCouponCodeRemainingValue", 0);

            // Running gift cards query
            var query = GetGiftCards().OnSite(site).WhereTrue("GiftCardEnabled");

            return query.And(new WhereCondition().Where("GiftCardValidFrom", QueryOperator.LessThan, date)
                                             .Or()
                                             .WhereNull("GiftCardValidFrom"))
                        .And(new WhereCondition().Where("GiftCardValidTo", QueryOperator.LargerThan, date)
                                             .Or()
                                             .WhereNull("GiftCardValidTo"))
                        .And(new WhereCondition().WhereIn("GiftCardID", codesWhere));
        }

        #endregion
    }
}