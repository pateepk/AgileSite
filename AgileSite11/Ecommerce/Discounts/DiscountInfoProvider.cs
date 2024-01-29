using System;
using System.Linq;

using CMS.Base;
using CMS.DataEngine;
using CMS.DataEngine.Query;
using CMS.SiteProvider;
using CMS.Helpers;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Class providing DiscountInfo management.
    /// </summary>
    public class DiscountInfoProvider : AbstractInfoProvider<DiscountInfo, DiscountInfoProvider>
    {
        #region "Constructors"

        /// <summary>
        /// Constructor.
        /// </summary>
        public DiscountInfoProvider()
            : base(DiscountInfo.TYPEINFO, new HashtableSettings
                {
                    ID = true,
                    Name = true,
                    Load = LoadHashtableEnum.All
                })
        {
        }

        #endregion


        #region "Public methods - Basic"

        /// <summary>
        /// Returns discount with specified ID.
        /// </summary>
        /// <param name="discountId">Discount ID</param>        
        public static DiscountInfo GetDiscountInfo(int discountId)
        {
            return ProviderObject.GetInfoById(discountId);
        }


        /// <summary>
        /// Returns discount with specified name.
        /// </summary>
        /// <param name="discountName">Discount name</param>                
        /// <param name="siteName">Site name</param>                
        public static DiscountInfo GetDiscountInfo(string discountName, string siteName)
        {
            return ProviderObject.GetInfoByCodeName(discountName, SiteInfoProvider.GetSiteID(siteName), true);
        }


        /// <summary>
        /// Sets (updates or inserts) specified discount.
        /// </summary>
        /// <param name="discountObj">Discount to be set</param>
        public static void SetDiscountInfo(DiscountInfo discountObj)
        {
            ProviderObject.SetInfo(discountObj);
        }


        /// <summary>
        /// Deletes specified discount.
        /// </summary>
        /// <param name="discountObj">Discount to be deleted</param>
        public static void DeleteDiscountInfo(DiscountInfo discountObj)
        {
            ProviderObject.DeleteInfo(discountObj);
        }


        /// <summary>
        /// Deletes discount with specified ID.
        /// </summary>
        /// <param name="discountId">Discount ID</param>
        public static void DeleteDiscountInfo(int discountId)
        {
            var discountObj = GetDiscountInfo(discountId);
            DeleteDiscountInfo(discountObj);
        }

        #endregion


        #region "Public methods - Advanced"

        /// <summary>
        /// Returns the query for all discounts.
        /// </summary>
        public static ObjectQuery<DiscountInfo> GetDiscounts()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns dataset of all discounts for specified site.
        /// </summary>
        /// <param name="siteId">Site ID</param>  
        /// <param name="onlyEnabled">True - only enable discounts are returned.
        /// False - both enabled and disabled discounts are returned.</param>
        public static ObjectQuery<DiscountInfo> GetDiscounts(int siteId, bool onlyEnabled = false)
        {
            return ProviderObject.GetDiscountsInternal(siteId, onlyEnabled);
        }


        /// <summary>
        /// Returns dataset of all discounts for specified conditions.
        /// </summary>
        /// <param name="parameters">Parameters to return discounts for.</param>
        /// <returns>Dataset of discounts</returns>
        public static ObjectQuery<DiscountInfo> GetDiscounts(DiscountsParameters parameters)
        {
            return ProviderObject.GetDiscountsInternal(parameters);
        }


        /// <summary>
        /// Indicates if user is authorized to modify discount.
        /// </summary>
        /// <param name="siteName">Site name</param>
        /// <param name="user">User to be checked</param>
        /// <param name="exceptionOnFailure">If <c>true</c>, PermissionCheckException is thrown whenever a permission check fails</param>
        public static bool IsUserAuthorizedToModifyDiscount(string siteName, IUserInfo user, bool exceptionOnFailure = false)
        {
            return ECommerceHelper.IsUserAuthorizedForPermission(EcommercePermissions.DISCOUNTS_MODIFY, siteName, user, exceptionOnFailure);
        }


        /// <summary>
        /// Indicates if user is authorized to read discount.
        /// </summary>
        /// <param name="site">Site identifier</param>
        /// <param name="user">User to be checked</param>
        public static bool IsUserAuthorizedToReadDiscount(SiteInfoIdentifier site, IUserInfo user)
        {
            return ECommerceHelper.IsUserAuthorizedForPermission(EcommercePermissions.DISCOUNTS_READ, site, user);
        }


        /// <summary>
        /// Informs this discount that it was applied.
        /// </summary>
        /// <param name="discount">The discount.</param>
        /// <param name="couponCode">The coupon code that was used for discount application.</param>
        public static void LogDiscountUseOnce(DiscountInfo discount, string couponCode = null)
        {
            ProviderObject.LogDiscountUseOnceInternal(discount, couponCode);
        }


        /// <summary>
        /// Gets running discounts on given sites in defined time. If discount uses coupons, only discounts with usable coupons are returned.
        /// </summary>
        /// <param name="site">Site to get active discounts for.</param>
        /// <param name="date">Time when discount should be active.</param>
        /// <param name="discountApplicationEnum">Type of discount which will be selected. If null all types are selected.</param>
        public static ObjectQuery<DiscountInfo> GetRunningDiscounts(SiteInfoIdentifier site, DateTime date, DiscountApplicationEnum? discountApplicationEnum = null)
        {
            return ProviderObject.GetRunningDiscountsInternal(site, date, discountApplicationEnum);
        }

        #endregion


        #region "Internal methods - Basic"

        /// <summary>
        /// Inserts or Updates the object to the database.
        /// </summary>
        /// <param name="info">Object to insert / update</param>
        protected override void SetInfo(DiscountInfo info)
        {
            // In case discount is not restricted by roles, delete selected roles configuration
            if (info.DiscountCustomerRestriction != DiscountCustomerEnum.SelectedRoles)
            {
                info.DiscountRoles = "";
            }

            base.SetInfo(info);
        }

        #endregion


        #region "Internal methods - Advanced"

        /// <summary>
        /// Returns dataset of all discounts for specified site.
        /// </summary>
        /// <param name="siteId">Site ID</param>  
        /// <param name="onlyEnabled">True - only enable discounts are returned.
        /// False - both enabled and disabled discounts are returned.</param>
        protected virtual ObjectQuery<DiscountInfo> GetDiscountsInternal(int siteId, bool onlyEnabled)
        {
            var query = GetDiscounts().OnSite(siteId);

            // Filter enabled discount levels
            if (onlyEnabled)
            {
                query.WhereTrue("DiscountEnabled");
            }

            return query;
        }


        /// <summary>
        /// Returns dataset of all discounts for specified conditions.
        /// </summary>
        /// <param name="parameters">Parameters to return discounts for.</param>
        /// <returns>Dataset of discounts</returns>
        protected virtual ObjectQuery<DiscountInfo> GetDiscountsInternal(DiscountsParameters parameters)
        {
            var query = GetDiscounts().OnSite(parameters.SiteID);

            // Filter enabled discount levels
            if (parameters.Enabled.HasValue)
            {
                query.WhereEquals("DiscountEnabled", parameters.Enabled.Value);
            }

            // Add time restriction
            if (parameters.DueDate.HasValue)
            {
                var date = parameters.DueDate.Value;
                query.Where(new WhereCondition().Where("DiscountValidFrom", QueryOperator.LessThan, date)
                                            .Or()
                                            .WhereNull("DiscountValidFrom"));

                query.Where(new WhereCondition().Where("DiscountValidTo", QueryOperator.LargerThan, date)
                                                .Or()
                                                .WhereNull("DiscountValidTo"));
            }

            // Select public discounts only
            if (parameters.User == null)
            {
                query.WhereEquals("DiscountCustomerRestriction", DiscountCustomerEnum.All.ToStringRepresentation());
            }

            var couponCodes = parameters.CouponCodes;

            // Check if we care about coupons
            if (couponCodes != null)
            {
                var couponsWhere = new WhereCondition().WhereFalse("DiscountUsesCoupons");
                var cartAppliedCodes = couponCodes.CartAppliedCodes.Select(x => x.Code).ToList();

                if (cartAppliedCodes.Any())
                {
                    var codesWhere = new IDQuery<CouponCodeInfo>("CouponCodeDiscountID");
                    var orderAppliedCodes = couponCodes.OrderAppliedCodes.Select(x => x.Code).ToList();

                    if (!orderAppliedCodes.Any())
                    {
                        codesWhere.WhereIn("CouponCodeCode", cartAppliedCodes);
                    }
                    else
                    {
                        codesWhere.Where(new WhereCondition().WhereIn("CouponCodeCode", cartAppliedCodes))
                                  .Or()
                                  .Where(new WhereCondition().WhereIn("CouponCodeCode", orderAppliedCodes)
                                                             .And()
                                                             .Where(new WhereCondition()
                                                                 .WhereNull("CouponCodeUseLimit")
                                                                 .Or()
                                                                 .WhereLessThan("CouponCodeUseCount", "CouponCodeUseLimit".AsColumn())));
                    }

                    couponsWhere.Or().WhereIn("DiscountID", codesWhere);
                }

                query.Where(couponsWhere);
            }

            return query;
        }


        /// <summary>
        /// Informs this discount that it was applied.
        /// </summary>
        /// <param name="discount">The discount.</param>
        /// <param name="couponCode">The coupon code that was used for discount application.</param>
        protected virtual void LogDiscountUseOnceInternal(DiscountInfo discount, string couponCode = null)
        {
            if ((discount == null) || (!discount.DiscountUsesCoupons))
            {
                return;
            }

            // Log coupon code use only if not exceeded
            var coupon = CouponCodeInfoProvider.GetDiscountCouponCode(discount.DiscountID, couponCode);
            if ((coupon != null) && !coupon.UseLimitExceeded)
            {
                coupon.CouponCodeUseCount++;
                coupon.Update();
            }
        }


        /// <summary>
        /// Gets running discounts on given sites in defined time. If discount uses coupons, only discounts with usable coupons are returned.
        /// </summary>
        /// <param name="site">Site to get active discounts for.</param>
        /// <param name="date">Time when discount should be active.</param>
        /// <param name="discountApplicationEnum">Type of discount which will be selected. If null all types are selected.</param>
        protected virtual ObjectQuery<DiscountInfo> GetRunningDiscountsInternal(SiteInfoIdentifier site, DateTime date, DiscountApplicationEnum? discountApplicationEnum = null)
        {
            // Running discounts query
            var query = GetDiscounts(site, true);

            if (discountApplicationEnum.HasValue)
            {
                query.WhereEquals("DiscountApplyTo", discountApplicationEnum.Value.ToStringRepresentation());
            }

            query.And(new WhereCondition().Where("DiscountValidFrom", QueryOperator.LessThan, date)
                                          .Or()
                                          .WhereNull("DiscountValidFrom"))
                 .And(new WhereCondition().Where("DiscountValidTo", QueryOperator.LargerThan, date)
                                          .Or()
                                          .WhereNull("DiscountValidTo"));

            // Catalog discounts never uses coupons
            if (discountApplicationEnum != DiscountApplicationEnum.Products)
            {
                // Discounts with available coupon codes
                var codesWhere = new IDQuery(CouponCodeInfo.OBJECT_TYPE, "CouponCodeDiscountID")
                                     .And(new WhereCondition()
                                     .WhereNull("CouponCodeUseLimit")
                                     .Or()
                                     .Where("CouponCodeUseCount < CouponCodeUseLimit"));

                query.And(new WhereCondition().WhereFalse("DiscountUsesCoupons")
                                             .Or()
                                             .WhereIn("DiscountID", codesWhere));
            }

            return query;
        }

        #endregion

    }
}