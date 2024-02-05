using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;

using CMS.DataEngine;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Class providing CouponCodeInfo management.
    /// </summary>
    public class CouponCodeInfoProvider : BaseCouponCodeInfoProvider<CouponCodeInfo, CouponCodeInfoProvider>
    {
        #region "Constructors"

        /// <summary>
        /// Constructor.
        /// </summary>
        public CouponCodeInfoProvider()
            : base(CouponCodeInfo.TYPEINFO)
        { }

        #endregion


        #region "Public methods - Basic"

        /// <summary>
        /// Returns a query for all the CouponCodeInfo objects.
        /// </summary>
        public static ObjectQuery<CouponCodeInfo> GetCouponCodes()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns a query for all the CouponCodeInfo objects on specified site.
        /// </summary>
        public static ObjectQuery<CouponCodeInfo> GetCouponCodes(SiteInfoIdentifier siteIdentifier)
        {
            return ProviderObject.GetCouponCodesInternal(siteIdentifier);
        }


        /// <summary>
        /// Returns CouponCodeInfo with specified ID.
        /// </summary>
        /// <param name="id">CouponCodeInfo ID.</param>
        public static CouponCodeInfo GetCouponCodeInfo(int id)
        {
            return ProviderObject.GetInfoById(id);
        }


        /// <summary>
        /// Returns CouponCodeInfo with specified name.
        /// </summary>
        /// <param name="name">CouponCodeInfo name.</param>
        /// <param name="siteIdentifier">The site identifier.</param>
        public static CouponCodeInfo GetCouponCodeInfo(string name, SiteInfoIdentifier siteIdentifier)
        {
            return ProviderObject.GetCouponCodeInfoInternal(name, siteIdentifier);
        }


        /// <summary>
        /// Returns CouponCodeInfo with specified GUID.
        /// </summary>
        /// <param name="guid">CouponCodeInfo GUID.</param>                
        public static CouponCodeInfo GetCouponCodeInfo(Guid guid)
        {
            return ProviderObject.GetInfoByGuid(guid);
        }


        /// <summary>
        /// Sets (updates or inserts) specified CouponCodeInfo.
        /// </summary>
        /// <param name="infoObj">CouponCodeInfo to be set.</param>
        public static void SetCouponCodeInfo(CouponCodeInfo infoObj)
        {
            ProviderObject.SetInfo(infoObj);
        }


        /// <summary>
        /// Deletes specified CouponCodeInfo.
        /// </summary>
        /// <param name="infoObj">CouponCodeInfo to be deleted.</param>
        public static void DeleteCouponCodeInfo(CouponCodeInfo infoObj)
        {
            ProviderObject.DeleteInfo(infoObj);
        }


        /// <summary>
        /// Deletes CouponCodeInfo with specified ID.
        /// </summary>
        /// <param name="id">CouponCodeInfo ID.</param>
        public static void DeleteCouponCodeInfo(int id)
        {
            CouponCodeInfo infoObj = GetCouponCodeInfo(id);
            DeleteCouponCodeInfo(infoObj);
        }

        #endregion


        #region "Public methods - Advanced"

        /// <summary>
        /// Gets info about how many coupons are available in specific discount and how many of them were already used.
        /// Returns <see cref="ObjectQuery"/> with one table containing CouponCodeDiscountID, Uses, UnlimitedCodeCount and Limit.
        /// </summary>
        /// <param name="discountIDs">IDs of discounts to get coupon counts for. Use null for all relevant discounts.</param>
        public static ObjectQuery GetCouponCodeUseCount(IEnumerable<int> discountIDs)
        {
            return ProviderObject.GetCouponCodeUseCountInternal(discountIDs);
        }


        /// <summary>
        /// Returns formatted message about codes.
        /// </summary>
        /// <param name="parent">Parent offer discount of coupon codes.</param>
        /// <param name="dataOnly">If <c>true</c>, only statistics (used / limit) are returned.</param>
        public static string GetCouponUsageInfoMessage(DiscountInfo parent, bool dataOnly = false)
        {
            // Parent is null
            if (parent == null)
            {
                return String.Empty;
            }

            // Get info
            var codes = GetCouponCodeUseCount(new[] { parent.DiscountID });
            return GetCouponUsageInfoMessage(codes, dataOnly);
        }


        /// <summary>
        /// Creates and saves new coupon for specified discount.
        /// NOTE: The uniqueness is not checked.
        /// </summary>
        /// <param name="discount">The discount.</param>
        /// <param name="code">The code.</param>
        /// <param name="numberOfUses">The number of uses.</param>
        public static CouponCodeInfo CreateCoupon(DiscountInfo discount, string code, int numberOfUses)
        {
            return ProviderObject.CreateCouponInternal(discount, code, numberOfUses);
        }

        #endregion


        #region "Internal methods - Basic"

        /// <summary>
        /// Returns a query for all the CouponCodeInfo objects on specified site.
        /// </summary>
        protected virtual ObjectQuery<CouponCodeInfo> GetCouponCodesInternal(SiteInfoIdentifier siteIdentifier)
        {
            return GetObjectQuery()
                .Source(s => s.Join<DiscountInfo>("CouponCodeDiscountID", "DiscountID", JoinTypeEnum.Inner,
                    new WhereCondition("DiscountSiteID", QueryOperator.Equals, siteIdentifier.ObjectID)));
        }


        /// <summary>
        /// Returns CouponCodeInfo with specified name on specified site.
        /// </summary>
        /// <param name="name">CouponCodeInfo name.</param>
        /// <param name="siteIdentifier">The site identifier.</param>
        protected virtual CouponCodeInfo GetCouponCodeInfoInternal(string name, SiteInfoIdentifier siteIdentifier)
        {
            var query = GetCouponCodesInternal(siteIdentifier)
                .Where("CouponCodeCode", QueryOperator.Equals, name);

            return query.FirstOrDefault();
        }


        /// <summary>
        /// Inserts or Updates the object to the database.
        /// </summary>
        /// <param name="info">Object to insert / update</param>
        protected override void SetInfo(CouponCodeInfo info)
        {
            var discount = info.Parent as DiscountInfo;
            if ((discount != null) && discount.IsCatalogDiscount)
            {
                throw new Exception("[CouponCodeInfoProvider.SetCouponCodeInfoInternal]: Coupon code cannot be assigned to catalog discount.");
            }

            base.SetInfo(info);
        }

        #endregion


        #region "Internal methods - Advanced"

        /// <summary>
        /// Gets info about how many coupons are available in specific discount and how many of them were already used.
        /// Returns <see cref="ObjectQuery"/> with one table containing CouponCodeDiscountID, Uses, UnlimitedCodeCount and Limit.
        /// </summary>
        /// <param name="discountIDs">IDs of discounts to get coupon counts for. Use null for all relevant discounts.</param>
        protected virtual ObjectQuery GetCouponCodeUseCountInternal(IEnumerable<int> discountIDs)
        {
            return GetCouponCodeUseCount(discountIDs, "CouponCodeUseCount", "CouponCodeUseLimit");
        }


        /// <summary>
        /// Creates and saves new coupon for specified discount.
        /// NOTE: The uniqueness is not checked.
        /// </summary>
        /// <param name="discount">The discount.</param>
        /// <param name="code">The code.</param>
        /// <param name="numberOfUses">The number of uses.</param>
        protected virtual CouponCodeInfo CreateCouponInternal(DiscountInfo discount, string code, int numberOfUses)
        {
            var coupon = new CouponCodeInfo
            {
                CouponCodeUseLimit = numberOfUses,
                CouponCodeDiscountID = discount.DiscountID,
                CouponCodeCode = code,
                CouponCodeUseCount = 0
            };

            SetCouponCodeInfo(coupon);

            return coupon;
        }


        /// <summary>
        /// Returns a query for <see cref="CouponCodeInfo"/> objects for the given <paramref name="discountID"/>.
        /// </summary>
        /// <param name="discountID">DiscountInfo identifier.</param>
        internal static ObjectQuery<CouponCodeInfo> GetDiscountCouponCodes(int discountID)
        {
            return GetCouponCodes().WhereEquals("CouponCodeDiscountID", discountID);
        }


        /// <summary>
        /// Returns <see cref="CouponCodeInfo"/> for the discount with specified <paramref name="couponCode"/>.
        /// Returns <c>null</c> if no code found.
        /// </summary>
        /// <param name="discountID">DiscountInfo identifier</param>
        /// <param name="couponCode">CouponCode code</param>
        internal static CouponCodeInfo GetDiscountCouponCode(int discountID, string couponCode)
        {
            return GetCouponCodes()
                .WhereEquals("CouponCodeDiscountID", discountID)
                .WhereEquals("CouponCodeCode", couponCode)
                .TopN(1)
                .FirstObject;
        }

        #endregion
    }
}