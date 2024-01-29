using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

using CMS.Base;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.DataEngine.Query;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Base provider for MultiBuyCouponCodeInfoProvider and CouponCodeInfoProvider
    /// </summary>    
    public abstract class BaseCouponCodeInfoProvider<TInfo, TProvider> : AbstractInfoProvider<TInfo, TProvider>
        where TInfo : AbstractInfo<TInfo>, new()
        where TProvider : AbstractInfoProvider<TInfo, TProvider>, new()
    {
        internal const string USES_COUPON_ALIAS = "Uses";
        internal const string UNLIMITED_COUPON_COUNT_ALIAS = "UnlimitedCodeCount";
        internal const string LIMIT_COUPON_ALIAS = "Limit";


        /// <summary>
        /// Initializes a new instance of the BaseCouponCodeInfoProvider{TInfo, TProvider} class.
        /// </summary>
        /// <param name="typeInfo">Object type information</param>
        protected BaseCouponCodeInfoProvider(ObjectTypeInfo typeInfo)
            : base(typeInfo, new HashtableSettings
            {
                Name = true,
                Load = LoadHashtableEnum.None,
                UseWeakReferences = true
            })
        {
        }


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
            var used = ValidationHelper.GetLong(dr[USES_COUPON_ALIAS], 0);
            var isUnlimited = ValidationHelper.GetLong(dr[UNLIMITED_COUPON_COUNT_ALIAS], 0) != 0;
            var limitMax = ValidationHelper.GetLong(dr[LIMIT_COUPON_ALIAS], 0);
            var limit = isUnlimited ? ResHelper.GetString("com.couponcode.unlimited") : limitMax.ToString();
            var message = dataOnly ? "" : $"{ResHelper.GetString("com.couponcode.numberofuses")}: ";

            // Format message
            return $"{message}{used} / {limit}";
        }


        /// <summary>
        /// Gets info about how many coupons are available in specific discount and how many of them were already used.
        /// </summary>
        /// <remarks>
        /// Returns <see cref="ObjectQuery"/> with one table containing columns: discount ID, Uses, UnlimitedCodeCount and Limit.
        /// </remarks>
        /// <param name="discountIDs">IDs of discounts to get coupon counts for. Use null for all relevant discounts.</param>
        /// <param name="useCountColumn">Name of column containing count of already executed redemptions.</param>
        /// <param name="useLimitColumn">Name of column containing count of redemption limit.</param>
        protected ObjectQuery GetCouponCodeUseCount(IEnumerable<int> discountIDs, string useCountColumn, string useLimitColumn)
        {
            var parentColumnName = TypeInfo.ParentIDColumn;
            var unlimitedExpression = GetUnlimitedCodeCountExpression(useLimitColumn);

            var query = new ObjectQuery(TypeInfo.ObjectType);
            query.Columns(
                new AggregatedColumn(AggregationType.Sum, useCountColumn).As(USES_COUPON_ALIAS),
                new CountColumn(unlimitedExpression).As(UNLIMITED_COUPON_COUNT_ALIAS),
                new AggregatedColumn(AggregationType.Sum, useLimitColumn.AsColumn().Cast(FieldDataType.LongInteger).GetExpression()).As(LIMIT_COUPON_ALIAS),
                new QueryColumn(parentColumnName)
            );

            if (discountIDs != null)
            {
                query = query.WhereIn(parentColumnName, discountIDs.ToArray());
            }

            query = query.GroupBy(parentColumnName);

            return query;
        }


        /// <summary>
        /// Checks user's read/modify discount permissions. Permission check result is stored in result property.
        /// </summary>
        /// <param name="permission">Permission type</param>
        /// <param name="siteName">Name of the site</param>
        /// <param name="userInfo"><see cref="IUserInfo"/> object</param>
        /// <param name="exceptionOnFailure">If <c>true</c>, PermissionCheckException is thrown whenever a permission check fails</param>
        /// <param name="result">Check permissions result.</param>
        /// <returns>Returns <c>true</c> if permission type was handled; <c>false</c> otherwise</returns>
        internal static bool TryCheckDiscountPermissions(PermissionsEnum permission, string siteName, IUserInfo userInfo, bool exceptionOnFailure, out bool result)
        {
            result = false;

            switch (permission)
            {
                case PermissionsEnum.Read:
                    result = ECommerceHelper.IsUserAuthorizedForPermission(EcommercePermissions.DISCOUNTS_READ, siteName, userInfo, exceptionOnFailure);
                    break;

                case PermissionsEnum.Create:
                case PermissionsEnum.Delete:
                case PermissionsEnum.Modify:
                    result = ECommerceHelper.IsUserAuthorizedForPermission(EcommercePermissions.DISCOUNTS_MODIFY, siteName, userInfo, exceptionOnFailure);
                    break;

                default:
                    return false;
            }

            return true;
        }


        private static string GetUnlimitedCodeCountExpression(string useLimitColumn)
        {
            var builder = new WhereBuilder();
            var statement = builder.GetIsNull(useLimitColumn, false);

            var dictionary = new Dictionary<string, string>
            {
                { statement, "1" }
            };

            return SqlHelper.GetCase(dictionary, builder.NULL);
        }
    }
}
