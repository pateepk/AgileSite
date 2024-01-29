using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

using CMS.DataEngine;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Class providing MultiBuyCouponCodeInfo management.
    /// </summary>
    public class MultiBuyCouponCodeInfoProvider : BaseCouponCodeInfoProvider<MultiBuyCouponCodeInfo, MultiBuyCouponCodeInfoProvider>
    {
        #region "Constructors"

        /// <summary>
        /// Constructor
        /// </summary>
        public MultiBuyCouponCodeInfoProvider()
            : base(MultiBuyCouponCodeInfo.TYPEINFO)
        { }

        #endregion


        #region "Public methods - Basic"

        /// <summary>
        /// Returns a query for all the MultiBuyCouponCodeInfo objects.
        /// </summary>
        public static ObjectQuery<MultiBuyCouponCodeInfo> GetMultiBuyCouponCodes()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns a query for all the MultiBuyCouponCodeInfo objects on specified site.
        /// </summary>
        public static ObjectQuery<MultiBuyCouponCodeInfo> GetMultiBuyCouponCodes(SiteInfoIdentifier siteIdentifier)
        {
            return ProviderObject.GetMultiBuyCouponCodesInternal(siteIdentifier);
        }


        /// <summary>
        /// Returns MultiBuyCouponCodeInfo with specified ID.
        /// </summary>
        /// <param name="id">MultiBuyCouponCodeInfo ID</param>
        public static MultiBuyCouponCodeInfo GetMultiBuyCouponCodeInfo(int id)
        {
            return ProviderObject.GetInfoById(id);
        }


        /// <summary>
        /// Returns MultiBuyCouponCodeInfo with specified name.
        /// </summary>
        /// <param name="name">MultiBuyCouponCodeInfo name.</param>
        /// <param name="siteIdentifier">The site identifier.</param>
        public static MultiBuyCouponCodeInfo GetMultiBuyCouponCodeInfo(string name, SiteInfoIdentifier siteIdentifier)
        {
            return ProviderObject.GetMultiBuyCouponCodeInfoInternal(name, siteIdentifier);
        }


        /// <summary>
        /// Returns MultiBuyCouponCodeInfo with specified GUID.
        /// </summary>
        /// <param name="guid">MultiBuyCouponCodeInfo GUID</param>                
        public static MultiBuyCouponCodeInfo GetMultiBuyCouponCodeInfo(Guid guid)
        {
            return ProviderObject.GetInfoByGuid(guid);
        }


        /// <summary>
        /// Sets (updates or inserts) specified MultiBuyCouponCodeInfo.
        /// </summary>
        /// <param name="infoObj">MultiBuyCouponCodeInfo to be set</param>
        public static void SetMultiBuyCouponCodeInfo(MultiBuyCouponCodeInfo infoObj)
        {
            ProviderObject.SetInfo(infoObj);
        }


        /// <summary>
        /// Deletes specified MultiBuyCouponCodeInfo.
        /// </summary>
        /// <param name="infoObj">MultiBuyCouponCodeInfo to be deleted</param>
        public static void DeleteMultiBuyCouponCodeInfo(MultiBuyCouponCodeInfo infoObj)
        {
            ProviderObject.DeleteInfo(infoObj);
        }


        /// <summary>
        /// Deletes MultiBuyCouponCodeInfo with specified ID.
        /// </summary>
        /// <param name="id">MultiBuyCouponCodeInfo ID</param>
        public static void DeleteMultiBuyCouponCodeInfo(int id)
        {
            MultiBuyCouponCodeInfo infoObj = GetMultiBuyCouponCodeInfo(id);
            DeleteMultiBuyCouponCodeInfo(infoObj);
        }

        #endregion


        #region "Public methods - Advanced"

        /// <summary>
        /// Gets info about how many coupons are available in specific bultibuy discount and how many of them were already used.
        /// Returns <see cref="ObjectQuery"/> with one table containing MultiBuyCouponCodeMultiBuyDiscountID, Uses, UnlimitedCodeCount and Limit.
        /// </summary>
        /// <param name="discountIDs">IDs of discounts to get coupon counts for. Use null for all relevant bultibuy discounts.</param>
        public static ObjectQuery GetCouponCodeUseCount(IEnumerable<int> discountIDs)
        {
            return ProviderObject.GetMultiBuyCouponCodeUseCountInternal(discountIDs);
        }


        /// <summary>
        /// Returns formatted message about codes.
        /// </summary>
        /// <param name="parent">Parent offer multibuy discount of coupon codes</param>
        /// <param name="dataOnly">If <c>true</c>, only statistics (used / limit) are returned.</param>
        public static string GetMultiBuyCouponUsageInfoMessage(MultiBuyDiscountInfo parent, bool dataOnly = false)
        {
            // Parent is null
            if (parent == null)
            {
                return String.Empty;
            }

            // Get info
            var codes = GetCouponCodeUseCount(new[] { parent.MultiBuyDiscountID });
            return GetCouponUsageInfoMessage(codes, dataOnly);
        }


        /// <summary>
        /// Creates and saves new coupon for specified discount.
        /// NOTE: The uniqueness is not checked.
        /// </summary>
        /// <param name="discount">The discount.</param>
        /// <param name="code">The code.</param>
        /// <param name="numberOfUses">The number of uses.</param>
        public static MultiBuyCouponCodeInfo CreateCoupon(MultiBuyDiscountInfo discount, string code, int numberOfUses)
        {
            return ProviderObject.CreateCouponInternal(discount, code, numberOfUses);
        }

        #endregion


        #region "Internal methods - Basic"

        /// <summary>
        /// Returns a query for all the MultiBuyCouponCodeInfo objects on specified site.
        /// </summary>
        protected virtual ObjectQuery<MultiBuyCouponCodeInfo> GetMultiBuyCouponCodesInternal(SiteInfoIdentifier siteIdentifier)
        {
            return GetObjectQuery()
                .Source(s => s.Join<MultiBuyDiscountInfo>("MultiBuyCouponCodeMultiBuyDiscountID", "MultiBuyDiscountID", JoinTypeEnum.Inner,
                    new WhereCondition("MultiBuyDiscountSiteID", QueryOperator.Equals, siteIdentifier.ObjectID)));
        }


        /// <summary>
        /// Returns MultiBuyCouponCodeInfo with specified name on specified site.
        /// </summary>
        /// <param name="name">MultiBuyCouponCodeInfo name.</param>
        /// <param name="siteIdentifier">The site identifier.</param>
        protected virtual MultiBuyCouponCodeInfo GetMultiBuyCouponCodeInfoInternal(string name, SiteInfoIdentifier siteIdentifier)
        {
            var query = GetMultiBuyCouponCodesInternal(siteIdentifier)
                .Where("MultiBuyCouponCodeCode", QueryOperator.Equals, name);

            return query.FirstOrDefault();
        }

        #endregion


        #region "Internal methods - Advanced"

        /// <summary>
        /// Gets info about how many coupons are available in specific multibuy discount and how many of them were already used.
        /// Returns <see cref="ObjectQuery"/> with one table containing MultiBuyCouponCodeMultiBuyDiscountID, Uses, UnlimitedCodeCount and Limit.
        /// </summary>
        /// <param name="discountIDs">IDs of discounts to get coupon counts for. Use null for all relevant multibuy discounts.</param>
        protected virtual ObjectQuery GetMultiBuyCouponCodeUseCountInternal(IEnumerable<int> discountIDs)
        {
            return GetCouponCodeUseCount(discountIDs, "MultiBuyCouponCodeUseCount", "MultiBuyCouponCodeUseLimit");
        }


        /// <summary>
        /// Creates and saves new coupon for specified discount.
        /// NOTE: The uniqueness is not checked.
        /// </summary>
        /// <param name="discount">The discount.</param>
        /// <param name="code">The code.</param>
        /// <param name="numberOfUses">The number of uses.</param>
        protected virtual MultiBuyCouponCodeInfo CreateCouponInternal(MultiBuyDiscountInfo discount, string code, int numberOfUses)
        {
            var coupon = new MultiBuyCouponCodeInfo
            {
                MultiBuyCouponCodeUseLimit = numberOfUses,
                MultiBuyCouponCodeMultiBuyDiscountID = discount.MultiBuyDiscountID,
                MultiBuyCouponCodeCode = code,
                MultiBuyCouponCodeUseCount = 0
            };

            SetMultiBuyCouponCodeInfo(coupon);

            return coupon;
        }


        /// <summary>
        /// Returns a query for <see cref="MultiBuyCouponCodeInfo"/> objects for the given <paramref name="discountID"/>.
        /// </summary>
        /// <param name="discountID">MultiBuyDiscountInfo identifier.</param>
        internal static ObjectQuery<MultiBuyCouponCodeInfo> GetDiscountCouponCodes(int discountID)
        {
            return GetMultiBuyCouponCodes().WhereEquals("MultiBuyCouponCodeMultiBuyDiscountID", discountID);
        }


        /// <summary>
        /// Returns <see cref="MultiBuyCouponCodeInfo"/> for the discount with specified <paramref name="couponCode"/>.
        /// Returns <c>null</c> if no code found.
        /// </summary>
        /// <param name="discountID">MultiBuyDiscountInfo identifier</param>
        /// <param name="couponCode">MultiBuyCouponCode code</param>
        internal static MultiBuyCouponCodeInfo GetDiscountCouponCode(int discountID, string couponCode)
        {
            return GetMultiBuyCouponCodes()
                .WhereEquals("MultiBuyCouponCodeMultiBuyDiscountID", discountID)
                .WhereEquals("MultiBuyCouponCodeCode", couponCode)
                .TopN(1)
                .FirstObject;
        }

        #endregion
    }
}