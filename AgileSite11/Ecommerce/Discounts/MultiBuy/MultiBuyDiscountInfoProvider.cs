using System;

using CMS.DataEngine;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Class providing MultiBuyDiscountInfo management.
    /// </summary>
    public class MultiBuyDiscountInfoProvider : AbstractInfoProvider<MultiBuyDiscountInfo, MultiBuyDiscountInfoProvider>
    {
        #region "Constructors"

        /// <summary>
        /// Constructor
        /// </summary>
        public MultiBuyDiscountInfoProvider()
            : base(MultiBuyDiscountInfo.TYPEINFO, new HashtableSettings
            {
                ID = true,
                Name = false,
                Load = LoadHashtableEnum.All
            })
        {
        }

        #endregion


        #region "Public methods - Basic"

        /// <summary>
        /// Returns a query for all the multi buy discount objects.
        /// </summary>
        public static ObjectQuery<MultiBuyDiscountInfo> GetMultiBuyDiscounts()
        {
            return ProviderObject.GetMultiBuyDiscountsInternal();
        }


        /// <summary>
        /// Returns a query for all the product coupon discount objects.
        /// </summary>
        public static ObjectQuery<MultiBuyDiscountInfo> GetProductCouponDiscounts()
        {
            return ProviderObject.GetProductCouponDiscountsInternal();
        }


        /// <summary>
        /// Returns MultiBuyDiscountInfo with specified ID.
        /// </summary>
        /// <param name="id">MultiBuyDiscountInfo ID</param>
        public static MultiBuyDiscountInfo GetMultiBuyDiscountInfo(int id)
        {
            return ProviderObject.GetInfoById(id);
        }


        /// <summary>
        /// Returns MultiBuyDiscountInfo with specified name.
        /// </summary>
        /// <param name="name">MultiBuyDiscountInfo name</param>
        /// <param name="siteIdentifier">Site identifier (site ID or site name)</param>  
        public static MultiBuyDiscountInfo GetMultiBuyDiscountInfo(string name, SiteInfoIdentifier siteIdentifier)
        {
            return ProviderObject.GetInfoByCodeName(name, siteIdentifier);
        }


        /// <summary>
        /// Returns MultiBuyDiscountInfo with specified GUID.
        /// </summary>
        /// <param name="guid">MultiBuyDiscountInfo GUID</param>                
        public static MultiBuyDiscountInfo GetMultiBuyDiscountInfo(Guid guid)
        {
            return ProviderObject.GetInfoByGuid(guid);
        }


        /// <summary>
        /// Sets (updates or inserts) specified MultiBuyDiscountInfo.
        /// </summary>
        /// <param name="infoObj">MultiBuyDiscountInfo to be set</param>
        public static void SetMultiBuyDiscountInfo(MultiBuyDiscountInfo infoObj)
        {
            ProviderObject.SetInfo(infoObj);
        }


        /// <summary>
        /// Deletes specified MultiBuyDiscountInfo.
        /// </summary>
        /// <param name="infoObj">MultiBuyDiscountInfo to be deleted</param>
        public static void DeleteMultiBuyDiscountInfo(MultiBuyDiscountInfo infoObj)
        {
            ProviderObject.DeleteInfo(infoObj);
        }


        /// <summary>
        /// Deletes MultiBuyDiscountInfo with specified ID.
        /// </summary>
        /// <param name="id">MultiBuyDiscountInfo ID</param>
        public static void DeleteMultiBuyDiscountInfo(int id)
        {
            MultiBuyDiscountInfo infoObj = GetMultiBuyDiscountInfo(id);
            DeleteMultiBuyDiscountInfo(infoObj);
        }

        #endregion


        #region "Public methods - Advanced"

        /// <summary>
        /// Returns a query for all the MultiBuyDiscountInfo objects of a specified site.
        /// </summary>
        /// <param name="siteIdentifier">Site identifier (site ID or site name)</param>  
        public static ObjectQuery<MultiBuyDiscountInfo> GetMultiBuyDiscounts(SiteInfoIdentifier siteIdentifier)
        {
            return ProviderObject.GetMultiBuyDiscountsInternal().OnSite(siteIdentifier);
        }


        /// <summary>
        /// Returns a query for all the product coupon discount objects.
        /// </summary>
        public static ObjectQuery<MultiBuyDiscountInfo> GetProductCouponDiscounts(SiteInfoIdentifier siteIdentifier)
        {
            return ProviderObject.GetProductCouponDiscountsInternal().OnSite(siteIdentifier);
        }


        /// <summary>
        /// Indicates if given coupon code is suitable for discount. Returns false if discount has no codes assigned.
        /// </summary>
        /// <param name="discount">The discount.</param>
        /// <param name="couponCode">Code to be checked</param>
        /// <param name="ignoreUseLimit">Indicates if use limitation is to be ignored.</param>        
        public static bool AcceptsCoupon(MultiBuyDiscountInfo discount, string couponCode, bool ignoreUseLimit)
        {
            return ProviderObject.AcceptsCouponInternal(discount, couponCode, ignoreUseLimit);
        }


        /// <summary>
        /// Gets running multibuy discounts on given sites in defined time. If discount uses coupons, only discounts with usable coupons are returned.
        /// </summary>
        /// <param name="site">Site to get active discounts for.</param>
        /// <param name="date">Time when discount should be active.</param>      
        public static ObjectQuery<MultiBuyDiscountInfo> GetRunningMultiBuyDiscounts(SiteInfoIdentifier site, DateTime date)
        {
            return ProviderObject.GetRunningMultiBuyDiscountsInternal(site, date);
        }


        /// <summary>
        /// Gets running product coupon discounts on given sites in defined time. If discount uses coupons, only discounts with usable coupons are returned.
        /// </summary>
        /// <param name="site">Site to get active discounts for.</param>
        /// <param name="date">Time when discount should be active.</param>      
        public static ObjectQuery<MultiBuyDiscountInfo> GetRunningProductCouponDiscounts(SiteInfoIdentifier site, DateTime date)
        {
            return ProviderObject.GetRunningProductCouponDiscountsInternal(site, date);
        }

        #endregion


        #region "Internal methods - Advanced"

        /// <summary>
        /// Returns a query for all the multi buy discount objects.
        /// </summary>
        protected virtual ObjectQuery<MultiBuyDiscountInfo> GetMultiBuyDiscountsInternal()
        {
            var condition = MultiBuyDiscountInfo.TYPEINFO.TypeCondition.GetWhereCondition();
            return ProviderObject.GetObjectQuery().Where(condition);
        }


        /// <summary>
        /// Returns a query for all the product coupon discount objects.
        /// </summary>
        protected virtual ObjectQuery<MultiBuyDiscountInfo> GetProductCouponDiscountsInternal()
        {
            var condition = MultiBuyDiscountInfo.TYPEINFO_PRODUCT_COUPON.TypeCondition.GetWhereCondition();
            return ProviderObject.GetObjectQuery().Where(condition);
        }


        /// <summary>
        /// Indicates if given coupon code is suitable for discount. Returns false if discount has no codes assigned.
        /// </summary>
        /// <param name="discount">The discount.</param>
        /// <param name="couponCode">Code to be checked</param>
        /// <param name="ignoreUseLimit">Indicates if use limitation is to be ignored.</param>        
        protected virtual bool AcceptsCouponInternal(MultiBuyDiscountInfo discount, string couponCode, bool ignoreUseLimit)
        {
            if (!discount.MultiBuyDiscountUsesCoupons)
            {
                return false;
            }

            var code = MultiBuyCouponCodeInfoProvider.GetDiscountCouponCode(discount.MultiBuyDiscountID, couponCode);
            return (code != null) && (ignoreUseLimit || !code.UseLimitExceeded);
        }


        /// <summary>
        /// Gets running discounts on given sites in defined time. If discount uses coupons, only discounts with usable coupons are returned.
        /// </summary>
        /// <param name="site">Site to get active discounts for.</param>
        /// <param name="date">Time when discount should be active.</param>
        protected virtual ObjectQuery<MultiBuyDiscountInfo> GetRunningMultiBuyDiscountsInternal(SiteInfoIdentifier site, DateTime date)
        {
            return GetMultiBuyDiscounts(site)
                .Where(GetRunningDiscountsWhereCondition(date));
        }


        /// <summary>
        /// Gets running product coupon discounts on given sites in defined time. If discount uses coupons, only discounts with usable coupons are returned.
        /// </summary>
        /// <param name="site">Site to get active discounts for.</param>
        /// <param name="date">Time when discount should be active.</param>
        protected virtual ObjectQuery<MultiBuyDiscountInfo> GetRunningProductCouponDiscountsInternal(SiteInfoIdentifier site, DateTime date)
        {
            return GetProductCouponDiscounts(site)
                .Where(GetRunningDiscountsWhereCondition(date));
        }


        private static WhereCondition GetRunningDiscountsWhereCondition(DateTime date)
        {
            // Discounts with available coupon codes
            var codesWhere = new IDQuery(MultiBuyCouponCodeInfo.OBJECT_TYPE, "MultiBuyCouponCodeMultiBuyDiscountID")
                .And(new WhereCondition()
                    .WhereNull("MultiBuyCouponCodeUseLimit")
                    .Or()
                    .Where("MultiBuyCouponCodeUseCount < MultiBuyCouponCodeUseLimit"));
            
            return new WhereCondition()
                .WhereTrue("MultiBuyDiscountEnabled")
                .And(new WhereCondition().WhereLessThan("MultiBuyDiscountValidFrom", date)
                                         .Or()
                                         .WhereNull("MultiBuyDiscountValidFrom"))
                .And(new WhereCondition().WhereGreaterThan("MultiBuyDiscountValidTo", date)
                                         .Or()
                                         .WhereNull("MultiBuyDiscountValidTo"))
                .And(new WhereCondition().WhereFalse("MultiBuyDiscountUsesCoupons")
                                         .Or()
                                         .WhereIn("MultiBuyDiscountID", codesWhere));
        }

        #endregion
    }
}